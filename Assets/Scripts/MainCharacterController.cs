using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuestRoom
{
    [RequireComponent(typeof(CharacterController))]
    public class MainCharacterController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] public float m_WalkSpeed = 5;
        [SerializeField] public float m_RunSpeed = 10;
        [SerializeField] public bool m_IsWalking;
        [SerializeField] [Range(0f, 1f)] public float m_RunStepLenghten;
        [SerializeField] public float m_JumpSpeed;
        [SerializeField] public float m_StepInterval;
        [SerializeField] public float m_GravityMultiplier;
        [SerializeField] public MouseLook m_MouseLook;
        [SerializeField] public float m_StickToGroundForce;
        //  [SerializeField] public MouseLook m_MoseLook;

        public bool m_Jump;
        private bool m_Jumping;
        private bool m_PreviouslyGrounded;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private float m_StepCycle;
        private float m_NextStep;
        private Camera m_Camera;
        private Vector2 m_Input;
        private Vector3 m_OriginalCameraPosition;
        private Vector3 m_MoveDir = Vector3.zero;

        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;
            m_Jumping = false;
            m_MouseLook.Init(transform, m_Camera.transform);
        }

        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }

            if(!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }

            if(!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if(m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if(m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

                ProgressStepCycle(speed);
        }

        private void ProgressStepCycle(float speed)
        {
            if(m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunStepLenghten))) *
                    Time.fixedDeltaTime;
            }
            if(!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;
        }

        private void GetInput(out float speed)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);

            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            if(m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

           // if(m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
           // {
           //     
           // }
        }

        private void RotateView()
        {
           m_MouseLook.LookRotation(transform, m_Camera.transform);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;

            if(m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }
            if(body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
