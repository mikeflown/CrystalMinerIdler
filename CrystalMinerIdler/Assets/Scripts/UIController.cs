using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject settingsPanel;
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button crystalButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button prestigeButton;
    [SerializeField] private Button resetButton;
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI crystalsText;
    [SerializeField] private TextMeshProUGUI cpsText;
    [Header("Upgrades")]
    [SerializeField] private List<Button> upgradeButtons = new List<Button>();
    [SerializeField] private List<TextMeshProUGUI> upgradeLabels = new List<TextMeshProUGUI>();
    [Header("Reset Hold")]
    [SerializeField] private Image resetButtonImage;
    [SerializeField] private Image holdProgressBar;
    private bool isHoldingReset;
    private float holdTimer;
    private const float HoldDuration = 6f;
    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlay);
        if (crystalButton != null)
            crystalButton.onClick.AddListener(OnCrystalClick);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);
        if (prestigeButton != null)
            prestigeButton.onClick.AddListener(OnPrestige);
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            int index = i;
            if (upgradeButtons[i] != null)
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeClick(index));
        }
        SetupResetHold();
        if (GameManager.Instance != null)
            GameManager.Instance.OnResourcesChanged += RefreshUI;
        RefreshUI();
    }
    private void SetupResetHold()
    {
        if (resetButton == null) return;
        var trigger = resetButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = resetButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.Clear();
        var down = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown
        };
        down.callback.AddListener(_ =>
        {
            isHoldingReset = true;
            holdTimer = 0f;
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 0f;
        });
        trigger.triggers.Add(down);
        var up = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp
        };
        up.callback.AddListener(_ =>
        {
            isHoldingReset = false;
            holdTimer = 0f;
            if (holdProgressBar != null)
                holdProgressBar.fillAmount = 0f;
            if (resetButtonImage != null)
                resetButtonImage.color = new Color(0.5f, 0.05f, 0.05f);
        });
        trigger.triggers.Add(up);
    }
    private void Update()
    {
        if (!isHoldingReset) return;
        holdTimer += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(holdTimer / HoldDuration);
        if (holdProgressBar != null)
            holdProgressBar.fillAmount = t;
        if (resetButtonImage != null)
        {
            float pulse = Mathf.Lerp(1f, 5f, t);
            float flash = Mathf.PingPong(Time.unscaledTime * pulse, 1f);
            resetButtonImage.color = Color.Lerp(new Color(0.5f, 0.05f, 0.05f), Color.red, flash);
        }
        if (holdTimer >= HoldDuration)
        {
            isHoldingReset = false;
            if (GameManager.Instance != null)
                GameManager.Instance.FullReset();
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
    private void OnPlay()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
    }
    private void OnCrystalClick()
    {
        if (GameManager.Instance == null) return;
        double gain = GameManager.Instance.ClickPower;
        GameManager.Instance.AddCrystalsFromClick();
        if (FloatingText.Instance != null && crystalButton != null)
            FloatingText.Instance.ShowCrystalGain(gain, crystalButton.transform.position);
        var effect = crystalButton != null ? crystalButton.GetComponent<CrystalClickEffect>() : null;
        if (effect != null) effect.Play();
        RefreshUI();
    }
    private void OnUpgradeClick(int index)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.TryBuyUpgrade(index))
        {
            if (FloatingText.Instance != null && index < upgradeButtons.Count && upgradeButtons[index] != null)
                FloatingText.Instance.ShowUpgrade("Upgrade!", upgradeButtons[index].transform.position);
        }
        RefreshUI();
    }
    private void OnPrestige()
    {
        if (GameManager.Instance != null && GameManager.Instance.CanPrestige())
            GameManager.Instance.DoPrestige();
        RefreshUI();
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
        holdTimer = 0f;
        if (holdProgressBar != null)
            holdProgressBar.fillAmount = 0f;
    }
    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;
        if (crystalsText != null)
            crystalsText.text = $"Crystals: {GameManager.Instance.Crystals:F0}";
        if (cpsText != null)
            cpsText.text = $"CPS: {GameManager.Instance.CrystalsPerSecond:F1}";
        var upgrades = GameManager.Instance.GetUpgrades();
        if (upgrades == null) return;
        for (int i = 0; i < upgradeLabels.Count && i < upgrades.Count; i++)
        {
            if (upgradeLabels[i] == null) continue;
            var up = upgrades[i];
            string locked = up.IsUnlocked ? "" : " [LOCKED]";
            upgradeLabels[i].text = $"{up.Name}  Lv.{up.CurrentLevel}\n{up.CurrentCost:F0}{locked}";
        }
    }
}