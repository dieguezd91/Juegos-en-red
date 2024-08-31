using Photon.Pun;
using UnityEngine;

public class BulletPrefab : MonoBehaviourPunCallbacks
{
    public BulletTypes bulletType;
    private float _currentSpeed;
    private float _currentDamage;
    private PhotonView _pv;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        Invoke(nameof(DestroyBullet), bulletType.lifeTime);
        _currentSpeed = Random.Range(bulletType.speed * 0.9f, bulletType.speed);
        _currentDamage = bulletType.damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_pv.IsMine)
        {
            if (collision.CompareTag("Player"))
            {
                var targetPv = collision.GetComponent<PhotonView>();
                if (targetPv != null && !targetPv.IsMine)
                {
                    targetPv.RPC("ApplyDamage", RpcTarget.All, bulletType.damage);
                    DestroyBullet();
                }
            }
        }
    }

    private void DestroyBullet()
    {
        if (_pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
