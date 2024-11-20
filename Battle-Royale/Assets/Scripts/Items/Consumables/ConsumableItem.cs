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
        OnCollected += Test;
        OnInfoAssigned += SetSprite;
    }

    void Test(PlayerController player)
    {        
        Photon.Pun.PhotonNetwork.Destroy(this.gameObject);
        print("Consumable collected");
    }

    

    private void AddToInventory(PlayerController player)
    {
        player.AddItemToInventory(consumableInfo);
    }

    public void SetInfo(ConsumableInfo info)
    {
        consumableInfo = info;
        OnInfoAssigned();
    }

    private void SetSprite()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = consumableInfo.Icon;
    }
}
