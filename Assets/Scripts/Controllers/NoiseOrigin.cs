using System.Collections;
using UnityEngine;

public class NoiseOrigin : MonoBehaviour
{
    [SerializeField] GameObject SphereCollider;

    ParticleSystem particle;
    ParticleSystem.MainModule main;
    ParticleSystem.SizeOverLifetimeModule sizeOverLifetime;

    public bool Setup(NoiseOptions options, Vector3 position)
    {
        transform.position = position;

        particle = GetComponentInChildren<ParticleSystem>();

        if (particle == null)
        {
            Debug.LogError("Particle System not found");
            return false;
        }

        if (SphereCollider == null)
        {
            Debug.LogError("Sphere Collider not found");
            return false;
        }

        main = particle.main;
        main.duration = options.duration;
        main.startSize = options.startSize;

        sizeOverLifetime = particle.sizeOverLifetime;
        sizeOverLifetime.size = options.sizeOverLifeTime;

        SphereCollider.transform.localScale = Vector3.zero;

        return true;
    }

    public void PlayEffect()
    {
        particle.Play();
        StartCoroutine(ExpandCollider());
    }

    private IEnumerator ExpandCollider()
    {
        float duration = main.duration;
        float startSize = main.startSize.constant;

        float sizeMultiplier = 0;
        float size = 0;

        while (particle.time < duration)
        {
            sizeMultiplier = sizeOverLifetime.size.Evaluate(particle.time);
            size = startSize * sizeMultiplier;

            SphereCollider.transform.localScale = Vector3.one * size;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
