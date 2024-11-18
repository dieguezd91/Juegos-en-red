using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WeaponDictionary
{
    private static List<WeaponSO> gameWeapons = new List<WeaponSO>();
    private static Dictionary<WeaponSO, int> weaponIds = new Dictionary<WeaponSO, int>();

    public static int GetWeaponID(WeaponSO weapon)
    {
        if (gameWeapons.Contains(weapon))
        {
            //weaponIds.TryGetValue(weapon, out int id);
            return weaponIds.GetValueOrDefault(weapon);
        }
        else
        {
            gameWeapons.Add(weapon);
            weaponIds.Add(weapon, gameWeapons.IndexOf(weapon));
            GetWeaponID(weapon);
        }
        return 0;
    }
    
    public static WeaponSO GetWeapon(int id)
    {
        if(id < gameWeapons.Count)
        {
            return gameWeapons[id];
        }

        //else

        Debug.Log("The weapon you requested has not been loaded in the dictionary");
        return null;
    }
}
