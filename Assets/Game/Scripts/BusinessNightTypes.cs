using System;
using System.Collections.Generic;
using UnityEngine;

namespace BusinessNight
{
    public enum BusinessNightActionType
    {
        Context,
        Inspect,
        Talk,
        UseItem,
        Move
    }

    public enum BusinessNightTransitionType
    {
        HardCut,
        FadeToBlack,
        Dissolve
    }

    [Serializable]
    public sealed class BusinessNightPalette
    {
        public Color shadows = new Color32(8, 10, 14, 255);
        public Color midtones = new Color32(46, 54, 64, 255);
        public Color highlights = new Color32(202, 211, 220, 255);
        public Color dangerAccent = new Color32(219, 78, 74, 255);
        public Color uiText = new Color32(240, 242, 232, 255);
        public Color uiBorders = new Color32(126, 151, 165, 255);
        public Color disabledText = new Color32(111, 120, 128, 255);
    }

    [Serializable]
    public sealed class BusinessNightSubtitleLine
    {
        public string speaker = "Narrator";
        [TextArea] public string text = "Placeholder subtitle.";
        public string flagOnFirstPlay;
        public bool playOnce;
        public float holdSeconds = 2.2f;
    }

    [Serializable]
    public sealed class BusinessNightInventoryItem
    {
        public string id = "prototype_item";
        public string displayName = "Prototype Item";
        [TextArea] public string inspectLine = "A neutral placeholder item.";
        public bool hidden;
        public bool disabled;
    }

    [Serializable]
    public sealed class BusinessNightSceneDefinition
    {
        public string sceneId = "RoomPrototypeA";
        public string displayName = "Prototype Room A";
        [TextArea] public string description = "A placeholder room for framework testing.";
        public string backgroundArtReference = "Assets/Game/Art/Placeholder";
        public List<string> charactersPresent = new();
        public List<string> ambientEffects = new();
        public List<string> narrativeBeats = new();
        public List<string> requiredFlags = new();
        public string completionFlag;
        public bool debugJumpEnabled = true;
    }

    [Serializable]
    public sealed class BusinessNightSaveData
    {
        public int currentChapter = 1;
        public string currentScene = "RoomTitle";
        public List<string> visitedScenes = new();
        public List<string> storyFlags = new();
        public List<string> puzzleFlags = new();
        public List<string> collectedItems = new();
        public List<string> dialogueFlags = new();
        public BusinessNightSettingsData settings = new();
        public long savedAtUnixSeconds;
    }

    [Serializable]
    public sealed class BusinessNightSettingsData
    {
        public bool muted;
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float ambienceVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        public bool typewriterSubtitles = true;
        [Range(14, 36)] public int subtitleSize = 22;
        public bool crtFilter;
        public bool integerScaling = true;
    }
}
