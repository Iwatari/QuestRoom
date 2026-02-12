using UnityEngine;

namespace QuestRoom
{
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] private string m_NickName;
        public string Nickname => m_NickName;
    }
}
