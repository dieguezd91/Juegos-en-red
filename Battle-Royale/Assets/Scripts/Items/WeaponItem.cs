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
        if (!player.gameObject.GetComponent<PlayerWeaponController>().WeaponSlotsFull)
        {
            player.gameObject.GetComponent<PlayerWeaponController>().CollectWeapon(weaponInfo);
            PhotonNetwork.Destroy(this.gameObject);
        }

    }

    
    public void SetInfo(WeaponSO info) //Must be executed be whoever spawns this WeaponItem
    {
        weaponInfo = info;
        gameObject.GetComponent<SpriteRenderer>().sprite = weaponInfo.weaponIcon;
    }
}