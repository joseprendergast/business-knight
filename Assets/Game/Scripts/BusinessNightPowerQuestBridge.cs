using System;
using System.Reflection;
using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightPowerQuestBridge : MonoBehaviour
    {
        [Tooltip("Keep true. The bridge will bind by reflection when PowerQuest is imported, without making this framework fail to compile before import.")]
        [SerializeField] bool autoBind = true;

        public bool PowerQuestAvailable { get; private set; }

        void Awake()
        {
            if (autoBind)
                DetectPowerQuest();
        }

        public void DetectPowerQuest()
        {
            PowerQuestAvailable = Type.GetType("PowerTools.PowerQuest, Assembly-CSharp") != null
                || Type.GetType("PowerTools.PowerQuest, PowerQuest") != null;
        }

        public void RequestRoomChange(string roomName)
        {
            if (!PowerQuestAvailable)
            {
                BusinessNightSceneManager.Instance?.ChangeScene(roomName);
                return;
            }

            Type powerQuestType = Type.GetType("PowerTools.PowerQuest, Assembly-CSharp")
                ?? Type.GetType("PowerTools.PowerQuest, PowerQuest");

            object instance = powerQuestType?.GetProperty("Get", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            MethodInfo changeRoom = powerQuestType?.GetMethod("ChangeRoom", BindingFlags.Public | BindingFlags.Instance);
            changeRoom?.Invoke(instance, new object[] { roomName });
        }
    }
}
