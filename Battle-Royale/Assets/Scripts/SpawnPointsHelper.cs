using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsHelper : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints;    

    private void Awake()
    {
        GameManager.Instance.LoadSpawnPoints(spawnPoints);
    }
}
