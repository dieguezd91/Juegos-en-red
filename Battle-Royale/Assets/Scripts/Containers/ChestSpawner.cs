using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] GameObject chestPrefab;
    [SerializeField] WeaponLootTable weaponLootTable;
    [SerializeField] LootTable granadeLootTable;
    [SerializeField] LootTable consumablesLootTable;

    private float currentWeight = 80;

    

    private List<GameObject> chestsSpawned = new List<GameObject>();
    private void Awake()
    {

    }
    private void Start()
    {
        StartSpawning();
    }

    private void SpawnChests()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Random.Range(0, 100) < currentWeight)
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

    public void StartSpawning()
    {
        Shuffle();
        SpawnChests();
        FillChests();
    }

    private void Shuffle()
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int r = Random.Range(i, spawnPoints.Count);
            var temp = spawnPoints[r];
            spawnPoints[r] = spawnPoints[i];
            spawnPoints[i] = temp;
        }
    }

    private void FillChests()
    {
        foreach (var chest in chestsSpawned)
        {
            var tempList = new List<ItemBase>();
            var tempWeaponList = new List<WeaponSO>();

            tempWeaponList.Add(weaponLootTable.DrawWeapon());

            //tempList.Add(granadeLootTable.DrawItem());            
            tempList.Add(consumablesLootTable.DrawItem());
            tempList.Add(consumablesLootTable.DrawItem());

            var chestComp = chest.GetComponent<Chest>();
            chestComp.FillChestWithWeapons(tempWeaponList);
            chestComp.FillChestWithItems(tempList);
        }
    }
}