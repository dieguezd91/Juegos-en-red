using UnityEngine;
using Photon.Pun;

public class ShotgunWeapon : WeaponBase
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private int pelletCount = 8; // Cantidad de perdigones por disparo
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

        // Disparar m�ltiples perdigones
        for (int i = 0; i < pelletCount; i++)
        {
            // Aplicar dispersi�n de perdig�n
            float spread = Random.Range(-weaponData.bulletSpread, weaponData.bulletSpread);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spread;

            // Crear la rotaci�n para el perdig�n
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            // Calcular una posici�n ligeramente aleatoria alrededor del punto de disparo
            Vector2 randomOffset = Random.insideUnitCircle * 0.1f;
            Vector3 spawnPosition = shootPoint.position + (Vector3)randomOffset;

            // Crear la direcci�n del disparo con el spread aplicado
            Vector2 spreadDirection = rotation * Vector2.right;

            // Instanciar el perdig�n
            GameObject bullet = PhotonNetwork.Instantiate(
                weaponData.bulletPrefab.name,
                spawnPosition,
                rotation
            );

            // Configurar el perdig�n
            var bulletController = bullet.GetComponent<BulletPrefab>();
            if (bulletController != null)
            {
                // Los perdigones pierden da�o m�s r�pido con la distancia
                bulletController.Initialize(weaponData.bulletType, spreadDirection, true);
            }
        }

        // Reproducir efectos de sonido
        if (audioSource != null && weaponData.shootSound != null)
        {
            audioSource.PlayOneShot(weaponData.shootSound);
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