using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    string _sceneIndex;
    public string SceneIndex => _sceneIndex;

    public void ChangeScene(string newSceneIndex)
    {
        _sceneIndex = newSceneIndex;
        SceneManager.LoadScene(newSceneIndex);
    }

}
