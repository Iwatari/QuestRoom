using System.Collections.Generic;
using UnityEngine;

namespace QuestRoom
{
    public class InventoryManager : MonoBehaviour
    {
        public static bool IsOpened { get; private set; }

        public GameObject UIBackGround;
        public GameObject crossHair;
        public Transform InventoryPanel;
        public List<InventorySlot> slots = new List<InventorySlot>();
        public float reachDistance = 3f;
        private Camera mainCamera;
        private MouseLook mouseLook;

        public QuickslotInventory quickSlotInventory;

        private void Start()
        {
            mainCamera = Camera.main;
            for (int i = 0; i < InventoryPanel.childCount; i++)
            {
                if (InventoryPanel.GetChild(i).GetComponent<InventorySlot>() != null)
                {
                    slots.Add(InventoryPanel.GetChild(i).GetComponent<InventorySlot>());
                }
            }
            var controller = FindFirstObjectByType<MainCharacterController>();
            if (controller != null)
                mouseLook = controller.MouseLook;
            else
                Debug.LogError("MainCharacterController not found! MouseLook unavailable.");

            UIBackGround.SetActive(false);
            InventoryPanel.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                IsOpened = !IsOpened;

                if (IsOpened)
                {
                    UIBackGround.SetActive(true);
                    InventoryPanel.gameObject.SetActive(true);
                    crossHair.SetActive(false);
                    if (mouseLook != null)
                        mouseLook.SetCursorLock(false);
                    HeldItemManager.Instance?.Show();
                }
                else
                {
                    UIBackGround.SetActive(false);
                    InventoryPanel.gameObject.SetActive(false);
                    crossHair.SetActive(true);
                    if (mouseLook != null)
                        mouseLook.SetCursorLock(true);
                    HeldItemManager.Instance?.Hide();
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryPickupItem();
            }
        }

        private void TryPickupItem()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, reachDistance))
            {
                Item itemComp = hit.collider.GetComponent<Item>();
                if (itemComp != null)
                {
                    bool pickedUp = false;

                    // Сначала пробуем добавить в быстрый доступ
                    if (quickSlotInventory != null)
                    {
                        pickedUp = quickSlotInventory.TryAddItem(itemComp.item, itemComp.amount);
                    }

                    // Если не получилось — пробуем добавить в основной инвентарь
                    if (!pickedUp)
                    {
                        pickedUp = AddItem(itemComp.item, itemComp.amount);
                    }

                    // Уничтожаем объект только если успешно добавили
                    if (pickedUp)
                    {
                        Destroy(hit.collider.gameObject);
                    }
                    // иначе предмет остаётся в мире
                }
            }
        }

        // Возвращает true, если весь предмет успешно добавлен
        private bool AddItem(ItemScriptableObject _item, int _amount)
        {
            int remaining = _amount;

            // Сначала пытаемся добавить в существующие стаки
            foreach (InventorySlot slot in slots)
            {
                if (slot.item != null &&
                    slot.item.itemID == _item.itemID &&
                    slot.amount < _item.maxAmount)
                {
                    int space = _item.maxAmount - slot.amount;
                    int add = Mathf.Min(space, remaining);
                    slot.amount += add;
                    slot.itemAmountText.text = slot.amount.ToString();
                    remaining -= add;
                    if (remaining <= 0) return true;
                }
            }

            // Затем в пустые слоты
            foreach (InventorySlot slot in slots)
            {
                if (slot.isEmpty)
                {
                    slot.item = _item;
                    slot.amount = remaining;
                    slot.isEmpty = false;
                    slot.SetIcon(_item.icon);
                    slot.itemAmountText.text = remaining.ToString();
                    return true; // весь остаток поместился в один пустой слот
                }
            }

            // Если дошли сюда — места нет
            Debug.Log("Инвентарь полон! Предмет не подобран.");
            return false;
        }
    }
}