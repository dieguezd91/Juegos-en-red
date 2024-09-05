using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay; 
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private LayerMask damageableLayers;
    private PhotonView _pv;

    private bool hasExploded = false;
    private float countdown;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        countdown = explosionDelay;
    }

    private void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    private void Explode()
    {
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
