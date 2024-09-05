using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkingZone : MonoBehaviour
{
    [SerializeField] private float roundDuration; // Duracion total de la partida
    [SerializeField] private float shrinkDuration; // Duracion de cada fase de reduccion
    [SerializeField] private float minRadius = 3f; // Radio minimo al que se reducirá la zona
    [SerializeField] private float initialRadius = 20f; // Radio inicial de la zona
    [SerializeField] private float damagePerSecondOutside = 10f; // Daño por segundo fuera de la zona
    [SerializeField] private SpriteRenderer zoneRenderer;

    private float _currentRadius;
    private CircleCollider2D _zoneCollider;
    private HashSet<PlayerController> playersOutsideZone = new HashSet<PlayerController>();
    private Vector3 initialScale;

    private void Start()
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        _zoneCollider = GetComponent<CircleCollider2D>();

        roundDuration = GameManager.Instance.roundDuration;

        _currentRadius = initialRadius;
        initialScale = zoneRenderer.transform.localScale;

        // Inicia el ciclo de reduccion en tres periodos (75% de la partida, 50% y 25%)
        StartCoroutine(ScheduleShrinkZones());
    }

    private IEnumerator ScheduleShrinkZones()
    {
        yield return new WaitForSeconds(roundDuration * 0.25f); // Espera hasta el 75% de la partida
        yield return StartCoroutine(ShrinkZoneToRadius(initialRadius * 0.66f)); // Reduce la zona al 66% del tamaño inicial

        yield return new WaitForSeconds(roundDuration * 0.25f); // Espera hasta el 50% de la partida
        yield return StartCoroutine(ShrinkZoneToRadius(initialRadius * 0.5f)); // Reduce la zona al 50% del tamaño inicial

        yield return new WaitForSeconds(roundDuration * 0.25f); // Espera hasta el 25% de la partida
        yield return StartCoroutine(ShrinkZoneToRadius(minRadius)); // Reduce la zona al tamaño mínimo
    }

    // Función que reduce la zona durante un tiempo determinado (shrinkDuration)
    private IEnumerator ShrinkZoneToRadius(float targetRadius)
    {
        float elapsedTime = 0f;
        float startingRadius = _currentRadius; // El radio inicial de esta fase es el actual

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            _currentRadius = Mathf.Lerp(startingRadius, targetRadius, elapsedTime / shrinkDuration);
            UpdateZoneVisual();
            yield return null;
        }
    }

    private void UpdateZoneVisual()
    {
        float scaleFactor = _currentRadius / initialRadius;
        zoneRenderer.transform.localScale = initialScale * scaleFactor;
    }

    // Daño a los jugadores fuera de la zona
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !playersOutsideZone.Contains(player))
            {
                playersOutsideZone.Add(player);
                StartCoroutine(DealDamageOverTime(player));
            }
        }
    }

    // Deja de aplicar daño cuando el jugador vuelve a la zona
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && playersOutsideZone.Contains(player))
            {
                playersOutsideZone.Remove(player);
                StopCoroutine(DealDamageOverTime(player));
            }
        }
    }

    // Aplica daño con el tiempo a los jugadores fuera de la zona
    private IEnumerator DealDamageOverTime(PlayerController player)
    {
        while (playersOutsideZone.Contains(player))
        {
            player.GetComponent<LifeController>().ApplyDamage(damagePerSecondOutside);
            yield return new WaitForSeconds(1f);
        }
    }
}
