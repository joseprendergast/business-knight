using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BusinessNight
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BusinessNightHotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Hotspot")]
        public string hotspotId = "prototype_hotspot";
        public string displayName = "Prototype Hotspot";
        [TextArea] public string inspectLine = "A neutral hotspot inspection line.";
        [TextArea] public string interactLine = "A neutral interaction result.";
        [TextArea] public string repeatedInspectLine = "There is nothing else to inspect here.";

        [Header("Requirements and Results")]
        public List<string> requiredFlags = new();
        public List<string> setFlags = new();
        public string collectItemId;
        public string requiredItemId;
        public string roomChangeSceneId;
        public BusinessNightTransitionType transition = BusinessNightTransitionType.FadeToBlack;

        [Header("Dialogue Beat")]
        public BusinessNightSubtitleLine dialogueBeat = new()
        {
            speaker = "System",
            text = "This placeholder subtitle proves the cinematic text path works.",
            flagOnFirstPlay = "m_seenOpeningBeat",
            playOnce = true
        };

        bool inspected;

        void OnMouseEnter()
        {
            BusinessNightUi.Instance?.ShowHotspotLabel(displayName, Input.mousePosition);
        }

        void OnMouseExit()
        {
            BusinessNightUi.Instance?.HideHotspotLabel();
        }

        void OnMouseOver()
        {
            BusinessNightUi.Instance?.ShowHotspotLabel(displayName, Input.mousePosition);
        }

        void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(1))
                Inspect();
            else
                ContextAction();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            BusinessNightUi.Instance?.ShowHotspotLabel(displayName, Input.mousePosition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            BusinessNightUi.Instance?.HideHotspotLabel();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                Inspect();
            else
                ContextAction();
        }

        public void ContextAction()
        {
            string selectedItem = BusinessNightInventory.Instance != null ? BusinessNightInventory.Instance.SelectedItemId : string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedItem))
            {
                UseItem(selectedItem);
                return;
            }

            Interact();
        }

        public void Inspect()
        {
            if (!CanUse())
                return;

            string line = inspected && !string.IsNullOrWhiteSpace(repeatedInspectLine) ? repeatedInspectLine : inspectLine;
            inspected = true;
            BusinessNightDialogue.Instance?.Say("Inspect", line);
        }

        public void Interact()
        {
            if (!CanUse())
                return;

            if (!string.IsNullOrWhiteSpace(requiredItemId))
            {
                if (BusinessNightGlobals.Instance != null && BusinessNightGlobals.Instance.HasItem(requiredItemId))
                {
                    UseItem(requiredItemId);
                    return;
                }

                BusinessNightDialogue.Instance?.Say("Ari", interactLine);
                return;
            }

            foreach (string flag in setFlags)
                BusinessNightGlobals.Instance?.SetFlag(flag);

            if (!string.IsNullOrWhiteSpace(collectItemId))
                BusinessNightInventory.Instance?.Collect(collectItemId);

            if (dialogueBeat != null && !string.IsNullOrWhiteSpace(dialogueBeat.text))
                BusinessNightDialogue.Instance?.Say(dialogueBeat);
            else
                BusinessNightDialogue.Instance?.Say("Interact", interactLine);

            if (!string.IsNullOrWhiteSpace(roomChangeSceneId))
                BusinessNightSceneManager.Instance?.ChangeScene(roomChangeSceneId, transition);
        }

        public void UseItem(string itemId)
        {
            if (!CanUse())
                return;

            if (itemId != requiredItemId)
            {
                BusinessNightDialogue.Instance?.Say("Use", "That does not seem useful here.");
                return;
            }

            BusinessNightGlobals.Instance?.SetFlag("m_unlockedSecondRoom");
            BusinessNightGlobals.Instance?.SetFlag("m_sceneOneComplete");
            BusinessNightInventory.Instance?.ClearSelection();
            BusinessNightDialogue.Instance?.Say("Ari", "The stamp clicks against the audit line. The door accepts the lie and opens.");

            if (!string.IsNullOrWhiteSpace(roomChangeSceneId))
                BusinessNightSceneManager.Instance?.ChangeScene(roomChangeSceneId, transition);
        }

        bool CanUse()
        {
            if (BusinessNightGlobals.Instance == null)
                return true;

            foreach (string flag in requiredFlags)
            {
                if (!BusinessNightGlobals.Instance.HasFlag(flag))
                {
                    BusinessNightDialogue.Instance?.Say("Locked", "Something else needs to happen first.");
                    return false;
                }
            }

            return true;
        }
    }
}
