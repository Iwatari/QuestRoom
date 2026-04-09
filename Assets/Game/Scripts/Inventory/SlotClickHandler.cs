using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuestRoom
{
    public class SlotClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private InventorySlot slot;

        private void Awake()
        {
            slot = GetComponent<InventorySlot>();
            if (slot == null)
                Debug.LogError("SlotClickHandler: InventorySlot not found on " + gameObject.name);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            HeldItemManager held = HeldItemManager.Instance;
            if (!held.HasItem) return;

            if (slot.isEmpty)
            {
                slot.item = held.CurrentItem;
                slot.amount = held.CurrentAmount;
                slot.isEmpty = false;
                slot.SetIcon(held.CurrentItem.icon);
                slot.itemAmountText.text = held.CurrentAmount.ToString();
                held.Clear();
            }
            else
            {
                // Непустой слот – проверяем совместимость
                Debug.Log($"SlotClick: itemID slot={slot.item?.itemID}, hand={held.CurrentItem?.itemID}");
                if (slot.item.itemID != held.CurrentItem.itemID) return;

                int maxStack = slot.item.maxAmount;
                int space = maxStack - slot.amount;
                if (space <= 0) return; // слот полон

                int transfer = Mathf.Min(space, held.CurrentAmount);
                slot.amount += transfer;
                slot.itemAmountText.text = slot.amount.ToString();
                held.AddAmount(-transfer); // уменьшаем руку
            }
        }
    }
}