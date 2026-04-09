using UnityEngine;
using UnityEngine.EventSystems;

namespace QuestRoom
{
    public class PanelClickHandler : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            HeldItemManager held = HeldItemManager.Instance;
            if (!held.HasItem) return;

            Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;

            // Создаём объект с ВСЕМ количеством из руки
            GameObject itemObject = Instantiate(held.CurrentItem.itemPrefab,
                player.position + Vector3.up + player.forward, Quaternion.identity);
            itemObject.GetComponent<Item>().amount = held.CurrentAmount;

            // Полностью очищаем руку
            held.Clear();
        }
    }
}