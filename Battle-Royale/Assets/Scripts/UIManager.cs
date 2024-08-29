using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private int maxPlayers;
    [SerializeField] private TMPro.TMP_InputField createInput;
    [SerializeField] private TMPro.TMP_InputField joinInput;
    [SerializeField] private GameObject createInputGO;
    [SerializeField] private GameObject joinInputGO;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        createButton.onClick.AddListener(CreateRoom);
        joinButton.onClick.AddListener(JoinRoom);
    }

    void Start()
    {

    }

    void Update()
    {

    }

    private void OnDestroy()
    {
        createButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
    }

    public void Play()
    {
        if (playPanel != null)
        {
            if(!playPanel.activeInHierarchy) 
                playPanel.SetActive(true);
        }
    }

    public void ShowHideOptions()
    {
        if (optionsPanel != null)
        {
            if (!optionsPanel.activeInHierarchy)
            {
                optionsPanel.SetActive(true);
            }
        }
    }

    public void ShowHideCredits()
    {
        if (creditsPanel != null)
        {
            if (!creditsPanel.activeInHierarchy)
            {
                creditsPanel.SetActive(true);
            }
        }
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
        //if (joinInputGO != null)
        //{
        //    if (!joinInputGO.activeInHierarchy)
        //    {
        //        joinInputGO.SetActive(true);
        //    }
        //}
    }

    public void CreateRoom()
    {
        RoomOptions roomConfig = new RoomOptions();
        roomConfig.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(createInput.text, roomConfig);
        //if (createInputGO != null)
        //{
        //    if (!createInputGO.activeInHierarchy)
        //    {
        //        createInputGO.SetActive(true);
        //    }
        //}
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }
}
