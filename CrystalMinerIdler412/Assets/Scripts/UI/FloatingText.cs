using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public static FloatingText Instance { get; private set; }

    [SerializeField] private float floatDistance = 80f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float randomRadius = 60f;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string text, Vector3 worldPosition, Color color)
    {
        StartCoroutine(AnimateText(text, worldPosition, color));
    }

    public void ShowCrystalGain(double amount, Vector3 origin)
    {
        Vector2 randomOffset = Random.insideUnitCircle * randomRadius;
        Vector3 pos = origin + new Vector3(randomOffset.x, randomOffset.y, 0);
        Show($"+{amount:F0}", pos, new Color(0.4f, 0.9f, 1f));
    }

    public void ShowUpgrade(string upgradeName, Vector3 origin)
    {
        Vector2 randomOffset = Random.insideUnitCircle * randomRadius;
        Vector3 pos = origin + new Vector3(randomOffset.x, randomOffset.y, 0);
        Show(upgradeName, pos, new Color(1f, 0.85f, 0.3f));
    }

    private IEnumerator AnimateText(string text, Vector3 startPos, Color color)
    {
        var go = new GameObject("FloatingText");
        go.transform.SetParent(transform, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 40);
        rect.position = startPos;

        Vector3 endPos = startPos + Vector3.up * floatDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.position = Vector3.Lerp(startPos, endPos, t);

            Color c = color;
            c.a = 1f - t;
            tmp.color = c;

            yield return null;
        }

        Destroy(go);
    }
}