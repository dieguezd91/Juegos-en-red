using UnityEngine;
using Photon.Pun;

public class LifeController : MonoBehaviourPunCallbacks
{
    public PlayerSO PlayerData;
    
    public float HealingAmount;
    public float HealingDuration;
    public bool _isHealing = false;

    [SerializeField] public float currentHp;
    [SerializeField] public float maxShield;
    [SerializeField] public float currentShield;
    [SerializeField] public float shieldDamageReduction = 0.5f; // 50% damage reduction

    private PhotonView _pv;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        currentHp = PlayerData.MaxHP;
        currentShield = 0f;
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
            PhotonNetwork.Destroy(gameObject);
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

    public bool CanHeal() => !_isHealing && currentHp < PlayerData.MaxHP;

    public void StartHealing()
    {
        _isHealing = true;
        StartCoroutine(HealingCoroutine());
    }

    private System.Collections.IEnumerator HealingCoroutine()
    {
        Debug.Log("Healing started...");
        yield return new WaitForSeconds(HealingDuration);

        // Curación completada
        CompleteHealing(HealingAmount);
    }

    private void CompleteHealing(float healingAmount)
    {
        if (_pv.IsMine)
        {
            currentHp = Mathf.Min(currentHp + healingAmount, PlayerData.MaxHP);
            _pv.RPC("SyncHealthAndShield", RpcTarget.All, currentHp, currentHp);
            _isHealing = false;
        }
        Debug.Log("Healing Complete: " + currentHp + "HP");
    }
}