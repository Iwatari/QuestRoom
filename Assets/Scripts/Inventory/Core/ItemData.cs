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
        Weapom,
        Consumable,
        BuildingBlock
    }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Inforamtion")]
        public string m_ItemName;
        public string m_Description;
        public ItemType m_Type;

        [Header("Visuals")]
        public Sprite m_Icon;
        public GameObject m_Prefab;
        public Vector3 m_HoldPosition = new Vector3(0.5f, -0.5f, 1f);
        public Vector3 m_HoldRotation = Vector3.zero;

        [Header("Properties")]
        public int m_MaxStack = 100;
        public bool m_IsStackable = true;
        public float m_Weight = 0.1f;

        [Header("Usage")]
        public float m_Durability = 100f;
        public int m_Damage = 1;
        public float m_UseCooldown = 0.5f;

        [TextArea(3, 5)]
        public string m_OnUseEffect;
    }
}
