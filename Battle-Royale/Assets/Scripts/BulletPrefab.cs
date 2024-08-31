using Photon.Pun;
using UnityEngine;

public class BulletPrefab : MonoBehaviourPunCallbacks
{
    public BulletTypes bulletType;
    float currentSpeed;
    float currentDamage;
    PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        Invoke(nameof(DestroyBullet), bulletType.lifeTime);
        currentSpeed = Random.Range(bulletType.speed * 0.9f, bulletType.speed);
        currentDamage = bulletType.damage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (pv.IsMine)
        {
            if (collision.CompareTag("Player"))
            {
                PhotonView targetPv = collision.GetComponent<PhotonView>();
                if (targetPv != null && !targetPv.IsMine)
                {
                    targetPv.RPC("ApplyDamage", RpcTarget.All, bulletType.damage);
                    DestroyBullet();
                }
            }
        }
    }

    void DestroyBullet()
    {
        if (pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
