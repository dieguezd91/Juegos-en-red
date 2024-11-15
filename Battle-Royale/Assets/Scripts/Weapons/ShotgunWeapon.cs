using Photon.Pun;
using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private int pelletCount = 8;
    private AudioSource audioSource;
    private PhotonView weaponPhotonView;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        weaponPhotonView = GetComponentInParent<PhotonView>();

        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }

    public override void Shoot(Vector2 direction)
    {
        if (!CanShoot() || weaponData == null || weaponData.bulletPrefab == null) return;

        for (int i = 0; i < pelletCount; i++)
        {
            float spread = Random.Range(-weaponData.bulletSpread, weaponData.bulletSpread);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spread;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 randomOffset = Random.insideUnitCircle * 0.1f;
            Vector3 spawnPosition = shootPoint.position + (Vector3)randomOffset;
            Vector2 spreadDirection = rotation * Vector2.right;

            GameObject bullet = PhotonNetwork.Instantiate(
                weaponData.bulletPrefab.name,
                spawnPosition,
                rotation
            );

            var bulletController = bullet.GetComponent<BulletPrefab>();
            if (bulletController != null)
            {
                bulletController.Initialize(weaponData.bulletType, spreadDirection, weaponPhotonView, true);
            }
        }

        if (audioSource != null && weaponData.shootSound != null)
        {
            audioSource.PlayOneShot(weaponData.shootSound);
        }

        currentAmmo--;
        nextFireTime = Time.time + weaponData.fireRate;
    }

    public override void Reload()
    {
        if (!isReloading && currentAmmo < weaponData.magazineSize)
        {
            base.Reload();
            if (audioSource != null && weaponData.reloadSound != null)
            {
                audioSource.PlayOneShot(weaponData.reloadSound);
            }
        }
    }
}