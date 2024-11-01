using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FOVReceiver : MonoBehaviourPunCallbacks
{
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        CollectRenderers();
    }

    private void CollectRenderers()
    {
        renderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));
    }

    public void HideSprites()
    {
        if (renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    public void ShowSprites()
    {
        if (renderers == null) return;

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }
    }
}