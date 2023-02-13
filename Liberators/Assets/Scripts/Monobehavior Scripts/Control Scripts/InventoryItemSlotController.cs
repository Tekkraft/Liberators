using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemSlotController : MonoBehaviour, IDropHandler
{
    [SerializeField]
    bool validateWeapon;

    [SerializeField]
    bool validateArmor;

    GameObject currentItem;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrop(PointerEventData pointerEventData)
    {
        GameObject item = pointerEventData.pointerDrag;
        AddItemToSlot(item);
    }

    public void AddItemToSlot(GameObject item)
    {
        if (item == null)
        {
            return;
        }
        if (item.GetComponent<InventoryItemController>().GetLinkedItem().GetType() != typeof(WeaponInstance) && validateWeapon)
        {
            return;
        }
        if (item.GetComponent<InventoryItemController>().GetLinkedItem().GetType() != typeof(ArmorInstance) && validateArmor)
        {
            return;
        }
        if (currentItem != null)
        {
            currentItem.GetComponent<InventoryItemController>().SetSlotted(false);
            currentItem.GetComponent<InventoryItemController>().ResetPosition();
            PlayerInventory.PushItem(currentItem.GetComponent<InventoryItemController>().GetLinkedItem());
            currentItem = null;
        }
        item.GetComponent<InventoryItemController>().SetSlotted(true);
        item.GetComponent<InventoryItemController>().SetDroppedInSlot(true);
        item.transform.SetParent(transform);
        item.GetComponent<InventoryItemController>().ResetPosition();
        PlayerInventory.PullItem(item.GetComponent<InventoryItemController>().GetLinkedItem());
        currentItem = item;
    }

    public GameObject GetCurrentItem()
    {
        return currentItem;
    }
}
