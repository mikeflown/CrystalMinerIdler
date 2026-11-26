using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KenneyUIStyler : MonoBehaviour
{
    [Header("Kenney Sprites (перетащи из Art/UI)")]
    [SerializeField] private Sprite buttonBlue;
    [SerializeField] private Sprite buttonGreen;
    [SerializeField] private Sprite buttonYellow;
    [SerializeField] private Sprite buttonRed;
    [SerializeField] private Sprite buttonGrey;
    [SerializeField] private Sprite panelBlue;
    [SerializeField] private Sprite panelDark;
    [SerializeField] private Sprite iconCrystal;
    [SerializeField] private Sprite sliderBackground;
    [SerializeField] private Sprite sliderFill;
    [SerializeField] private Sprite sliderHandle;

    private void Start()
    {
        Invoke(nameof(StyleAll), 0.1f);
    }

    private void StyleAll()
    {
        StyleButtons();
        StylePanels();
        StyleCrystal();
        StyleSlider();
        StyleTexts();
    }

    private void StyleButtons()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            var img = btn.GetComponent<Image>();
            if (img == null) continue;

            string name = btn.name.ToLower();

            if (name.Contains("reset") || name.Contains("hold"))
                img.sprite = buttonRed;
            else if (name.Contains("prestige"))
                img.sprite = buttonYellow;
            else if (name.Contains("settings") || name.Contains("close"))
                img.sprite = buttonGrey;
            else if (name.Contains("play") || name.Contains("crystal"))
                img.sprite = buttonBlue;
            else
                img.sprite = buttonGreen;

            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var colors = btn.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            btn.colors = colors;
        }
    }

    private void StylePanels()
    {
        var images = FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (var img in images)
        {
            if (img.GetComponent<Button>() != null) continue;

            string name = img.gameObject.name.ToLower();
            if (name.Contains("panel") || name.Contains("settings") || name.Contains("gameplay") || name.Contains("start"))
            {
                img.sprite = name.Contains("settings") ? panelDark : panelBlue;
                img.type = Image.Type.Sliced;
                img.color = new Color(1f, 1f, 1f, 0.92f);
            }
        }
    }

    private void StyleCrystal()
    {
        var crystal = GameObject.Find("CRYSTALBtn");
        if (crystal == null) return;

        var img = crystal.GetComponent<Image>();
        if (img != null && iconCrystal != null)
        {
            img.sprite = iconCrystal;
            img.color = Color.white;
            img.type = Image.Type.Simple;
        }

        var text = crystal.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = "";
    }

    private void StyleSlider()
    {
        var slider = FindFirstObjectByType<Slider>();
        if (slider == null) return;

        if (sliderBackground != null && slider.fillRect != null)
        {
            var bg = slider.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = sliderBackground;
                bg.type = Image.Type.Sliced;
            }
        }

        if (sliderFill != null && slider.fillRect != null)
        {
            var fill = slider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                fill.sprite = sliderFill;
                fill.type = Image.Type.Sliced;
                fill.color = new Color(0.3f, 0.7f, 1f);
            }
        }

        if (sliderHandle != null && slider.handleRect != null)
        {
            var handle = slider.handleRect.GetComponent<Image>();
            if (handle != null)
            {
                handle.sprite = sliderHandle;
                handle.color = Color.white;
            }
        }
    }

    private void StyleTexts()
    {
        var texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tmp in texts)
        {
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.outlineWidth = 0.15f;
            tmp.outlineColor = new Color(0, 0, 0, 0.6f);
        }
    }
}
