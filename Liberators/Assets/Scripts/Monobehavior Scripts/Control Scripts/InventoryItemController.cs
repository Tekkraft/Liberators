using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    ItemData linkedItem;
    Vector2Int homePosition;
    Transform originalParent;
    Transform mainCanvas;
    bool slotted;
    bool droppedInSlot = false;
    bool itemDisabled = false;
    InventoryController inventoryController;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeItem(ItemData linkedItem, Vector2Int homePosition, Transform originalParent, Transform mainCanvas, InventoryController inventoryController)
    {
        this.linkedItem = linkedItem;
        switch (linkedItem.itemType)
        {
            case "weapon":
                GetComponent<Image>().sprite = (this.linkedItem as WeaponData).LoadWeaponData().GetSprite();
                break;

            case "armor":
                GetComponent<Image>().sprite = (this.linkedItem as ArmorData).LoadArmorData().GetSprite();
                break;

        }
        this.homePosition = homePosition;
        this.originalParent = originalParent;
        this.mainCanvas = mainCanvas;
        transform.localPosition = CalculateFromHomePosition();
        slotted = false;
        this.inventoryController = inventoryController;
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

    public ItemData GetLinkedItem()
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

    public bool GetSlotted()
    {
        return slotted;
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

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        switch (linkedItem.itemType)
        {
            case "weapon":
                inventoryController.SetItemHoverText((this.linkedItem as WeaponData).LoadWeaponData().GetName());
                break;

            case "armor":
                inventoryController.SetItemHoverText((this.linkedItem as ArmorData).LoadArmorData().GetName());
                break;

        }
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        inventoryController.ClearItemHoverText();
    }

    //Index from 0
    public Vector3 CalculateFromHomePosition()
    {
        float posX = homePosition.x * (GetComponent<RectTransform>().sizeDelta.x + (7 * (GetComponent<RectTransform>().sizeDelta.x / 32))) + GetComponent<RectTransform>().sizeDelta.x / 2 + (7 * (GetComponent<RectTransform>().sizeDelta.x / 32));
        float posY = -(homePosition.y * (GetComponent<RectTransform>().sizeDelta.y + (7 * (GetComponent<RectTransform>().sizeDelta.y / 32))) + GetComponent<RectTransform>().sizeDelta.y / 2 + (7 * (GetComponent<RectTransform>().sizeDelta.y / 32)));
        return new Vector3(posX, posY, 0);
    }
}
