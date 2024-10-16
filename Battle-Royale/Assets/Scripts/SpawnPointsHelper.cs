using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsHelper : MonoBehaviour
{
    [SerializeField] List<Transform> Spawnpoints;

    private void Awake()
    {
        GameManager.Instance.LoadSpawnPoints(Spawnpoints);
    }
}
