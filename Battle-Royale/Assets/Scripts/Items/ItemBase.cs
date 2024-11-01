using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    [SerializeField] protected float weight;

    [SerializeField] protected Sprite icon;

    //[SerializeField] protected GameObject prefab;
    //public GameObject Prefab { get { return prefab; } }
    public float Weight { get { return weight; }}

    public abstract void Spawn(Vector3 position, Quaternion rotation);
}
