using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    static int rowItems = 11;

    //Unit Specific
    WeaponInstance equippedMainHandWeapon;

    WeaponInstance equippedOffHandWeapon;

    ArmorInstance equippedArmor;

    string loadScene;

    //Globals
    [SerializeField]
    GameObject inventoryItem;

    [SerializeField]
    GameObject inventoryFrame;

    [SerializeField]
    GameObject mainhandLabel;

    [SerializeField]
    GameObject offhandLabel;

    [SerializeField]
    GameObject armorLabel;

    [SerializeField]
    GameObject mainhandSlot;

    [SerializeField]
    GameObject offhandSlot;

    [SerializeField]
    GameObject armorSlot;

    [SerializeField]
    GameObject itemHoverText;

    // Start is called before the first frame update
    void Start()
    {
        List<ItemInstance> inventory = PlayerInventory.GetInventory();

        equippedMainHandWeapon = InventoryTransitionController.equippedMainHandWeapon;
        equippedOffHandWeapon = InventoryTransitionController.equippedOffHandWeapon;
        equippedArmor = InventoryTransitionController.equippedArmor;
        loadScene = InventoryTransitionController.origin;

        mainhandSlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);
        offhandSlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);
        armorSlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);

        InventoryTransitionController.activated = true;

        ClearItemHoverText();

        float multiplier = 0;
        float offset = 0;
        int index;
        for (index = 0; index < inventory.Count; index++)
        {
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(inventory[index], new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            if (multiplier == 0)
            {
                multiplier = temp.GetComponent<RectTransform>().sizeDelta.y + (7 * (temp.GetComponent<RectTransform>().sizeDelta.y / 32));
                offset = 7 * (temp.GetComponent<RectTransform>().sizeDelta.y / 32);
            }
        }
        index--;
        if (!equippedMainHandWeapon.NullCheckBase())
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedMainHandWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            mainhandSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (!equippedOffHandWeapon.NullCheckBase())
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedOffHandWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            offhandSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (!equippedArmor.NullCheckBase())
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedArmor, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            armorSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        inventoryFrame.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, multiplier * Mathf.Ceil(index / 11) + offset * 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            mainhandLabel.GetComponent<TextMeshProUGUI>().text = mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem().GetInstanceName();
        } else
        {
            mainhandLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
        if (offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            offhandLabel.GetComponent<TextMeshProUGUI>().text = offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem().GetInstanceName();
        }
        else
        {
            offhandLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
        if (armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            armorLabel.GetComponent<TextMeshProUGUI>().text = armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem().GetInstanceName();
        }
        else
        {
            armorLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    public void ExitInventory()
    {
        if (armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            InventoryTransitionController.equippedArmor = armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as ArmorInstance;
        }
        else
        {
            InventoryTransitionController.equippedArmor = null;
        }
        if (mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            InventoryTransitionController.equippedMainHandWeapon = mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponInstance;
        }
        else
        {
            InventoryTransitionController.equippedMainHandWeapon = null;
        }
        if (offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            InventoryTransitionController.equippedOffHandWeapon = offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponInstance;
        } else
        {
            InventoryTransitionController.equippedOffHandWeapon = null;
        }
        SceneManager.LoadSceneAsync(loadScene);
    }

    public void ValidateEquipment()
    {
        GameObject offhandItem = offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem();
        GameObject mainhandItem = mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem();
        if (offhandItem)
        {
            if (offhandItem.GetComponent<InventoryItemController>().GetLinkedItem() != null)
            {
                offhandSlot.GetComponent<InventoryItemSlotController>().KickItemFromSlot();
            }
        }
        if (mainhandItem)
        {
            if (mainhandItem.GetComponent<InventoryItemController>().GetLinkedItem() != null)
            {
                mainhandSlot.GetComponent<InventoryItemSlotController>().KickItemFromSlot();
            }
        }
    }

    public void SetItemHoverText(string message)
    {
        itemHoverText.GetComponent<TextMeshProUGUI>().text = message;
    }

    public void ClearItemHoverText()
    {
        itemHoverText.GetComponent<TextMeshProUGUI>().text = "";
    }
}