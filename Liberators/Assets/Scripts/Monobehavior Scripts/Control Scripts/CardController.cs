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

    IItem item;
    InventoryController inventory;

    public void setup(IItem item, InventoryController inventory)
    {
        this.item = item;
        cardName.GetComponent<TextMeshProUGUI>().text = item.getName();
        //TODO: Implement Sprite implementation
        this.inventory = inventory;
    }

    public void toggleItem()
    {
        cardButton.GetComponent<Button>().interactable = !cardButton.GetComponent<Button>().interactable;
    }

    public IItem getItem()
    {
        return item;
    }

    public void buttonPressed()
    {
        inventory.updateItem(item);
        return;
    }
}
