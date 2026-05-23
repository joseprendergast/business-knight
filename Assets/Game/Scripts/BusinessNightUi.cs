using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BusinessNight
{
    public sealed class BusinessNightUi : MonoBehaviour
    {
        public static BusinessNightUi Instance { get; private set; }

        [Header("Palette")]
        public BusinessNightPalette palette = new();

        [Header("UI References")]
        [SerializeField] CanvasGroup fadeGroup;
        [SerializeField] CanvasGroup subtitleGroup;
        [SerializeField] Text subtitleSpeaker;
        [SerializeField] Text subtitleText;
        [SerializeField] Text hotspotLabel;
        [SerializeField] Text roomTitle;
        [SerializeField] Transform inventoryStrip;
        [SerializeField] Button inventoryButtonTemplate;
        [SerializeField] GameObject debugPanel;
        [SerializeField] Text debugText;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideHotspotLabel();
            if (subtitleGroup != null)
                subtitleGroup.alpha = 0f;
            if (fadeGroup != null)
                fadeGroup.alpha = 0f;
            if (debugPanel != null)
                debugPanel.SetActive(false);
            if (inventoryButtonTemplate != null)
                inventoryButtonTemplate.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            if (BusinessNightGlobals.Instance != null)
                BusinessNightGlobals.Instance.InventoryChanged += RefreshInventory;
        }

        void OnDisable()
        {
            if (BusinessNightGlobals.Instance != null)
                BusinessNightGlobals.Instance.InventoryChanged -= RefreshInventory;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                ToggleDebug();

            if (debugPanel != null && debugPanel.activeSelf)
                RefreshDebugText();
        }

        public void SetRoomTitle(string value)
        {
            if (roomTitle != null)
                roomTitle.text = value;
        }

        public void ShowHotspotLabel(string label, Vector2 screenPosition)
        {
            if (hotspotLabel == null)
                return;

            hotspotLabel.text = label;
            hotspotLabel.gameObject.SetActive(true);
            hotspotLabel.transform.position = screenPosition + new Vector2(0f, 34f);
        }

        public void HideHotspotLabel()
        {
            if (hotspotLabel != null)
                hotspotLabel.gameObject.SetActive(false);
        }

        public IEnumerator ShowSubtitle(string speaker, string text)
        {
            if (subtitleGroup == null || subtitleText == null)
                yield break;

            subtitleSpeaker.text = speaker;
            subtitleText.text = string.Empty;
            subtitleText.fontSize = BusinessNightGlobals.Instance != null ? BusinessNightGlobals.Instance.settings.subtitleSize : 22;
            subtitleGroup.alpha = 1f;

            bool typewriter = BusinessNightGlobals.Instance == null || BusinessNightGlobals.Instance.settings.typewriterSubtitles;
            if (!typewriter)
            {
                subtitleText.text = text;
                yield break;
            }

            for (int i = 0; i <= text.Length; i++)
            {
                subtitleText.text = text.Substring(0, i);
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    subtitleText.text = text;
                    yield break;
                }

                yield return new WaitForSeconds(0.015f);
            }
        }

        public IEnumerator HideSubtitle()
        {
            if (subtitleGroup == null)
                yield break;

            float start = subtitleGroup.alpha;
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                subtitleGroup.alpha = Mathf.Lerp(start, 0f, t / 0.15f);
                yield return null;
            }

            subtitleGroup.alpha = 0f;
        }

        public IEnumerator FadeOut(float seconds)
        {
            yield return Fade(0f, 1f, seconds);
        }

        public IEnumerator FadeIn(float seconds)
        {
            yield return Fade(1f, 0f, seconds);
        }

        public void ForceClearFade()
        {
            if (fadeGroup == null)
                return;

            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        public void RefreshInventory()
        {
            if (inventoryStrip == null || inventoryButtonTemplate == null || BusinessNightInventory.Instance == null)
                return;

            foreach (Transform child in inventoryStrip)
            {
                if (child != inventoryButtonTemplate.transform)
                    Destroy(child.gameObject);
            }

            inventoryButtonTemplate.gameObject.SetActive(false);

            foreach (BusinessNightInventoryItem item in BusinessNightInventory.Instance.OwnedItems)
            {
                Button button = Instantiate(inventoryButtonTemplate, inventoryStrip);
                button.gameObject.SetActive(true);
                Text label = button.GetComponentInChildren<Text>();
                if (label != null)
                    label.text = item.displayName;

                string itemId = item.id;
                button.onClick.AddListener(() => BusinessNightInventory.Instance.Select(itemId));
            }
        }

        public void SetInventoryVisible(bool visible)
        {
            if (inventoryStrip != null)
                inventoryStrip.gameObject.SetActive(visible);
        }

        public void ToggleDebug()
        {
            if (debugPanel == null)
                return;

            debugPanel.SetActive(!debugPanel.activeSelf);
            RefreshDebugText();
        }

        IEnumerator Fade(float from, float to, float seconds)
        {
            if (fadeGroup == null)
                yield break;

            fadeGroup.blocksRaycasts = to > 0.5f;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                fadeGroup.alpha = Mathf.Lerp(from, to, t / seconds);
                yield return null;
            }

            fadeGroup.alpha = to;
            fadeGroup.blocksRaycasts = to > 0.5f;
        }

        void RefreshDebugText()
        {
            if (debugText == null || BusinessNightGlobals.Instance == null)
                return;

            debugText.text =
                "Debug\n" +
                $"Scene: {BusinessNightGlobals.Instance.currentScene}\n" +
                $"Selected Item: {BusinessNightInventory.Instance?.SelectedItemId}\n" +
                $"Flags: {string.Join(", ", BusinessNightGlobals.Instance.GetStoryFlags())}\n" +
                $"Items: {string.Join(", ", BusinessNightGlobals.Instance.GetItems())}";
        }
    }
}
