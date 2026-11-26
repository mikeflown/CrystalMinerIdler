using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    public static FloatingText Instance { get; private set; }

    [SerializeField] private float floatDistance = 90f;
    [SerializeField] private float duration = 2.8f;
    [SerializeField] private float randomRadius = 70f;

    private void Awake() => Instance = this;

    public void Show(string text, Vector3 worldPosition, Color color, float size = 28f)
    {
        StartCoroutine(AnimateText(text, worldPosition, color, size));
    }

    public void ShowCrystalGain(double amount, Vector3 origin)
    {
        Vector2 offset = Random.insideUnitCircle * randomRadius;
        Show($"+{amount:F0}", origin + (Vector3)offset, new Color(0.5f, 0.95f, 1f), 32f);
    }

    public void ShowUpgrade(string name, Vector3 origin)
    {
        Vector2 offset = Random.insideUnitCircle * randomRadius;
        Show(name, origin + (Vector3)offset, new Color(1f, 0.85f, 0.4f), 26f);
    }

    public void ShowOffline(double amount)
    {
        Show($"Offline +{amount:F0}", new Vector3(Screen.width * 0.5f, Screen.height * 0.7f, 0), new Color(0.6f, 1f, 0.6f), 36f);
    }

    public void ShowPrestige()
    {
        Show("PRESTIGE!", new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0), new Color(1f, 0.4f, 1f), 48f);
    }

    private IEnumerator AnimateText(string text, Vector3 startPos, Color color, float size)
    {
        var go = new GameObject("FloatingText");
        go.transform.SetParent(transform, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.fontStyle = FontStyles.Bold;

        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 50);
        rect.position = startPos;

        Vector3 endPos = startPos + Vector3.up * floatDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float ease = 1f - Mathf.Pow(1f - t, 2f);

            rect.position = Vector3.Lerp(startPos, endPos, ease);

            Color c = color;
            c.a = 1f - t;
            tmp.color = c;

            yield return null;
        }

        Destroy(go);
    }
}