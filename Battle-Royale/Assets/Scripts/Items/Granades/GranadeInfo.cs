using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGranade", menuName = "ItemInfo/GranadeInfo")]
public class GranadeInfo : ItemBase
{
    [SerializeField] float bounciness;

    [SerializeField] float damage;

    [SerializeField] float range;

    [SerializeField] GameObject prefab;

    public override void Spawn(Vector3 position, Quaternion rotation)
    {
        throw new System.NotImplementedException();
    }

    public void Throw()
    {
        // instantiate explosive granade prefab
    }
}
