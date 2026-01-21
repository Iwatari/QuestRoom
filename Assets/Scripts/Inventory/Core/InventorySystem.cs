using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using QuestRoom;
using System.Runtime.CompilerServices;

namespace Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int m_InventorySize = 36;
        [SerializeField] private int m_HotbarSize = 9;
        [SerializeField] private Transform m_DropPoint;

        [Header("UI References")]
        [SerializeField] private GameObject m_InventoryUI;
        [SerializeField] private GameObject m_HotbarUI;
        [SerializeField] private Transform m_SlotContainer;

        [Header("Audio")]
        [SerializeField] private AudioClip m_PickupSound;
        [SerializeField] private AudioClip m_DropSound;

        private List<InventoryItem> m_Inventory = new List<InventoryItem>();
        private int m_SelectedHotbarSlot = 0;
        private GameObject m_HeldItemObject;
        private bool m_IsInventoryOpen = false;

        public event Action<InventoryItem> m_OnItemAdded;
        public event Action<int> m_OnItemRemoved;
        public event Action<int> m_OnHotbarSelectionChanged;

        private MainCharacterController m_CharacterController;
        private MouseLook m_MouseLook;
        private AudioSource m_AudioSource;

        private void Awake()
        {
            m_CharacterController = GetComponent<MainCharacterController>();
            m_MouseLook = GetComponent<MouseLook>();
            m_AudioSource = GetComponent<AudioSource>();

            if(m_AudioSource == null)
               m_AudioSource= gameObject.AddComponent<AudioSource>();

            InitializeInventory();
        }

        private void Start()
        {
            m_InventoryUI.SetActive(false);
            UpdateHotbarSelection();
        }

        private void Update()
        {
            HandleInput();
           // UpdateHeldItem();
        }

        private void HandleInput()
        {
            if(Keyboard.current.tabKey.wasPressedThisFrame ||
                Keyboard.current.iKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }

            HandleHotbarSelection();

            if(Mouse.current.rightButton.wasPressedThisFrame && !m_IsInventoryOpen)
            {
              //  UseSelectedItem();
            }

            if(Keyboard.current.qKey.wasPressedThisFrame && !m_IsInventoryOpen)
            {
             //   DropSelectedItem();
            }
        }

        private void HandleHotbarSelection()
        {
            for (int i = 0; i < m_HotbarSize; i++)
            {
                if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                {
                    SetHotbarSlot(i);
                        return;
                }
            }

            float m_Scroll = Mouse.current.scroll.ReadValue().y;
            if (m_Scroll > 0)
            {
                SetHotbarSlot((m_SelectedHotbarSlot + 1) % m_HotbarSize);
            }
            else if (m_Scroll < 0)
            {
                SetHotbarSlot((m_SelectedHotbarSlot - 1 + m_HotbarSize) % m_HotbarSize);
            }
        }
         private void SetHotbarSlot(int m_SlotIndex)
        {
            if (m_SlotIndex < 0 || m_SlotIndex >= m_HotbarSize) return;

            m_SelectedHotbarSlot = m_SlotIndex;
            UpdateHotbarSelection();
            m_OnHotbarSelectionChanged?.Invoke(m_SlotIndex);
        }
        private void UpdateHotbarSelection()
        {

        }

        private void ToggleInventory()
        {
            m_IsInventoryOpen = !m_IsInventoryOpen;
            m_InventoryUI.SetActive(m_IsInventoryOpen);

            if(m_MouseLook != null)
            {
                m_MouseLook.IsActive = !m_IsInventoryOpen;

                if(m_IsInventoryOpen)
                {
                    m_MouseLook.SetExternalCursorControl(
                        enable: true,
                        lockMode: CursorLockMode.None,
                        visible: true
                        );
                }
                else
                {
                    m_MouseLook.SetExternalCursorControl(false, CursorLockMode.Locked, false);
                }
            }

            if(m_CharacterController != null)
            {
                m_CharacterController.enabled = !m_IsInventoryOpen;
            }
        }
        private void InitializeInventory()
        {
            for(int i = 0; i < m_InventorySize; i++)
            {
                m_Inventory.Add(null);
            }
        }
    }
}
