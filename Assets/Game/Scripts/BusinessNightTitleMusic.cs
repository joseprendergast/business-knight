using System.Collections;
using UnityEngine;

namespace BusinessNight
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BusinessNightTitleMusic : MonoBehaviour
    {
        [SerializeField] AudioClip titleTheme;
        [SerializeField, Range(0f, 1f)] float targetVolume = 0.72f;
        [SerializeField] float fadeInSeconds = 1.4f;
        [SerializeField] float fadeOutSeconds = 0.7f;

        AudioSource source;
        Coroutine fadeRoutine;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            source.clip = titleTheme;
        }

        public void PlayAfterInput()
        {
            if (source == null || titleTheme == null)
                return;

            source.clip = titleTheme;
            if (!source.isPlaying)
                source.Play();

            FadeTo(targetVolume, fadeInSeconds, stopAfterFade: false);
        }

        public void FadeOut()
        {
            FadeTo(0f, fadeOutSeconds, stopAfterFade: true);
        }

        void FadeTo(float volume, float seconds, bool stopAfterFade)
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(FadeRoutine(volume, seconds, stopAfterFade));
        }

        IEnumerator FadeRoutine(float volume, float seconds, bool stopAfterFade)
        {
            float start = source.volume;
            float duration = Mathf.Max(0.01f, seconds);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(start, volume, t / duration);
                yield return null;
            }

            source.volume = volume;
            if (stopAfterFade)
                source.Stop();
        }
    }
}
