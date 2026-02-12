using System.Collections.Generic;
using NUnit.Framework;
using Unity.Cinemachine;
using UnityEngine;

namespace QuestRoom
{
    public class InventoryManager : MonoBehaviour
    {
        public static bool IsOpened { get; private set; }

        public GameObject UIPanel;
        public GameObject crossHair;
        public Transform InventoryPanel;
        public List<InventorySlot> slots = new List<InventorySlot>();
        public float reachDistance = 3f;
        private Camera mainCamera;
        private MouseLook mouseLook;

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

            UIPanel.SetActive(false);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                IsOpened = !IsOpened;

                if (IsOpened == true)
                {
                    UIPanel.SetActive(true);
                    crossHair.SetActive(false);
                      if (mouseLook != null)
                        mouseLook.SetCursorLock(false);
                }
                else
                {
                    UIPanel.SetActive(false);
                    crossHair.SetActive(true);
                    if (mouseLook != null)
                    {
                        mouseLook.SetCursorLock(true);
                    }
                }
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.Raycast(ray, out hit, reachDistance))
                {
                    if (hit.collider.gameObject.GetComponent<Item>() != null)
                    {
                        AddItem(hit.collider.gameObject.GetComponent<Item>().item, hit.collider.gameObject.GetComponent<Item>().amount);
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
        private void AddItem(ItemScriptableObject _item, int _amount)
        {
            foreach (InventorySlot slot in slots)
            {
                // Условие для заполнения инвентаря (разные предметы и не больше 64)
                if (slot.item == _item && slot.amount < 64)
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
                    break;
                }
            }
        }
    }
}
