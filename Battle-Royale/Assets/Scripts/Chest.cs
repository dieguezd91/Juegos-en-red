using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PhotonView pv;
    private bool _open = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }   

    public void Interact()
    {
        if (_open == false)
        {
            pv.RPC("OpenChest", RpcTarget.AllViaServer);
            print("interacted");
        }        
    }

    [PunRPC]
    private void OpenChest()
    {
        _open = true;
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        print("chest open");
    }
}
