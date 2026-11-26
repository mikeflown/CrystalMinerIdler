using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;

public class UIBootstrap : MonoBehaviour
{
    private TextMeshProUGUI crystalsText;
    private TextMeshProUGUI cpsText;
    private GameObject settingsPanel;
    private List<TextMeshProUGUI> upgradeTexts = new List<TextMeshProUGUI>();

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    private bool isHoldingReset;
    private float holdTimer;
    private Image resetButtonImage;
    private Image holdProgressBar;
    private const float HoldDuration = 6f;

    private void Start()
    {
        EnsureGameManager();
        EnsureFloatingText();

        var canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        foreach (Transform child in transform)
            if (child.name != "FloatingTextManager")
                child.gameObject.SetActive(false);

        var startPanel = CreatePanel("StartPanel", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        CreateText(startPanel.transform, "CRYSTAL MINER", 48, new Vector2(0, 80));
        var playBtn = CreateButton(startPanel.transform, "PLAY", new Vector2(0, -40));
        playBtn.onClick.AddListener(() =>
        {
            startPanel.SetActive(false);
            CreateGameplay();
        });
    }

    private void Update()
    {
        if (!isHoldingReset || resetButtonImage == null) return;

        holdTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(holdTimer / HoldDuration);

        if (holdProgressBar != null)
            holdProgressBar.fillAmount = t;

        float pulse = Mathf.Lerp(1f, 4f, t);
        float flash = Mathf.PingPong(Time.unscaledTime * pulse, 1f);
        resetButtonImage.color = Color.Lerp(new Color(0.6f, 0.1f, 0.1f), Color.red, flash);

        if (holdTimer >= HoldDuration)
        {
            ResetAllProgress();
            isHoldingReset = false;
            holdTimer = 0;
            if (holdProgressBar) holdProgressBar.fillAmount = 0;
            resetButtonImage.color = new Color(0.6f, 0.1f, 0.1f);
        }
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }

    private void EnsureFloatingText()
    {
        if (FloatingText.Instance == null)
        {
            var go = new GameObject("FloatingTextManager");
            go.transform.SetParent(transform, false);
            go.AddComponent<FloatingText>();
        }
        FloatingText.Instance.gameObject.SetActive(true);
    }

    private void CreateGameplay()
    {
        EnsureGameManager();

        var panel = CreatePanel("Gameplay", new Color(0, 0, 0, 0.3f));

        // Crystal (центр)
        var crystal = CreateButton(panel.transform, "CRYSTAL", new Vector2(-200, 0));
        crystal.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 160);
        crystal.GetComponent<Image>().color = new Color(0.2f, 0.6f, 1f);

        var clickEffect = crystal.gameObject.AddComponent<CrystalClickEffect>();
        crystal.gameObject.AddComponent<UpgradeVisuals>();

        crystal.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null)
            {
                double gain = GameManager.Instance.ClickPower;
                GameManager.Instance.AddCrystalsFromClick();
                if (FloatingText.Instance != null)
                    FloatingText.Instance.ShowCrystalGain(gain, crystal.transform.position);
            }
            clickEffect.Play();
            RefreshUI();
        });

        crystalsText = CreateText(panel.transform, "Crystals: 0", 36, new Vector2(-200, 220));
        cpsText = CreateText(panel.transform, "CPS: 0", 28, new Vector2(-200, 170));

        // Апгрейды справа
        CreateUpgradeButtons(panel.transform);

        // Settings button
        var settingsBtn = CreateButton(panel.transform, "SETTINGS", new Vector2(-200, -250));
        settingsBtn.onClick.AddListener(OpenSettings);

        // Prestige
        var prestigeBtn = CreateButton(panel.transform, "PRESTIGE", new Vector2(200, -250));
        prestigeBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null && GameManager.Instance.CanPrestige())
            {
                GameManager.Instance.DoPrestige();
                if (FloatingText.Instance != null)
                    FloatingText.Instance.Show("PRESTIGE!", prestigeBtn.transform.position, Color.magenta);
            }
            RefreshUI();
        });

        CreateSettingsPanel();

        if (GameManager.Instance != null)
            GameManager.Instance.OnResourcesChanged += RefreshUI;

        RefreshUI();
        InvokeRepeating(nameof(RefreshUI), 0.5f, 0.5f);
    }

    private void CreateUpgradeButtons(Transform parent)
    {
        if (GameManager.Instance == null) return;
        var upgrades = GameManager.Instance.GetUpgrades();
        if (upgrades == null) return;

        for (int i = 0; i < upgrades.Count; i++)
        {
            int index = i;
            var up = upgrades[i];
            float y = 280 - i * 48;

            var btn = CreateButton(parent, up.Name, new Vector2(420, y));
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 42);

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            upgradeTexts.Add(label);

            btn.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null && GameManager.Instance.TryBuyUpgrade(index))
                {
                    if (FloatingText.Instance != null)
                        FloatingText.Instance.ShowUpgrade(up.Name + "!", btn.transform.position);
                }
                RefreshUI();
            });
        }
    }

    private void CreateSettingsPanel()
    {
        settingsPanel = CreatePanel("Settings", new Color(0.05f, 0.05f, 0.1f, 0.96f));
        settingsPanel.SetActive(false);

        CreateText(settingsPanel.transform, "SETTINGS", 36, new Vector2(0, 200));

        // Volume
        CreateText(settingsPanel.transform, "Volume", 24, new Vector2(0, 80));
        var sliderGO = new GameObject("VolumeSlider");
        sliderGO.transform.SetParent(settingsPanel.transform, false);
        volumeSlider = sliderGO.AddComponent<Slider>();
        var sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 30);
        sliderRect.anchoredPosition = new Vector2(0, 30);
        volumeSlider.minValue = 0.0001f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = 0.75f;
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Reset button
        var resetBtn = CreateButton(settingsPanel.transform, "HOLD TO RESET", new Vector2(0, -60));
        resetBtn.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f);
        resetButtonImage = resetBtn.GetComponent<Image>();

        var eventTrigger = resetBtn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((_) => { isHoldingReset = true; holdTimer = 0; });
        eventTrigger.triggers.Add(pointerDown);

        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((_) => { isHoldingReset = false; holdTimer = 0; if (holdProgressBar) holdProgressBar.fillAmount = 0; resetButtonImage.color = new Color(0.6f, 0.1f, 0.1f); });
        eventTrigger.triggers.Add(pointerUp);

        // Progress bar
        var barBG = new GameObject("HoldBarBG");
        barBG.transform.SetParent(settingsPanel.transform, false);
        var bgImg = barBG.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var bgRect = barBG.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(300, 16);
        bgRect.anchoredPosition = new Vector2(0, -20);

        var bar = new GameObject("HoldBar");
        bar.transform.SetParent(barBG.transform, false);
        holdProgressBar = bar.AddComponent<Image>();
        holdProgressBar.color = Color.red;
        holdProgressBar.type = Image.Type.Filled;
        holdProgressBar.fillMethod = Image.FillMethod.Horizontal;
        var barRect = bar.GetComponent<RectTransform>();
        barRect.anchorMin = Vector2.zero;
        barRect.anchorMax = Vector2.one;
        barRect.sizeDelta = Vector2.zero;

        // Close
        var closeBtn = CreateButton(settingsPanel.transform, "CLOSE", new Vector2(0, -160));
        closeBtn.onClick.AddListener(CloseSettings);
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isHoldingReset = false;
        holdTimer = 0;
    }

    private void SetVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
            audioMixer.SetFloat("MasterVolume", dB);
        }
    }

    private void ResetAllProgress()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.FullReset();

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        if (crystalsText) crystalsText.text = $"Crystals: {GameManager.Instance.Crystals:F0}";
        if (cpsText) cpsText.text = $"CPS: {GameManager.Instance.CrystalsPerSecond:F1}";

        var upgrades = GameManager.Instance.GetUpgrades();
        if (upgrades == null) return;

        for (int i = 0; i < upgradeTexts.Count && i < upgrades.Count; i++)
        {
            var up = upgrades[i];
            string lockText = up.IsUnlocked ? "" : " [LOCKED]";
            upgradeTexts[i].text = $"{up.Name} Lv.{up.CurrentLevel}\n{up.CurrentCost:F0}{lockText}";
        }
    }

    private GameObject CreatePanel(string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        return go;
    }

    private Button CreateButton(Transform parent, string text, Vector2 pos)
    {
        var go = new GameObject(text + "Btn");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.5f, 0.8f);
        var btn = go.AddComponent<Button>();
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 60);
        rect.anchoredPosition = pos;

        var t = new GameObject("Text");
        t.transform.SetParent(go.transform, false);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        t.GetComponent<RectTransform>().sizeDelta = new Vector2(380, 50);
        return btn;
    }

    private TextMeshProUGUI CreateText(Transform parent, string text, int size, Vector2 pos)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500, 50);
        rect.anchoredPosition = pos;
        return tmp;
    }
}