using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVReciver : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> Renderers;    

    public void HideSprites()
    {
        foreach (var renderer in Renderers)
        {
            renderer.enabled = false;
        }
    }

    public void ShowSprites()
    {
        foreach (var renderer in Renderers)
        {
            renderer.enabled = true;
        }
    }
}
