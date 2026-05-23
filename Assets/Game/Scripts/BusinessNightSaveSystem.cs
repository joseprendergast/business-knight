using System.IO;
using UnityEngine;

namespace BusinessNight
{
    public static class BusinessNightSaveSystem
    {
        public const string SaveFileName = "business-night-save.json";

        static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static bool HasSave => File.Exists(SavePath);

        public static void Save(BusinessNightGlobals globals)
        {
            if (globals == null)
                return;

            try
            {
                string json = JsonUtility.ToJson(globals.CaptureSaveData(), true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Save skipped: {exception.Message}");
            }
        }

        public static BusinessNightSaveData Load()
        {
            if (!HasSave)
                return null;

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<BusinessNightSaveData>(json);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Load skipped: {exception.Message}");
                return null;
            }
        }

        public static void Reset()
        {
            if (HasSave)
                File.Delete(SavePath);
        }
    }
}
