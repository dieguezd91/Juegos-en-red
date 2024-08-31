using UnityEngine;
using Photon.Pun;

public class BulletPrefab : MonoBehaviourPunCallbacks
{
    [SerializeField] float damage = 10f;
    [SerializeField] float lifetime = 5f;
    PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        Invoke(nameof(DestroyBullet), lifetime);
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
                    targetPv.RPC("ApplyDamage", RpcTarget.All, damage);
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

