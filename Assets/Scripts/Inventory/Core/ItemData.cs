using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace InventorySystem
{
    public enum ItemType
    {
        Consumable,
        Weapon,
        Armor,
        Material,
        Quest,
        Key
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string m_ItemID;
        [SerializeField] private string m_DisplayName;
        [SerializeField][TextArea(3, 5)] private string m_Description;
        [SerializeField] private Sprite m_Icon;

        [Header("Item Properties")]
        [SerializeField] private ItemType m_ItemType = ItemType.Consumable;
        [SerializeField] private ItemRarity m_ItemRarity = ItemRarity.Common;
        [SerializeField] private int m_MaxStack = 1;
        [SerializeField] private float m_Weight = 0f;
        [SerializeField] private int m_BaseValue = 1;

        [Header("World Representation")]
        [SerializeField] private GameObject m_WorldPrefab;
        [SerializeField] private Vector3 m_HoldPosition;
        [SerializeField] private Vector3 m_HoldRotation;

        [Header("Audio")]
        [SerializeField] private AudioClip m_PickSound;
        [SerializeField] private AudioClip m_UseSound;

        public string ItemID => m_ItemID;
        public string DisplayName => m_DisplayName;
        public string Description => m_Description;
        public Sprite Icon => m_Icon;
        public ItemType ItemType => m_ItemType;
        public ItemRarity ItemRarity => m_ItemRarity;
        public int MaxStack => m_MaxStack;
        public float Weight => m_Weight;
        public int BaseValue => m_BaseValue;
        public GameObject WorldPrefab => m_WorldPrefab;
        public Vector3 HoldPosition => m_HoldPosition;
        public Vector3 HoldRotation => m_HoldRotation;
        public AudioClip PickSound => m_PickSound;
        public AudioClip UseSound => m_UseSound;

        // јвтоматическа€ генераци€ ID в случае если он был не задан
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(m_ItemID))
            {
                m_ItemID = GenerateItemID();
            }
        }

        private string GenerateItemID()
        {
            // √енераци€ уникального ID на основе имени и типа
            return $"item_{m_ItemType.ToString().ToLower()}_{m_DisplayName.ToLower().Replace(" ", "_")}";
        }

        public virtual bool CanUse() => false;
        public virtual void Use() => Debug.Log($"Using {m_DisplayName}");
        public virtual string GetTooltip()
        {
            string color = GetRarityColor();
            return $"<color={color}>{m_DisplayName}</color>\n" +
                   $"{m_Description}\n" +
                   $"<color=#AAAAAA>Type: {m_ItemType}</color>\n" +
                   $"<color=#AAAAAA>Weight: {m_Weight}kg</color>\n" +
                   $"<color=#AAAAAA>Value: {m_BaseValue} gold</color>";
        }

        private string GetRarityColor()
        {
            return m_ItemRarity switch
            {
                ItemRarity.Common => "#FFFFFF",
                ItemRarity.Uncommon => "#00FF00",
                ItemRarity.Rare => "#0070DD",
                ItemRarity.Epic => "#A335EE",
                ItemRarity.Legendary => "#FF8000",
                _ => "#FFFFFF"
            };
        }
    }
}
