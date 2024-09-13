using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    public string GetName();
    public void Fire();

    public void LoadAmmo(int amount);

    public int CurrentAmmo();
    
}
