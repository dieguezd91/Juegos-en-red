using UnityEngine;
using Photon.Pun;

public class PlayerWeaponController : MonoBehaviourPunCallbacks
{
    private PhotonView _pv;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private Transform playerSprite;
    private GameObject bulletPrefab;
    private float fireRate = 0.5f;
    private float damage;
    private float _nextFireTime = 0f;
    public Quaternion shootingDirection;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            Aim();
            if (Input.GetButton("Fire1") && Time.time >= _nextFireTime)
            {
                Shoot();
                _nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Aim()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = mousePosition - transform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        weaponObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        if (angle > 90 || angle < -90)
        {
            playerSprite.localScale = new Vector3(-1, 1, 1);
            weaponObject.transform.localScale = new Vector3(-1, -1, 1);
        }
        else
        {
            playerSprite.localScale = new Vector3(1, 1, 1);
            weaponObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null)
        {
            var bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootingPoint.position, weaponObject.transform.rotation);
            var rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 shootDirection = (shootingPoint.position - weaponObject.transform.position).normalized;
                rb.velocity = shootDirection * bullet.GetComponent<BulletPrefab>().bulletType.speed;
            }
        }
    }

    public void UpdateWeaponInfo(WeaponInfo newWeapon)
    {
        if (newWeapon != null && weaponObject != null)
        {
            SpriteRenderer weaponSprite = weaponObject.GetComponent<SpriteRenderer>();
            if (weaponSprite != null && newWeapon.weaponPrefab != null)
            {
                weaponSprite.sprite = newWeapon.weaponPrefab.GetComponent<SpriteRenderer>()?.sprite;
            }
            bulletPrefab = newWeapon.bulletPrefab;
            damage = newWeapon.damage;
            fireRate = newWeapon.fireRate;
        }
    }
}