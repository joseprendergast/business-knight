using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightBattleHotspot : MonoBehaviour
    {
        [SerializeField] string opponentName = "Perry Audit";

        void OnMouseEnter()
        {
            BusinessNightUi.Instance?.ShowHotspotLabel(opponentName, Input.mousePosition);
        }

        void OnMouseExit()
        {
            BusinessNightUi.Instance?.HideHotspotLabel();
        }

        void OnMouseDown()
        {
            BusinessNightBattle.Instance?.StartBattle(opponentName);
        }

        public void Trigger()
        {
            BusinessNightBattle.Instance?.StartBattle(opponentName);
        }
    }
}
