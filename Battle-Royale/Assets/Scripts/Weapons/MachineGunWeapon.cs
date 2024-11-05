using UnityEngine;
using Photon.Pun;

public class MachineGunWeapon : WeaponBase
{
    [SerializeField] private Transform shootPoint;
    private AudioSource audioSource;

    private float currentSpread;
    private float maxSpread = 20f;
    private float spreadIncreasePerShot = 1f;
    private float spreadRecoveryRate = 5f;
    private float lastShotTime;
    private float spreadRecoveryDelay = 0.1f;

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
        if (Time.time - lastShotTime > spreadRecoveryDelay)
        {
            currentSpread = Mathf.Max(0, currentSpread - spreadRecoveryRate * Time.deltaTime);
        }

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

        float totalSpread = weaponData.bulletSpread + currentSpread;
        float spread = Random.Range(-totalSpread, totalSpread);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + spread;

        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        Vector2 spreadDirection = rotation * Vector2.right;

        try
        {
            recoilDirection = -spreadDirection;

            GameObject bullet = PhotonNetwork.Instantiate(
                weaponData.bulletPrefab.name,
                shootPoint.position,
                rotation,
                0
            );

            if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = spreadDirection * weaponData.bulletType.speed;
            }

            ApplyRecoil();

            currentSpread = Mathf.Min(maxSpread, currentSpread + spreadIncreasePerShot);
            lastShotTime = Time.time;

            if (audioSource != null && weaponData.shootSound != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(weaponData.shootSound);
            }

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
            currentSpread = 0f;
        }
    }
}