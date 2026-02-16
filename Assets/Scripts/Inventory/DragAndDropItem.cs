using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace QuestRoom
{
    public class DragAndDropItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public InventorySlot oldSlot;
        private Transform player;
        private bool isRightButton = false;
        private Coroutine holdCoroutine;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            oldSlot = transform.GetComponentInParent<InventorySlot>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (oldSlot.isEmpty || eventData.button != PointerEventData.InputButton.Left)
                return;
            GetComponent<RectTransform>().position += new Vector3(eventData.delta.x, eventData.delta.y);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (oldSlot.isEmpty) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Левая кнопка: начинаем перетаскивание
                GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.75f);
                GetComponentInChildren<Image>().raycastTarget = false;
                Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
                transform.SetParent(rootCanvas.transform);
                transform.SetAsLastSibling();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Правая кнопка: начинаем удержание
                isRightButton = true;
                if (holdCoroutine != null)
                    StopCoroutine(holdCoroutine);
                holdCoroutine = StartCoroutine(HoldRightClick());
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (oldSlot.isEmpty) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Левая кнопка отпущена – завершаем перетаскивание
                GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1f);
                GetComponentInChildren<Image>().raycastTarget = true;
                transform.SetParent(oldSlot.transform);
                transform.position = oldSlot.transform.position;

                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    if (eventData.pointerCurrentRaycast.gameObject.tag == "Panel")
                    {
                        // Выброс предмета
                        GameObject itemObject = Instantiate(oldSlot.item.itemPrefab,
                            player.position + Vector3.up + player.forward, Quaternion.identity);
                        itemObject.GetComponent<Item>().amount = oldSlot.amount;
                        NullifySlotData();
                    }
                    else if (eventData.pointerCurrentRaycast.gameObject.transform.parent.parent?.GetComponent<InventorySlot>() != null)
                    {
                        // Обмен/стакание
                        SlotsManager(eventData.pointerCurrentRaycast.gameObject.transform.parent.parent.GetComponent<InventorySlot>());
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Правая кнопка отпущена – останавливаем удержание
                isRightButton = false;
                if (holdCoroutine != null)
                {
                    StopCoroutine(holdCoroutine);
                    holdCoroutine = null;
                }
            }
        }

        private IEnumerator HoldRightClick()
        {
            // Небольшая задержка перед первым действием (как в Minecraft)
            yield return new WaitForSeconds(0.2f);

            // Выполняем действие один раз
            PerformRightClickAction();

            // Затем с интервалом
            float interval = 0.1f;
            while (isRightButton)
            {
                yield return new WaitForSeconds(interval);
                PerformRightClickAction();
            }
        }

        private void PerformRightClickAction()
        {
            // Получаем слот под мышью
            InventorySlot targetSlot = HeldItemManager.Instance?.GetSlotUnderMouse();
            if (targetSlot == null) return;

            HeldItemManager held = HeldItemManager.Instance;

            if (!held.HasItem)
            {
                // Рука пуста – пытаемся взять один предмет из слота
                held.TakeOneFromSlot(targetSlot);
            }
            else
            {
                // В руке есть предмет – пытаемся положить один в слот
                held.PutOneToSlot(targetSlot);
            }
        }

        public void NullifySlotData()
        {
            oldSlot.item = null;
            oldSlot.amount = 0;
            oldSlot.isEmpty = true;
            oldSlot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            oldSlot.iconGameObject.GetComponent<Image>().sprite = null;
            oldSlot.itemAmountText.text = "";
        }

        private void SlotsManager(InventorySlot newSlot)
        {
            if (newSlot == oldSlot) return;

            // Пустой слот – просто перемещаем
            if (newSlot.isEmpty)
            {
                newSlot.item = oldSlot.item;
                newSlot.amount = oldSlot.amount;
                newSlot.isEmpty = false;
                newSlot.SetIcon(oldSlot.iconGameObject.GetComponent<Image>().sprite);
                newSlot.itemAmountText.text = oldSlot.amount.ToString();

                oldSlot.item = null;
                oldSlot.amount = 0;
                oldSlot.isEmpty = true;
                oldSlot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                oldSlot.iconGameObject.GetComponent<Image>().sprite = null;
                oldSlot.itemAmountText.text = "";
                return;
            }

            // Одинаковые предметы – стакаем
            if (oldSlot.item != null && newSlot.item != null && oldSlot.item.itemID == newSlot.item.itemID)
            {
                int maxStack = oldSlot.item.maxAmount;
                int total = oldSlot.amount + newSlot.amount;

                if (total <= maxStack)
                {
                    newSlot.amount = total;
                    newSlot.itemAmountText.text = total.ToString();

                    oldSlot.item = null;
                    oldSlot.amount = 0;
                    oldSlot.isEmpty = true;
                    oldSlot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                    oldSlot.iconGameObject.GetComponent<Image>().sprite = null;
                    oldSlot.itemAmountText.text = "";
                }
                else
                {
                    newSlot.amount = maxStack;
                    newSlot.itemAmountText.text = maxStack.ToString();

                    oldSlot.amount = total - maxStack;
                    oldSlot.itemAmountText.text = oldSlot.amount.ToString();
                }
                return;
            }

            // Разные предметы – обмен
            ItemScriptableObject newItem = newSlot.item;
            int newAmount = newSlot.amount;
            bool newIsEmpty = newSlot.isEmpty;
            Sprite newIconSprite = newSlot.iconGameObject.GetComponent<Image>().sprite;
            string newAmountText = newSlot.itemAmountText.text;

            newSlot.item = oldSlot.item;
            newSlot.amount = oldSlot.amount;
            newSlot.isEmpty = oldSlot.isEmpty;
            if (!oldSlot.isEmpty)
            {
                newSlot.SetIcon(oldSlot.iconGameObject.GetComponent<Image>().sprite);
                newSlot.itemAmountText.text = oldSlot.amount.ToString();
            }
            else
            {
                newSlot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                newSlot.iconGameObject.GetComponent<Image>().sprite = null;
                newSlot.itemAmountText.text = "";
            }

            oldSlot.item = newItem;
            oldSlot.amount = newAmount;
            oldSlot.isEmpty = newIsEmpty;
            if (!newIsEmpty)
            {
                oldSlot.SetIcon(newIconSprite);
                oldSlot.itemAmountText.text = newAmountText;
            }
            else
            {
                oldSlot.iconGameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                oldSlot.iconGameObject.GetComponent<Image>().sprite = null;
                oldSlot.itemAmountText.text = "";
            }
        }
    }
}