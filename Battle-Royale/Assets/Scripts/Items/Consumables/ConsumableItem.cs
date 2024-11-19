using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : CollectableItem
{
    private ConsumableInfo consumableInfo;
    void Start()
    {
        OnCollected += AddToInventory;
        OnCollected += Test;
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
    }
}
