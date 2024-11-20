using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "LootTable")]
public class LootTable : ScriptableObject
{
    [SerializeField] ItemBase[] items;
    private bool gettingMaxWeight = false;
    private bool hasMaxWeight = false;
    private float maxWeight;

    public void GetMaxWeight()
    {
        gettingMaxWeight = true;
        if (items == null)
        {
            Debug.Log("item pool is null");
        }
        float weight = 0;
        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.Log("Item Does not exist");
            }
            weight += item.Weight;
            Debug.Log("Loaded Weight ="+item.Weight);
        }
        Debug.Log("Max Weight"+weight);
        hasMaxWeight = true;
        maxWeight = weight;
    }

    public ItemBase DrawItem()
    {
        float temp = Random.Range(1, maxWeight);

        foreach (var item in items)
        {
            if (temp > 0)
            {
                temp -= item.Weight;
            }
            else return item;
        }

        Debug.Log("Draw Failed");
        if(items == null)
        {
            Debug.Log("item pool is null");
        }
        Debug.Log("chosen weight: " + temp);
        
        return null;
    }
}
