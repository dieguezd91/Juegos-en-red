using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    private SceneController sceneManager;
    public SceneController SceneManager => sceneManager;


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
    }

    
    private void Update()
    {
        
    }

    public void ChangeScene(string newScene)
    {
        sceneManager.ChangeScene(newScene);
    }

    public void Quit()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
