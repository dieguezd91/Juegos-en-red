using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject inputField;

    void Start()
    {

    }

    void Update()
    {

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

    public void JoinGame()
    {
        if (inputField != null)
        {
            if (!inputField.activeInHierarchy)
            {
                inputField.SetActive(true);
            }
        }
    }
}
