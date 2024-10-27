using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] protected float weight;
    public float Weight { get { return weight; }}
}
