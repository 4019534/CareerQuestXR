using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoScrollCredits : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollDuration = 10f; 
    public bool scrollFromBottom = true;

    private Coroutine scrollCoroutine;

    private void OnEnable()
    {
        if (scrollRect != null)
            scrollCoroutine = StartCoroutine(ScrollRoutine());
    }

    private void OnDisable()
    {
        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);
    }

    private IEnumerator ScrollRoutine()
    {
        float startPos = scrollFromBottom ? 0f : 1f;
        float endPos   = scrollFromBottom ? 1f : 0f;

        while (true) 
        {
            scrollRect.verticalNormalizedPosition = startPos;

            float t = 0f;
            while (t < scrollDuration)
            {
                t += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(t / scrollDuration);
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, endPos, normalizedTime);
                yield return null;
            }

            scrollRect.verticalNormalizedPosition = endPos;
            yield return new WaitForSeconds(1f);
        }
    }
}
