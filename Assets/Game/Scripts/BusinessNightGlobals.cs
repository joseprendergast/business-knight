using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightGlobals : MonoBehaviour
    {
        public static BusinessNightGlobals Instance { get; private set; }

        [Header("Core State")]
        public int currentChapter = 1;
        public string currentScene = "RoomTitle";
        public BusinessNightSettingsData settings = new();

        [Header("Placeholder Flags")]
        public bool m_gameStarted;
        public bool m_seenOpeningBeat;
        public bool m_collectedFirstItem;
        public bool m_unlockedSecondRoom;
        public bool m_talkedToFirstCharacter;
        public bool m_sceneOneComplete;

        readonly HashSet<string> visitedScenes = new();
        readonly HashSet<string> storyFlags = new();
        readonly HashSet<string> puzzleFlags = new();
        readonly HashSet<string> collectedItems = new();
        readonly HashSet<string> dialogueFlags = new();

        public event Action<string, bool> FlagChanged;
        public event Action InventoryChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SeedPlaceholderFlags();
        }

        public bool HasFlag(string flag)
        {
            if (string.IsNullOrWhiteSpace(flag))
                return true;

            return storyFlags.Contains(flag) || puzzleFlags.Contains(flag) || dialogueFlags.Contains(flag);
        }

        public void SetFlag(string flag, bool value = true, BusinessNightFlagBucket bucket = BusinessNightFlagBucket.Story)
        {
            if (string.IsNullOrWhiteSpace(flag))
                return;

            HashSet<string> target = bucket switch
            {
                BusinessNightFlagBucket.Puzzle => puzzleFlags,
                BusinessNightFlagBucket.Dialogue => dialogueFlags,
                _ => storyFlags
            };

            bool changed = value ? target.Add(flag) : target.Remove(flag);
            SyncPlaceholderFlag(flag, value);

            if (changed)
                FlagChanged?.Invoke(flag, value);
        }

        public void VisitScene(string sceneId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
                return;

            currentScene = sceneId;
            visitedScenes.Add(sceneId);
        }

        public bool HasVisited(string sceneId) => visitedScenes.Contains(sceneId);

        public void CollectItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return;

            if (collectedItems.Add(itemId))
            {
                if (itemId == "prototype_item")
                    m_collectedFirstItem = true;

                InventoryChanged?.Invoke();
            }
        }

        public bool HasItem(string itemId) => collectedItems.Contains(itemId);
        public IReadOnlyCollection<string> GetItems() => collectedItems;
        public IReadOnlyCollection<string> GetStoryFlags() => storyFlags;
        public IReadOnlyCollection<string> GetPuzzleFlags() => puzzleFlags;
        public IReadOnlyCollection<string> GetDialogueFlags() => dialogueFlags;

        public BusinessNightSaveData CaptureSaveData()
        {
            return new BusinessNightSaveData
            {
                currentChapter = currentChapter,
                currentScene = currentScene,
                visitedScenes = visitedScenes.ToList(),
                storyFlags = storyFlags.ToList(),
                puzzleFlags = puzzleFlags.ToList(),
                collectedItems = collectedItems.ToList(),
                dialogueFlags = dialogueFlags.ToList(),
                settings = settings,
                savedAtUnixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public void Restore(BusinessNightSaveData data)
        {
            if (data == null)
                return;

            currentChapter = data.currentChapter;
            currentScene = string.IsNullOrWhiteSpace(data.currentScene) ? "RoomTitle" : data.currentScene;
            settings = data.settings ?? new BusinessNightSettingsData();

            visitedScenes.Clear();
            storyFlags.Clear();
            puzzleFlags.Clear();
            collectedItems.Clear();
            dialogueFlags.Clear();

            foreach (string value in data.visitedScenes)
                visitedScenes.Add(value);
            foreach (string value in data.storyFlags)
                SetFlag(value, true, BusinessNightFlagBucket.Story);
            foreach (string value in data.puzzleFlags)
                SetFlag(value, true, BusinessNightFlagBucket.Puzzle);
            foreach (string value in data.dialogueFlags)
                SetFlag(value, true, BusinessNightFlagBucket.Dialogue);
            foreach (string value in data.collectedItems)
                collectedItems.Add(value);

            InventoryChanged?.Invoke();
        }

        public void ResetProgress()
        {
            currentChapter = 1;
            currentScene = "RoomTitle";
            visitedScenes.Clear();
            storyFlags.Clear();
            puzzleFlags.Clear();
            collectedItems.Clear();
            dialogueFlags.Clear();
            m_gameStarted = false;
            m_seenOpeningBeat = false;
            m_collectedFirstItem = false;
            m_unlockedSecondRoom = false;
            m_talkedToFirstCharacter = false;
            m_sceneOneComplete = false;
            SeedPlaceholderFlags();
            InventoryChanged?.Invoke();
        }

        void SeedPlaceholderFlags()
        {
            SyncPlaceholderFlag("m_gameStarted", m_gameStarted);
            SyncPlaceholderFlag("m_seenOpeningBeat", m_seenOpeningBeat);
            SyncPlaceholderFlag("m_collectedFirstItem", m_collectedFirstItem);
            SyncPlaceholderFlag("m_unlockedSecondRoom", m_unlockedSecondRoom);
            SyncPlaceholderFlag("m_talkedToFirstCharacter", m_talkedToFirstCharacter);
            SyncPlaceholderFlag("m_sceneOneComplete", m_sceneOneComplete);
        }

        void SyncPlaceholderFlag(string flag, bool value)
        {
            switch (flag)
            {
                case "m_gameStarted": m_gameStarted = value; break;
                case "m_seenOpeningBeat": m_seenOpeningBeat = value; break;
                case "m_collectedFirstItem": m_collectedFirstItem = value; break;
                case "m_unlockedSecondRoom": m_unlockedSecondRoom = value; break;
                case "m_talkedToFirstCharacter": m_talkedToFirstCharacter = value; break;
                case "m_sceneOneComplete": m_sceneOneComplete = value; break;
            }

            if (value)
                storyFlags.Add(flag);
            else
                storyFlags.Remove(flag);
        }
    }

    public enum BusinessNightFlagBucket
    {
        Story,
        Puzzle,
        Dialogue
    }
}
