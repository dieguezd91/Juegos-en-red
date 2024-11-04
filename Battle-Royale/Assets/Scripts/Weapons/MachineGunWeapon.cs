using UnityEngine;
using Photon.Pun;

public class MachineGunWeapon : WeaponBase
{
    [SerializeField] private Transform shootPoint;
    private AudioSource audioSource;

    // Variables para el control de dispersión
    private float currentSpread;
    private float maxSpread = 20f;
    private float spreadIncreasePerShot = 1f;
    private float spreadRecoveryRate = 5f;
    private float lastShotTime;
    private float spreadRecoveryDelay = 0.1f;

    // Variables para el efecto de retroceso
    private Vector3 originalLocalPosition;
    private float recoilAmount = 0.1f;
    private float recoilRecoverySpeed = 5f;
    private bool isRecoiling;
    private Vector2 recoilDirection;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (shootPoint == null)
        {
            shootPoint = transform;
        }
        originalLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        // Recuperar la dispersión con el tiempo
        if (Time.time - lastShotTime > spreadRecoveryDelay)
        {
            currentSpread = Mathf.Max(0, currentSpread - spreadRecoveryRate * Time.deltaTime);
        }

        // Recuperar del retroceso
        if (isRecoiling)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPosition,
                recoilRecoverySpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.localPosition, originalLocalPosition) < 0.001f)
            {
                transform.localPosition = originalLocalPosition;
                isRecoiling = false;
            }
        }
    }

    public override void Shoot(Vector2 direction)
    {
        if (!CanShoot() || weaponData == null || weaponData.bulletPrefab == null) return;

        // Aplicar dispersión que aumenta con cada disparo
        float totalSpread = weaponData.bulletSpread + currentSpread;
        float spread = Random.Range(-totalSpread, totalSpread);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spread;

        // Crear la rotación para la bala
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Calcular la dirección del disparo con el spread
        Vector2 spreadDirection = rotation * Vector2.right;

        try
        {
            // Guardar la dirección para el recoil
            recoilDirection = -spreadDirection;

            // Instanciar la bala
            GameObject bullet = PhotonNetwork.Instantiate(
                weaponData.bulletPrefab.name,
                shootPoint.position,
                rotation,
                0
            );

            // Configurar la velocidad de la bala
            if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = spreadDirection * weaponData.bulletType.speed;
            }

            // Aplicar retroceso
            ApplyRecoil();

            // Aumentar la dispersión
            currentSpread = Mathf.Min(maxSpread, currentSpread + spreadIncreasePerShot);
            lastShotTime = Time.time;

            // Reproducir efectos de sonido
            if (audioSource != null && weaponData.shootSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(weaponData.shootSound);
            }

            // Actualizar estado del arma
            currentAmmo--;
            nextFireTime = Time.time + weaponData.fireRate;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al disparar la machine gun: {e.Message}");
        }
    }

    private void ApplyRecoil()
    {
        // Aplicar retroceso en la dirección opuesta al disparo
        Vector3 recoilOffset = new Vector3(recoilDirection.x, recoilDirection.y, 0) * recoilAmount;
        transform.localPosition = originalLocalPosition + recoilOffset;
        isRecoiling = true;
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
            // Resetear la dispersión al recargar
            currentSpread = 0f;
        }
    }
}