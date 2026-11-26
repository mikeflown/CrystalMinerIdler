using System;
using UnityEngine;

[Serializable]
public class UpgradeData
{
    public string Id;
    public string Name;
    public string Description;
    public float BaseCost;
    public float CostMultiplier = 1.15f;
    public float EffectPerLevel;
    public int MaxLevel = 100;
    public int CurrentLevel;
    public bool IsUnlocked = true;

    public float CurrentCost => BaseCost * Mathf.Pow(CostMultiplier, CurrentLevel);
    public bool CanUpgrade => CurrentLevel < MaxLevel && IsUnlocked;
}
