using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData")]
public class PlayerSO : ScriptableObject
{
    [SerializeField] private float _maxHP;
    [SerializeField] private float _maxShield;
    [SerializeField] private float _maxStamina;
    [SerializeField] private float _staminaDrainRate;
    [SerializeField] private float _staminaRegenRate;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashStaminaCost;
    [SerializeField] private float _speedMovement;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private float _interactionRange;
    [SerializeField] private LayerMask _interactionLayer;
    
    public float MaxHP => _maxHP;
    public float MaxShield => _maxShield;
    public float MaxStamina => _maxStamina;
    public float StaminaDrainRate => _staminaDrainRate;
    public float StaminaRegenRate => _staminaRegenRate;
    public float DashDuration => _dashDuration;
    public float DashStaminaCost => _dashStaminaCost;
    public float SpeedMovement => _speedMovement;
    public float SprintSpeed => _sprintSpeed;
    public float DashSpeed => _dashSpeed;
    public float DashCooldown => _dashCooldown;
    public float InteractionRange => _interactionRange;
    public LayerMask InteractionLayer => _interactionLayer;
}
