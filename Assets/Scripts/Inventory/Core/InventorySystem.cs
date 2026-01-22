using System;
using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuestRoom.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int inventorySize = 36;
        [SerializeField] private int hotbarSize = 9;
        [SerializeField] private Transform dropPoint;

        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private GameObject hotbarUI;
        [SerializeField] private Transform slotsContainer;

        [Header("Audio")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropSound;

        private List<InventoryItem> inventory = new List<InventoryItem>();
        private int selectedHotbarSlot = 0;
        private GameObject heldItemObject;
        private bool isInventoryOpen = false;

        // События для UI
        public event Action<InventoryItem> OnItemAdded;
        public event Action<int> OnItemRemoved;
        public event Action<int> OnHotbarSelectionChanged;
        public event Action<bool> OnInventoryToggled; // Новое событие

        private MainCharacterController characterController;
        private MouseLook mouseLook;
        private AudioSource audioSource;

        private void Awake()
        {
            characterController = GetComponent<MainCharacterController>();
            mouseLook = GetComponent<MouseLook>();
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            // Подписываемся на события MouseLook
            if (mouseLook != null)
            {
                mouseLook.OnActiveStateChanged += OnMouseLookActiveChanged;
                mouseLook.OnCursorStateChanged += OnMouseLookCursorChanged;
            }

            InitializeInventory();
        }

        private void Start()
        {
            inventoryUI.SetActive(false);
            UpdateHotbarSelection();
        }

        private void Update()
        {
            HandleInput();
            UpdateHeldItem();
        }

        private void HandleInput()
        {
            // Открытие/закрытие инвентаря (Tab или I)
            if (Keyboard.current.tabKey.wasPressedThisFrame ||
                Keyboard.current.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }

            // Выбор слота горячей панели (1-9 или колесо мыши)
            HandleHotbarSelection();

            // Использование предмета (ПКМ)
            if (Mouse.current.rightButton.wasPressedThisFrame && !isInventoryOpen)
            {
                UseSelectedItem();
            }

            // Выбросить предмет (Q)
            if (Keyboard.current.qKey.wasPressedThisFrame && !isInventoryOpen)
            {
                DropSelectedItem();
            }
        }

        private void HandleHotbarSelection()
        {
            // Цифровые клавиши 1-9
            for (int i = 0; i < hotbarSize; i++)
            {
                if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                {
                    SetHotbarSlot(i);
                    return;
                }
            }

            // Колесо мыши
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0)
            {
                if (scroll > 0)
                {
                    SetHotbarSlot((selectedHotbarSlot - 1 + hotbarSize) % hotbarSize);
                }
                else if (scroll < 0)
                {
                    SetHotbarSlot((selectedHotbarSlot + 1) % hotbarSize);
                }
            }
        }

        private void SetHotbarSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= hotbarSize) return;

            selectedHotbarSlot = slotIndex;
            UpdateHotbarSelection();
            OnHotbarSelectionChanged?.Invoke(slotIndex);
        }

        private void UpdateHotbarSelection()
        {
            // Визуальное обновление выбранного слота
            // Этот метод должен вызывать UI для обновления
        }

        private void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryUI.SetActive(isInventoryOpen);

            // Управление состоянием MouseLook
            if (mouseLook != null)
            {
                // Отключаем/включаем вращение камеры
                mouseLook.IsActive = !isInventoryOpen;

                // Управляем курсором
                if (isInventoryOpen)
                {
                    // Открываем инвентарь - показываем курсор
                    mouseLook.SetCursorState(CursorLockMode.None, true);
                }
                else
                {
                    // Закрываем инвентарь - возвращаем курсор в состояние по умолчанию
                    mouseLook.ResetCursorState();
                }
            }

            // Отключаем/включаем управление персонажем
            if (characterController != null)
            {
                characterController.enabled = !isInventoryOpen;
            }

            // Вызываем событие
            OnInventoryToggled?.Invoke(isInventoryOpen);
        }

        // Обработчики событий MouseLook
        private void OnMouseLookActiveChanged(bool isActive)
        {
            Debug.Log($"MouseLook активность изменена: {isActive}");
        }

        private void OnMouseLookCursorChanged(CursorLockMode lockMode, bool visible)
        {
            Debug.Log($"Курсор изменен: LockMode = {lockMode}, Visible = {visible}");
        }

        private void InitializeInventory()
        {
            for (int i = 0; i < inventorySize; i++)
            {
                inventory.Add(null);
            }
        }

        public bool AddItem(ItemObject item, int amount = 1)
        {
            // Сначала пробуем добавить в существующие стеки
            if (item.isStackable)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i] != null &&
                        inventory[i].item == item &&
                        !inventory[i].IsFull)
                    {
                        int spaceLeft = inventory[i].item.maxStack - inventory[i].amount;
                        int toAdd = Mathf.Min(amount, spaceLeft);

                        inventory[i].AddAmount(toAdd);
                        amount -= toAdd;

                        OnItemAdded?.Invoke(inventory[i]);

                        if (amount <= 0) return true;
                    }
                }
            }

            // Затем ищем пустые слоты
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == null)
                {
                    int toAdd = Mathf.Min(amount, item.maxStack);
                    inventory[i] = new InventoryItem(item, toAdd, i);
                    amount -= toAdd;

                    OnItemAdded?.Invoke(inventory[i]);

                    if (amount <= 0) return true;
                }
            }

            // Если не хватило места, возвращаем false
            return amount <= 0;
        }

        public void RemoveItem(int slotIndex, int amount = 1)
        {
            if (slotIndex < 0 || slotIndex >= inventory.Count) return;
            if (inventory[slotIndex] == null) return;

            inventory[slotIndex].ReduceAmount(amount);

            if (inventory[slotIndex].amount <= 0)
            {
                inventory[slotIndex] = null;
            }

            OnItemRemoved?.Invoke(slotIndex);
        }

        private void UseSelectedItem()
        {
            int slotIndex = selectedHotbarSlot;
            if (inventory[slotIndex] == null) return;

            InventoryItem item = inventory[slotIndex];

            // Использование расходуемого предмета
            if (item.item.type == ItemType.Consumable)
            {
                // Здесь можно добавить эффекты (здоровье, сытость и т.д.)
                Debug.Log($"Using {item.item.itemName}: {item.item.onUseEffect}");

                RemoveItem(slotIndex, 1);
            }
            // Для инструментов/оружия уменьшаем прочность
            else if (item.item.type == ItemType.Tool || item.item.type == ItemType.Weapon)
            {
                item.UseDurability(1);

                if (item.durability <= 0)
                {
                    RemoveItem(slotIndex, 1);
                }
            }
        }

        private void DropSelectedItem()
        {
            int slotIndex = selectedHotbarSlot;
            if (inventory[slotIndex] == null) return;

            InventoryItem item = inventory[slotIndex];

            if (item.item.prefab != null && dropPoint != null)
            {
                GameObject droppedItem = Instantiate(
                    item.item.prefab,
                    dropPoint.position,
                    Quaternion.identity
                );

                // Добавляем физику
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb == null) rb = droppedItem.AddComponent<Rigidbody>();

                // Бросаем вперед
                Vector3 throwDirection = transform.forward + Vector3.up * 0.2f;
                rb.AddForce(throwDirection * 5f, ForceMode.Impulse);

                // Проигрываем звук
                if (dropSound != null)
                {
                    audioSource.PlayOneShot(dropSound);
                }
            }

            RemoveItem(slotIndex, 1);
        }

        private void UpdateHeldItem()
        {
            int slotIndex = selectedHotbarSlot;
            if (inventory[slotIndex] == null)
            {
                if (heldItemObject != null)
                {
                    Destroy(heldItemObject);
                    heldItemObject = null;
                }
                return;
            }

            // Обновляем предмет в руке
            if (heldItemObject == null && inventory[slotIndex].item.prefab != null)
            {
                heldItemObject = Instantiate(
                    inventory[slotIndex].item.prefab,
                    transform
                );

                heldItemObject.transform.localPosition = inventory[slotIndex].item.holdPosition;
                heldItemObject.transform.localEulerAngles = inventory[slotIndex].item.holdRotation;

                // Отключаем физику у предмета в руке
                Rigidbody rb = heldItemObject.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider col = heldItemObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        public void PickupItem(ItemObject item, int amount = 1)
        {
            bool success = AddItem(item, amount);

            if (success && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
        }

        private void OnDestroy()
        {
            // Отписываемся от событий MouseLook
            if (mouseLook != null)
            {
                mouseLook.OnActiveStateChanged -= OnMouseLookActiveChanged;
                mouseLook.OnCursorStateChanged -= OnMouseLookCursorChanged;
            }
        }

        public List<InventoryItem> GetInventory() => inventory;
        public int GetSelectedHotbarSlot() => selectedHotbarSlot;
        public InventoryItem GetSelectedItem() =>
            inventory[selectedHotbarSlot];
        public bool IsInventoryOpen() => isInventoryOpen;
    }
}