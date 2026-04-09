using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace QuestRoom
{
    public class HeldItemManager : MonoBehaviour
    {
        public static HeldItemManager Instance { get; private set; }

        [Header("UI References")]
        public GameObject heldItemObject;
        public Image iconImage;
        public TextMeshProUGUI amountText;

        private ItemScriptableObject currentItem;
        private int currentAmount;
        private RectTransform rectTransform;
        private Canvas canvas;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            rectTransform = heldItemObject.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            heldItemObject.SetActive(false);
        }

        private void Update()
        {
            if (heldItemObject.activeSelf)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out Vector2 localPoint);
                rectTransform.localPosition = localPoint;
            }
        }

        public bool HasItem => currentItem != null && currentAmount > 0;
        public ItemScriptableObject CurrentItem => currentItem;
        public int CurrentAmount => currentAmount;

        public void SetItem(ItemScriptableObject item, int amount)
        {
            if (item == null || amount <= 0)
            {
                Clear();
                return;
            }
            currentItem = item;
            currentAmount = amount;
            UpdateVisuals();
            heldItemObject.SetActive(true);
        }

        public void Clear()
        {
            currentItem = null;
            currentAmount = 0;
            heldItemObject.SetActive(false);
        }

        public void AddAmount(int delta)
        {
            if (currentItem == null) return;
            currentAmount += delta;
            if (currentAmount <= 0)
                Clear();
            else
                UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            iconImage.sprite = currentItem.icon;
            amountText.text = currentAmount.ToString();
        }

        public void Show()
        {
            if (HasItem)
                heldItemObject.SetActive(true);
        }

        public void Hide()
        {
            heldItemObject.SetActive(false);
        }

        // Взять один предмет из слота (возвращает true, если успешно)
        public bool TakeOneFromSlot(InventorySlot slot)
        {
            if (slot == null || slot.isEmpty || slot.item == null) return false;
            if (HasItem && currentItem.itemID != slot.item.itemID) return false;
            if (HasItem && currentItem.itemID == slot.item.itemID && currentAmount >= currentItem.maxAmount) return false;

            // Сохраняем предмет до изменения слота
            ItemScriptableObject itemTaken = slot.item;

            slot.amount -= 1;
            if (slot.amount <= 0)
            {
                slot.isEmpty = true;
                slot.item = null;
                slot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                slot.iconGameObject.GetComponent<Image>().sprite = null;
                slot.itemAmountText.text = "";
            }
            else
            {
                slot.itemAmountText.text = slot.amount.ToString();
            }

            if (HasItem)
            {
                currentAmount += 1;
                UpdateVisuals();
            }
            else
            {
                SetItem(itemTaken, 1); // используем сохранённый предмет
            }
            return true;
        }

        // Получить слот под курсором
        public InventorySlot GetSlotUnderMouse()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                InventorySlot slot = result.gameObject.GetComponentInParent<InventorySlot>();
                if (slot != null)
                    return slot;
            }
            return null;
        }
        public void PlaceAllInSlot(InventorySlot targetSlot)
        {
            if (!HasItem || targetSlot == null) return;

            if (targetSlot.isEmpty)
            {
                targetSlot.item = currentItem;
                targetSlot.amount = currentAmount;
                targetSlot.isEmpty = false;
                targetSlot.SetIcon(currentItem.icon);
                targetSlot.itemAmountText.text = currentAmount.ToString();
                Clear();
            }
            else
            {
                if (targetSlot.item.itemID != currentItem.itemID) return;

                int maxStack = targetSlot.item.maxAmount;
                int space = maxStack - targetSlot.amount;
                if (space <= 0) return;

                int transfer = Mathf.Min(space, currentAmount);
                targetSlot.amount += transfer;
                targetSlot.itemAmountText.text = targetSlot.amount.ToString();
                AddAmount(-transfer);
            }
        }
    }
}