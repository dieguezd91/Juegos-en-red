using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBullet", menuName = "BulletType")]
public class BulletTypes : ScriptableObject
{
    public float speed;
    public float lifeTime;
    public float dispersion;
    public float damage;
}
