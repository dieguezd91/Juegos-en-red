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

    [SerializeField] public TMP_InputField createInput;
    [SerializeField] public TMP_InputField joinInput;
    [SerializeField] private GameObject createInputGo;
    [SerializeField] private GameObject joinInputGo;

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button exitBtn;

    [Header ("HUD")]
    [SerializeField] private GameObject timer;
    [SerializeField] private Image lifeBar;
    [SerializeField] private Image staminaBar;
    [SerializeField] private Image shieldBar;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI currentWeaponText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image weaponIcon;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        PlayerController.OnPlayerControllerInstantiated += OnPlayerControllerInstantiated;        
    }

    private void Start()
    {
        playBtn.onClick.AddListener(Play);
        createButton.onClick.AddListener(GameManager.Instance.CreateRoom);
        joinButton.onClick.AddListener(GameManager.Instance.JoinRoom);
        exitBtn.onClick.AddListener(GameManager.Instance.Quit);
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
            var remainingTime = GameManager.Instance.roundDuration;
            timer.gameObject.GetComponent<TextMeshProUGUI>().text = FormatTime(remainingTime);
        }

        if (_playerController != null && _playerController.pv.IsMine)
        {
            UpdateLifeBar();
            UpdateStaminaBar();
            UpdateShieldBar();
            UpdateAmmoCount();
        }
    }

    private void OnDestroy()
    {
        createButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
        playBtn.onClick.RemoveAllListeners();
        exitBtn.onClick.RemoveAllListeners();
        PlayerController.OnPlayerControllerInstantiated -= OnPlayerControllerInstantiated;
        if (_weaponController != null)
        {
            _weaponController.OnWeaponChanged -= UpdateWeaponUI;
        }
    }

    private void OnPlayerControllerInstantiated(PlayerController player)
    {
        _playerController = player;
        _weaponController = player.GetComponent<PlayerWeaponController>();

        if (_weaponController != null)
        {
            _weaponController.OnWeaponChanged += UpdateWeaponUI;
        }
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
            var hpPercentage = _playerController.GetComponent<LifeController>().currentHp / _playerController.GetComponent<LifeController>().PlayerData.MaxHP;
            lifeBar.fillAmount = hpPercentage;
            lifeBar.color = GetHealthColor(hpPercentage);
        }
    }

    private void UpdateStaminaBar()
    {
        if (_playerController.pv.IsMine)
        {
            var staminaPercentage = _playerController.currentStamina / _playerController.maxStamina;
            staminaBar.fillAmount = staminaPercentage;
            staminaBar.color = GetStaminaColor(staminaPercentage);
        }
    }

    private void UpdateShieldBar()
    {
        if (_playerController.pv.IsMine)
        {
            var lifeController = _playerController.GetComponent<LifeController>();
            if (lifeController != null)
            {
                var shieldPercentage = lifeController.currentShield / lifeController.maxShield;
                shieldBar.fillAmount = shieldPercentage;
                shieldBar.color = GetShieldColor(shieldPercentage);
            }
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
        var minutes = Mathf.FloorToInt((time % 3600) / 60);
        var seconds = Mathf.FloorToInt(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
}
