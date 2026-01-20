using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
    public class ConsumableItem : ItemData
    {
        [Header("Consumable Effects")]
        [SerializeField] private int m_HealthRestore = 0;
        [SerializeField] private int m_ManaRestore = 0;
        [SerializeField] private float m_EffectDuration = 0;
        [SerializeField] private StatModifier[] m_StatModifiers;

        [System.Serializable]
        public class StatModifier
        {
            public string m_StatName;
            public float m_Value;
            public bool m_IsPercentage;
        }

    }
}
