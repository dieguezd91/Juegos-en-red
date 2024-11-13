using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class GameController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    private float waitTime = 0.3f;
    private float currentWaitTime;
    private bool initialized = false;

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
        if (!initialized && currentWaitTime >= waitTime)
        {
            SpawnPlayer();
        }
        else if (!initialized)
        {
            currentWaitTime += Time.deltaTime;
        }
    }

    private void SpawnPlayer()
    {
        if (PhotonNetwork.IsConnected)
        {
            var player = PhotonNetwork.Instantiate(playerPrefab.name,
                new Vector2(Random.Range(-4, 4), Random.Range(-4, 4)),
                Quaternion.identity);

            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (OnPlayerSpawn != null)
                {
                    OnPlayerSpawn(playerController);
                }

                if (virtualCamera != null)
                {
                    virtualCamera.Follow = player.transform;
                }
            }

            initialized = true;
        }
    }
}