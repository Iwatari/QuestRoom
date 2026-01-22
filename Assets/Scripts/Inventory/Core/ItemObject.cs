using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Inventory
{
    public enum ItemType
    {
        Material,
        Tool,
        Weapon,
        Consumable,
        BuildingBlock
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemObject : ScriptableObject
    {
        [Header("Basic Inforamtion")]
        public string itemName;
        public string description;
        public ItemType type;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
        public Vector3 holdPosition = new Vector3(0.5f, -0.5f, 1f);
        public Vector3 holdRotation = Vector3.zero;

        [Header("Properties")]
        public int maxStack = 100;
        public bool isStackable = true;
        public float weight = 0.1f;

        [Header("Usage")]
        public float durability = 100f;
        public int damage = 1;
        public float useCooldown = 0.5f;

        [TextArea(3, 5)]
        public string onUseEffect;
    }
}
