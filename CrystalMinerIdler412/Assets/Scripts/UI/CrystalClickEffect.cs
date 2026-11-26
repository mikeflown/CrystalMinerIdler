using UnityEngine;

public class CrystalClickEffect : MonoBehaviour
{
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float duration = 0.12f;
    private Vector3 originalScale;
    private ParticleSystem particles;
    private void Awake()
    {
        originalScale = transform.localScale;
        particles = GetComponent<ParticleSystem>();
        if (particles == null)
            particles = gameObject.AddComponent<ParticleSystem>();
    }
    public void Play()
    {
        if (particles != null)
            particles.Play();
        StopAllCoroutines();
        StartCoroutine(Punch());
    }
    private System.Collections.IEnumerator Punch()
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(punchScale, 1f, t / duration);
            transform.localScale = originalScale * s;
            yield return null;
        }
        transform.localScale = originalScale;
    }
}