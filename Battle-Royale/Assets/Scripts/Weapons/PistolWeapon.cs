using UnityEngine;
using Photon.Pun;

public class PistolWeapon : WeaponBase
{
    [SerializeField] private Transform shootPoint;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
    }

    public override void Shoot(Vector2 direction)
    {
        if (!CanShoot() || weaponData == null || weaponData.bulletPrefab == null) return;

        // Aplicar dispersión de bala (spread)
        float spread = Random.Range(-weaponData.bulletSpread, weaponData.bulletSpread);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spread;

        // Crear la rotación para la bala
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Crear la dirección del disparo con el spread aplicado
        Vector2 spreadDirection = rotation * Vector2.right;

        // Instanciar la bala
        GameObject bullet = PhotonNetwork.Instantiate(
            weaponData.bulletPrefab.name,
            shootPoint.position,
            rotation
        );

        // Configurar la bala
        var bulletController = bullet.GetComponent<BulletPrefab>();
        if (bulletController != null)
        {
            bulletController.Initialize(weaponData.bulletType, spreadDirection);
        }

        // Actualizar estado del arma
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