using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "LootTable")]
public class LootTable : ScriptableObject
{
    [SerializeField] ItemBase[] items;
    private float maxWeight;

    private void Awake()
    {
        GetMaxWeight();
    }
    private void GetMaxWeight()
    {       
        float weight = 0;
        foreach (var item in items)
        {           
            weight += item.Weight;
            Debug.Log("Loaded Weight ="+item.Weight);
        }
        maxWeight = weight;
        
    }

    public ItemBase DrawItem()
    {
        if (maxWeight > 0 && items != null)
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
        }

        Debug.Log("Draw failed");

        return items[Random.Range(0, items.Length-1)];
    }        
}
