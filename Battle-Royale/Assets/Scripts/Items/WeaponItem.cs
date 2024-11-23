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

    
    //public void SetInfo(WeaponSO info) //Must be executed be whoever spawns this WeaponItem
    //{
    //    weaponInfo = info;
    //    gameObject.GetComponent<SpriteRenderer>().sprite = weaponInfo.weaponIcon;
    //}

    [PunRPC]
    private void SetInfoRPC(string itemInfo, int pvId)
    {
        if (photonView.ViewID == pvId)
        {
            var temp = GameManager.Instance.itemDictionary.GetValueOrDefault(itemInfo);
            //print(temp.name);
            //print(temp.GetType());
            //if (temp.GetType() == typeof(WeaponSO))
            //{
                //print("WeaponSO confirmed");
                weaponInfo = temp as WeaponSO;
                SetSprite();
            //}

        }
    }

    public void SetInfo(string itemInfoId)
    {
        photonView.RPC("SetInfoRPC", RpcTarget.All, itemInfoId, photonView.ViewID);
    }

    private void SetSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = weaponInfo.weaponIcon;
        //print("sprite set");
    }
}