using UnityEngine;
using UnityEngine.UI;

namespace QuestRoom
{
    public class QuickslotInventory : MonoBehaviour
    {
        [Header("References")]
        public Transform quickslotParent;           // Родительский объект со слотами
        public Sprite selectedSprite;                // Спрайт для выбранного слота
        public Sprite notSelectedSprite;             // Спрайт для невыбранного слота

        [Header("State")]
        public int currentQuickslotID = 0;           // Текущий выбранный слот

        // Кэшированные массивы для быстрого доступа
        private InventorySlot[] slots;
        private Image[] slotImages;
        private int slotCount;

        private void Awake()
        {
            // Инициализация кэша
            if (quickslotParent == null)
            {
                Debug.LogError("QuickslotParent не назначен!");
                return;
            }

            slotCount = quickslotParent.childCount;
            slots = new InventorySlot[slotCount];
            slotImages = new Image[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                Transform child = quickslotParent.GetChild(i);
                slots[i] = child.GetComponent<InventorySlot>();
                slotImages[i] = child.GetComponent<Image>();

                if (slots[i] == null)
                    Debug.LogWarning($"Слот {i} не имеет компонента InventorySlot");
                if (slotImages[i] == null)
                    Debug.LogWarning($"Слот {i} не имеет компонента Image");
            }

            // Убедимся, что начальный слот отображается как выбранный
            UpdateSlotVisuals();
        }

        private void Update()
        {
            HandleScrollWheel();
            HandleNumberKeys();
            HandleUseItem();
        }

        public bool TryAddItem(ItemScriptableObject item, int amount)
        {
            if (item == null || amount <= 0) return false;

            for (int i = 0; i < slotCount; i++)
            {
                if (slots[i].item != null &&
                    slots[i].item.itemID == item.itemID &&
                    slots[i].amount < item.maxAmount)
                {
                    int space = item.maxAmount - slots[i].amount;
                    int add = Mathf.Min(space, amount);
                    slots[i].amount += add;
                    slots[i].itemAmountText.text = slots[i].amount.ToString();
                    // Убедимся, что слот не помечен как пустой
                    slots[i].isEmpty = false;
                    amount -= add;
                    if (amount <= 0) return true;
                }
            }

            for (int i = 0; i < slotCount; i++)
            {
                if (slots[i].isEmpty)
                {
                    slots[i].item = item;
                    slots[i].amount = amount;
                    slots[i].isEmpty = false;
                    slots[i].SetIcon(item.icon);
                    slots[i].itemAmountText.text = amount.ToString();
                    return true;
                }
            }

            return false;
        }
        /// <summary>Обработка колесика мыши</summary>
        private void HandleScrollWheel()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.1f) return;

            // Убираем выделение с текущего слота
            SetSlotSelected(currentQuickslotID, false);

            // Изменяем индекс
            if (scroll < 0) // Вперёд
            {
                currentQuickslotID = (currentQuickslotID + 1) % slotCount;
            }
            else // Назад
            {
                currentQuickslotID = (currentQuickslotID - 1 + slotCount) % slotCount;
            }

            // Выделяем новый слот
            SetSlotSelected(currentQuickslotID, true);
        }

        /// <summary>Обработка цифровых клавиш 1-9</summary>
        private void HandleNumberKeys()
        {
            for (int i = 0; i < slotCount && i < 9; i++) // Ограничим 9 слотами
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (currentQuickslotID == i)
                    {
                        // Переключение выделения на том же слоте
                        bool isSelected = slotImages[i].sprite == selectedSprite;
                        SetSlotSelected(i, !isSelected);
                    }
                    else
                    {
                        // Снимаем выделение с предыдущего, выделяем новый
                        SetSlotSelected(currentQuickslotID, false);
                        currentQuickslotID = i;
                        SetSlotSelected(currentQuickslotID, true);
                    }
                    break;
                }
            }
        }

        /// <summary>Использование предмета из выбранного слота (ЛКМ)</summary>
        private void HandleUseItem()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;
            if (InventoryManager.IsOpened) return; // Инвентарь открыт — не используем

            InventorySlot slot = slots[currentQuickslotID];
            if (slot == null || slot.item == null) return;
            if (!slot.item.isConsumeable) return;
            if (slotImages[currentQuickslotID].sprite != selectedSprite) return; // Слот не активен

            // Используем предмет
            if (slot.amount <= 1)
            {
                // Полностью очищаем слот
                DragAndDropItem drag = slot.GetComponentInChildren<DragAndDropItem>();
                if (drag != null)
                    drag.NullifySlotData();
                else
                    Debug.LogWarning("DragAndDropItem не найден в слоте");
            }
            else
            {
                slot.amount--;
                slot.itemAmountText.text = slot.amount.ToString();
            }

            // Здесь можно добавить эффект использования (здоровье, голод и т.д.)
        }

        /// <summary>Установить выделение слота (true/false)</summary>
        private void SetSlotSelected(int index, bool selected)
        {
            if (index < 0 || index >= slotCount) return;
            slotImages[index].sprite = selected ? selectedSprite : notSelectedSprite;
        }

        /// <summary>Обновить визуал всех слотов согласно текущему selected</summary>
        private void UpdateSlotVisuals()
        {
            for (int i = 0; i < slotCount; i++)
            {
                slotImages[i].sprite = (i == currentQuickslotID) ? selectedSprite : notSelectedSprite;
            }
        }
    }
}