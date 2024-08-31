using UnityEngine;
using Photon.Pun;

public class LifeController : MonoBehaviourPunCallbacks
{
    [SerializeField] public float maxHp = 100f;
    [SerializeField] public float currentHp;
    private PhotonView _pv;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        currentHp = maxHp;
    }

    [PunRPC]
    public void ApplyDamage(float damage)
    {
        if (_pv.IsMine)
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
        if (_pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}

