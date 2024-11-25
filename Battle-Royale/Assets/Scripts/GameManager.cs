using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        {
            return instance;
        }
        
    }

    private PhotonView pv;

    [SerializeField] private GameItemsList itemList;
    public Dictionary<string,ItemBase> itemDictionary { get; private set; }
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
    private bool winner = false;
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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            pv = GetComponent<PhotonView>();
        }
        //else if (instance != this)
        //{
        //    //Destroy(gameObject);
        //    return;
        //}

        //objectInstance = Resources.Load<GameObject>("GameManager");
    }

    private void Start()
    {
        SceneManager = FindObjectOfType<SceneController>();

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        itemDictionary = new Dictionary<string, ItemBase>();

        foreach (ItemBase item in itemList.GameItems)
        {
            itemDictionary.Add(item.ID, item);
        }
    }

    private void Update()
    {
        if (!inRoom) return;

        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("UpdateMaxPlayers", RpcTarget.AllBuffered, maxPlayers);

            int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

            if (!roundStarted &&
                !isCountingDown &&
                practiceTime &&
                currentPlayers == maxPlayers)
            {
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


    public override void OnLeftRoom()
    {
        RemoveNicknameFromRoom(PhotonNetwork.NickName);
        base.OnLeftRoom();
        CleanupManager();

        //if (instance == this)
        //{
        //    instance = null;
        //    //Destroy(gameObject);
        //}
    }

    private void OnDestroy()
    {
        if (gameController != null)
        {
            gameController.OnPlayerSpawn -= AddPlayer;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private void UpdateRoundTime()
    {
        if (!roundStarted || practiceTime) return;

        double elapsedTime = PhotonNetwork.Time - roundStartTimeStamp;
        currentRoundTime = Mathf.Max(0, roundDuration - (float)elapsedTime);

        if (currentRoundTime <= 0 && PhotonNetwork.IsMasterClient)
        {
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

            // Verificar jugadores existentes
            PlayerController[] existingPlayers = FindObjectsOfType<PlayerController>();
            foreach (var player in existingPlayers)
            {
                if (player.IsInitialized() && !playerList.Contains(player))
                {
                    AddPlayer(player);
                }
            }
        }
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            UIManager.Instance.ShowError("Please set a nickname first");
            UIManager.Instance.ShowNicknamePanel();
            return;
        }

        PhotonNetwork.JoinRoom(UIManager.Instance.joinInput.text);
        inRoom = true;
    }

    public void CreateRoom(int _maxPlayers, bool isPrivate)
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            UIManager.Instance.ShowError("Please set a nickname first");
            UIManager.Instance.ShowNicknamePanel();
            return;
        }

        var roomConfig = new RoomOptions
        {
            MaxPlayers = _maxPlayers,
            IsVisible = !isPrivate,
            PlayerTtl = 600000,
            EmptyRoomTtl = 0,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "UsedNicknames", new string[0] }
            }
            
        };
        maxPlayers = _maxPlayers;
        PhotonNetwork.CreateRoom(UIManager.Instance.createInput.text, roomConfig);
        inRoom = true;

        //var map = PhotonNetwork.Instantiate("MapPrefab", Vector3.zero, Quaternion.identity, 0);
        //PhotonView mapView = map.GetPhotonView();// GetComponent<PhotonView>();
        //mapView.RPC("NotifyMapReady", RpcTarget.All);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (IsNicknameInUse(PhotonNetwork.NickName))
        {
            PhotonNetwork.LeaveRoom();
            UIManager.Instance.ShowError("Nickname already in use. Please choose another one");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNicknamePanel();
            }
            return;
        }
        

        // Agregar el nickname a la lista de la sala
        AddNicknameToRoom(PhotonNetwork.NickName);

        inRoom = true;
        ClearLists();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStats(PhotonNetwork.CurrentRoom.PlayerCount, 0);
        }

        PhotonNetwork.LoadLevel("Gameplay");
        if (PhotonNetwork.IsMasterClient)
        {
            // Instanciar el mapa si no existe
            if (GameObject.Find("MapPrefab(Clone)") == null)
            {
                var map = PhotonNetwork.Instantiate("MapPrefab", Vector3.zero, Quaternion.identity, 0);
                PhotonView mapView = map.GetPhotonView();//.GetComponent<PhotonView>();
                //mapView.RPC("NotifyMapReady", RpcTarget.All);
                print(map.name);
                print(mapView.ViewID);
            }
        }
    }
    [PunRPC]
    private void NotifyMapReady()
    {
        Debug.Log("Map is ready and synchronized.");
        // Realiza aquí cualquier lógica adicional para notificar a los jugadores.
    }

    private bool IsNicknameInUse(string nickname)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            string[] usedNicknames = (string[])PhotonNetwork.CurrentRoom.CustomProperties["UsedNicknames"];
            if (usedNicknames != null)
            {
                return usedNicknames.Contains(nickname);
            }
        }
        return false;
    }

    private void AddNicknameToRoom(string nickname)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            var properties = PhotonNetwork.CurrentRoom.CustomProperties;
            string[] currentNicknames = (string[])properties.GetValueOrDefault("UsedNicknames", new string[0]);

            string[] newNicknames = new string[currentNicknames.Length + 1];
            Array.Copy(currentNicknames, newNicknames, currentNicknames.Length);
            newNicknames[currentNicknames.Length] = nickname;

            properties["UsedNicknames"] = newNicknames;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
    }

    private void RemoveNicknameFromRoom(string nickname)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            var properties = PhotonNetwork.CurrentRoom.CustomProperties;
            string[] currentNicknames = (string[])properties.GetValueOrDefault("UsedNicknames", new string[0]);

            string[] newNicknames = currentNicknames.Where(n => n != nickname).ToArray();

            properties["UsedNicknames"] = newNicknames;
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }
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

    private bool HasPlayer(int playerId)
    {
        return playerList.Any(p => p != null && p.pv != null && p.pv.ViewID == playerId);
    }

    public void AddPlayer(PlayerController playerToAdd)
    {
        if (playerToAdd == null || playerToAdd.pv == null)
            return;

        int playerId = playerToAdd.pv.ViewID;
        bool playerExists = playerList.Any(p => p != null && p.pv != null && p.pv.ViewID == playerId);

        if (!playerExists)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Transform spawnPoint = GetAvailableSpawnPoint();
                if (spawnPoint != null)
                {
                    int playerActorNumber = playerToAdd.pv.Owner.ActorNumber;
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

            playerList.Add(playerToAdd);
            pv.RPC("SyncPlayerCount", RpcTarget.All, GetPlayersAlive());
            UpdateAllPlayersUI();
        }
    }

    [PunRPC]
    private void SyncAddPlayer(int playerId)
    {
        if (!playerList.Any(p => p != null && p.pv != null && p.pv.ViewID == playerId))
        {
            PhotonView[] views = FindObjectsOfType<PhotonView>();
            foreach (PhotonView view in views)
            {
                if (view.ViewID == playerId)
                {
                    PlayerController player = view.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        playerList.Add(player);
                        break;
                    }
                }
            }
            UpdateAllPlayersUI();
        }
    }

    [PunRPC]
    private void SyncPlayerCount(int count)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStats(count, 0);
        }
    }

    private Transform GetAvailableSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
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
            //RemovePlayer(playerToHandle);
            UIManager.Instance.ShowGameOverScreen(playerToHandle, winner);
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
        if (playerToRemove == null || playerToRemove.pv == null) return;

        if (playerToRemove.pv.IsMine)
        {
            playerList.Remove(playerToRemove);
            PhotonNetwork.LeaveRoom();
            ResetBools();
            ClearLists();
            ChangeScene("MainMenu");
        }
        else
        {
            playerList.Remove(playerToRemove);
            UpdateAllPlayersUI();
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
            AssignNewMasterClient();
        }
        
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

        UpdateAllPlayersUI();
    }
    public void AssignNewMasterClient()
    {
        Player bestCandidate = null;

        // Ejemplo: Seleccionar el jugador con el ID más bajo como MasterClient
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (bestCandidate == null || player.ActorNumber < bestCandidate.ActorNumber)
            {
                bestCandidate = player;
            }
        }

        if (bestCandidate != null)
        {
            PhotonNetwork.SetMasterClient(bestCandidate);
            Debug.Log($"Se asignó manualmente a {bestCandidate.NickName} como nuevo MasterClient.");
        }
    }

    
    [PunRPC]
    private void EndMatch()
    {
        ResetBools();
        ClearLists();

        if (Instance == this)
        {
            CleanupManager();
        }

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStats(PhotonNetwork.CurrentRoom.PlayerCount, 0);
        }
    }

    private void StartActualMatch()
    {
        if (roundStarted) return;
        if (PhotonNetwork.IsMasterClient)
        {
            // Instanciar el mapa solo si aún no existe
            if (GameObject.Find("MapPrefab(Clone)") == null)
            {
                PhotonNetwork.Instantiate("MapPrefab", Vector3.zero, Quaternion.identity, 0);
            }
        }
        foreach (PlayerController player in playerList)
        {
            if (player != null)
            {
                RespawnPlayer(player);
            }
        }

        practiceTime = false;
        roundStarted = true;

        UpdateAllPlayersUI();

        if (PhotonNetwork.IsMasterClient)
        {
            //PhotonNetwork.Instantiate("MapPrefab", Vector3.zero, Quaternion.identity);
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
    }

    public int GetPlayersAlive()
    {
        if (practiceTime)
        {
            return PhotonNetwork.CurrentRoom.PlayerCount;
        }
        return playerList.Count;
    }

    private void UpdateAllPlayersUI()
    {
        if (!PhotonNetwork.InRoom) return;

        int currentPlayers = GetPlayersAlive();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStats(currentPlayers, 0);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void CleanupManager()
    {
        ClearLists();
        //if (instance == this)
        //{
        //    instance = null;
        //}
        //Destroy(gameObject);
        //ClearDelegates();
    }
    
    private void ClearDelegates()
    {
     OnPracticeTimeOver = delegate { };
     OnPlayerRespawn = delegate { };
    }
}