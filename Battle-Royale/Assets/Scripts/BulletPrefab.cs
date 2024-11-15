using Photon.Pun;
using UnityEngine;

public class BulletPrefab : MonoBehaviourPunCallbacks, IPunObservable
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
    private bool isShotgunPellet = false;
    private bool isDestroyed = false;
    private PhotonView shooterPV;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(BulletTypes type, Vector2 direction, PhotonView shooter, bool isPellet = false)
    {
        bulletType = type;
        _initialPosition = transform.position;
        _pierceCount = 0;
        isShotgunPellet = isPellet;
        shooterPV = shooter;

        if (isShotgunPellet)
        {
            _currentSpeed = Random.Range(bulletType.speed * 0.7f, bulletType.speed * 1.1f);
            _currentDamage = bulletType.damage / 8f;
        }
        else
        {
            _currentSpeed = Random.Range(bulletType.speed * 0.8f, bulletType.speed);
            _currentDamage = bulletType.damage;
        }

        if (_rb != null)
        {
            _rb.velocity = direction.normalized * _currentSpeed;
        }

        float lifeTime = isShotgunPellet ? bulletType.lifeTime * 0.7f : bulletType.lifeTime;
        if (lifeTime > 0)
        {
            if (_pv.IsMine)
            {
                Invoke(nameof(NetworkDestroyBullet), lifeTime);
            }
        }
    }

    private void Update()
    {
        if (_pv.IsMine && bulletType != null)
        {
            float distance = Vector3.Distance(_initialPosition, transform.position);
            float damageDropoff = isShotgunPellet ? bulletType.damageDropoff * 1.5f : bulletType.damageDropoff;
            _currentDamage = bulletType.damage * (1f - (distance * damageDropoff));
            _currentDamage = Mathf.Max(_currentDamage, bulletType.damage * 0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_pv.IsMine || bulletType == null || isDestroyed) return;

        if (collision.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        else if (collision.gameObject.layer == 11)
        {
            NetworkDestroyBullet();
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
                lifeController.photonView.RPC("ApplyDamage", RpcTarget.All, _currentDamage, shooterPV);

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
                NetworkDestroyBullet();
            }
        }
    }

    private void NetworkDestroyBullet()
    {
        if (!_pv.IsMine || isDestroyed) return;

        isDestroyed = true;
        _pv.RPC("DestroyBulletRPC", RpcTarget.All);
    }

    [PunRPC]
    private void DestroyBulletRPC()
    {
        if (_pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject, 0.1f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentDamage);
            stream.SendNext(isDestroyed);
            stream.SendNext(_pierceCount);
        }
        else
        {
            _currentDamage = (float)stream.ReceiveNext();
            isDestroyed = (bool)stream.ReceiveNext();
            _pierceCount = (int)stream.ReceiveNext();
        }
    }
}