using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    private PhotonView pv;
    public SceneController SceneManager { get; private set; }

    [SerializeField] public float roundDuration;
    [SerializeField] private int maxPlayers;

    private List<Transform> spawnPoints = new List<Transform>();
    public List<Transform> SpawnPoints { get { return spawnPoints; } }
    private List<PlayerController> playerList = new List<PlayerController>();
    private Dictionary<PlayerController, Transform> playerSpawns = new Dictionary<PlayerController, Transform>();
    private bool practiceTime = true;
    private bool roundStarted = false;
    private bool inRoom;
    private bool roomInitialized = false;
    public bool PracticeTime { get { return practiceTime; } }

    public event System.Action OnPracticeTimeOver = delegate { };

    private float enterRoomWaitTime = 0.1f;
    private float currentRoomWaitTime;

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
    }

    private void Update()
    {
        if (inRoom)
        {
            if (currentRoomWaitTime < enterRoomWaitTime)
            {
                currentRoomWaitTime += Time.deltaTime;
            }
            else
            {
                roomInitialized = true;
                //print("ClientRoom Initialized");
            }



            if (roomInitialized)
            {
                if (roundStarted == false && playerList.Count == maxPlayers)
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
                    print("Practice time Over");

                    practiceTime = false;
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

    public void CreateRoom()
    {
        var roomConfig = new RoomOptions();
        maxPlayers = UIManager.Instance.MaxPlayers.value + 2;
        roomConfig.MaxPlayers = maxPlayers;
        roomConfig.IsVisible = !UIManager.Instance.IsPrivate;
        PhotonNetwork.CreateRoom(UIManager.Instance.createInput.text, roomConfig);
        pv.RPC("UpdateMaxPlayers", RpcTarget.AllBuffered, maxPlayers);
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
        else
        {
            Debug.LogError("SceneController is not initialized. Cannot change scene.");
        }
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void LoadSpawnPoints(List<Transform> spawnLocations)
    {
        spawnPoints = spawnLocations;
    }

    public void AddPlayer(PlayerController playerToAdd)
    {
        playerList.Add(playerToAdd);
        Transform temp = spawnPoints[Random.Range(0, spawnPoints.Count)];
        playerToAdd.gameObject.transform.position = temp.position;
        playerSpawns.Add(playerToAdd, temp);
        pv.RPC("RemoveSpawnPoint", RpcTarget.AllBuffered, spawnPoints.IndexOf(temp));
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
        Transform spawnPoint = playerSpawns.GetValueOrDefault(playerToRespawn);
        playerToRespawn.transform.position = spawnPoint.position;
        playerToRespawn.gameObject.GetComponent<LifeController>().FullRestoreHealth();
        playerToRespawn.gameObject.SetActive(true);
        print("Player Respawned");
    }

    private void RemovePlayer(PlayerController playerToRemove)
    {

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
}
