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

        private bool isDragging = false;
        private Coroutine holdTimer;
        private bool isRightClickHolding = false;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            oldSlot = transform.GetComponentInParent<InventorySlot>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || oldSlot.isEmpty || eventData.button != PointerEventData.InputButton.Left)
                return;
            GetComponent<RectTransform>().position += new Vector3(eventData.delta.x, eventData.delta.y);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (oldSlot.isEmpty) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Если в руке есть предмет – размещаем его в этом слоте
                if (HeldItemManager.Instance != null && HeldItemManager.Instance.HasItem)
                {
                    // Принудительно останавливаем удержание ПКМ (если активно)
                    StopRightClickHold();
                    HeldItemManager.Instance.PlaceAllInSlot(oldSlot);
                    return;
                }

                // Рука пуста – начинаем перетаскивание
                isDragging = true;
                GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.75f);
                GetComponentInChildren<Image>().raycastTarget = false;
                Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
                transform.SetParent(rootCanvas.transform);
                transform.SetAsLastSibling();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Правая кнопка – сразу берём один предмет из текущего слота
                HeldItemManager.Instance?.TakeOneFromSlot(oldSlot);

                // Запускаем таймер для обнаружения удержания
                if (holdTimer != null)
                    StopCoroutine(holdTimer);
                holdTimer = StartCoroutine(HoldTimer());
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (oldSlot.isEmpty) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!isDragging) return; // если не перетаскивали – игнорируем

                // Завершение перетаскивания
                GetComponentInChildren<Image>().color = new Color(1, 1, 1, 1f);
                GetComponentInChildren<Image>().raycastTarget = true;
                transform.SetParent(oldSlot.transform);
                transform.position = oldSlot.transform.position;
                isDragging = false;

                if (eventData.pointerCurrentRaycast.gameObject != null)
                {
                    if (eventData.pointerCurrentRaycast.gameObject.tag == "Panel")
                    {
                        // Выброс предмета из слота
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
                // Отпускаем правую кнопку – останавливаем удержание
                StopRightClickHold();
            }
        }

        private void StopRightClickHold()
        {
            isRightClickHolding = false;
            if (holdTimer != null)
            {
                StopCoroutine(holdTimer);
                holdTimer = null;
            }
        }

        private IEnumerator HoldTimer()
        {
            yield return new WaitForSeconds(0.2f); // начальная задержка

            // Если кнопка всё ещё зажата (проверка через Input)
            if (Input.GetKey(KeyCode.Mouse1))
            {
                isRightClickHolding = true;

                while (isRightClickHolding)
                {
                    // Дополнительная проверка: если кнопка не зажата, выходим
                    if (!Input.GetKey(KeyCode.Mouse1))
                        break;

                    InventorySlot currentSlot = HeldItemManager.Instance?.GetSlotUnderMouse();
                    if (currentSlot != null && !currentSlot.isEmpty)
                    {
                        bool success = HeldItemManager.Instance?.TakeOneFromSlot(currentSlot) ?? false;
                        if (!success)
                            break; // не удалось взять (рука полна, другой тип)
                        if (currentSlot.isEmpty)
                            break; // слот опустел – прекращаем, чтобы не брать из других автоматически
                    }
                    yield return new WaitForSeconds(0.15f); // интервал
                }
            }
            // Сбрасываем флаг (на всякий случай)
            isRightClickHolding = false;
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