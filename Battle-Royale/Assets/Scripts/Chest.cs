using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private PhotonView pv;
    private bool _open = false;
    private WeaponSO[] _weaponRewards;
    private ItemBase[] _itemRewards;

    [SerializeField] private float spreadRadius = 3f;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void Interact()
    {
        print("Chest Interacted");
        if (_open == false && pv != null)
        {
            pv.RPC("OpenChest", RpcTarget.AllViaServer);

            if (_weaponRewards != null)
            {
                foreach (var weapon in _weaponRewards)
                {
                    if (weapon != null)
                        SpawnWeapon(weapon);
                }
            }

            if (_itemRewards != null)
            {
                foreach (var item in _itemRewards)
                {
                    if (item != null)
                        item.Spawn(GetRandomPosition(), Quaternion.identity);
                }
            }

            Debug.Log("Chest interacted");
        }
    }

    private Vector2 GetRandomPosition()
    {
        return new Vector2(
            Random.Range(transform.position.x - spreadRadius, transform.position.x + spreadRadius),
            Random.Range(transform.position.y - spreadRadius, transform.position.y + spreadRadius)
        );
    }

    private void SpawnWeapon(WeaponSO weapon)
    {
        var weaponPickup = PhotonNetwork.Instantiate("WeaponPickup", GetRandomPosition(), Quaternion.identity);
        var weaponItem = weaponPickup.GetComponent<WeaponItem>();
        if (weaponItem != null)
        {
            weaponItem.SetInfo(weapon);
            Debug.Log($"Spawned weapon: {weapon.weaponName}");
        }
    }

    [PunRPC]
    private void OpenChest()
    {
        _open = true;
        var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;
        Debug.Log("Chest opened");
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
    }
}