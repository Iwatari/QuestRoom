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
            if(heldItemObject.activeSelf)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    Input.mousePosition,
                    canvas.worldCamera,
                    out Vector2 localpoint);
                rectTransform.localPosition = localpoint;
            }
        }

        public bool HasItem => currentItem != null && currentAmount > 0;
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

        // Взять один предмет из слота
        public bool TakeOneFromSlot(InventorySlot slot)
        {
            if (slot == null || slot.isEmpty || slot.item == null) return false;

            // Если в руке уже есть предмет, проверяем совместимость
            if (HasItem)
            {
                if (currentItem.itemID != slot.item.itemID) return false; // разные предметы
                if (currentAmount >= currentItem.maxAmount) return false; // рука полна
            }

            if (slot.amount < 1) return false;

            // Уменьшаем слот
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

            // Добавляем в руку
            if (HasItem)
            {
                currentAmount += 1;
                UpdateVisuals();
            }
            else
            {
                SetItem(slot.item, 1);
            }
            return true;
        }

        // Положить один предмет в слот
        public bool PutOneToSlot(InventorySlot slot)
        {
            if (!HasItem)
            {
                Debug.Log("PutOneToSlot: нет предмета в руке");
                return false;
            }
            if (slot == null)
            {
                Debug.Log("PutOneToSlot: слот null");
                return false;
            }

            if (slot.isEmpty)
            {
                Debug.Log("PutOneToSlot: слот пустой, кладём предмет");
                slot.item = currentItem;
                slot.amount = 1;
                slot.isEmpty = false;
                slot.SetIcon(currentItem.icon);
                slot.itemAmountText.text = "1";
                AddAmount(-1);
                return true;
            }
            else
            {
                Debug.Log($"PutOneToSlot: слот не пустой, itemID слота={slot.item.itemID}, currentItemID={currentItem.itemID}, amount={slot.amount}, max={slot.item.maxAmount}");
                if (slot.item.itemID != currentItem.itemID)
                {
                    Debug.Log("PutOneToSlot: разные предметы");
                    return false;
                }
                if (slot.amount >= slot.item.maxAmount)
                {
                    Debug.Log("PutOneToSlot: слот полон");
                    return false;
                }
                slot.amount += 1;
                slot.itemAmountText.text = slot.amount.ToString();
                AddAmount(-1);
                Debug.Log("PutOneToSlot: успешно добавили");
                return true;
            }
        }

        // Получить слот под курсором (UI)
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
    }
}
