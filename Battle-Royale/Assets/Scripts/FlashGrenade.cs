using Photon.Pun;
using System.Collections;
using UnityEngine;

public class FlashGrenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private float flashDuration = 2f;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private SpriteRenderer grenadeRenderer;
    FlashEffect flashEffect;

    private PhotonView _pv;
    private bool hasExploded = false;
    private float countdown;
    private Rigidbody2D rb;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        grenadeRenderer = GetComponent<SpriteRenderer>();
        flashEffect = GetComponent<FlashEffect>();
        countdown = explosionDelay;
    }

    private void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= warningDuration && !hasExploded)
        {
            StartCoroutine(ShowWarningAndExplode());
        }
    }

    private IEnumerator ShowWarningAndExplode()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        yield return new WaitForSeconds(warningDuration);

        if (_pv.IsMine)
        {
            _pv.RPC("RPC_InitiateExplosion", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_InitiateExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;
        StartCoroutine(ExplosionSequence());
    }

    private IEnumerator ExplosionSequence()
    {
        if (grenadeRenderer != null)
        {
            grenadeRenderer.enabled = false;
        }

        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }
        if (rb != null)
        {
            rb.simulated = false;
        }

        Collider2D[] playersInRadius = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D playerCollider in playersInRadius)
        {
            PhotonView playerPV = playerCollider.GetComponent<PhotonView>();
            if (playerPV != null)
            {
                _pv.RPC("RPC_ApplyFlashEffect", RpcTarget.All, playerPV.ViewID, flashDuration);
            }
        }

        yield return new WaitForSeconds(flashDuration + (1f / flashEffect.fadeOutSpeed));

        if (_pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void RPC_ApplyFlashEffect(int playerViewID, float duration)
    {
        PhotonView targetPlayer = PhotonView.Find(playerViewID);
        if (targetPlayer != null && targetPlayer.IsMine)
        {
            flashEffect.ShowFlash(duration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}