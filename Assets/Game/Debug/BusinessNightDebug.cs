using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightDebug : MonoBehaviour
    {
        [SerializeField] bool enableInProduction;

        void Update()
        {
            if (!Debug.isDebugBuild && !enableInProduction)
                return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                BusinessNightSceneManager.Instance?.ChangeScene("RoomTitle", BusinessNightTransitionType.HardCut);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                BusinessNightSceneManager.Instance?.ChangeScene("RoomPrototypeA", BusinessNightTransitionType.HardCut);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                BusinessNightSceneManager.Instance?.ChangeScene("RoomPrototypeB", BusinessNightTransitionType.HardCut);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                BusinessNightSceneManager.Instance?.ChangeScene("RoomPrototypeC", BusinessNightTransitionType.HardCut);
            if (Input.GetKeyDown(KeyCode.G))
                BusinessNightInventory.Instance?.Collect("prototype_item");
            if (Input.GetKeyDown(KeyCode.R))
            {
                BusinessNightSaveSystem.Reset();
                BusinessNightGlobals.Instance?.ResetProgress();
            }
        }
    }
}
