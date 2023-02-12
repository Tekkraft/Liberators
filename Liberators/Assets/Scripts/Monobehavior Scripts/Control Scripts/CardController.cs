using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField]
    GameObject cardName;

    [SerializeField]
    GameObject cardImage;

    [SerializeField]
    GameObject cardButton;

    ItemInstance item;
    InventoryController inventory;

    public void setup(ItemInstance item, InventoryController inventory)
    {
        this.item = item;
        cardName.GetComponent<TextMeshProUGUI>().text = item.GetInstanceName();
        cardImage.GetComponent<Image>().sprite = item.GetInstanceSprite();
        this.inventory = inventory;
    }

    public void toggleItem()
    {
        cardButton.GetComponent<Button>().interactable = !cardButton.GetComponent<Button>().interactable;
    }

    public ItemInstance getItem()
    {
        return item;
    }

    public void buttonPressed()
    {
        inventory.updateItem(item);
        return;
    }
}
