using UnityEngine;
using Photon.Pun;

public class PlayerAimController : MonoBehaviourPunCallbacks
{
    private PhotonView _pv;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private GameObject bulletPrefab;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            RotatePlayer();
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
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

    void Shoot()
    {
        var bullet = PhotonNetwork.Instantiate(bulletPrefab.name, shootingPoint.position, shootingPoint.rotation);
        var rb = bullet.GetComponent<Rigidbody2D>();
        rb.velocity = shootingPoint.up * bullet.GetComponent<BulletPrefab>().bulletType.speed;
    }
}
