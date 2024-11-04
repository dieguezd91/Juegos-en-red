using Photon.Pun;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviourPunCallbacks
{
    private PhotonView _pv;
    [SerializeField] private Transform arm;
    [SerializeField] private Transform weaponPlaceHolder;
    [SerializeField] private Transform playerSprite;
    [SerializeField] private WeaponSO defaultWeapon;

    public WeaponBase currentWeapon { get; private set; }
    public event System.Action<WeaponBase> OnWeaponChanged;
    private Camera mainCamera;
    private bool isFacingLeft = false;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        mainCamera = Camera.main;

        if (_pv.IsMine && defaultWeapon != null)
        {
            EquipWeapon(defaultWeapon);
        }
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            Aim();
            HandleShooting();

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon?.Reload();
            }
        }
    }

    private void Aim()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = mousePosition - transform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        isFacingLeft = Mathf.Abs(angle) > 90;

        // Ajustar el sprite del personaje
        playerSprite.localScale = new Vector3(isFacingLeft ? -1 : 1, 1, 1);

        // Calcular la rotaci�n del brazo
        float armRotation;
        if (isFacingLeft)
        {
            armRotation = angle + 180;
        }
        else
        {
            armRotation = angle;
        }

        // Aplicar rotaci�n al brazo
        arm.rotation = Quaternion.Euler(0, 0, armRotation);

        // Ajustar el arma
        if (currentWeapon != null)
        {
            if (isFacingLeft)
            {
                currentWeapon.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                currentWeapon.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        // Mantener el weaponPlaceHolder sin modificaciones
        weaponPlaceHolder.localScale = Vector3.one;
        weaponPlaceHolder.localRotation = Quaternion.identity;
    }

    private void HandleShooting()
    {
        if (currentWeapon == null) return;

        bool shouldShoot = currentWeapon.weaponData.automatic ?
            Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (shouldShoot && currentWeapon.CanShoot())
        {
            // Calcular la direcci�n del disparo basada en la posici�n del mouse
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)(mousePosition - transform.position)).normalized;

            // Ya no necesitamos invertir la direcci�n, usamos la direcci�n real del mouse
            currentWeapon.Shoot(direction);
        }
    }

    public void EquipWeapon(WeaponSO weaponData)
    {
        if (weaponData == null || weaponPlaceHolder == null) return;

        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        GameObject weaponInstance = Instantiate(weaponData.weaponPrefab, weaponPlaceHolder);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        currentWeapon = weaponInstance.GetComponent<WeaponBase>();
        if (currentWeapon != null)
        {
            currentWeapon.Initialize(weaponData);
            // Notificar el cambio de arma
            OnWeaponChanged?.Invoke(currentWeapon);
        }
    }
}