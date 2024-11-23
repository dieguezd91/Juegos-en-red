using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ConsumableItem : CollectableItem
{
    private ConsumableInfo consumableInfo;
    private System.Action OnInfoAssigned = delegate { };
    private PhotonView pv;
    private void Awake()
    {
        pv = gameObject.GetComponent<PhotonView>();
    }

    void Start()
    {
        OnCollected += AddToInventory;
    }
    

    private void AddToInventory(PlayerController player)
    {
        if(player.model.itemInventoryFull == false)
        {
            if (player.pv.IsMine)
            {
                player.AddItemToInventory(consumableInfo);
            }
            PhotonNetwork.Destroy(this.gameObject);
        }        
    }

    [PunRPC]
    private void SetInfoRPC(string itemInfo, int pvId)
    {
        if (pv.ViewID == pvId)
        {
            var temp = GameManager.Instance.itemDictionary.GetValueOrDefault(itemInfo);
            if(temp.GetType() == typeof(ConsumableInfo))
            {
                consumableInfo = temp as ConsumableInfo;
                SetSprite();
            }
            
        }
    }
    
    public void SetInfo(string itemInfoId)
    {
        pv.RPC("SetInfoRPC", RpcTarget.All, itemInfoId, pv.ViewID);
    }

    private void SetSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = consumableInfo.Icon;
        //print("sprite set");
    }
}
