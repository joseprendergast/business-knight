using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BusinessNight
{
    public sealed class BusinessNightInventory : MonoBehaviour
    {
        public static BusinessNightInventory Instance { get; private set; }

        [SerializeField] List<BusinessNightInventoryItem> catalog = new()
        {
            new BusinessNightInventoryItem
            {
                id = "prototype_item",
                displayName = "Black Stamp",
                inspectLine = "A compact authorization stamp from the Midnight Registry."
            }
        };

        public string SelectedItemId { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public IReadOnlyList<BusinessNightInventoryItem> Catalog => catalog;

        public IEnumerable<BusinessNightInventoryItem> OwnedItems
        {
            get
            {
                BusinessNightGlobals globals = BusinessNightGlobals.Instance;
                if (globals == null)
                    return Enumerable.Empty<BusinessNightInventoryItem>();

                return catalog.Where(item => globals.HasItem(item.id) && !item.hidden);
            }
        }

        public BusinessNightInventoryItem GetItem(string itemId)
        {
            return catalog.FirstOrDefault(item => item.id == itemId);
        }

        public void Collect(string itemId)
        {
            BusinessNightGlobals.Instance?.CollectItem(itemId);
        }

        public void Select(string itemId)
        {
            if (BusinessNightGlobals.Instance == null || !BusinessNightGlobals.Instance.HasItem(itemId))
                return;

            SelectedItemId = SelectedItemId == itemId ? string.Empty : itemId;
            BusinessNightUi.Instance?.RefreshInventory();
        }

        public void ClearSelection()
        {
            SelectedItemId = string.Empty;
            BusinessNightUi.Instance?.RefreshInventory();
        }
    }
}
