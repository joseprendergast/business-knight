using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightRoom : MonoBehaviour
    {
        public BusinessNightSceneDefinition definition = new();
        [TextArea] public string openingSubtitle;
        public string openingSubtitleFlag;

        void Start()
        {
            if (BusinessNightGlobals.Instance != null)
                BusinessNightGlobals.Instance.VisitScene(definition.sceneId);

            BusinessNightUi.Instance?.SetRoomTitle(definition.displayName);

            if (!string.IsNullOrWhiteSpace(openingSubtitle))
                BusinessNightDialogue.Instance?.Say("Ari", openingSubtitle, openingSubtitleFlag, !string.IsNullOrWhiteSpace(openingSubtitleFlag), 3.2f);
        }
    }
}
