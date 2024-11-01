using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "ItemInfo/WeaponInfo")]
public class WeaponInfo : ItemBase
{
    [SerializeField] private float _fireRate;
    public float fireRate { get { return _fireRate; }}
    [SerializeField] private float _damage;
    public float damage { get { return _damage; }}
    [SerializeField] private GameObject _weaponPrefab;
    public GameObject weaponPrefab { get { return _weaponPrefab; } }
    [SerializeField] private GameObject _bulletPrefab; 
    public GameObject bulletPrefab { get { return _bulletPrefab; }}


    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        var temp = Photon.Pun.PhotonNetwork.Instantiate("WeaponCollectable", position, rotation);
        temp.GetComponent<WeaponItem>().SetInfo(this);

    }
}