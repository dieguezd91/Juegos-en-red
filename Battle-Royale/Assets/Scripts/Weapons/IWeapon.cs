using UnityEngine;

public enum WeaponType
{
    Pistol,
    Rifle,
    Shotgun,
    SniperRifle,
    SubmachineGun
}

public interface IWeapon
{
    void Initialize(WeaponSO weaponData);
    void Shoot(Vector2 direction);
    void Reload();
    bool CanShoot();
}
