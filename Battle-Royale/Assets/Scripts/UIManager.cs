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
    private PlayerWeaponController _weaponController;

    [SerializeField] private GameObject mainMenuCanvas;    
    [SerializeField] private GameObject hudCanvas;    

    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject RoomCreationPanel;
    [SerializeField] private GameObject gameOverScreen;

    [SerializeField] public TMP_InputField createInput;
    [SerializeField] public TMP_InputField joinInput;
    //[SerializeField] private GameObject createInputGo;
    [SerializeField] private GameObject joinInputGo;

    [SerializeField] private Button backToMainMenu;
    [SerializeField] private Button createButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button backToMenuCR;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Toggle isPrivate;
    [SerializeField] private Button returnToMainMenu;
    private bool isPrivateValue;
    [SerializeField] private TMP_Dropdown maxPlayers;
    private int maxPlayersValue;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI defeatText;

    [Header ("HUD")]
    [SerializeField] private GameObject timer;
    [SerializeField] private Image lifeBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image shieldBar;
    [SerializeField] private TextMeshProUGUI playersAliveText;
    [SerializeField] private TextMeshProUGUI playerKillsText;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI currentWeaponText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject weaponReloadIndicator;
    [SerializeField] private Image reloadTimer;
    [SerializeField] private TextMeshProUGUI lethalGrenadeAmount;
    [SerializeField] private TextMeshProUGUI tacticalGrenadeAmount;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image item01Icon;
    [SerializeField] private Image item02Icon;
    [SerializeField] private Image grenadeIcon;
    [SerializeField] private Image tacticalGrenadeIcon;

    [Header("Countdown")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Nickname Panel")]
    [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button confirmNicknameButton;

    [Header("Error Handling")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button errorOkButton;
    [SerializeField] private float errorDisplayTime = 3f;
    private Coroutine errorCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        PlayerController.OnPlayerControllerInstantiated += OnPlayerControllerInstantiated;

        HideCountdown();
    }

    private void Start()
    {
        backToMainMenu.onClick.AddListener(BackToMainMenuScreen);
        playBtn.onClick.AddListener(Play);
        backToMenuCR.onClick.AddListener(ShowHideRoomCreateScreen);
        createRoomButton.onClick.AddListener(ShowHideRoomCreateScreen);
        createButton.onClick.AddListener(RequestRoomCreation);
        joinButton.onClick.AddListener(GameManager.Instance.JoinRoom);
        exitBtn.onClick.AddListener(GameManager.Instance.Quit);
        confirmNicknameButton.onClick.AddListener(ValidateAndSetNickname);
        if (errorOkButton != null)
        {
            errorOkButton.onClick.AddListener(HideError);
        }
        //maxPlayers.onValueChanged.AddListener(SetMaxPlayers);

        //maxPlayersValue = maxPlayers.value;
        //isPrivateValue = isPrivate.isOn;

        item01Icon.enabled = false;
        item02Icon.enabled = false;
        weaponReloadIndicator.SetActive(false);
        gameOverScreen.SetActive(false);

        createButton.interactable = false;
        joinButton.interactable = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCountdownUpdate += UpdateCountdownDisplay;
            GameManager.Instance.OnCountdownComplete += HideCountdown;
        }

        ShowNicknamePanel();
    }

    private void Update()
    {
        if (GameManager.Instance.SceneManager.SceneIndex == "MainMenu")
        {
            mainMenuCanvas.SetActive(true);
            loadingPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }

        if (GameManager.Instance.SceneManager.SceneIndex == "Gameplay")
        {
            mainMenuCanvas.SetActive(false);
            hudCanvas.SetActive(true);

            float remainingTime = GameManager.Instance.GetCurrentRoundTime();
            if (timer != null && timer.gameObject != null)
            {
                timer.gameObject.GetComponent<TextMeshProUGUI>().text = FormatTime(remainingTime);
            }
        }

        if (_playerController != null && _playerController.pv.IsMine)
        {
            UpdateStaminaBar();
        }

        if (GameManager.Instance != null && (!GameManager.Instance.PracticeTime || !GameManager.Instance.IsCountingDown))
        {
            if (countdownPanel != null && countdownPanel.activeSelf)
            {
                HideCountdown();
            }
        }
    }

    private void OnDestroy()
    {
        createButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        playBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
        if (errorOkButton != null)
        {
            errorOkButton.onClick.RemoveListener(HideError);
        }
        PlayerController.OnPlayerControllerInstantiated -= OnPlayerControllerInstantiated;
        if (_weaponController != null)
        {
            _weaponController.OnWeaponChanged -= UpdateWeaponUI;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCountdownUpdate -= UpdateCountdownDisplay;
            GameManager.Instance.OnCountdownComplete -= HideCountdown;
        }
    }

    private int SetMaxPlayers()
    {
        if(maxPlayers.value <= 14)
        {
            return maxPlayers.value + 2;
        }

        return 1;
            
    }

    private void RequestRoomCreation()
    {
        GameManager.Instance.CreateRoom(SetMaxPlayers(), isPrivate.isOn);
    }

    private void OnPlayerControllerInstantiated(PlayerController player)
    {
        if (player.pv.IsMine)
        {
            _playerController = player;
            _weaponController = player.GetComponent<PlayerWeaponController>();

            if (_weaponController != null)
            {
                _weaponController.OnWeaponChanged += UpdateWeaponUI;
            }

            player.gameObject.GetComponent<LifeController>().OnDamageClientSide += UpdateLifeBar;
            player.gameObject.GetComponent<LifeController>().OnDamageClientSide += UpdateShieldBar;
            player.gameObject.GetComponent<PlayerWeaponController>().OnWeaponFired += UpdateAmmoCount;

        }

    }

    public void ShowGameOverScreen(PlayerController player, bool win)
    {
        if (player.pv.IsMine)
        {
            gameOverScreen.SetActive(true);
            returnToMainMenu.onClick.AddListener(Disconect);
            if (win)
            {
                winText.enabled = true;
            }
            else
            {
                defeatText.enabled = true;
            }           
        }
    }

    private void Disconect()
    {
        gameOverScreen.SetActive(false);
        winText.enabled = false;
        defeatText.enabled = false;
        GameManager.Instance.RemovePlayer(_playerController);        
    }

    private void UpdateWeaponUI(WeaponBase weapon)
    {
        if (weapon == null || weapon.weaponData == null) return;

        // Actualizar nombre del arma
        if (currentWeaponText != null)
        {
            currentWeaponText.text = weapon.weaponData.weaponName;
        }

        // Actualizar icono si existe
        if (weaponIcon != null && weapon.weaponData.weaponIcon != null)
        {
            weaponIcon.sprite = weapon.weaponData.weaponIcon;
            weaponIcon.enabled = true;
        }

        UpdateAmmoCount();        
    }

    private void UpdateAmmoCount()
    {
        if (_weaponController == null || _weaponController.currentWeapon == null) return;

        if (ammoText != null)
        {
            var currentAmmo = _weaponController.currentWeapon.currentAmmo;
            var maxAmmo = _weaponController.currentWeapon.weaponData.magazineSize;
            ammoText.text = $"{currentAmmo}/{maxAmmo}";            
        }

        if (_weaponController.currentWeapon.IsReloading)
        {
            print("reloading = " + _weaponController.currentWeapon.IsReloading);
            weaponReloadIndicator.SetActive(true);
            reloadTimer.fillAmount -= Time.deltaTime / _weaponController.currentWeapon.weaponData.reloadTime;
        }
        else if (weaponReloadIndicator.activeSelf && _weaponController.currentWeapon.IsReloading == false)
        {
            weaponReloadIndicator.SetActive(false);
            reloadTimer.fillAmount = 1;
        }
    }

    public void Play()
    {
        if (playPanel != null)
        {
            if(!playPanel.activeInHierarchy) 
                playPanel.SetActive(true);
        }
    }

    public void BackToMainMenuScreen()
    {
        if (playPanel != null)
        {
            playPanel.SetActive(false);
        }
    }
    public void ShowHideRoomCreateScreen()
    {
        if (RoomCreationPanel != null)
        {
            if (!RoomCreationPanel.activeInHierarchy)
                RoomCreationPanel.SetActive(true);
            else if (RoomCreationPanel.activeInHierarchy)
            {
                RoomCreationPanel.SetActive(false);
            }
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
            var hpPercentage = _playerController.GetComponent<LifeController>().currentHp / _playerController.GetComponent<LifeController>().PlayerData.MaxHP;
            lifeBar.fillAmount = hpPercentage;
            lifeBar.color = GetHealthColor(hpPercentage);
        //print("CurrntHp: " + _playerController.GetComponent<LifeController>().currentHp);
        //print("hpPercentage:" +_playerController.GetComponent<LifeController>().currentHp / _playerController.GetComponent<LifeController>().PlayerData.MaxHP);
        //print("fillAmount:" + lifeBar.fillAmount);
    }

    private void UpdateStaminaBar()
    {
        if (_playerController.pv.IsMine)
        {
            var staminaPercentage = _playerController.model.CurrentStamina / _playerController.model.MaxStamina;
            staminaBar.fillAmount = staminaPercentage;
            staminaBar.color = GetStaminaColor(staminaPercentage);
        }
    }

    private void UpdateShieldBar()
    {
            var lifeController = _playerController.GetComponent<LifeController>();
            if (lifeController != null)
            {
                var shieldPercentage = lifeController.currentShield / lifeController.maxShield;
                shieldBar.fillAmount = shieldPercentage;
                shieldBar.color = GetShieldColor(shieldPercentage);
            }
    }

    private Color GetHealthColor(float healthPercentage)
    {
        return Color.Lerp(Color.red, Color.green, healthPercentage);
    }

    private Color GetStaminaColor(float staminaPercentage)
    {
        return Color.Lerp(Color.yellow, Color.blue, staminaPercentage);
    }

    private Color GetShieldColor(float shieldPercentage)
    {
        return Color.Lerp(Color.cyan, Color.blue, shieldPercentage);
    }

    private string FormatTime(float time)
    {
        time = Mathf.Max(0, time);
        var minutes = Mathf.FloorToInt(time / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    private void UpdateCountdownDisplay(int currentCount)
    {
        if (countdownPanel != null && countdownText != null &&
            GameManager.Instance.SceneManager.SceneIndex == "Gameplay" &&
            GameManager.Instance.PracticeTime &&
            GameManager.Instance.IsCountingDown)
        {
            countdownPanel.SetActive(true);
            countdownText.gameObject.SetActive(true);
            countdownText.text = currentCount.ToString();

            if (currentCount <= 3)
            {
                countdownText.fontSize = 72;
                countdownText.color = Color.red;
            }
            else
            {
                countdownText.fontSize = 48;
                countdownText.color = Color.white;
            }
        }
    }

    private void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    public void UpdateGrenadeAmounts(int lethalAmount, int tacticalAmount)
    {
        if (lethalGrenadeAmount != null)
        {
            lethalGrenadeAmount.text = lethalAmount.ToString();
            grenadeIcon.gameObject.SetActive(lethalAmount > 0);
        }

        if (tacticalGrenadeAmount != null)
        {
            tacticalGrenadeAmount.text = tacticalAmount.ToString();
            tacticalGrenadeIcon.gameObject.SetActive(tacticalAmount > 0);
        }
    }

    public void SetGrenadeIcons(Sprite lethalSprite, Sprite tacticalSprite)
    {
        if (grenadeIcon != null && lethalSprite != null)
        {
            grenadeIcon.sprite = lethalSprite;
            grenadeIcon.preserveAspect = true;
            grenadeIcon.enabled = true;
        }

        if (tacticalGrenadeIcon != null && tacticalSprite != null)
        {
            tacticalGrenadeIcon.sprite = tacticalSprite;
            tacticalGrenadeIcon.preserveAspect = true;
            tacticalGrenadeIcon.enabled = true;
        }
    }

    public void UpdatePlayerStats(int playersAlive, int kills)
    {
        if (playersAliveText != null)
        {
            playersAliveText.text = $"Players: {playersAlive}";
        }

        if (playerKillsText != null)
        {
            playerKillsText.text = $"Kills: {kills}";
        }
    }

    public void SetItemIcon(int index, Sprite icon)
    {
        if (index == 0)
        {
            if(icon != null)
            {
                item01Icon.sprite = icon;
                item01Icon.enabled = true;
            }
            else
            {
                item01Icon.enabled = false;
                print("icon deactivated");
            }
            
        }
        else if (index > 0)
        {
            if (icon != null)
            {
                item02Icon.sprite = icon;
                item02Icon.enabled = true;
            }
            else
            {
                item02Icon.enabled = false;
                print("icon deactivated");
            }

        }
    }

    public void ShowNicknamePanel()
    {
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(false);
        RoomCreationPanel.SetActive(false);
        nicknamePanel.SetActive(true);
    }

    private void ValidateAndSetNickname()
    {
        string nickname = nicknameInput.text.Trim();

        if (string.IsNullOrEmpty(nickname) || nickname.Length < 3)
        {
            ShowError("Nickname must be at least 3 characters long");
            return;
        }

        if (!IsValidNickname(nickname))
        {
            ShowError("Invalid nickname. Use only letters, numbers, and underscores");
            return;
        }

        PhotonNetwork.NickName = nickname;
        nicknamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        createButton.interactable = true;
        joinButton.interactable = true;
    }

    private bool IsValidNickname(string nickname)
    {
        // Solo permitir letras, numeros y guiones bajos
        return System.Text.RegularExpressions.Regex.IsMatch(nickname, @"^[a-zA-Z0-9_]+$");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        loadingPanel.SetActive(false);

        if (message.Contains("nickname already in use"))
        {
            ShowError("Nickname already in use. Please choose another one");
            ShowNicknamePanel();
        }
        else
        {
            ShowError($"Failed to join room: {message}");
        }
    }

    public void ShowError(string message)
    {
        if (errorPanel != null && errorText != null)
        {
            errorPanel.SetActive(true);
            errorText.text = message;

            if (errorCoroutine != null)
            {
                StopCoroutine(errorCoroutine);
            }

            errorCoroutine = StartCoroutine(HideErrorAfterDelay());
        }
    }

    public void HideError()
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }

    private IEnumerator HideErrorAfterDelay()
    {
        yield return new WaitForSeconds(errorDisplayTime);
        HideError();
        errorCoroutine = null;
    }

    private void UpdateReloadCircle()
    {

    }
}
