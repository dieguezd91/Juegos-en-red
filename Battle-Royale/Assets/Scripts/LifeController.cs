using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LifeController : MonoBehaviourPunCallbacks
{
    public PlayerSO PlayerData;

    public float HealingAmount;
    public float HealingDuration;
    public bool _isHealing = false;
    [SerializeField] public float currentHp;
    [SerializeField] public float maxShield;
    [SerializeField] public float currentShield;
    [SerializeField] public float shieldDamageReduction = 0.5f;

    private PhotonView _pv;
    private PhotonView lastDamageDealer;
    public int kills = 0;

    public System.Action<PlayerController> OnDeath = delegate { };

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        currentHp = PlayerData.MaxHP;
        currentShield = 0f;
        GameManager.Instance.OnPlayerRespawn += FullRestore;

        if (_pv.IsMine)
        {
            UpdateUI();
        }
    }

    //TESTING ONLY************
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ApplyDamage(1000);
        }
    }
    //TESTING ONLY************

    [PunRPC]
    public void ApplyDamage(float damage, int damageDealer = -1)
    {
        if (_pv.IsMine)
        {
            lastDamageDealer = PhotonView.Find(damageDealer);

            float remainingDamage = damage;
            if (currentShield > 0)
            {
                float shieldDamage = Mathf.Min(currentShield, damage * shieldDamageReduction);
                currentShield -= shieldDamage;
                remainingDamage -= shieldDamage / shieldDamageReduction;
            }

            if (remainingDamage > 0)
            {
                currentHp -= remainingDamage;
            }

            if (currentHp <= 0)
            {
                Die();
            }

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
            if (lastDamageDealer != null && lastDamageDealer != _pv)
            {
                lastDamageDealer.RPC("AddKill", RpcTarget.All);
            }

            OnDeath(gameObject.GetComponent<PlayerController>());
            print("On death");
        }
    }

    [PunRPC]
    private void AddKill()
    {
        kills++;
        if (_pv.IsMine)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStats(
                GameManager.Instance.GetPlayersAlive(),
                kills
            );
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

    private IEnumerator HealingCoroutine()
    {
        yield return new WaitForSeconds(HealingDuration);
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
    }

    private void FullRestore()
    {
        currentHp = PlayerData.MaxHP;
        currentShield = maxShield;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerRespawn -= FullRestore;
        }
    }
}