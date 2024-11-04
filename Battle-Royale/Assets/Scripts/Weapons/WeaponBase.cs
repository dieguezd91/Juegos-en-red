using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    public WeaponSO weaponData { get; protected set; }
    protected int currentAmmo;
    protected bool isReloading;
    protected float nextFireTime;

    public virtual void Initialize(WeaponSO data)
    {
        weaponData = data;
        currentAmmo = data.magazineSize;
        isReloading = false;
        nextFireTime = 0f;
    }

    public virtual bool CanShoot()
    {
        return currentAmmo > 0 && !isReloading && Time.time >= nextFireTime;
    }

    public abstract void Shoot(Vector2 direction);

    public virtual void Reload()
    {
        if (!isReloading && currentAmmo < weaponData.magazineSize)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    protected virtual IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(weaponData.reloadTime);
        currentAmmo = weaponData.magazineSize;
        isReloading = false;
    }
}