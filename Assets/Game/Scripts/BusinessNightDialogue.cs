using System.Collections;
using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightDialogue : MonoBehaviour
    {
        public static BusinessNightDialogue Instance { get; private set; }

        Coroutine activeRoutine;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Say(string speaker, string text, string flagOnFirstPlay = "", bool playOnce = false, float holdSeconds = 2.2f)
        {
            if (playOnce && BusinessNightGlobals.Instance != null && BusinessNightGlobals.Instance.HasFlag(flagOnFirstPlay))
                return;

            if (!string.IsNullOrWhiteSpace(flagOnFirstPlay))
                BusinessNightGlobals.Instance?.SetFlag(flagOnFirstPlay, true, BusinessNightFlagBucket.Dialogue);

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(SayRoutine(speaker, text, holdSeconds));
        }

        public void Say(BusinessNightSubtitleLine line)
        {
            if (line == null)
                return;

            Say(line.speaker, line.text, line.flagOnFirstPlay, line.playOnce, line.holdSeconds);
        }

        IEnumerator SayRoutine(string speaker, string text, float holdSeconds)
        {
            yield return BusinessNightUi.Instance?.ShowSubtitle(speaker, text);
            yield return new WaitForSeconds(Mathf.Max(0.2f, holdSeconds));
            yield return BusinessNightUi.Instance?.HideSubtitle();
            activeRoutine = null;
        }
    }
}
