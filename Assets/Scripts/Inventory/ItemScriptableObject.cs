using UnityEngine;
using UnityEngine.UI;

namespace QuestRoom
{
    public enum ItemType
    {
        Default,
        Food,
        Weapon,
    }

    public class ItemScriptableObject : ScriptableObject
    {
        public string itemName;
        public int maxAmount;
        public GameObject itemPrefab;
        public Sprite icon;
        public ItemType itemType;
        public string itemDescription;
        public bool isConsumeable;
    }
}
