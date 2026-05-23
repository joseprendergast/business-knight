using UnityEngine;
using UnityEngine.SceneManagement;

namespace BusinessNight
{
    public sealed class BusinessNightRoomInput : MonoBehaviour
    {
        [SerializeField] Camera roomCamera;
        [SerializeField] LayerMask hotspotMask = ~0;

        void Awake()
        {
            if (roomCamera == null)
                roomCamera = Camera.main;
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
                return;

            Vector3 world = roomCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(world.x, world.y);

            if (TryFirstRoomScreenHotspots())
                return;

            Collider2D hit = Physics2D.OverlapPoint(point, hotspotMask);

            if (hit != null && hit.TryGetComponent(out BusinessNightBattleHotspot battleHotspot))
            {
                battleHotspot.Trigger();
                return;
            }

            if (hit != null && hit.TryGetComponent(out BusinessNightHotspot hotspot))
            {
                if (Input.GetMouseButtonDown(1))
                    hotspot.Inspect();
                else
                    hotspot.ContextAction();
                return;
            }

            BusinessNightPlayer.Instance?.WalkTo(world.x);
        }

        bool TryFirstRoomScreenHotspots()
        {
            if (SceneManager.GetActiveScene().name != "RoomPrototypeA")
            {
                if (SceneManager.GetActiveScene().name == "RoomPrototypeB")
                {
                    Vector2 normalizedB = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                    if (normalizedB.x > 0.50f && normalizedB.x < 0.78f && normalizedB.y > 0.28f && normalizedB.y < 0.72f)
                    {
                        BusinessNightBattle.Instance?.StartBattle("Perry Audit");
                        return true;
                    }
                }

                return false;
            }

            Vector2 normalized = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

            if (normalized.x > 0.52f)
            {
                bool hasStamp = BusinessNightGlobals.Instance != null && BusinessNightGlobals.Instance.HasItem("prototype_item");
                TriggerHotspot(hasStamp ? "Hotspot_GlowingDoor" : "Hotspot_BlackStamp");
                return true;
            }

            return false;
        }

        void TriggerHotspot(string objectName)
        {
            GameObject hotspotObject = GameObject.Find(objectName);
            if (hotspotObject != null && hotspotObject.TryGetComponent(out BusinessNightHotspot hotspot))
            {
                if (Input.GetMouseButtonDown(1))
                    hotspot.Inspect();
                else
                    hotspot.ContextAction();
            }
        }
    }
}
