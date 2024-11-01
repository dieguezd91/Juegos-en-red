using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPointsHelper : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<Transform> spawnPoints;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.LoadSpawnPoints(spawnPoints);
        }
    }
}