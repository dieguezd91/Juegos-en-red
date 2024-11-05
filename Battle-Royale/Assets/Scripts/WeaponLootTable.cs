using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponLootTable", menuName = "LootTable/WeaponLootTable")]
public class WeaponLootTable : ScriptableObject
{
    [System.Serializable]
    public class WeaponEntry
    {
        public WeaponSO weapon;
        public float weight;
    }

    [SerializeField] WeaponEntry[] weapons;

    public float GetMaxWeight()
    {
        float weight = 0;
        foreach (var entry in weapons)
        {
            weight += entry.weight;
        }
        return weight;
    }

    public WeaponSO DrawWeapon()
    {
        var temp = Random.Range(0, GetMaxWeight());
        foreach (var entry in weapons)
        {
            if (temp > 0)
            {
                temp -= entry.weight;
            }
            else return entry.weapon;
        }
        return weapons[0].weapon;
    }
}