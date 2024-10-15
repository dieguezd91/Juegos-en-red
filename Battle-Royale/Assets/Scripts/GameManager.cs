using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public SceneController SceneManager { get; private set; }

    [SerializeField] public float roundDuration;
    [SerializeField] private int maxPlayers;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
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
    }

    private void Update()
    {
        if (SceneManager != null && SceneManager.SceneIndex == "Gameplay")
        {
            roundDuration -= Time.deltaTime;
        }
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(UIManager.Instance.joinInput.text);
    }

    public void CreateRoom()
    {
        var roomConfig = new RoomOptions();
        roomConfig.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(UIManager.Instance.createInput.text, roomConfig);
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
}
