using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace QuestRoom
{
    public class SphereArea : MonoBehaviour
    {
        [SerializeField] private float m_Radius;
        public float Radius => m_Radius;

        public Vector3 GetRandomInsideZone()
        {
            return transform.position + UnityEngine.Random.insideUnitSphere * m_Radius;
        }
#if UNITY_EDITOR


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);

            Gizmos.DrawSphere(transform.position, m_Radius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_Radius);
        }

#endif
    }
}
