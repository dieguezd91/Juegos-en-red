using UnityEngine;
using Photon.Pun;

public class LifeController : MonoBehaviourPunCallbacks
{
    [SerializeField] float maxHp = 100f;
    [SerializeField] float currentHp;
    PhotonView pv;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        currentHp = maxHp;
    }

    [PunRPC]
    public void ApplyDamage(float damage)
    {
        if (pv.IsMine)
        {
            currentHp -= damage;
            Debug.Log("Current HP: " + currentHp);

            if (currentHp <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

