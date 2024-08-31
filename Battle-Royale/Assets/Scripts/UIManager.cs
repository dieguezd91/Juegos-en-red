using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviourPunCallbacks
{
    public static UIManager Instance;

    private PlayerController _playerController;

    [SerializeField] private GameObject mainMenuCanvas;    
    [SerializeField] private GameObject hudCanvas;    

    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private int maxPlayers;
    [SerializeField] private TMP_InputField createInput;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private GameObject createInputGo;
    [SerializeField] private GameObject joinInputGo;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button exitBtn;

    [Header ("HUD")]
    [SerializeField]
    private GameObject timer;
    [SerializeField] private Image lifeBar;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        PlayerController.OnPlayerControllerInstantiated += OnPlayerControllerInstantiated;

        playBtn.onClick.AddListener(Play);
        createButton.onClick.AddListener(CreateRoom);
        joinButton.onClick.AddListener(JoinRoom);
        exitBtn.onClick.AddListener(GameManager.Instance.Quit);
    }

    private void Update()
    {
        if (GameManager.Instance.SceneManager.SceneIndex == "MainMenu")
        {
            mainMenuCanvas.SetActive(true);
        }

        if (GameManager.Instance.SceneManager.SceneIndex == "Gameplay")
        {
            mainMenuCanvas.SetActive(false);
            hudCanvas.SetActive(true);
            var remainingTime = GameManager.Instance.roundDuration;
            timer.gameObject.GetComponent<TextMeshProUGUI>().text = FormatTime(remainingTime);
        }

        if (_playerController != null && _playerController.pv.IsMine)
        {
            UpdateLifeBar();
        }
    }

    private void OnDestroy()
    {
        createButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        playBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
        PlayerController.OnPlayerControllerInstantiated -= OnPlayerControllerInstantiated;
    }

    private void OnPlayerControllerInstantiated(PlayerController player)
    {
        _playerController = player;
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

    private void UpdateLifeBar()
    {
        if (_playerController.pv.IsMine)
        {
            var hpPercentage = _playerController.GetComponent<LifeController>().currentHp / _playerController.GetComponent<LifeController>().maxHp;
            lifeBar.fillAmount = hpPercentage;
            lifeBar.color = GetHealthColor(hpPercentage);
        }
    }

    private Color GetHealthColor(float healthPercentage)
    {
        return Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    private void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    private void CreateRoom()
    {
        var roomConfig = new RoomOptions();
        roomConfig.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(createInput.text, roomConfig);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }

    private string FormatTime(float time)
    {
        var minutes = Mathf.FloorToInt((time % 3600) / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
