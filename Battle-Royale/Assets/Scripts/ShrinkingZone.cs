using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShrinkingZone : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float roundDuration;
    [SerializeField] private float shrinkDuration = 30f;
    [SerializeField] private float minRadius = 3f;
    [SerializeField] private float initialRadius = 20f;
    [SerializeField] private float damagePerSecondOutside = 10f;

    private float _currentRadius;
    private CircleCollider2D _zoneCollider;
    private SpriteRenderer zoneRenderer;
    private HashSet<PlayerController> playersOutsideZone = new HashSet<PlayerController>();
    private Vector3 initialScale;
    private PhotonView pv;
    private double startTimeStamp;
    private int currentPhase = 0;
    private bool isActive = false;
    private bool isInitialized = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        _zoneCollider = GetComponent<CircleCollider2D>();
        zoneRenderer = GetComponent<SpriteRenderer>();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPracticeTimeOver += OnMatchStart;
            roundDuration = GameManager.Instance.roundDuration;
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }

        _currentRadius = initialRadius;
        initialScale = transform.localScale;

        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!isInitialized)
        {
            InitializeZone();
        }
    }

    private void InitializeZone()
    {
        if (GameManager.Instance != null)
        {
            roundDuration = GameManager.Instance.roundDuration;
            isInitialized = true;
        }
    }

    private void OnMatchStart()
    {
        if (!isInitialized)
        {
            InitializeZone();
        }

        this.gameObject.SetActive(true);
        isActive = true;

        if (PhotonNetwork.IsMasterClient)
        {
            startTimeStamp = PhotonNetwork.Time;
            pv.RPC("SyncZoneStart", RpcTarget.All, startTimeStamp);
        }
    }

    [PunRPC]
    private void SyncZoneStart(double startTime)
    {
        startTimeStamp = startTime;
        currentPhase = 0;
        isActive = true;
        this.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive || !isInitialized) return;

        double elapsedTime = PhotonNetwork.Time - startTimeStamp;
        float phase1Time = roundDuration * 0.25f;
        float phase2Time = roundDuration * 0.50f;
        float phase3Time = roundDuration * 0.75f;

        // Determinar la fase actual
        if (elapsedTime < phase1Time)
        {
            UpdatePhase(0, initialRadius, initialRadius);
        }
        else if (elapsedTime < phase2Time)
        {
            float progress = (float)((elapsedTime - phase1Time) / shrinkDuration);
            UpdatePhase(1, initialRadius, initialRadius * 0.66f, progress);
        }
        else if (elapsedTime < phase3Time)
        {
            float progress = (float)((elapsedTime - phase2Time) / shrinkDuration);
            UpdatePhase(2, initialRadius * 0.66f, initialRadius * 0.5f, progress);
        }
        else
        {
            float progress = (float)((elapsedTime - phase3Time) / shrinkDuration);
            UpdatePhase(3, initialRadius * 0.5f, minRadius, progress);
        }

        UpdateZoneVisual();
    }

    private void UpdatePhase(int phase, float startRadius, float targetRadius, float progress = 0f)
    {
        if (currentPhase != phase && PhotonNetwork.IsMasterClient)
        {
            pv.RPC("SyncPhaseStart", RpcTarget.All, phase, PhotonNetwork.Time);
        }

        _currentRadius = Mathf.Lerp(startRadius, targetRadius, Mathf.Clamp01(progress));
    }

    [PunRPC]
    private void SyncPhaseStart(int phase, double phaseStartTime)
    {
        currentPhase = phase;
    }

    private void UpdateZoneVisual()
    {
        if (zoneRenderer != null)
        {
            float scaleFactor = _currentRadius / initialRadius;
            transform.localScale = initialScale * scaleFactor;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null && !playersOutsideZone.Contains(player))
        {
            playersOutsideZone.Add(player);
            StartCoroutine(DealDamageOverTime(player));
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playersOutsideZone.Remove(player);
        }
    }

    private IEnumerator DealDamageOverTime(PlayerController player)
    {
        while (playersOutsideZone.Contains(player) && player != null)
        {
            LifeController lifeController = player.GetComponent<LifeController>();
            if (lifeController != null)
            {
                lifeController.ApplyDamage(damagePerSecondOutside);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentRadius);
            stream.SendNext(currentPhase);
            stream.SendNext(isActive);
            stream.SendNext(startTimeStamp);
        }
        else
        {
            _currentRadius = (float)stream.ReceiveNext();
            currentPhase = (int)stream.ReceiveNext();
            isActive = (bool)stream.ReceiveNext();
            startTimeStamp = (double)stream.ReceiveNext();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPracticeTimeOver -= OnMatchStart;
        }
    }
}