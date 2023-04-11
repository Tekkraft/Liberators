using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    static int rowItems = 11;

    //Unit Specific
    UnitData unitData;

    string loadScene;

    //Globals
    [SerializeField]
    GameObject inventoryItem;

    [SerializeField]
    GameObject inventoryFrame;

    [SerializeField]
    GameObject mainLabel;

    [SerializeField]
    GameObject secondaryLabel;

    [SerializeField]
    GameObject armorLabel;

    [SerializeField]
    GameObject mainSlot;

    [SerializeField]
    GameObject secondarySlot;

    [SerializeField]
    GameObject armorSlot;

    [SerializeField]
    GameObject itemHoverText;

    // Start is called before the first frame update
    void Start()
    {
        List<ItemData> inventory = PlayerInventory.GetInventory();

        unitData = InventoryTransition.unitData;
        loadScene = InventoryTransition.origin;

        mainSlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);
        secondarySlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);
        armorSlot.GetComponent<InventoryItemSlotController>().InitializeSlot(this);

        InventoryTransition.activated = true;

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
        if (unitData.mainWeapon != null && unitData.mainWeapon.weaponBaseId != null && unitData.mainWeapon.weaponBaseId != "")
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(unitData.mainWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            mainSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (unitData.secondaryWeapon != null && unitData.secondaryWeapon.weaponBaseId != null && unitData.secondaryWeapon.weaponBaseId != "")
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(unitData.secondaryWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            secondarySlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (unitData.armor != null && unitData.armor.armorBaseId != null && unitData.armor.armorBaseId != "")
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(unitData.armor, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform, this);
            armorSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        inventoryFrame.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, multiplier * Mathf.Ceil(index / 11) + offset * 2);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: May break without slot verification
        if (mainSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            mainLabel.GetComponent<TextMeshProUGUI>().text = (mainSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponData).LoadWeaponData().GetName();
        } else
        {
            mainLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
        if (secondarySlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            secondaryLabel.GetComponent<TextMeshProUGUI>().text = (secondarySlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponData).LoadWeaponData().GetName();
        }
        else
        {
            secondaryLabel.GetComponent<TextMeshProUGUI>().text = "";
        }
        if (armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            armorLabel.GetComponent<TextMeshProUGUI>().text = (armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as ArmorData).LoadArmorData().GetName();
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
            unitData.armor = armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as ArmorData;
        }
        else
        {
            unitData.armor = null;
        }
        if (mainSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            unitData.mainWeapon = mainSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponData;
        }
        else
        {
            unitData.mainWeapon = null;
        }
        if (secondarySlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            unitData.secondaryWeapon = secondarySlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponData;
        } else
        {
            unitData.secondaryWeapon = null;
        }
        InventoryTransition.unitData = unitData;
        SceneManager.LoadSceneAsync(loadScene);
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