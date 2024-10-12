using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData")]
public class PlayerSO : ScriptableObject
{
    [SerializeField] private float _maxHP;
    [SerializeField] private float _maxShield;
    [SerializeField] private float _maxStamina;

    public float MaxHP => _maxHP;
    public float MaxShield => _maxShield;
    public float MaxStamina => _maxStamina;

}
