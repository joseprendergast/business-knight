using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightSettings : MonoBehaviour
    {
        public static BusinessNightSettings Instance { get; private set; }

        bool audioUnlocked;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Apply();
        }

        void Update()
        {
            if (!audioUnlocked && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
                UnlockBrowserAudio();
        }

        public void Apply()
        {
            BusinessNightSettingsData settings = BusinessNightGlobals.Instance != null
                ? BusinessNightGlobals.Instance.settings
                : new BusinessNightSettingsData();

            AudioListener.volume = settings.muted ? 0f : settings.masterVolume;
        }

        public void SetMuted(bool muted)
        {
            if (BusinessNightGlobals.Instance == null)
                return;

            BusinessNightGlobals.Instance.settings.muted = muted;
            Apply();
            BusinessNightSaveSystem.Save(BusinessNightGlobals.Instance);
        }

        public void SetMasterVolume(float value)
        {
            if (BusinessNightGlobals.Instance == null)
                return;

            BusinessNightGlobals.Instance.settings.masterVolume = Mathf.Clamp01(value);
            Apply();
            BusinessNightSaveSystem.Save(BusinessNightGlobals.Instance);
        }

        void UnlockBrowserAudio()
        {
            audioUnlocked = true;
            AudioListener.pause = false;
        }
    }
}
