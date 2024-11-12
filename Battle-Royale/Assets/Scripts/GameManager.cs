using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private PhotonView pv;
    public SceneController SceneManager { get; private set; }

    [SerializeField] public float roundDuration;
    [SerializeField] private int maxPlayers = 2;

    private List<Transform> spawnPoints = new List<Transform>();
    public List<Transform> SpawnPoints { get { return spawnPoints; } }
    private List<PlayerController> playerList = new List<PlayerController>();
    private Dictionary<PlayerController, Transform> playerSpawns = new Dictionary<PlayerController, Transform>();
    private Dictionary<int, Transform> assignedSpawnPoints = new Dictionary<int, Transform>();
    private HashSet<Transform> availableSpawnPoints = new HashSet<Transform>();
    private bool practiceTime = true;
    private bool roundStarted = false;
    private bool inRoom;
    //private bool roomInitialized = false;
    public bool PracticeTime { get { return practiceTime; } }

    public event System.Action OnPracticeTimeOver = delegate { };
    public event System.Action OnPlayerRespawn = delegate { };

    private void Awake()
    {        
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        pv = gameObject.GetComponent<PhotonView>();
    }

    private void Start()
    {
        SceneManager = FindObjectOfType<SceneController>();

        if (SceneManager == null)
        {
            Debug.LogError("SceneController not found in the scene. Please ensure it is present.");
        }
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        OnPracticeTimeOver += Test;
    }

    private void Test()
    {
        print("OnRoundStarted");
    }

    private void Update()
    {
        if (inRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (inRoom)
                {
                    pv.RPC("UpdateMaxPlayers", RpcTarget.AllBuffered, maxPlayers);
                }

                if (roundStarted == false && inRoom && playerList.Count == maxPlayers)
                {
                    OnPracticeTimeOver();
                    pv.RPC("StartMatch", RpcTarget.All);
                    roundStarted = true;
                }
            }

        }

    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(UIManager.Instance.joinInput.text);
        inRoom = true;
    }

    public void CreateRoom(int _maxPlayers, bool isPrivate)
    {
        var roomConfig = new RoomOptions();
        roomConfig.MaxPlayers = _maxPlayers;
        maxPlayers = _maxPlayers;
        roomConfig.IsVisible = isPrivate;
        PhotonNetwork.CreateRoom(UIManager.Instance.createInput.text, roomConfig);
        inRoom = true;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }

    public void ChangeScene(string newScene)
    {
        if (SceneManager != null)
        {
            SceneManager.ChangeScene(newScene);
        }
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void LoadSpawnPoints(List<Transform> spawnLocations)
    {
        spawnPoints = new List<Transform>(spawnLocations);
        availableSpawnPoints = new HashSet<Transform>(spawnLocations);

        if (PhotonNetwork.IsMasterClient)
        {
            int[] spawnPointIndex = new int[spawnLocations.Count];
            for (int i = 0; i < spawnLocations.Count; i++)
            {
                spawnPointIndex[i] = i;
            }
            photonView.RPC("SyncSpawnPoints", RpcTarget.All, spawnPointIndex);
        }
    }

    private void AddPlayer(PlayerController playerToAdd)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playerList.Add(playerToAdd);

        Transform spawnPoint = GetAvailableSpawnPoint();
        if (spawnPoint != null)
        {
            int playerActorNumber = playerToAdd.GetComponent<PhotonView>().Owner.ActorNumber;
            int spawnPointIndex = spawnPoints.IndexOf(spawnPoint);

            photonView.RPC("AssignSpawnPoint", RpcTarget.All, playerActorNumber, spawnPointIndex);

            playerToAdd.gameObject.transform.position = spawnPoint.position;
            playerSpawns.Add(playerToAdd, spawnPoint);
            playerToAdd.GetComponent<LifeController>().OnDeath += PlayerDeath;
        }
    }

    private Transform GetAvailableSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0) return null;

        int randomIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform selectedSpawn = availableSpawnPoints.ElementAt(randomIndex);

        return selectedSpawn;
    }

    [PunRPC]
    private void SyncSpawnPoints(int[] indices)
    {
        //spawnPoints = new List<Transform>();
        //availableSpawnPoints = new HashSet<Transform>();

        foreach (int index in indices)
        {
            if (index < spawnPoints.Count)
            {
                Transform spawnPoint = spawnPoints[index];
                availableSpawnPoints.Add(spawnPoint);
            }
        }
    }

    [PunRPC]
    private void AssignSpawnPoint(int playerActorNumber, int spawnPointIndex)
    {
        if (spawnPointIndex >= 0 && spawnPointIndex < spawnPoints.Count)
        {
            Transform spawnPoint = spawnPoints[spawnPointIndex];
            assignedSpawnPoints[playerActorNumber] = spawnPoint;
            availableSpawnPoints.Remove(spawnPoint);
        }
    }

    public void PlayerDeath(PlayerController playerToHandle)
    {
        if (practiceTime)
        {
            RespawnPlayer(playerToHandle);
        }
        else
        {
            RemovePlayer(playerToHandle);
        }
    }

    private void RespawnPlayer(PlayerController playerToRespawn)
    {
        print("Player Respawn start");
        int playerActorNumber = playerToRespawn.GetComponent<PhotonView>().Owner.ActorNumber;

        if (assignedSpawnPoints.TryGetValue(playerActorNumber, out Transform spawnPoint))
        {
            playerToRespawn.transform.position = spawnPoint.position;
            OnPlayerRespawn();
            playerToRespawn.gameObject.SetActive(true);
        }

        print("Player Respawned");
    }

    private void RemovePlayer(PlayerController playerToRemove)
    {

    }

    public void GetGameController(GameController controller)
    {
        controller.OnPlayerSpawn += AddPlayer;        
    }

    [PunRPC]
    private void RemoveSpawnPoint(int target)
    {
        spawnPoints.Remove(spawnPoints[target]);
    }

    [PunRPC]
    private void UpdateMaxPlayers(int value)
    {
        maxPlayers = value;
    }

    [PunRPC]
    private void StartMatch()
    {
        foreach (PlayerController player in playerList)
        {
            RespawnPlayer(player);
        }

        if (SceneManager != null && SceneManager.SceneIndex == "Gameplay")
        {
            roundDuration -= Time.deltaTime;
        }

        OnPracticeTimeOver();
        print("Match started");

        practiceTime = false;
        roundStarted = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (assignedSpawnPoints.TryGetValue(otherPlayer.ActorNumber, out Transform spawnPoint))
        {
            photonView.RPC("ReleaseSpawnPoint", RpcTarget.All, otherPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void ReleaseSpawnPoint(int playerActorNumber)
    {
        if (assignedSpawnPoints.TryGetValue(playerActorNumber, out Transform spawnPoint))
        {
            availableSpawnPoints.Add(spawnPoint);
            assignedSpawnPoints.Remove(playerActorNumber);
        }
    }
}
