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
    // public float StaminaRegenRate = 2.5f;
    // public float StaminaDrainRate = 15f;
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
    }
    public void RegenerateStamina()
    {
        if (!IsSprinting && !IsDashing && CurrentStamina < MaxStamina)
        {
            CurrentStamina += StaminaRegenRate;
            CurrentStamina = Mathf.Clamp(CurrentStamina, 0, MaxStamina);
        }
    }
}