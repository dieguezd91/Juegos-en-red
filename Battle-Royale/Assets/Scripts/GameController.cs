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
            {
                return;
            }

            Vector2 spawnPosition = new Vector2(Random.Range(-4, 4), Random.Range(-4, 4));
            GameObject playerGO = PhotonNetwork.Instantiate(
                playerPrefab.name,
                spawnPosition,
                Quaternion.identity
            );

            if (playerGO == null)
            {
                return;
            }

            StartCoroutine(SetupPlayerCoroutine(playerGO));
        }
        catch (System.Exception e)
        {
            isSpawning = false;
        }
    }

    private IEnumerator SetupPlayerCoroutine(GameObject playerGO)
    {
        yield return new WaitForEndOfFrame();

        try
        {
            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            PhotonView playerPV = playerGO.GetComponent<PhotonView>();

            if (playerController == null || playerPV == null)
            {
                yield break;
            }

            if (playerPV.IsMine)
            {
                if (virtualCamera != null)
                {
                    virtualCamera.Follow = playerGO.transform;
                }

                if (playerController.IsInitialized())
                {
                    OnPlayerSpawn?.Invoke(playerController);
                }
            }

            initialized = true;
        }
        catch (System.Exception e)
        {
            Debug.Log($"Error en SetupPlayerCoroutine");
        }
        finally
        {
            isSpawning = false;
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        initialized = false;
        currentWaitTime = 0f;
        isSpawning = false;
    }
}