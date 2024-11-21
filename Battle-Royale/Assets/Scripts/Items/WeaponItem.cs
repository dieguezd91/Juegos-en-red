using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponItem : CollectableItem
{
    private WeaponSO weaponInfo;
    private PhotonView photonView;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Start()
    {
        OnCollected += HandleWeaponCollection;
    }

    void HandleWeaponCollection(PlayerController player)
    {
        if (weaponInfo != null)
        {
            var weaponController = player.GetComponent<PlayerWeaponController>();
            if (weaponController != null)
            {
                weaponController.EquipWeapon(weaponInfo, player.gameObject.GetComponent<PlayerWeaponController>().pv.ViewID);

                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }

    public void SetInfo(WeaponSO info)
    {
        weaponInfo = info;        
    }
}