using UnityEngine;
using Photon.Pun;

public class GrenadeThrower : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private GameObject flashGrenadePrefab;
    [SerializeField] private float throwForce = 10f; // Fuerza con la que se lanza la granada
    [SerializeField] private float maxThrowDistance = 15f; // Distancia maxima a la que se puede lanzar la granada
    [SerializeField] private int maxGrenades = 3; // maximo de granadas que puede llevar el jugador

    public int currentGrenades;

    private PhotonView _pv;
    private Camera _camera;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_pv.IsMine && currentGrenades > 0)
        {
            if (Input.GetMouseButtonDown(1)) // Click derecho para granada normal
            {
                ThrowGrenade(grenadePrefab);
            }
            else if (Input.GetKeyDown(KeyCode.G)) // G para granada flash
            {
                ThrowGrenade(flashGrenadePrefab);
            }
        }
    }

    private void ThrowGrenade(GameObject grenadePrefab)
    {
        // Disminuir la cantidad de granadas disponibles
        currentGrenades--;

        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 throwDirection = (mousePosition - transform.position).normalized;

        // Limita la distancia de lanzamiento
        float distanceToMouse = Vector2.Distance(transform.position, mousePosition);
        Vector2 targetPosition = transform.position + (Vector3)throwDirection * Mathf.Min(distanceToMouse, maxThrowDistance);

        GameObject grenade = PhotonNetwork.Instantiate(grenadePrefab.name, transform.position, Quaternion.identity);

        // Aplica una fuerza hacia el objetivo, esta limitada por la distancia maxima
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 force = throwDirection * throwForce;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
