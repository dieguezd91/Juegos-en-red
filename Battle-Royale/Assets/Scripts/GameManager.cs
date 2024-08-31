using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    public SceneController SceneManager { get; private set; }

    [SerializeField] public float roundDuration;

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
    }

    private void Update()
    {
        if (SceneManager != null && SceneManager.SceneIndex == "Gameplay")
        {
            roundDuration -= Time.deltaTime;
        }
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
