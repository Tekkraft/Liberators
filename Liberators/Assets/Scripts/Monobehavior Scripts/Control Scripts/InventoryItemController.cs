using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    ItemInstance linkedItem;
    Vector2Int homePosition;
    Transform originalParent;
    Transform mainCanvas;
    bool slotted;
    bool droppedInSlot = false;
    bool itemDisabled = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeItem(ItemInstance linkedItem, Vector2Int homePosition, Transform originalParent, Transform mainCanvas)
    {
        this.linkedItem = linkedItem;
        GetComponent<Image>().sprite = this.linkedItem.GetInstanceSprite();
        this.homePosition = homePosition;
        this.originalParent = originalParent;
        this.mainCanvas = mainCanvas;
        transform.localPosition = CalculateFromHomePosition();
        slotted = false;
    }

    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        droppedInSlot = false;
        transform.SetParent(mainCanvas);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData pointerEventData)
    {
        transform.position = pointerEventData.position;
    }

    public void OnEndDrag(PointerEventData pointerEventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        if (!droppedInSlot)
        {
            slotted = false;
        }
        ResetPosition();
    }

    public ItemInstance GetLinkedItem()
    {
        return linkedItem;
    }

    public void ResetPosition()
    {
        if (slotted)
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0.5f);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition = new Vector2(1f, 1f);
            transform.SetParent(originalParent);
            transform.localPosition = CalculateFromHomePosition();
        }
    }

    public void SetSlotted(bool slotted)
    {
        this.slotted = slotted;
    }

    public void SetDroppedInSlot(bool droppedInSlot)
    {
        this.droppedInSlot = droppedInSlot;
    }

    public void DisableItem()
    {
        if (!itemDisabled)
        {
            itemDisabled = true;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            GetComponent<Image>().color = Color.gray;
        }
    }

    public void EnableItem()
    {
        if (itemDisabled)
        {
            itemDisabled = false;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            GetComponent<Image>().color = Color.white;
        }
    }

    //Index from 0
    public Vector3 CalculateFromHomePosition()
    {
        float posX = homePosition.x * (GetComponent<RectTransform>().sizeDelta.x + (7 * (GetComponent<RectTransform>().sizeDelta.x / 32))) + GetComponent<RectTransform>().sizeDelta.x / 2 + (7 * (GetComponent<RectTransform>().sizeDelta.x / 32));
        float posY = -(homePosition.y * (GetComponent<RectTransform>().sizeDelta.y + (7 * (GetComponent<RectTransform>().sizeDelta.y / 32))) + GetComponent<RectTransform>().sizeDelta.y / 2 + (7 * (GetComponent<RectTransform>().sizeDelta.y / 32)));
        return new Vector3(posX, posY, 0);
    }
}
