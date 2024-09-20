using UnityEngine;
using Photon.Pun;

public class PlayerWeaponController : MonoBehaviourPunCallbacks
{
    private PhotonView _pv;
    [SerializeField]private Transform shootingPoint;
    [SerializeField] private GameObject weaponObject;
    private GameObject bulletPrefab;
    private float fireRate = 0.5f; // Cadencia de disparo
    private float damage;


    private float _nextFireTime = 0f; // Tiempo para el proximo disparo

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            RotatePlayer();
            if (Input.GetButton("Fire1") && Time.time >= _nextFireTime)
            {
                Shoot();
                _nextFireTime = Time.time + fireRate; // Actualiza el tiempo para el siguiente disparo
            }
        }
    }

    private void RotatePlayer()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void Shoot()
    {
        var bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootingPoint.position, shootingPoint.rotation);
        var rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = shootingPoint.up * bullet.GetComponent<BulletPrefab>().bulletType.speed;
    }

    public void UpdateWeaponInfo(WeaponInfo newWeapon)
    {
        weaponObject.GetComponent<SpriteRenderer>().sprite = newWeapon.weaponPrefab.GetComponent<SpriteRenderer>().sprite;
        bulletPrefab = newWeapon.bulletPrefab;
        damage = newWeapon.damage;
        fireRate = newWeapon.fireRate;
    }
}
