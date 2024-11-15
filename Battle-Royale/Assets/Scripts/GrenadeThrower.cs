using Photon.Pun;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private GameObject flashGrenadePrefab;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowDistance = 15f;
    [SerializeField] private int maxLethalGrenades = 3;
    [SerializeField] private int maxTacticalGrenades = 2;

    public int currentLethalGrenades;
    public int currentTacticalGrenades;

    [SerializeField] private Sprite grenadeSprite;
    [SerializeField] private Sprite flashGrenadeSprite;

    private PhotonView _pv;
    private Camera _camera;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();
        _camera = Camera.main;

        // Inicializar cantidades de granadas
        currentLethalGrenades = maxLethalGrenades;
        currentTacticalGrenades = maxTacticalGrenades;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetGrenadeIcons(grenadeSprite, flashGrenadeSprite);
            UpdateGrenadesUI();
        }

        // Actualizar UI inicial
        UpdateGrenadesUI();
    }

    private void Update()
    {
        if (!_pv.IsMine) return;

        if (Input.GetMouseButtonDown(1) && currentLethalGrenades > 0)
        {
            ThrowGrenade(grenadePrefab, true);
        }
        else if (Input.GetKeyDown(KeyCode.G) && currentTacticalGrenades > 0)
        {
            ThrowGrenade(flashGrenadePrefab, false);
        }
    }

    private void ThrowGrenade(GameObject grenadePrefab, bool isLethal)
    {
        // Reducir la cantidad del tipo correspondiente
        if (isLethal)
        {
            currentLethalGrenades--;
        }
        else
        {
            currentTacticalGrenades--;
        }

        // Actualizar UI
        UpdateGrenadesUI();

        Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2 throwDirection = (mousePosition - transform.position).normalized;
        float distanceToMouse = Vector2.Distance(transform.position, mousePosition);
        Vector2 targetPosition = transform.position + (Vector3)throwDirection * Mathf.Min(distanceToMouse, maxThrowDistance);

        object[] instantiationData = new object[] { _pv.ViewID };
        GameObject grenade = PhotonNetwork.Instantiate(grenadePrefab.name, transform.position, Quaternion.identity, 0, instantiationData);

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        Vector2 force = throwDirection * throwForce;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void UpdateGrenadesUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGrenadeAmounts(currentLethalGrenades, currentTacticalGrenades);
        }
    }

    //Solo test
    public void RefillGrenades()
    {
        currentLethalGrenades = maxLethalGrenades;
        currentTacticalGrenades = maxTacticalGrenades;
        UpdateGrenadesUI();
    }
}