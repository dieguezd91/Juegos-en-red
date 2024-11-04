using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShotgunData", menuName = "Weapons/ShotgunData")]
public class ShotgunSO : WeaponSO
{
    [Header("Shotgun Specific")]
    public int pelletCount = 8;
    public float pelletSpread = 15f; // Dispersi�n m�s amplia que las armas normales
    public float effectiveRange = 10f; // Rango efectivo m�s corto
}
