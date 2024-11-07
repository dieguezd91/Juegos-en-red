using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadeItem : CollectableItem
{
    private GranadeInfo granadeInfo;
    void Start()
    {
        OnCollected += Test;
    }

    void Test(PlayerController player)
    {
        // Add the granade info to the player granade slot
        Photon.Pun.PhotonNetwork.Destroy(this.gameObject);
        print("Granade collected");
    }

    public void SetInfo(GranadeInfo info)
    {
        granadeInfo = info;
    }
}
