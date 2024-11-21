using Cinemachine;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float waitTime = 0.3f;
    private float currentWaitTime;
    private bool initialized = false;
    private bool isSpawning = false;

    public delegate void PlayerSpawnHandler(PlayerController player);
    public event PlayerSpawnHandler OnPlayerSpawn;

    private void Awake()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GetGameController(this);
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerControllerInstantiated += HandlePlayerInstantiated;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerControllerInstantiated -= HandlePlayerInstantiated;
    }

    private void HandlePlayerInstantiated(PlayerController player)
    {
        OnPlayerSpawn?.Invoke(player);
    }

    private void Update()
    {
        if (!initialized && !isSpawning)
        {
            if (currentWaitTime >= waitTime)
            {
                SpawnPlayer();
            }
            else
            {
                currentWaitTime += Time.deltaTime;
            }
        }
    }

    private void SpawnPlayer()
    {
        if (!PhotonNetwork.IsConnected || isSpawning) return;
        try
        {
            isSpawning = true;
            if (playerPrefab == null)
                return;

            Vector2 spawnPosition = new Vector2(Random.Range(-4, 4), Random.Range(-4, 4));
            GameObject playerGO = PhotonNetwork.Instantiate(
                playerPrefab.name,
                spawnPosition,
                Quaternion.identity
            );

            if (playerGO != null)
                StartCoroutine(SetupPlayerCoroutine(playerGO));
        }
        catch
        {
            isSpawning = false;
        }
    }

    private IEnumerator SetupPlayerCoroutine(GameObject playerGO)
    {
        if (playerGO == null)
        {
            isSpawning = false;
            yield break;
        }

        yield return new WaitForEndOfFrame();

        PlayerController playerController = playerGO.GetComponent<PlayerController>();
        PhotonView playerPV = playerGO.GetComponent<PhotonView>();

        if (playerController == null || playerPV == null)
        {
            isSpawning = false;
            yield break;
        }

        // Configurar la cámara solo para el jugador local
        if (playerPV.IsMine && virtualCamera != null)
        {
            virtualCamera.Follow = playerGO.transform;
        }

        isSpawning = false;
        initialized = true;
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        initialized = false;
        currentWaitTime = 0f;
        isSpawning = false;
    }
}