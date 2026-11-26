using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private double crystals;
    [SerializeField] private double crystalsPerSecond;
    [SerializeField] private double clickPower = 1;
    [SerializeField] private int prestigeLevel;
    [SerializeField] private double prestigeMultiplier = 1.0;

    [Header("Offline")]
    [SerializeField] private double offlineHoursMax = 2.0;
    [SerializeField] private double offlinePercent = 0.2;

    [Header("Upgrades")]
    [SerializeField] private List<UpgradeData> upgrades = new List<UpgradeData>();

    public double Crystals => crystals;
    public double CrystalsPerSecond => crystalsPerSecond * prestigeMultiplier;
    public double ClickPower => clickPower * prestigeMultiplier;
    public int PrestigeLevel => prestigeLevel;
    public double PrestigeMultiplier => prestigeMultiplier;
    public double OfflineHoursMax => offlineHoursMax;
    public double OfflinePercent => offlinePercent;

    public event System.Action OnResourcesChanged;
    public event System.Action OnUpgradesChanged;

    private float saveTimer;
    private const float SaveInterval = 5f;
    private const double PrestigeThreshold = 1000000;
    private const string LastTimeKey = "CrystalMinerLastTime";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeUpgrades();
        LoadGame();
        RecalculateStats();
        CalculateOfflineEarnings();
    }

    private void Update()
    {
        double cps = CrystalsPerSecond;
        if (cps > 0)
        {
            crystals += cps * Time.deltaTime;
            OnResourcesChanged?.Invoke();
        }

        saveTimer += Time.deltaTime;
        if (saveTimer >= SaveInterval)
        {
            saveTimer = 0;
            SaveGame();
        }
    }

    public void AddCrystalsFromClick()
    {
        crystals += ClickPower;
        OnResourcesChanged?.Invoke();
    }

    public bool TryBuyUpgrade(int index)
    {
        if (index < 0 || index >= upgrades.Count) return false;
        var up = upgrades[index];
        if (!up.CanUpgrade || crystals < up.CurrentCost) return false;

        crystals -= up.CurrentCost;
        up.CurrentLevel++;
        RecalculateStats();
        UnlockNextUpgrades();
        OnResourcesChanged?.Invoke();
        OnUpgradesChanged?.Invoke();
        SaveGame();
        return true;
    }

    public bool CanPrestige() => crystals >= PrestigeThreshold;

    public void DoPrestige()
    {
        if (!CanPrestige()) return;
        prestigeLevel++;
        prestigeMultiplier = 1.0 + prestigeLevel * 0.5;
        crystals = 0;
        foreach (var up in upgrades)
            up.CurrentLevel = 0;
        RecalculateStats();
        OnResourcesChanged?.Invoke();
        OnUpgradesChanged?.Invoke();
        SaveGame();
    }
    private void RecalculateStats()
    {
        crystalsPerSecond = 0;
        clickPower = 1;
        foreach (var up in upgrades)
        {
            if (!up.IsUnlocked) continue;
            if (up.Id.StartsWith("click_"))
                clickPower += up.CurrentLevel * up.EffectPerLevel;
            else if (up.Id != "offline_hours" && up.Id != "offline_percent")
                crystalsPerSecond += up.CurrentLevel * up.EffectPerLevel;
        }
        offlineHoursMax = 2.0 + GetUpgradeLevel("offline_hours");
        offlinePercent = 0.2 + GetUpgradeLevel("offline_percent") * 0.02;
        if (offlinePercent > 1.0) offlinePercent = 1.0;
    }
    private void UnlockNextUpgrades()
    {
        for (int i = 1; i < upgrades.Count; i++)
        {
            if (upgrades[i].Id == "offline_hours" || upgrades[i].Id == "offline_percent")
                continue;
            if (upgrades[i - 1].CurrentLevel >= 5)
                upgrades[i].IsUnlocked = true;
        }
    }
    private int GetUpgradeLevel(string id)
    {
        foreach (var up in upgrades)
            if (up.Id == id) return up.CurrentLevel;
        return 0;
    }
    private void CalculateOfflineEarnings()
    {
        if (!PlayerPrefs.HasKey(LastTimeKey))
        {
            SaveLastTime();
            return;
        }
        if (!long.TryParse(PlayerPrefs.GetString(LastTimeKey), out long lastTicks))
        {
            SaveLastTime();
            return;
        }
        var lastTime = new System.DateTime(lastTicks);
        double hoursAway = (System.DateTime.Now - lastTime).TotalHours;
        if (hoursAway <= 0)
        {
            SaveLastTime();
            return;
        }
        double cappedHours = System.Math.Min(hoursAway, offlineHoursMax);
        double earned = crystalsPerSecond * prestigeMultiplier * offlinePercent * cappedHours * 3600.0;
        if (earned > 1)
        {
            crystals += earned;
            Debug.Log($"Offline earnings: +{earned:F0} crystals ({cappedHours:F2}h)");
            OnResourcesChanged?.Invoke();
        }
        SaveLastTime();
    }
    private void SaveLastTime()
    {
        PlayerPrefs.SetString(LastTimeKey, System.DateTime.Now.Ticks.ToString());
        PlayerPrefs.Save();
    }
    private void OnApplicationQuit()
    {
        SaveLastTime();
        SaveGame();
    }
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveLastTime();
            SaveGame();
        }
    }
    private void InitializeUpgrades()
    {
        if (upgrades.Count > 0) return;
        upgrades.Add(new UpgradeData { Id = "click_pickaxe", Name = "Sharp Pickaxe", Description = "+1 Click", BaseCost = 10, CostMultiplier = 1.12f, EffectPerLevel = 1, IsUnlocked = true });
        upgrades.Add(new UpgradeData { Id = "click_gloves", Name = "Mining Gloves", Description = "+3 Click", BaseCost = 150, CostMultiplier = 1.13f, EffectPerLevel = 3, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "click_drill", Name = "Hand Drill", Description = "+8 Click", BaseCost = 2000, CostMultiplier = 1.14f, EffectPerLevel = 8, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "click_laser", Name = "Click Laser", Description = "+25 Click", BaseCost = 25000, CostMultiplier = 1.15f, EffectPerLevel = 25, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "miner", Name = "Auto Miner", Description = "+4 CPS", BaseCost = 80, CostMultiplier = 1.14f, EffectPerLevel = 4, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "drill", Name = "Crystal Drill", Description = "+20 CPS", BaseCost = 1200, CostMultiplier = 1.15f, EffectPerLevel = 20, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "factory", Name = "Crystal Factory", Description = "+100 CPS", BaseCost = 18000, CostMultiplier = 1.16f, EffectPerLevel = 100, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "reactor", Name = "Quantum Reactor", Description = "+400 CPS", BaseCost = 120000, CostMultiplier = 1.17f, EffectPerLevel = 400, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "portal", Name = "Dimension Portal", Description = "+1500 CPS", BaseCost = 800000, CostMultiplier = 1.18f, EffectPerLevel = 1500, IsUnlocked = false });
        upgrades.Add(new UpgradeData { Id = "offline_hours", Name = "Offline Hours", Description = "+1h offline", BaseCost = 3000, CostMultiplier = 1.2f, EffectPerLevel = 1, IsUnlocked = true });
        upgrades.Add(new UpgradeData { Id = "offline_percent", Name = "Offline Efficiency", Description = "+2% offline", BaseCost = 5000, CostMultiplier = 1.22f, EffectPerLevel = 0.02f, IsUnlocked = true });
    }
    public List<UpgradeData> GetUpgrades() => upgrades;
    private void SaveGame()
    {
        var data = new SaveData
        {
            Crystals = crystals,
            PrestigeLevel = prestigeLevel,
            PrestigeMultiplier = prestigeMultiplier,
            Upgrades = upgrades
        };
        PlayerPrefs.SetString("CrystalMinerSave", JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }
    private void LoadGame()
    {
        if (!PlayerPrefs.HasKey("CrystalMinerSave")) return;
        var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("CrystalMinerSave"));
        if (data == null) return;
        crystals = data.Crystals;
        prestigeLevel = data.PrestigeLevel;
        prestigeMultiplier = data.PrestigeMultiplier > 0 ? data.PrestigeMultiplier : 1.0;
        if (data.Upgrades != null && data.Upgrades.Count > 0)
            upgrades = data.Upgrades;
    }
    [System.Serializable]
    private class SaveData
    {
        public double Crystals;
        public int PrestigeLevel;
        public double PrestigeMultiplier;
        public List<UpgradeData> Upgrades;
    }
    public void FullReset()
    {
        crystals = 0;
        crystalsPerSecond = 0;
        clickPower = 1;
        prestigeLevel = 0;
        prestigeMultiplier = 1.0;
        offlineHoursMax = 2.0;
        offlinePercent = 0.2;

        upgrades.Clear();
        InitializeUpgrades();
        RecalculateStats();

        PlayerPrefs.DeleteKey("CrystalMinerSave");
        PlayerPrefs.DeleteKey(LastTimeKey);
        PlayerPrefs.Save();

        OnResourcesChanged?.Invoke();
        OnUpgradesChanged?.Invoke();
    }
}