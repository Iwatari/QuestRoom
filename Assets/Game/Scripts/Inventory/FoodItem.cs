using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

namespace QuestRoom
{
    [CreateAssetMenu(fileName = "Food Item", menuName = " Inventory/Items/New Food Item")]
    public class FoodItem : ItemScriptableObject
    {
        public float healAmount;

        private void Start()
        {
            itemType = ItemType.Food;
        }

    }
}
