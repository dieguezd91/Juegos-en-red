using UnityEngine;
using Photon.Pun;

public class LifeController : MonoBehaviourPunCallbacks
{
    [SerializeField] public float maxHp = 100f;
    [SerializeField] public float currentHp;
    [SerializeField] public float maxShield = 50f;
    [SerializeField] public float currentShield;
    [SerializeField] public float shieldDamageReduction = 0.5f; // 50% damage reduction

    private PhotonView _pv;

    public event System.Action<PlayerController> OnDeath = delegate { };

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        currentHp = maxHp;
        currentShield = maxShield;
    }

    [PunRPC]
    public void ApplyDamage(float damage)
    {
        if (_pv.IsMine)
        {
            float remainingDamage = damage;

            if (currentShield > 0)
            {
                float shieldDamage = Mathf.Min(currentShield, damage * shieldDamageReduction);
                currentShield -= shieldDamage;
                remainingDamage -= shieldDamage / shieldDamageReduction;
            }

            // Aplicar el daño restante a la salud
            if (remainingDamage > 0)
            {
                currentHp -= remainingDamage;
            }

            Debug.Log($"Current HP: {currentHp}, Current Shield: {currentShield}");

            if (currentHp <= 0)
            {
                Die();
            }

            // Sincronizar la salud y el escudo
            _pv.RPC("SyncHealthAndShield", RpcTarget.All, currentHp, currentShield);
        }
    }

    [PunRPC]
    private void SyncHealthAndShield(float health, float shield)
    {
        currentHp = health;
        currentShield = shield;
    }

    private void Die()
    {
        if (_pv.IsMine)
        {
            //PhotonNetwork.Destroy(gameObject);
            OnDeath(this.gameObject.GetComponent<PlayerController>());
            this.gameObject.SetActive(false);            
        }
    }

    public void RestoreShield(float amount)
    {
        if (_pv.IsMine)
        {
            currentShield = Mathf.Min(currentShield + amount, maxShield);
            _pv.RPC("SyncHealthAndShield", RpcTarget.All, currentHp, currentShield);
        }
    }

    public void RestoreHealth (float amount)
    {
        if (_pv.IsMine)
        {
            currentHp += amount;
        }
    }

    public void FullRestoreHealth()
    {
        if (_pv.IsMine)
        {
            currentHp = maxHp;
        }
    }
}