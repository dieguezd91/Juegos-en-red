using Photon.Pun;
using System;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviourPunCallbacks
{
    private PhotonView _pv;
    public PhotonView pv { get { return _pv; } }
    [SerializeField] private Transform arm;
    [SerializeField] private Transform weaponPlaceHolder;
    [SerializeField] private Transform playerSprite;
    [SerializeField] private WeaponSO defaultWeapon;

    public WeaponBase currentWeapon { get; private set; }
    public event Action<WeaponBase> OnWeaponChanged;
    private Camera mainCamera;
    private bool isFacingLeft = false;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        mainCamera = Camera.main;

        EquipWeapon(defaultWeapon, _pv.ViewID);
       
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

        playerSprite.localScale = new Vector3(isFacingLeft ? -1 : 1, 1, 1);

        float armRotation;
        if (isFacingLeft)
        {
            armRotation = angle + 180;
        }
        else
        {
            armRotation = angle;
        }

        arm.rotation = Quaternion.Euler(0, 0, armRotation);

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

        weaponPlaceHolder.localScale = Vector3.one;
        weaponPlaceHolder.localRotation = Quaternion.identity;

        _pv.RPC("TransmitRotation", RpcTarget.Others, armRotation, isFacingLeft,_pv.ViewID);
        
    }

    private void HandleShooting()
    {
        if (currentWeapon == null) return;

        bool shouldShoot = currentWeapon.weaponData.automatic ?
            Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (shouldShoot && currentWeapon.CanShoot())
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = ((Vector2)(mousePosition - transform.position)).normalized;

            currentWeapon.Shoot(direction);
        }
    }

    [PunRPC]
    public void EquipWeaponRPC(int weaponId, int id)
    {
        WeaponSO weaponData = WeaponDictionary.GetWeapon(weaponId);
        //if (_pv == null) EquipWeaponRPC(weaponId, viewID);
        if (_pv.ViewID == id)
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
                OnWeaponChanged?.Invoke(currentWeapon);
            }
        }
    }

    public void EquipWeapon(WeaponSO weaponData, int id)
    {
        int weaponId = WeaponDictionary.GetWeaponID(weaponData);
        _pv.RPC("EquipWeaponRPC", RpcTarget.AllBuffered, weaponId, id);
    }

    [PunRPC]
    private void TransmitRotation(float armRotationValue, bool direction, int id)
    {
        if (_pv.ViewID == id)
        {
            arm.rotation = Quaternion.Euler(0, 0, armRotationValue);
            playerSprite.localScale = new Vector3(direction ? -1 : 1, 1, 1);
        }
    }
}