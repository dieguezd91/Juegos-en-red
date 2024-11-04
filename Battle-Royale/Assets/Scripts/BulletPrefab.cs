using Photon.Pun;
using UnityEngine;

public class BulletPrefab : MonoBehaviourPunCallbacks
{
    [SerializeField] private BulletTypes bulletType;
    public BulletTypes BulletType
    {
        get => bulletType;
        set => bulletType = value;
    }

    private float _currentSpeed;
    private float _currentDamage;
    private PhotonView _pv;
    private Vector3 _initialPosition;
    private int _pierceCount;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BulletTypes type, Vector2 direction)
    {
        bulletType = type;
        _initialPosition = transform.position;
        _pierceCount = 0;
        _currentSpeed = Random.Range(bulletType.speed * 0.8f, bulletType.speed);
        _currentDamage = bulletType.damage;

        // Establecer la velocidad de la bala usando la dirección y velocidad
        if (_rb != null)
        {
            // Importante: Usamos la dirección directamente sin modificaciones
            _rb.velocity = direction.normalized * _currentSpeed;
        }

        if (bulletType.lifeTime > 0)
        {
            Invoke(nameof(DestroyBullet), bulletType.lifeTime);
        }
    }

    private void Update()
    {
        if (_pv.IsMine && bulletType != null)
        {
            float distance = Vector3.Distance(_initialPosition, transform.position);
            _currentDamage = bulletType.damage * (1f - (distance * bulletType.damageDropoff));
            _currentDamage = Mathf.Max(_currentDamage, bulletType.damage * 0.2f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_pv.IsMine || bulletType == null) return;

        if (collision.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        else if (collision.CompareTag("Environment"))
        {
            DestroyBullet();
        }
    }

    private void HandlePlayerCollision(Collider2D collision)
    {
        var targetPv = collision.GetComponent<PhotonView>();
        if (targetPv != null && !targetPv.IsMine)
        {
            var lifeController = collision.GetComponent<LifeController>();
            if (lifeController != null)
            {
                lifeController.photonView.RPC("ApplyDamage", RpcTarget.All, _currentDamage);

                if (bulletType.hitEffect != null)
                {
                    PhotonNetwork.Instantiate(bulletType.hitEffect.name,
                        collision.ClosestPoint(transform.position),
                        Quaternion.identity);
                }
            }

            if (bulletType.canPierce && _pierceCount < bulletType.maxPierceCount)
            {
                _pierceCount++;
                _currentDamage *= 0.7f;
            }
            else
            {
                DestroyBullet();
            }
        }
    }

    private void DestroyBullet()
    {
        if (_pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
