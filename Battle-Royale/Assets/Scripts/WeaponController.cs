using Photon.Pun;
using UnityEngine;

public class WeaponController : MonoBehaviourPunCallbacks
{
    PhotonView pv;
    [SerializeField] Transform shootingPoint;
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float bulletSpeed = 10f;

    void Start()
    {
        pv = GetComponentInParent<PhotonView>();
    }

    void Update()
    {
        if (pv.IsMine)
        {
            RotateWeapon();
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
    }

    void RotateWeapon()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    void Shoot()
    {        
        GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootingPoint.position, shootingPoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = shootingPoint.right * bulletSpeed;
    }
}
