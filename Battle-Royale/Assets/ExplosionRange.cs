using UnityEngine;

public class ExplosionRange : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float baseSize;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseSize = spriteRenderer.sprite.bounds.size.x;
        }

        gameObject.SetActive(false);
    }

    public void SetRadius(float radius)
    {
        if (spriteRenderer == null || baseSize == 0) return;

        float scaleFactor = (radius * 2) / baseSize;
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }
}