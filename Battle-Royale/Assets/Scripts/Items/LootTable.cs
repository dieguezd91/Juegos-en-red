using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLootTable", menuName = "LootTable")]
public class LootTable : ScriptableObject
{
    [SerializeField] ItemBase[] items;

    public float GetMaxWeight()
    {
        float weight = 0;
        foreach (var item in items)
        {
            weight += item.Weight;
        }

        return weight;
    }

    public ItemBase DrawItem()
    {
        var temp = Random.Range(0, GetMaxWeight());

        foreach (var item in items)
        {
            if (temp > 0)
            {
                temp -= item.Weight;
            }
            else return item;
        }
        return null;
    }
}
