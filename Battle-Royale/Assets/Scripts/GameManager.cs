using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager Instance;

    private PhotonView pv;
    public SceneController SceneManager { get; private set; }

    [SerializeField] private int maxPlayers;

    private List<Transform> spawnPoints = new List<Transform>();
    public List<Transform> SpawnPoints { get { return spawnPoints; } }
    private List<PlayerController> playerList = new List<PlayerController>();
    private Dictionary<PlayerController, Transform> playerSpawns = new Dictionary<PlayerController, Transform>();
    private Dictionary<int, Transform> assignedSpawnPoints = new Dictionary<int, Transform>();
    private HashSet<Transform> availableSpawnPoints = new HashSet<Transform>();
    private bool practiceTime = true;
    private bool roundStarted = false;
    private bool inRoom;
    public bool PracticeTime { get { return practiceTime; } }

    [SerializeField] public float roundDuration;
    private float currentRoundTime;
    private bool isRoundTimeRunning = false;
    private double roundStartTimeStamp;

    [SerializeField] private float countdownTime = 10f;
    private bool isCountingDown = false;
    public bool IsCountingDown { get { return isCountingDown; } }
    private float networkStartTime = 0f;
    private double startTimeStamp = 0;

    public delegate void CountdownUpdateHandler(int currentCount);
    public event CountdownUpdateHandler OnCountdownUpdate;

    public delegate void CountdownCompleteHandler();
    public event CountdownCompleteHandler OnCountdownComplete;

    public delegate void GameEvent();
    public event GameEvent OnPracticeTimeOver;
    public event GameEvent OnPlayerRespawn;

    private GameController gameController;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        SceneManager = FindObjectOfType<SceneController>();

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        if (!inRoom) return;

        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("UpdateMaxPlayers", RpcTarget.AllBuffered, maxPlayers);

            int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            Debug.Log($"Current Players: {currentPlayers}, Max Players: {maxPlayers}, Practice Time: {practiceTime}, Is Counting: {isCountingDown}, Round Started: {roundStarted}");

            if (!roundStarted &&
                !isCountingDown &&
                practiceTime &&
                currentPlayers == maxPlayers)
            {
                Debug.Log("Starting countdown");
                double startTime = PhotonNetwork.Time;
                pv.RPC("StartNetworkCountdown", RpcTarget.All, startTime);
            }
        }

        // Actualizar el countdown si esta activo
        if (isCountingDown)
        {
            UpdateNetworkCountdown();
        }

        // Actualizar el tiempo de ronda
        if (isRoundTimeRunning && roundStarted)
        {
            UpdateRoundTime();
        }
    }


    private void UpdateRoundTime()
    {
        if (!roundStarted || practiceTime) return;

        double elapsedTime = PhotonNetwork.Time - roundStartTimeStamp;
        currentRoundTime = Mathf.Max(0, roundDuration - (float)elapsedTime);

        if (currentRoundTime <= 0 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Round time ended");
            pv.RPC("EndMatch", RpcTarget.All);
        }
    }

    public float GetCurrentRoundTime()
    {
        if (!roundStarted || practiceTime)
            return roundDuration;

        return currentRoundTime;
    }

    [PunRPC]
    private void StartNetworkCountdown(double startTime)
    {
        if (isCountingDown || roundStarted)
        {
            return;
        }
        startTimeStamp = startTime;
        isCountingDown = true;
    }

    private void UpdateNetworkCountdown()
    {
        if (!isCountingDown) return;

        // Calcular el tiempo transcurrido desde el inicio de la cuenta regresiva
        double elapsedTime = PhotonNetwork.Time - startTimeStamp;
        int remainingSeconds = Mathf.CeilToInt((float)(countdownTime - elapsedTime));

        if (remainingSeconds > 0)
        {
            // Si el segundo actual es diferente al anterior, actualizar la UI
            int currentSecond = Mathf.CeilToInt((float)(countdownTime - elapsedTime));
            if (currentSecond != networkStartTime)
            {
                networkStartTime = currentSecond;
                OnCountdownUpdate?.Invoke(currentSecond);
            }
        }
        else if (isCountingDown)
        {
            // Finalizar la cuenta regresiva
            isCountingDown = false;
            OnCountdownComplete?.Invoke();
            StartActualMatch();
        }
    }

    [PunRPC]
    private void StartCountdown()
    {
        if (isCountingDown || roundStarted)
        {
            return;
        }
        isCountingDown = true;
        StartCoroutine(CountdownRoutine());
    }


    public void GetGameController(GameController controller)
    {
        gameController = controller;
        if (gameController != null)
        {
            gameController.OnPlayerSpawn += AddPlayer;
            Debug.Log("GameController registered successfully");
        }
        else
        {
            Debug.Log("Failed to register GameController");
        }
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(UIManager.Instance.joinInput.text);
        inRoom = true;
    }

    public void CreateRoom(int _maxPlayers, bool isPrivate)
    {
        var roomConfig = new RoomOptions
        {
            MaxPlayers = _maxPlayers,
            IsVisible = isPrivate
        };
        maxPlayers = _maxPlayers;
        PhotonNetwork.CreateRoom(UIManager.Instance.createInput.text, roomConfig);
        inRoom = true;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
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

    public void AddPlayer(PlayerController playerToAdd)
    {
        if (!playerList.Contains(playerToAdd))
        {
            playerList.Add(playerToAdd);
            Debug.Log($"Player added. Total players: {playerList.Count}");

            if (PhotonNetwork.IsMasterClient)
            {
                Transform spawnPoint = GetAvailableSpawnPoint();
                if (spawnPoint != null)
                {
                    int playerActorNumber = playerToAdd.GetComponent<PhotonView>().Owner.ActorNumber;
                    int spawnPointIndex = spawnPoints.IndexOf(spawnPoint);
                    pv.RPC("AssignSpawnPoint", RpcTarget.All, playerActorNumber, spawnPointIndex);

                    playerToAdd.transform.position = spawnPoint.position;
                    playerSpawns[playerToAdd] = spawnPoint;
                    LifeController lifeController = playerToAdd.GetComponent<LifeController>();
                    if (lifeController != null)
                    {
                        lifeController.OnDeath += HandlePlayerDeath;
                    }
                }
            }
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

    private void HandlePlayerDeath(PlayerController playerToHandle)
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
        int playerActorNumber = playerToRespawn.GetComponent<PhotonView>().Owner.ActorNumber;

        if (assignedSpawnPoints.TryGetValue(playerActorNumber, out Transform spawnPoint))
        {
            playerToRespawn.transform.position = spawnPoint.position;
            if (OnPlayerRespawn != null)
            {
                OnPlayerRespawn();
            }
            playerToRespawn.gameObject.SetActive(true);
        }

    }

    public void RemovePlayer(PlayerController playerToRemove)
    {
        if (playerToRemove.pv.IsMine)
        {
            PhotonNetwork.LeaveRoom();
            ResetBools();
            ClearLists();
            ChangeScene("MainMenu");
        }
    }

    private bool ShouldRemovePlayer(PlayerController player, Player otherPlayer)
    {
        if (player == null) return true;

        PhotonView playerView = player.GetComponent<PhotonView>();
        if (playerView == null) return true;

        return playerView.Owner.ActorNumber == otherPlayer.ActorNumber;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            List<PlayerController> playersToKeep = new List<PlayerController>();

            foreach (PlayerController player in playerList)
            {
                if (!ShouldRemovePlayer(player, otherPlayer))
                {
                    playersToKeep.Add(player);
                }
            }

            playerList = playersToKeep;

            if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayers && !practiceTime)
            {
                pv.RPC("EndMatch", RpcTarget.All);
            }
        }

        if (assignedSpawnPoints.TryGetValue(otherPlayer.ActorNumber, out Transform spawnPoint))
        {
            photonView.RPC("ReleaseSpawnPoint", RpcTarget.All, otherPlayer.ActorNumber);
        }
    }

    [PunRPC]
    private void EndMatch()
    {
        ResetBools();
        ClearLists();
        PhotonNetwork.LoadLevel("MainMenu");
    }

    private void ClearLists()
    {
        playerList.Clear();
        playerSpawns.Clear();
        spawnPoints.Clear();
        availableSpawnPoints.Clear();
        assignedSpawnPoints.Clear();
    }

    private void ResetBools()
    {
        inRoom = false;
        roundStarted = false;
        practiceTime = true;
        isCountingDown = false;
        isRoundTimeRunning = false;
        networkStartTime = 0f;
        startTimeStamp = 0;
        roundStartTimeStamp = 0;
        currentRoundTime = roundDuration;
    }

    public void ChangeScene(string newScene)
    {
        if (SceneManager != null)
        {
            SceneManager.ChangeScene(newScene);
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

    [PunRPC]
    private void UpdateMaxPlayers(int value)
    {
        maxPlayers = value;
    }

    private IEnumerator CountdownRoutine()
    {
        int count = Mathf.CeilToInt(countdownTime);

        while (count > 0)
        {
            OnCountdownUpdate?.Invoke(count);
            yield return new WaitForSeconds(1f);
            count--;
        }

        isCountingDown = false;

        OnCountdownComplete?.Invoke();

        yield return new WaitForSeconds(0.1f);

        StartActualMatch();
    }

    private void StartActualMatch()
    {
        if (roundStarted) return;

        Debug.Log("Starting actual match");
        foreach (PlayerController player in playerList)
        {
            if (player != null)
            {
                RespawnPlayer(player);
            }
        }

        practiceTime = false;
        roundStarted = true;

        // Iniciar el tiempo de ronda
        if (PhotonNetwork.IsMasterClient)
        {
            roundStartTimeStamp = PhotonNetwork.Time;
            pv.RPC("SyncMatchStart", RpcTarget.All);
        }
    }

    [PunRPC]
    private void EnsureCountdownHidden()
    {
        isCountingDown = false;
        OnCountdownComplete?.Invoke();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isCountingDown);
            stream.SendNext(startTimeStamp);
            stream.SendNext(roundStarted);
            stream.SendNext(practiceTime);
            stream.SendNext(roundStartTimeStamp);
            stream.SendNext(currentRoundTime);
            stream.SendNext(isRoundTimeRunning);
        }
        else
        {
            isCountingDown = (bool)stream.ReceiveNext();
            startTimeStamp = (double)stream.ReceiveNext();
            roundStarted = (bool)stream.ReceiveNext();
            practiceTime = (bool)stream.ReceiveNext();
            roundStartTimeStamp = (double)stream.ReceiveNext();
            currentRoundTime = (float)stream.ReceiveNext();
            isRoundTimeRunning = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    private void SyncMatchStart()
    {
        Debug.Log("SyncMatchStart called");
        isCountingDown = false;
        practiceTime = false;
        roundStarted = true;
        roundStartTimeStamp = PhotonNetwork.Time;
        isRoundTimeRunning = true;
        currentRoundTime = roundDuration;

        if (OnCountdownComplete != null)
        {
            OnCountdownComplete();
        }

        if (OnPracticeTimeOver != null)
        {
            OnPracticeTimeOver();
        }

        Debug.Log($"Match synchronized. Time: {currentRoundTime}");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (gameController != null)
        {
            gameController.OnPlayerSpawn -= AddPlayer;
        }
    }
}