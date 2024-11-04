using UnityEngine;

[CreateAssetMenu(fileName = "NewBullet", menuName = "Weapons/BulletType")]
public class BulletTypes : ScriptableObject
{
    [Header("Basic Properties")]
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 20f;

    [Header("Advanced Properties")]
    public bool canPierce = false;
    public int maxPierceCount = 0;
    public float damageDropoff = 0.2f;

    [Header("Effects")]
    public GameObject hitEffect;
    public AudioClip hitSound;
}
