using UnityEngine;
using System.Collections;

public class CrystalClickEffect : MonoBehaviour
{
    [SerializeField] private float punchScale = 1.25f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private Color glowColor = new Color(0.4f, 0.8f, 1f);
    private Vector3 originalScale;
    private UnityEngine.UI.Image image;
    private ParticleSystem particles;
    private void Awake()
    {
        originalScale = transform.localScale;
        image = GetComponent<UnityEngine.UI.Image>();
        particles = GetComponent<ParticleSystem>();
        if (particles == null)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startColor = glowColor;
            main.startSize = 0.3f;
            main.startLifetime = 0.6f;
            main.maxParticles = 20;
        }
    }
    public void Play()
    {
        if (particles != null) particles.Play();
        StopAllCoroutines();
        StartCoroutine(PunchAndGlow());
    }
    private IEnumerator PunchAndGlow()
    {
        float t = 0;
        Color original = image != null ? image.color : Color.white;
        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(punchScale, 1f, t / duration);
            transform.localScale = originalScale * s;
            if (image != null)
                image.color = Color.Lerp(glowColor, original, t / duration);
            yield return null;
        }
        transform.localScale = originalScale;
        if (image != null) image.color = original;
    }
}