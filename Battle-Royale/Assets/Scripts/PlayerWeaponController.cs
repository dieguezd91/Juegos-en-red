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

    private WeaponSO[] weaponSlots = new WeaponSO[2];
    private bool weaponSlotsFull = false;
    public bool WeaponSlotsFull { get { return weaponSlotsFull; } }

    public WeaponBase currentWeapon { get; private set; }
    public event Action<WeaponBase> OnWeaponChanged;
    public Action OnWeaponFired = delegate { };
    private Camera mainCamera;
    private bool isFacingLeft = false;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }
    private void Start()
    {
        
        mainCamera = Camera.main;

        weaponSlots[0] = defaultWeapon;
        EquipWeapon(_pv.ViewID, 0);        
       
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
            OnWeaponFired();
        }
    }

    [PunRPC]
    public void EquipWeaponRPC(string weaponId, int id)
    {
        var temp = GameManager.Instance.itemDictionary.TryGetValue(weaponId, out ItemBase item);
        WeaponSO weaponData = item as WeaponSO;
        //if (_pv == null) EquipWeaponRPC(weaponId, viewID);
        //print("view ID: " + _pv.ViewID);
        if (_pv.ViewID == id)
        {
            if (weaponData == null || weaponPlaceHolder == null) return;

            if (currentWeapon != null)
            {
                //PhotonNetwork.Destroy(currentWeapon.gameObject);
                Destroy(currentWeapon.gameObject);
            }

            //GameObject weaponInstance = PhotonNetwork.Instantiate(weaponData.weaponPrefab.name, Vector3.zero, Quaternion.identity);
            GameObject weaponInstance = Instantiate(weaponData.weaponPrefab, weaponPlaceHolder);
            weaponInstance.transform.SetParent(weaponPlaceHolder);
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

    private void EquipWeapon(int photonId, int weaponSlot = 1)
    {
        string weaponId = weaponSlots[weaponSlot].ID;
        _pv.RPC("EquipWeaponRPC", RpcTarget.AllBuffered, weaponId, photonId);
    }

    public void CollectWeapon(WeaponSO weaponData)
    {
        int chosenSlot = 0;
        for (int i = 1; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = weaponData;
                chosenSlot = i;
                //print("Weapon equiped on slot "+i);
                break;
            }          
        }

        int count = 0;
        for(int i= 0; i < weaponSlots.Length; i++)
        {
            if(weaponSlots[i] == null)
            {
                weaponSlotsFull = false;
                break;
            }
            else
            {
                count++;
            }
        }

        if(count == weaponSlots.Length)
        {
            weaponSlotsFull = true;
            print("weapon slot full");
        }

        if (currentWeapon.weaponData == defaultWeapon)
        {
            SwitchWeapon(chosenSlot);
            SwitchWeapon(chosenSlot);
            print("autoSwitch");
        }
    }

    public void DropWeapon(int weaponSlot = 1)
    {
        SwitchWeapon(0);
        weaponSlots[weaponSlot].SpawnWeaponInWorld(new Vector3(transform.position.x + 3, transform.position.y, transform.position.z), Quaternion.identity);
        weaponSlots[weaponSlot] = null;
        weaponSlotsFull = false;
    }

    public void SwitchWeapon(int weaponSlot)
    {
        EquipWeapon(_pv.ViewID, weaponSlot);
    }

    public WeaponSO CheckWeaponSlot(int slot)
    {
        if (slot < weaponSlots.Length)
        {
            return weaponSlots[slot];
        }

        print("Error. Tried to access weapon slot " + slot);
        return null;
        
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