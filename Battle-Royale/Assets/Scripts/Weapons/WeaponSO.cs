using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon")]
public class WeaponSO : ItemBase
{
    [Header("Basic Settings")]
    public string weaponName;
    public GameObject weaponPrefab;
    public GameObject bulletPrefab;
    public BulletTypes bulletType;
    public WeaponType weaponType;
    public Sprite weaponIcon;

    [Header("Stats")]
    public float fireRate = 0.5f;
    public float reloadTime = 1.5f;
    public int magazineSize = 12;
    public float bulletSpread = 3f;
    public bool automatic = false;

    [Header("Effects")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public GameObject muzzleFlashPrefab;

    public void SpawnWeaponInWorld(Vector3 position, Quaternion rotation)
    {
        var weaponPickup = PhotonNetwork.Instantiate("WeaponPickup", position, rotation);
        var weaponItem = weaponPickup.GetComponent<WeaponItem>();
        if (weaponItem != null)
        {
            weaponItem.SetInfo(this);
        }
    }   

    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        throw new System.NotImplementedException();
    }
}
