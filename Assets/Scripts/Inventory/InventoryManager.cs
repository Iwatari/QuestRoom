using System.Collections.Generic;
using NUnit.Framework;
using Unity.Cinemachine;
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

                if (IsOpened == true)
                {
                    UIBackGround.SetActive(true);
                    InventoryPanel.gameObject.SetActive(true);
                    crossHair.SetActive(false);
                    if (mouseLook != null)
                        mouseLook.SetCursorLock(false);
                }
                else
                {
                    UIBackGround.SetActive(false);
                    InventoryPanel.gameObject.SetActive(false);
                    crossHair.SetActive(true);
                    if (mouseLook != null)
                    {
                        mouseLook.SetCursorLock(true);
                    }
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
                    // Сначала пробуем добавить в быстрый доступ
                    bool addedToQuick = false;
                    if (quickSlotInventory != null)
                    {
                        addedToQuick = quickSlotInventory.TryAddItem(itemComp.item, itemComp.amount);
                    }

                    // Если не получилось — добавляем в основной инвентарь
                    if (!addedToQuick)
                    {
                        AddItem(itemComp.item, itemComp.amount);
                    }

                    // Уничтожаем объект в любом случае
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        private void AddItem(ItemScriptableObject _item, int _amount)
        {
            foreach (InventorySlot slot in slots)
            {
                // Условие для заполнения инвентаря (разные предметы и не больше maxAmount)
                if (slot.item != null && 
                    slot.item.itemID == _item.itemID && 
                    slot.amount < _item.maxAmount)
                {
                    slot.amount += _amount;
                    slot.itemAmountText.text = slot.amount.ToString();
                    return;
                }
            }
            foreach (InventorySlot slot in slots)
            {
                if (slot.isEmpty == true)
                {
                    slot.item = _item;
                    slot.amount = _amount;
                    slot.isEmpty = false;
                    slot.SetIcon(_item.icon);
                    slot.itemAmountText.text = _amount.ToString();
                    return;
                }
            }
            Debug.Log("Инвентарь полон!");
        }
    }
}
