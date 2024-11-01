using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PhotonView pv;
    private bool _open = false;
    //[SerializeField] private GameObject[] availableRewards;
    private ItemBase[] _rewards;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }   

    public void Interact()
    {
        if (_open == false)
        {
            pv.RPC("OpenChest", RpcTarget.AllViaServer);
            foreach(var item in _rewards)
            {
                item.Spawn(new Vector2(Random.Range(transform.position.x - 3, transform.position.x + 3), Random.Range(transform.position.y - 3, transform.position.y + 3)), Quaternion.identity);
            }            
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

    public void FillChest(List<ItemBase> items)
    {
        _rewards = items.ToArray();
    }
}
