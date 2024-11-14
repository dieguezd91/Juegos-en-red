using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float warningDuration = 1f;
    [SerializeField] private ExplosionRange explosionRange;

    private PhotonView _pv;
    private bool hasExploded = false;
    private float countdown;
    private Rigidbody2D rb;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        countdown = explosionDelay;

        // Configurar el radio en el indicador visual
        if (explosionRange != null)
        {
            explosionRange.SetRadius(explosionRadius);
        }
    }

    private void Update()
    {
        countdown -= Time.deltaTime;

        // Mostrar la advertencia cuando quede poco tiempo
        if (countdown <= warningDuration && !hasExploded && explosionRange != null && !explosionRange.gameObject.activeSelf)
        {
            StartCoroutine(ShowWarningAndExplode());
        }
    }

    private IEnumerator ShowWarningAndExplode()
    {
        // Detener la granada
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Mostrar el rango de explosion
        explosionRange.gameObject.SetActive(true);

        // Esperar la duracion de la advertencia
        yield return new WaitForSeconds(warningDuration);

        // Explotar
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;
        Debug.Log("BOOM!");

        // Detectar los objetos en el radio de la explosion
        Collider2D[] objectsInRadius = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);

        // Aplicar daño a los objetos dentro del radio
        foreach (Collider2D obj in objectsInRadius)
        {
            LifeController lifeController = obj.GetComponent<LifeController>();
            if (lifeController != null)
            {
                lifeController.ApplyDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}