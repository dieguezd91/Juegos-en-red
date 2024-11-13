using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShrinkingZone : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float roundDuration;
    [SerializeField] private float shrinkDuration;
    [SerializeField] private float minRadius = 3f;
    [SerializeField] private float initialRadius = 20f;
    [SerializeField] private float damagePerSecondOutside = 10f;
    [SerializeField] private SpriteRenderer zoneRenderer;

    private float _currentRadius;
    private CircleCollider2D _zoneCollider;
    private HashSet<PlayerController> playersOutsideZone = new HashSet<PlayerController>();
    private Vector3 initialScale;
    private PhotonView pv;
    private double startTimeStamp;
    private int currentPhase = 0;
    private bool isActive = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        this.gameObject.SetActive(false);
        GameManager.Instance.OnPracticeTimeOver += OnMatchStart;
    }

    private void Start()
    {
        zoneRenderer = GetComponent<SpriteRenderer>();
        _zoneCollider = GetComponent<CircleCollider2D>();
        roundDuration = GameManager.Instance.roundDuration;
        _currentRadius = initialRadius;
        initialScale = zoneRenderer.transform.localScale;
    }

    private void OnMatchStart()
    {
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
    }

    private void Update()
    {
        if (!isActive) return;

        double elapsedTime = PhotonNetwork.Time - startTimeStamp;
        float phase1Time = roundDuration * 0.25f;
        float phase2Time = roundDuration * 0.50f;
        float phase3Time = roundDuration * 0.75f;

        // Determinar en que fase estamos basandonos en el tiempo transcurrido
        if (elapsedTime < phase1Time)
        {
            // Primera fase - zona al tamaño inicial
            _currentRadius = initialRadius;
        }
        else if (elapsedTime < phase2Time)
        {
            // Segunda fase - reduccion al 66%
            if (currentPhase < 1)
            {
                currentPhase = 1;
                if (PhotonNetwork.IsMasterClient)
                {
                    pv.RPC("SyncPhaseStart", RpcTarget.All, 1, PhotonNetwork.Time);
                }
            }
            float phaseProgress = (float)((elapsedTime - phase1Time) / shrinkDuration);
            _currentRadius = Mathf.Lerp(initialRadius, initialRadius * 0.66f, Mathf.Clamp01(phaseProgress));
        }
        else if (elapsedTime < phase3Time)
        {
            // Tercera fase - reduccion al 50%
            if (currentPhase < 2)
            {
                currentPhase = 2;
                if (PhotonNetwork.IsMasterClient)
                {
                    pv.RPC("SyncPhaseStart", RpcTarget.All, 2, PhotonNetwork.Time);
                }
            }
            float phaseProgress = (float)((elapsedTime - phase2Time) / shrinkDuration);
            _currentRadius = Mathf.Lerp(initialRadius * 0.66f, initialRadius * 0.5f, Mathf.Clamp01(phaseProgress));
        }
        else
        {
            // Fase final - reduccion al tamaño minimo
            if (currentPhase < 3)
            {
                currentPhase = 3;
                if (PhotonNetwork.IsMasterClient)
                {
                    pv.RPC("SyncPhaseStart", RpcTarget.All, 3, PhotonNetwork.Time);
                }
            }
            float phaseProgress = (float)((elapsedTime - phase3Time) / shrinkDuration);
            _currentRadius = Mathf.Lerp(initialRadius * 0.5f, minRadius, Mathf.Clamp01(phaseProgress));
        }

        UpdateZoneVisual();
    }

    [PunRPC]
    private void SyncPhaseStart(int phase, double phaseStartTime)
    {
        currentPhase = phase;
        Debug.Log($"Zone Phase {phase} started at network time: {phaseStartTime}");
    }

    private void UpdateZoneVisual()
    {
        float scaleFactor = _currentRadius / initialRadius;
        zoneRenderer.transform.localScale = initialScale * scaleFactor;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isActive) return;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && playersOutsideZone.Contains(player))
            {
                playersOutsideZone.Remove(player);
            }
        }
    }

    private IEnumerator DealDamageOverTime(PlayerController player)
    {
        while (playersOutsideZone.Contains(player))
        {
            player.GetComponent<LifeController>().ApplyDamage(damagePerSecondOutside);
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