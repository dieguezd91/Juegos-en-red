using UnityEngine;

[CreateAssetMenu(fileName = "MachineGunData", menuName = "Weapons/MachineGunData")]
public class MachineGunSO : WeaponSO
{
    [Header("Machine Gun Specific")]
    public float maxSpread = 20f;
    public float spreadIncreasePerShot = 1f;
    public float spreadRecoveryRate = 5f;
    public float recoilAmount = 0.1f;
    public float muzzleFlashDuration = 0.05f;
}
