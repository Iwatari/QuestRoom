using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuestRoom
{
    [Serializable]
    public class MouseLook
    {
        [Header("Sensitivity Settings")]
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;

        [Header("Rotation Limits")]
        public bool clampVerticalRotation = true;
        public float MinimumX = -90f;
        public float MaximumX = 90f;

        [Header("Smoothing")]
        public bool smooth = true;
        public float smoothTime = 5f;

        [Header("Cursor Control")]
        public bool enableCursorControl = true;
        public CursorLockMode defaultLockMode = CursorLockMode.Locked;
        public bool cursorVisible = false;

        // Состояние
        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_IsActive = true;
        private bool m_ExternalCursorControl = false;
        private CursorLockMode m_ExternalLockMode;
        private bool m_ExternalCursorVisible;

        // События для управления извне
        public event Action<bool> OnActiveStateChanged;
        public event Action<CursorLockMode, bool> OnCursorStateChanged;

        public bool IsActive
        {
            get => m_IsActive;
            set
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    OnActiveStateChanged?.Invoke(value);
                }
            }
        }

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;

            // Инициализация курсора
            if (enableCursorControl && !m_ExternalCursorControl)
            {
                SetCursorState(defaultLockMode, cursorVisible);
            }
        }

        public void LookRotation(Transform character, Transform camera)
        {
            if (!m_IsActive) return;

            // Используем новый Input System
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float yRot = mouseDelta.x * XSensitivity * 0.1f;
            float xRot = mouseDelta.y * YSensitivity * 0.1f;

            // Альтернатива: для совместимости со старым Input
            // float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            // float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(
                    character.localRotation,
                    m_CharacterTargetRot,
                    smoothTime * Time.deltaTime
                );
                camera.localRotation = Quaternion.Slerp(
                    camera.localRotation,
                    m_CameraTargetRot,
                    smoothTime * Time.deltaTime
                );
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            // Внутреннее управление курсором (если не переопределено извне)
            if (enableCursorControl && !m_ExternalCursorControl)
            {
                UpdateInternalCursorControl();
            }
        }

        /// <summary>
        /// Позволяет другим системам взять управление курсором
        /// </summary>
        public void SetExternalCursorControl(bool enable, CursorLockMode lockMode, bool visible)
        {
            m_ExternalCursorControl = enable;

            if (enable)
            {
                // Сохраняем текущее состояние для восстановления
                m_ExternalLockMode = Cursor.lockState;
                m_ExternalCursorVisible = Cursor.visible;

                // Устанавливаем новое состояние
                Cursor.lockState = lockMode;
                Cursor.visible = visible;

                OnCursorStateChanged?.Invoke(lockMode, visible);
            }
            else
            {
                // Восстанавливаем внутреннее управление
                if (enableCursorControl)
                {
                    SetCursorState(defaultLockMode, cursorVisible);
                }
            }
        }

        /// <summary>
        /// Установить состояние курсора (для внутреннего использования)
        /// </summary>
        private void SetCursorState(CursorLockMode lockMode, bool visible)
        {
            Cursor.lockState = lockMode;
            Cursor.visible = visible;
            OnCursorStateChanged?.Invoke(lockMode, visible);
        }

        /// <summary>
        /// Внутренняя логика управления курсором (Esc для разблокировки, ЛКМ для блокировки)
        /// </summary>
        private void UpdateInternalCursorControl()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorState(CursorLockMode.None, true);
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorState(defaultLockMode, cursorVisible);
            }
        }

        /// <summary>
        /// Получить текущее состояние курсора
        /// </summary>
        public (CursorLockMode lockMode, bool visible) GetCursorState()
        {
            return (Cursor.lockState, Cursor.visible);
        }

        /// <summary>
        /// Сбросить вращение к начальным значениям
        /// </summary>
        public void ResetRotation(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }

        /// <summary>
        /// Ограничить вертикальное вращение
        /// </summary>
        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}