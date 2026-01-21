using UnityEngine;
using System;

namespace Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public ItemData m_Item;
        public int m_Amount;
        public float m_Durability;
        public int m_SlotIndex;

        public InventoryItem(ItemData m_Item, int m_Amount, int m_SlotIndex)
        {
            this.m_Item = m_Item; 
            this.m_Amount = m_Amount;
            this.m_SlotIndex = m_SlotIndex;
            this.m_Durability = m_Item.m_Durability;
        }
        public bool m_IsFull => m_Amount >= m_Item.m_MaxStack;

        public void AddAmount(int value)
        {
            m_Amount += value;
        }
        
        public void ReduceAmount(int value)
        {
            m_Amount -= value;
        }

        public void UseDurability(float value)
        {
            m_Durability -= value;
        }
    }
}
