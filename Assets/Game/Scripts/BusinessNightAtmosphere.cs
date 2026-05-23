using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightAtmosphere : MonoBehaviour
    {
        [Header("Overlay Hooks")]
        public bool rain;
        public bool fog;
        public bool lightFlicker;
        public bool vignette = true;
        public AudioClip roomTone;
        public AudioClip transitionSting;

        [SerializeField] SpriteRenderer overlay;
        [SerializeField] AudioSource ambienceSource;
        [SerializeField] float flickerStrength = 0.08f;
        [SerializeField] float overlayDrift = 0.03f;

        Color baseColor;

        void Awake()
        {
            if (overlay != null)
                baseColor = overlay.color;

            if (ambienceSource != null)
            {
                ambienceSource.clip = roomTone;
                ambienceSource.loop = true;
                ambienceSource.playOnAwake = false;
            }
        }

        void Start()
        {
            if (ambienceSource != null && roomTone != null)
                ambienceSource.Play();
        }

        void Update()
        {
            if (overlay == null)
                return;

            Vector3 position = overlay.transform.localPosition;
            if (rain || fog)
                overlay.transform.localPosition = new Vector3(position.x + overlayDrift * Time.deltaTime, position.y, position.z);

            if (lightFlicker)
            {
                float flicker = 1f + Mathf.Sin(Time.time * 17.0f) * flickerStrength;
                overlay.color = baseColor * flicker;
            }
        }
    }
}
