using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : CollectableItem
{
    private ConsumableInfo consumableInfo;
    private System.Action OnInfoAssigned = delegate { };
    void Start()
    {
        OnCollected += AddToInventory;
    }
    

    private void AddToInventory(PlayerController player)
    {
        if(player.model.itemInventoryFull == false)
        {
            player.AddItemToInventory(consumableInfo);
            Photon.Pun.PhotonNetwork.Destroy(this.gameObject);
            print("Consumable collected");
        }        
    }

    public void SetInfo(ConsumableInfo info)
    {
        consumableInfo = info;
        SetSprite();
    }

    private void SetSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = consumableInfo.Icon;
        //print("sprite set");
    }
}
