using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChestSpawner : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] GameObject chestPrefab;
    [SerializeField] LootTable weaponLootTable;
    [SerializeField] LootTable granadeLootTable;
    [SerializeField] LootTable consumablesLootTable;

    private float currentWeight = 80;
    private List<GameObject> chestsSpawned = new List<GameObject>();
    private void SpawnChests()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if(Random.Range(0,100) < currentWeight)
            {
                var chest = PhotonNetwork.Instantiate(chestPrefab.name, spawnPoint.position, Quaternion.identity);
                chestsSpawned.Add(chest);
            }

            if (chestsSpawned.Count > 5)
            {
                currentWeight = 60;
            }
            else if (chestsSpawned.Count > 10)
            {
                currentWeight = 30;
            }
        }
    }

    private void Shuffle()
    {
        for(int i = 0; i < spawnPoints.Count; i++)
        {
            int r = Random.Range(i, spawnPoints.Count);
            var temp = spawnPoints[r];
            spawnPoints[r] = spawnPoints[i];
            spawnPoints[i] = temp;
        }
    }

    private void FillChests()
    {
        List<ItemBase> tempList = new List<ItemBase>();

        foreach (var chest in chestsSpawned)
        {
            tempList.Add(weaponLootTable.DrawItem());
            tempList.Add(granadeLootTable.DrawItem());
            tempList.Add(consumablesLootTable.DrawItem());
            tempList.Add(consumablesLootTable.DrawItem());

            chest.GetComponent<Chest>().FillChest(tempList);

            tempList.Clear();
        }
    }
}
