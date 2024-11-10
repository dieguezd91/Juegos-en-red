using UnityEngine;

public class PlayerModel
{
    public PlayerSO PlayerData { get; set; }
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; set; }
    public float CurrentStamina { get; set; }
    public float MaxStamina { get; set; }
    public Vector2 InputMovement { get; set; }
    public Vector2 LastDirection { get; set; }
    public bool IsSprinting { get; set; }
    public bool IsDashing { get; set; }
    public bool CanDash { get; set; }
    public int CurrentWeapon { get; set; }
    public WeaponInfo[] EquippedWeapons { get; set; }
    public float StaminaRegenRate { get; set; }
    public float StaminaDrainRate { get; set; }
    public float DashDuration { get; set; }
    public float DashStaminaCost { get; set; }
    public float SpeedMovement { get; set; }
    public float SprintSpeed { get; set; }
    public float DashSpeed { get; set; }
    public float DashCooldown { get; set; }
    public float InteractionRange { get; set; }
    public LayerMask InteractionLayer { get; set; }
    
    public PlayerModel(PlayerSO playerData)
    {
        PlayerData = playerData;
        MaxHealth = playerData.MaxHP;
        CurrentHealth = MaxHealth;
        MaxStamina = playerData.MaxStamina;
        CurrentStamina = MaxStamina;
        LastDirection = Vector2.up;
        CanDash = true;
        EquippedWeapons = new WeaponInfo[2];
        StaminaDrainRate = playerData.StaminaDrainRate;
        StaminaRegenRate = playerData.StaminaRegenRate;
        DashDuration = playerData.DashDuration;
        DashStaminaCost = playerData.DashStaminaCost;
        SpeedMovement = playerData.SpeedMovement;
        SprintSpeed = playerData.SprintSpeed;
        DashSpeed = playerData.DashSpeed;
        DashCooldown = playerData.DashCooldown;
        InteractionRange = playerData.InteractionRange;
        InteractionLayer = playerData.InteractionLayer;
    }
    public void RegenerateStamina()
    {
        if (!IsSprinting && !IsDashing && CurrentStamina < MaxStamina)
        {
            CurrentStamina += StaminaRegenRate;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
        }
    }
    public void StaminaCost(float cost)
    {
        CurrentStamina -= cost;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
    }
    public void Sprint(float cost)
    {
        IsSprinting = true;
        CurrentStamina -= cost * Time.deltaTime;
        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
    }
}