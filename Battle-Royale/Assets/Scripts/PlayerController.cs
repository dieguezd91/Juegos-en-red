using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private FSM<PlayerStateEnum> _fsm;
    private ITreeNode _root;
    public PlayerSO PlayerData;
    public LifeController LifeController;
    public PlayerStateEnum _currentState = PlayerStateEnum.Idle;

    public PhotonView pv;
    private Rigidbody2D _rb;
    [SerializeField] private float speedMovement = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] public float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 2.5f;
    [SerializeField] private float staminaDrainRate = 15f;
    [SerializeField] private float dashStaminaCost = 40f;

    [SerializeField] private float interactionRange;
    [SerializeField] private LayerMask interactionLayer;

    private Vector2 _inputMovement;
    public Vector2 InputMovement => _inputMovement;

    public float currentStamina;
    public bool isSprinting;

    private bool isDashing;
    public bool IsDashing => isDashing;

    private bool canDash = true;
    private int _ammo;

    private Vector2 lastDirection = Vector2.up;

    [SerializeField] private WeaponInfo startingWeapon;
    private int currentWeapon;
    private WeaponInfo[] equipedWeapons = new WeaponInfo[2];

    public static event System.Action<PlayerController> OnPlayerControllerInstantiated;

    public Animator animator;

    private void Awake()
    {
        EquipWeapon(startingWeapon, 0);
        SwitchWeapon(0);
    }

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        currentStamina = PlayerData.MaxStamina;
        LifeController = GetComponent<LifeController>();
        if (pv.IsMine)
        {
            OnPlayerControllerInstantiated?.Invoke(this);
            StartCoroutine(RegenerateStamina());
        }
        InitializedTree();
        InitializedFSM();
    }

    private void Update()
    {
        Debug.Log(animator);
        //if (pv.IsMine)
        //{
        //    HandleInput();
        //}
        //if (LifeController.CanHeal() && !LifeController._isHealing && Input.GetKey(KeyCode.K)) //Input Provisorio
        //{
        //    LifeController.StartHealing();
        //}
        _fsm.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (pv.IsMine/* && !isDashing*/ && !ImDodge() && !ImHealing())
        {
            Move();
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
    bool ImDodge() => isDashing;
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

        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }
        else
        {
            isSprinting = false;
        }

        // Presionar Space para hacer el dash
        if (Input.GetKeyDown(KeyCode.Space) && canDash && currentStamina >= dashStaminaCost)
        {
            isDashing = true;
            StartCoroutine(Dash());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

        // Cambiar de arma con teclas 1 y 2
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
    }

    private void Move()
    {
        float movementSpeed = isSprinting ? sprintSpeed : speedMovement;

        _rb.velocity = _inputMovement * movementSpeed;
        _rb.rotation = 0f;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Consumir stamina
        currentStamina -= dashStaminaCost;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Direccion del dash (ultima direccion de movimiento si no hay input actual)
        Vector2 dashDirection = _inputMovement == Vector2.zero ? lastDirection : _inputMovement;
        _rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // Esperar el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator RegenerateStamina()
    {
        while (true)
        {
            // Regenerar stamina si no esta corriendo ni haciendo dash
            if (!isSprinting && !isDashing && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate/* * Time.deltaTime*/;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }

            // Regenerar stamina continuamente
            yield return new WaitForSeconds(1f);
        }
    }

    private void Interact()
    {
        Collider2D[] interactable = new Collider2D[1];
        interactable[0] = Physics2D.OverlapCircle(transform.position, interactionRange, interactionLayer);

        try
        {
            if (interactable[0] != null)
            {
                if (interactable[0].TryGetComponent<IInteractable>(out IInteractable interactTarget))
                {
                    interactTarget.Interact();
                }
            }
        }
        catch { }
    }

    #region WEAPON_CONTROL
    // Equipar un arma
    public void EquipWeapon(WeaponInfo weapon, int slot)
    {
        if (equipedWeapons[slot] == null)
        {
            equipedWeapons[slot] = weapon;
        }
        else
        {
            DiscardWeapon();
            equipedWeapons[slot] = weapon;
        }
    }

    // Descartar el arma en el slot 1
    private void DiscardWeapon()
    {
        if (equipedWeapons[1] != null)
        {
            WeaponInfo weapon = equipedWeapons[1];
            SwitchWeapon(0);
            equipedWeapons[1] = null;
            PhotonNetwork.Instantiate(weapon.weaponPrefab.name, new Vector2(transform.position.x - 2, transform.position.y), Quaternion.identity);
        }
    }

    public void SwitchWeapon(int weaponSlot)
    {
        gameObject.GetComponent<PlayerWeaponController>().UpdateWeaponInfo(equipedWeapons[weaponSlot]);
        currentWeapon = weaponSlot;
    }

    public int GetCurrentAmmo()
    {
        return _ammo;
    }
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    
}
