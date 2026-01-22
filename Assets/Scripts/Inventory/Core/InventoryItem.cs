using UnityEngine;
using System;

namespace Inventory
{
    [Serializable]
    public class InventoryItem
    {
        public ItemObject item;
        public int amount;
        public float durability;
        public int slotIndex;

        public InventoryItem(ItemObject m_Item, int m_Amount, int m_SlotIndex)
        {
            this.item = m_Item; 
            this.amount = m_Amount;
            this.slotIndex = m_SlotIndex;
            this.durability = m_Item.durability;
        }
        public bool IsFull => amount >= item.maxStack;

        public void AddAmount(int value)
        {
            amount += value;
        }
        
        public void ReduceAmount(int value)
        {
            amount -= value;
        }

        public void UseDurability(float value)
        {
            durability -= value;
        }
    }
}
