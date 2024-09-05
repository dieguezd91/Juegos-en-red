using System.Collections;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public PhotonView pv;
    private Rigidbody2D _rb;
    [SerializeField] private float speedMovement = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] public float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 1f;
    [SerializeField] private float staminaDrainRate = 2.5f;

    private Vector2 _inputMovement;
    public float currentStamina;
    private bool isSprinting;

    public static event System.Action<PlayerController> OnPlayerControllerInstantiated;

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
        if (pv.IsMine)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        var moveX = Input.GetAxis("Horizontal");
        var moveY = Input.GetAxis("Vertical");
        _inputMovement = new Vector2(moveX, moveY).normalized;

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
    }

    private void Move()
    {
        float movementSpeed = isSprinting ? sprintSpeed : speedMovement;
        _rb.velocity = _inputMovement * movementSpeed;
    }

    private IEnumerator RegenerateStamina()
    {
        while (true)
        {
            // Si no esta corriendo y la stamina no esta llena, regenera stamina
            if (!isSprinting && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }

            // Esperar un segundo antes de regenerar nuevamente
            yield return new WaitForSeconds(1f);
        }
    }
}
