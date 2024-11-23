using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PlayerModel model;
    public PlayerView view;
    private FSM<PlayerStateEnum> _fsm;
    private ITreeNode _root;
    public PlayerSO PlayerData;
    public LifeController LifeController;
    public PhotonView pv;
    private Rigidbody2D _rb;

    private Vector2 _inputMovement;
    public Vector2 InputMovement => _inputMovement;
    private Vector2 lastDirection = Vector2.up;
    public static event System.Action<PlayerController> OnPlayerControllerInstantiated;
    private bool isInitialized = false;
    //private int _ammo;
    private ItemBase itemSelected;
    private bool dropMode = false;

    [SerializeField] private TextMeshProUGUI nicknameText;
    private string nickname;
    [SerializeField] private Transform nicknameCanvas;

    private void Start()
    {
        model = new PlayerModel(PlayerData);
        view = GetComponent<PlayerView>();
        pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody2D>();
        LifeController = GetComponent<LifeController>();

        InitializedTree();
        InitializedFSM();
        isInitialized = true;

        if (pv.IsMine)
        {
            pv.RPC("SetPlayerNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }

        OnPlayerControllerInstantiated?.Invoke(this);

        if (pv.IsMine)
        {
            StartCoroutine(RegenerateStamina());
        }
    }

    public bool IsInitialized()
    {
        return isInitialized && pv != null;
    }

    private void Update()
    {
        _fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (pv.IsMine && !ImDodge() && !ImHealing())
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        if (nicknameCanvas != null)
        {
            nicknameCanvas.localScale = Vector3.one;
            nicknameCanvas.forward = Camera.main.transform.forward;
        }
    }


    void InitializedFSM()
    {
        _fsm = new FSM<PlayerStateEnum>();
        var _states = new List<PlayerStateBase<PlayerStateEnum>>();

        var idle = new PlayerIdleState<PlayerStateEnum>(_root);
        var moving = new PlayerMovingState<PlayerStateEnum>(_root);
        var dodge = new PlayerDodgeState<PlayerStateEnum>(_root);
        var healing = new PlayerHealingState<PlayerStateEnum>(_root);
        var reloading = new PlayerReloadingState<PlayerStateEnum>(_root);
        var died = new PlayerDiedState<PlayerStateEnum>();

        _states.Add(idle);
        _states.Add(moving);
        _states.Add(dodge);
        _states.Add(healing);
        _states.Add(reloading);
        _states.Add(died);

        for (int i = 0; i < _states.Count; i++)
        {
            _states[i].InitializedState(this, _fsm);
        }

        #region ADD TRANSITIONS
        idle.AddTransition(PlayerStateEnum.Moving, moving);
        idle.AddTransition(PlayerStateEnum.Healing, healing);
        idle.AddTransition(PlayerStateEnum.Dodge, dodge);
        idle.AddTransition(PlayerStateEnum.Died, died);
        idle.AddTransition(PlayerStateEnum.Reloading, reloading);

        moving.AddTransition(PlayerStateEnum.Idle, idle);
        moving.AddTransition(PlayerStateEnum.Healing, healing);
        moving.AddTransition(PlayerStateEnum.Dodge, dodge);
        moving.AddTransition(PlayerStateEnum.Died, died);
        moving.AddTransition(PlayerStateEnum.Reloading, reloading);

        dodge.AddTransition(PlayerStateEnum.Idle, idle);
        dodge.AddTransition(PlayerStateEnum.Moving, moving);
        dodge.AddTransition(PlayerStateEnum.Died, died);

        healing.AddTransition(PlayerStateEnum.Idle, idle);
        healing.AddTransition(PlayerStateEnum.Moving, moving);
        healing.AddTransition(PlayerStateEnum.Died, died);

        reloading.AddTransition(PlayerStateEnum.Idle, idle);
        reloading.AddTransition(PlayerStateEnum.Moving, moving);
        reloading.AddTransition(PlayerStateEnum.Died, died);
        #endregion

        _fsm.SetInit(idle);
    }

    #region ACTIONS-PLAYER
    void ActionIdle() => _fsm.Transitions(PlayerStateEnum.Idle);
    void ActionMoving() => _fsm.Transitions(PlayerStateEnum.Moving);
    void ActionDodge() => _fsm.Transitions(PlayerStateEnum.Dodge);
    void ActionHealing() => _fsm.Transitions(PlayerStateEnum.Healing);
    void ActionDied() => _fsm.Transitions(PlayerStateEnum.Died);
    #endregion
    #region QUESTIONS-PLAYER
    bool ImAlive() => LifeController.currentHp > 0f;
    bool ImMoving() => InputMovement != Vector2.zero;
    bool ImDodge() => model.IsDashing;
    bool ImHealing() => LifeController._isHealing;
    //bool ImReloading() => _isReloading;
    #endregion
    void InitializedTree()
    {
        var idle = new TreeAction(ActionIdle);
        var moving = new TreeAction(ActionMoving);
        var dodge = new TreeAction(ActionDodge);
        var healing = new TreeAction(ActionHealing);
        var died = new TreeAction(ActionDied);


        var imMoving = new TreeQuestion(ImMoving, moving, idle);
        var imHealing = new TreeQuestion(ImHealing, healing, imMoving);
        var imDodge = new TreeQuestion(ImDodge, dodge, imHealing);
        var imAlive = new TreeQuestion(ImAlive, imDodge, died);

        _root = imAlive;
    }

    public void HandleInput()
    {
        if (LifeController.CanHeal() && !LifeController._isHealing && Input.GetKey(KeyCode.K)) //Input Provisorio
        {
            LifeController.StartHealing();
        }

        var moveX = Input.GetAxisRaw("Horizontal");
        var moveY = Input.GetAxisRaw("Vertical");

        _inputMovement = new Vector2(moveX, moveY).normalized;

        // Guardar la ultima direccion de movimiento
        if (_inputMovement != Vector2.zero)
        {
            lastDirection = _inputMovement;
        }

        if (Input.GetKey(KeyCode.LeftShift) && model.CurrentStamina > 0 && ImMoving())
        {
            model.Sprint(model.StaminaDrainRate);
        }
        else
        {
            model.IsSprinting = false;
        }

        // Presionar Space para hacer el dash
        if (Input.GetKeyDown(KeyCode.Space) && model.CanDash && model.CurrentStamina >= model.DashStaminaCost)
        {
            model.IsDashing = true;
            StartCoroutine(Dash());
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
            //print("Player Interacted");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            //dropMode = !dropMode;
            if (gameObject.GetComponent<PlayerWeaponController>().CheckWeaponSlot(1) != null)
            {
                gameObject.GetComponent<PlayerWeaponController>().DropWeapon();
            }
        }

        //-------------------------------Use Item----------------------------
        if (dropMode == false)
        {
            if (Input.GetKeyDown(KeyCode.Alpha5) && model.itemsInventory[0] != null)
            {
                if (model.itemsInventory[0] != null)
                {
                    model.itemsInventory[0].Use(this);
                    model.itemsInventory[0] = null;
                    UIManager.Instance.SetItemIcon(0, null);
                    model.itemInventoryFull = false;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6) && model.itemsInventory[1] != null)
            {
                if (model.itemsInventory[1] != null)
                {
                    model.itemsInventory[1].Use(this);
                    model.itemsInventory[1] = null;
                    UIManager.Instance.SetItemIcon(1, null);
                    model.itemInventoryFull = false;
                }
            }

//-------------------------------Switch Weapon---------------------------------------------
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (gameObject.GetComponent<PlayerWeaponController>().CheckWeaponSlot(1) != null)
                {
                    gameObject.GetComponent<PlayerWeaponController>().SwitchWeapon(1);
                }
                else
                {
                    print("No weapon detected on slot 1");
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && pv.IsMine)
            {
                if (gameObject.GetComponent<PlayerWeaponController>().CheckWeaponSlot(0) != null)
                {
                    gameObject.GetComponent<PlayerWeaponController>().SwitchWeapon(0);
                }
                else
                {
                    print("No weapon detected on slot 0");
                }
            }
        }
        //--------------------------Drop Weapons-----------------------------------------------
        //else if (dropMode == true)
        //{
        //    if (Input.GetKeyDown(KeyCode.Alpha1))
        //    {
        //        if (gameObject.GetComponent<PlayerWeaponController>().CheckWeaponSlot(1) != null)
        //        {
        //            gameObject.GetComponent<PlayerWeaponController>().DropWeapon();
        //            dropMode = false;
        //        }
        //    }
        //}
    }

    private void Move()
    {
        float movementSpeed = model.IsSprinting ? model.SprintSpeed : model.SpeedMovement;
        _rb.velocity = _inputMovement * movementSpeed;
        _rb.rotation = 0f;
    }

    private IEnumerator Dash()
    {
        model.IsDashing = true;
        model.CanDash = false;

        // Consumir stamina
        model.StaminaCost(model.DashStaminaCost);

        // Direccion del dash (ultima direccion de movimiento si no hay input actual)
        Vector2 dashDirection = _inputMovement == Vector2.zero ? lastDirection : _inputMovement;
        _rb.velocity = dashDirection * model.DashSpeed;

        yield return new WaitForSeconds(model.DashDuration);

        model.IsDashing = false;

        // Esperar el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(model.DashCooldown);
        model.CanDash = true;
    }

    private IEnumerator RegenerateStamina()
    {
        while (true)
        {
            // Regenerar stamina continuamente
            model.RegenerateStamina();
            yield return new WaitForSeconds(1f);
        }
    }

    public void Interact()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, model.InteractionRange, model.InteractionLayer);
        Collider2D closestCollider = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D col in hitColliders)
        {
            if (col.gameObject == gameObject) continue;

            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = col;
                }
            }
        }

        if (closestCollider != null)
        {
            IInteractable closestInteractable = closestCollider.GetComponent<IInteractable>();
            if (closestInteractable != null)
            {
                closestInteractable.Interact();
            }
        }
    }

    public void Interact(LayerMask mask)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, model.InteractionRange, mask);
        foreach (Collider2D col in hitColliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null) interactable.Interact();
        }
    }

    public void AddItemToInventory(ItemBase item)
    {
        if (model.itemInventoryFull == false)
        {
            if (item.GetType() == typeof(ConsumableInfo))
            {
                if (model.itemsInventory[0] == null)
                {
                    model.itemsInventory[0] = item as ConsumableInfo;
                    UIManager.Instance.SetItemIcon(0, item.Icon);
                }
                else if (model.itemsInventory[1] == null)
                {
                    model.itemsInventory[1] = item as ConsumableInfo;
                    UIManager.Instance.SetItemIcon(1, item.Icon);
                }
                else
                {
                    print("inventory is full, please use an Item and then try again");
                }

                if (model.itemsInventory[0] != null && model.itemsInventory[1] != null)
                {
                    model.itemInventoryFull = true;
                    print("inventory full");
                }
            }
        }  
       
    }

    [PunRPC]
    public void SetPlayerNickname(string newNickname)
    {
        nickname = newNickname;

        if (nicknameText != null)
        {
            nicknameText.text = nickname;
        }
    }

    public string GetNickname()
    {
        return nickname;
    }

    //private void DropItem(ItemBase item)
    //{
    //    if (item.GetType() == typeof(GranadeInfo))
    //    {
    //        GranadeInfo granade = item as GranadeInfo;

    //        granade.Spawn(transform.forward*3, Quaternion.identity);
    //    }
    //}
}
