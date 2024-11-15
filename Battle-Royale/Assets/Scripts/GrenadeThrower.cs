using Photon.Pun;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private GameObject flashGrenadePrefab;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowDistance = 15f;
    [SerializeField] private int maxGrenades = 3;
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
            if (Input.GetMouseButtonDown(1))
            {
                ThrowGrenade(grenadePrefab);
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                ThrowGrenade(flashGrenadePrefab);
            }
        }
    }

    private void ThrowGrenade(GameObject grenadePrefab)
    {
        currentGrenades--;

        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2 throwDirection = (mousePosition - transform.position).normalized;

        float distanceToMouse = Vector2.Distance(transform.position, mousePosition);
        Vector2 targetPosition = transform.position + (Vector3)throwDirection * Mathf.Min(distanceToMouse, maxThrowDistance);

        // Pasar el ID del lanzador al instanciar la granada
        object[] instantiationData = new object[] { _pv.ViewID };
        GameObject grenade = PhotonNetwork.Instantiate(grenadePrefab.name, transform.position, Quaternion.identity, 0, instantiationData);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 force = throwDirection * throwForce;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
