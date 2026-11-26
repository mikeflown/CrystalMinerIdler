using UnityEngine;
using System.Collections.Generic;

public class UpgradeVisuals : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeEffect
    {
        public string upgradeId;
        public Color color = Color.cyan;
        public float orbitRadius = 80f;
        public float speed = 1f;
        public GameObject visual;
    }
    [SerializeField] private List<UpgradeEffect> effects = new List<UpgradeEffect>();
    [SerializeField] private Transform crystalTransform;
    private void Start()
    {
        if (crystalTransform == null)
            crystalTransform = transform;

        CreateDefaultEffects();
    }
    private void Update()
    {
        if (GameManager.Instance == null) return;
        var upgrades = GameManager.Instance.GetUpgrades();
        for (int i = 0; i < effects.Count && i < upgrades.Count; i++)
        {
            var effect = effects[i];
            var up = upgrades[i];
            if (effect.visual == null) continue;
            bool active = up.CurrentLevel > 0 && up.IsUnlocked;
            effect.visual.SetActive(active);
            if (active)
            {
                float angle = Time.time * effect.speed + i * 0.8f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * effect.orbitRadius;
                effect.visual.transform.position = crystalTransform.position + offset;
            }
        }
    }
    private void CreateDefaultEffects()
    {
        if (effects.Count > 0) return;
        string[] ids = { "pickaxe", "miner", "drill", "laser", "factory", "reactor", "portal", "singularity" };
        Color[] colors = {
            Color.white, Color.green, Color.yellow, Color.red,
            Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), Color.white
        };
        for (int i = 0; i < ids.Length; i++)
        {
            var go = new GameObject("Effect_" + ids[i]);
            go.transform.SetParent(transform);
            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = colors[i % colors.Length];
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(16, 16);
            effects.Add(new UpgradeEffect
            {
                upgradeId = ids[i],
                color = colors[i % colors.Length],
                orbitRadius = 70 + i * 12,
                speed = 1.2f + i * 0.15f,
                visual = go
            });
            go.SetActive(false);
        }
    }
}