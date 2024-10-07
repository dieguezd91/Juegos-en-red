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

    private Vector2 lastDirection = Vector2.up;

    [SerializeField] private WeaponInfo startingWeapon;
    private int currentWeapon;
    private WeaponInfo[] equipedWeapons = new WeaponInfo[2];

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
                currentStamina += staminaRegenRate * Time.deltaTime;
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
