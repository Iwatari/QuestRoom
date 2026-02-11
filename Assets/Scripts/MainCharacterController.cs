using UnityEngine;
using UnityEngine.InputSystem; // Рекомендую использовать Input System

namespace QuestRoom
{
    [RequireComponent(typeof(CharacterController))]
    public class MainCharacterController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float m_WalkSpeed = 4f;
        [SerializeField] private float m_RunSpeed = 8f;
        [SerializeField] private float m_JumpHeight = 3f;
        [SerializeField] private float m_GravityMultiplier = 2f;

        [Header("Camera Settings")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private MouseLook m_MouseLook;
        public MouseLook MouseLook => m_MouseLook;

        [Header("Ground Check")]
        [SerializeField] private float m_GroundCheckDistance = 0.1f;
        [SerializeField] private LayerMask m_GroundLayerMask;

        // Свойства вместо публичных полей
        public bool IsWalking { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsJumping { get; private set; }

        private CharacterController m_CharacterController;
        private Vector3 m_Velocity;
        private float m_VerticalVelocity;
        private bool m_JumpRequested;
        private Transform m_CameraTransform;

        // Кэшированные значения
        private float m_OriginalStepOffset;
        private const float m_MaxFallVelocity = 53f;

        private void Awake()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_OriginalStepOffset = m_CharacterController.stepOffset;

            if (m_Camera == null)
                m_Camera = Camera.main;

            m_CameraTransform = m_Camera.transform;
            m_MouseLook?.Init(transform, m_CameraTransform);
            m_MouseLook?.ForceLockCursor();
            m_MouseLook?.SetCursorLock(true);
        }

        private void Update()
        {
            HandleInput();
            RotateView();
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        private void HandleInput()
        {
            // Использовать Input System вместо старого Input
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                m_JumpRequested = true;
            }
        }

        private void HandleMovement()
        {
            UpdateGroundStatus();
            ApplyGravity();
            HandleJump();
            ApplyMovement();
        }

        private void UpdateGroundStatus()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGrounded();

            if (IsGrounded && !wasGrounded)
            {
                m_VerticalVelocity = -2f; // Небольшая сила приземления
                IsJumping = false;
            }

            // Восстанавливаем stepOffset при приземлении
            m_CharacterController.stepOffset = IsGrounded ? m_OriginalStepOffset : 0f;
        }

        private bool CheckGrounded()
        {
            // Более эффективная проверка земли
            if (m_CharacterController.isGrounded)
                return true;

            return Physics.Raycast(
                transform.position + Vector3.up * 0.1f,
                Vector3.down,
                m_GroundCheckDistance + 0.1f,
                m_GroundLayerMask
            );
        }

        private void ApplyGravity()
        {
            if (IsGrounded && m_VerticalVelocity < 0)
            {
                m_VerticalVelocity = -2f; // Небольшая постоянная сила вниз
            }
            else
            {
                m_VerticalVelocity += Physics.gravity.y * m_GravityMultiplier * Time.fixedDeltaTime;
                m_VerticalVelocity = Mathf.Max(m_VerticalVelocity, -m_MaxFallVelocity);
            }
        }

        private void HandleJump()
        {
            if (m_JumpRequested && IsGrounded)
            {
                // Физически корректная формула прыжка: v = √(2 * g * h)
                m_VerticalVelocity = Mathf.Sqrt(m_JumpHeight * -2f * Physics.gravity.y);
                IsJumping = true;
                m_JumpRequested = false;
            }
        }

        private void ApplyMovement()
        {
            Vector2 input = GetMovementInput();
            bool wantsToRun = Keyboard.current.leftShiftKey.isPressed;

            float speed = wantsToRun ? m_RunSpeed : m_WalkSpeed;
            IsWalking = !wantsToRun;

            Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;

            m_Velocity = new Vector3(move.x, m_VerticalVelocity, move.z);
            m_CharacterController.Move(m_Velocity * Time.fixedDeltaTime);
        }

        private Vector2 GetMovementInput()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;

            return Vector2.ClampMagnitude(input, 1f);
        }

        private void RotateView()
        {
            if (!InventoryManager.IsInventoryOpen) 
            {
                m_MouseLook?.LookRotation(transform, m_CameraTransform);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Визуализация проверки земли для отладки
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 0.1f,
                transform.position + Vector3.up * 0.1f + Vector3.down * (m_GroundCheckDistance + 0.1f)
            );
        }
#endif
    }
}