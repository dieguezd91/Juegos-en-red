using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
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
    public float currentStamina;
    private bool isSprinting;
    private bool isDashing;
    private bool canDash = true;
    private int _ammo;


    //Equipment

    [SerializeField]private WeaponInfo startingWeapon;
    private int currentWeapon;
    private WeaponInfo[] equipedWeapons = new WeaponInfo[2];
    //private IWeapon[] equipedGranades;



    public static event System.Action<PlayerController> OnPlayerControllerInstantiated;
    private void Awake()
    {
        EquipWeapon(startingWeapon, 0);
        SwitchWeapon(0);
    }

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody2D>();
        currentStamina = maxStamina; 

        if (pv.IsMine)
        {
            OnPlayerControllerInstantiated?.Invoke(this);
            StartCoroutine(RegenerateStamina());
        }

    }

    private void Update()
    {
        if (pv.IsMine)
        {
            HandleInput();
        }
    }

    private void FixedUpdate()
    {
        if (pv.IsMine && !isDashing)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        var moveX = Input.GetAxis("Horizontal");
        var moveY = Input.GetAxis("Vertical");
        _inputMovement = new Vector2(moveX, moveY).normalized;

        // Verificar si el jugador esta corriendo
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

        // Presiona Space para hacer el dash
        if (Input.GetKeyDown(KeyCode.Space) && canDash && currentStamina >= dashStaminaCost)
        {
            StartCoroutine(Dash());
        }
        
        // Presiona E para interactuar con cofres u otros elementos
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }

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
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Consumir stamina
        currentStamina -= dashStaminaCost;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        // Realizar el dash
        Vector2 dashDirection = _inputMovement;
        if (dashDirection == Vector2.zero)  // Si no hay movimiento, usar la direccion hacia adelante
        {
            dashDirection = Vector2.up;  // O cualquier direccion por defecto
        }

        _rb.velocity = dashDirection * dashSpeed;

        // Esperar la duracion del dash
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // Esperar el cooldown antes de permitir hacer otro dash
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator RegenerateStamina()
    {
        while (true)
        {
            // Si no está corriendo y la stamina no esta llena, regenera stamina
            if (!isSprinting && !isDashing && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }

            // Regenera stamina cada un segundo
            yield return new WaitForSeconds(1f);
        }
    }

    //Logica de interaccion
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

    private void DiscardWeapon()
    {
        if(equipedWeapons[1] != null)
        {
            WeaponInfo weapon = equipedWeapons[1];
            SwitchWeapon(0);
            equipedWeapons[1] = null;
            PhotonNetwork.Instantiate(weapon.weaponPrefab.name, new Vector2(transform.position.x - 2, transform.position.y), Quaternion.identity);
        }
        
    }

    private void DiscardGranade(int index)
    {

    }

    public int GetCurrentAmmo()
    {
        return _ammo;
    }

    public void SwitchWeapon(int weaponSlot)
    {
        gameObject.GetComponent<PlayerWeaponController>().UpdateWeaponInfo(equipedWeapons[weaponSlot]);
        currentWeapon = weaponSlot;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

}
