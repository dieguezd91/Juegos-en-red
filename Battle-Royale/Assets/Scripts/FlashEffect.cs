using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    [SerializeField] private Image flashPanel;
    [SerializeField] public float fadeOutSpeed = 1f;

    private Coroutine flashCoroutine;

    public void ShowFlash(float duration)
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashCoroutine(duration));
    }

    private IEnumerator FlashCoroutine(float duration)
    {
        Color panelColor = Color.white;
        flashPanel.color = panelColor;
        flashPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        while (panelColor.a > 0)
        {
            panelColor.a -= Time.deltaTime * fadeOutSpeed;
            flashPanel.color = panelColor;
            yield return null;
        }

        flashPanel.gameObject.SetActive(false);
    }
}