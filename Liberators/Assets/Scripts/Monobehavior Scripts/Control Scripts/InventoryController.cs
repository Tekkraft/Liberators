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

    // Start is called before the first frame update
    void Start()
    {
        List<ItemInstance> inventory = PlayerInventory.GetInventory();

        equippedMainHandWeapon = InventoryTransitionController.equippedWeapon;
        equippedArmor = InventoryTransitionController.equippedArmor;
        loadScene = InventoryTransitionController.origin;

        float multiplier = 0;
        float offset = 0;
        int index;
        for (index = 0; index < inventory.Count; index++)
        {
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(inventory[index], new Vector2Int(xPos, yPos), inventoryFrame.transform, transform);
            if (multiplier == 0)
            {
                multiplier = temp.GetComponent<RectTransform>().sizeDelta.y + (7 * (temp.GetComponent<RectTransform>().sizeDelta.y / 32));
                offset = 7 * (temp.GetComponent<RectTransform>().sizeDelta.y / 32);
            }
        }
        if (equippedMainHandWeapon != null)
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedMainHandWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform);
            mainhandSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (equippedOffHandWeapon != null)
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedOffHandWeapon, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform);
            offhandSlot.GetComponent<InventoryItemSlotController>().AddItemToSlot(temp);
        }
        if (equippedArmor != null)
        {
            index++;
            int xPos = index % rowItems;
            int yPos = Mathf.FloorToInt(index / rowItems);
            GameObject temp = GameObject.Instantiate(inventoryItem, inventoryFrame.transform);
            temp.GetComponent<InventoryItemController>().InitializeItem(equippedArmor, new Vector2Int(xPos, yPos), inventoryFrame.transform, transform);
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
        }
        if (offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            offhandLabel.GetComponent<TextMeshProUGUI>().text = offhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem().GetInstanceName();
        }
        if (armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem())
        {
            armorLabel.GetComponent<TextMeshProUGUI>().text = armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem().GetInstanceName();
        }
    }

    public void ExitInventory()
    {
        InventoryTransitionController.equippedArmor = armorSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as ArmorInstance;
        InventoryTransitionController.equippedWeapon = mainhandSlot.GetComponent<InventoryItemSlotController>().GetCurrentItem().GetComponent<InventoryItemController>().GetLinkedItem() as WeaponInstance;
        SceneManager.LoadSceneAsync(loadScene);
    }
}
