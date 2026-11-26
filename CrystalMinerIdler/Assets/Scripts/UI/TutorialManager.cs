using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private GameObject tutorialPanel;

    private int step;
    private readonly string[] steps =
    {
        "Добро пожаловать! Кликни по большому кристаллу, чтобы добыть первый ресурс.",
        "Отлично! Теперь открой магазин и купи Better Pickaxe.",
        "Улучшение куплено. Подожди немного — Auto Miner скоро откроется.",
        "Купи Auto Miner. Теперь кристаллы добываются сами!",
        "Продолжай улучшать. Когда накопишь 1 000 000 — сможешь сделать Prestige.",
        "Туториал завершён. Удачи в добыче!"
    };

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        ShowStep();
    }

    public void NextStep()
    {
        step++;
        if (step >= steps.Length)
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
            return;
        }
        ShowStep();
    }

    private void ShowStep()
    {
        if (tutorialText != null && step < steps.Length)
            tutorialText.text = steps[step];
    }

    public void NotifyCrystalClicked()
    {
        if (step == 0) NextStep();
    }

    public void NotifyUpgradeBought()
    {
        if (step == 1 || step == 3) NextStep();
    }
}
