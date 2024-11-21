using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PhotonView pv;
    private bool _open = false;
    private WeaponSO[] _weaponRewards = new WeaponSO[0];
    private ItemBase[] _itemRewards = new ItemBase[1];

    [SerializeField] private float spreadRadius = 3f;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void Interact()
    {
        //print("Chest Interacted");
        if (_open == false && pv != null)
        {
            pv.RPC("OpenChest", RpcTarget.AllViaServer);
        }
    }

    private Vector2 GetRandomPosition()
    {
        var location = new Vector2(
            Random.Range(transform.position.x - spreadRadius, transform.position.x + spreadRadius),
            Random.Range(transform.position.y - spreadRadius, transform.position.y + spreadRadius)
        );
        if(Physics2D.OverlapPoint(location,11))
        {
            return GetRandomPosition();
        }

        return location;
    }

    private void SpawnWeapon(WeaponSO weapon)
    {
        var weaponPickup = PhotonNetwork.Instantiate("WeaponPickup", GetRandomPosition(), Quaternion.identity);
        var weaponItem = weaponPickup.GetComponent<WeaponItem>();
        if (weaponItem != null)
        {
            weaponItem.SetInfo(weapon);
            //Debug.Log($"Spawned weapon: {weapon.weaponName}");
        }
    }

    private void SpawnRewards()
    {
        foreach (WeaponSO weapon in _weaponRewards)
        {
            SpawnWeapon(weapon);
        }

        foreach (ItemBase item in _itemRewards)
        {
            item.Spawn(GetRandomPosition(), Quaternion.identity);
        }
    }

    [PunRPC]
    private void OpenChest()
    {
        _open = true;
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
        SpawnRewards();
        //Debug.Log("Chest opened");
    }

    public void FillChestWithWeapons(List<WeaponSO> weapons)
    {
        if (weapons != null)
            _weaponRewards = weapons.ToArray();
    }

    public void FillChestWithItems(List<ItemBase> items)
    {
        if (items != null)
            _itemRewards = items.ToArray();
        for(int i = 0; i < _itemRewards.Length; i++)
        {
            pv.RPC("TransmitItem", RpcTarget.Others, _itemRewards[i].ID, i, pv.ViewID);
        }
    }

    [PunRPC]
    private void TransmitItem(string itemId, int index, int chestId)
    {
        if(pv.ViewID == chestId)
        {
          _itemRewards[index] = GameManager.Instance.itemDictionary.GetValueOrDefault(itemId);
        }
    }
}