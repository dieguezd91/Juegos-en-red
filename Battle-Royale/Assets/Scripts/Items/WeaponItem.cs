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
        var weaponController = player.gameObject.GetComponent<PlayerWeaponController>();
        bool weaponSlotsfull = weaponController.WeaponSlotsFull;
        if (!weaponSlotsfull)
        {
            weaponController.CollectWeapon(weaponInfo);
            PhotonNetwork.Destroy(this.gameObject);
        }

    }

    
    public void SetInfo(WeaponSO info) //Must be executed be whoever spawns this WeaponItem
    {
        weaponInfo = info;
        gameObject.GetComponent<SpriteRenderer>().sprite = weaponInfo.weaponIcon;
    }
}