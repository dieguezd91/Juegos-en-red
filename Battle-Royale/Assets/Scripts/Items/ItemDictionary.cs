using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemDictionary
{
    private static List<ItemBase> gameItems = new List<ItemBase>();
    private static Dictionary<ItemBase, int> itemIds = new Dictionary<ItemBase, int>();

    public static int GetItemID(ItemBase item)
    {
        if (gameItems.Contains(item))
        {
            //weaponIds.TryGetValue(weapon, out int id);
            return itemIds.GetValueOrDefault(item);
        }
        else
        {
            gameItems.Add(item);
            itemIds.Add(item, gameItems.IndexOf(item));
            GetItemID(item);
        }
        return 0;
    }

    public static ItemBase GetItem(int id)
    {
        if (id < gameItems.Count)
        {
            return gameItems[id];
        }

        //else

        Debug.Log("The weapon you requested has not been loaded in the dictionary");
        return null;
    }
}
