using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    //Unit Specific
    Weapon equippedWeapon;

    Armor equippedArmor;

    string loadScene;

    //Globals
    [SerializeField]
    List<Weapon> weapons;

    [SerializeField]
    List<Armor> armors;

    [SerializeField]
    GameObject card;

    [SerializeField]
    GameObject weaponList;

    [SerializeField]
    GameObject armorList;

    [SerializeField]
    GameObject weaponLabel;

    [SerializeField]
    GameObject armorLabel;

    Dictionary<IItem, GameObject> itemCards = new Dictionary<IItem, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        equippedWeapon = InventoryTransitionController.equippedWeapon;
        equippedArmor = InventoryTransitionController.equippedArmor;
        loadScene = InventoryTransitionController.origin;

        weaponLabel.GetComponent<TextMeshProUGUI>().text = equippedWeapon.getName();
        armorLabel.GetComponent<TextMeshProUGUI>().text = equippedArmor.getName();

        for (int i = 0; i < weapons.Count; i++)
        {
            GameObject temp = GameObject.Instantiate(card, weaponList.GetComponent<RectTransform>());
            temp.GetComponent<CardController>().setup(weapons[i], this);
            if (weapons[i] == equippedWeapon)
            {
                temp.GetComponent<CardController>().toggleItem();
            }
            temp.transform.Translate(Vector3.down * 50 * i);
            itemCards.Add(weapons[i], temp);
        }
        weaponList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, weapons.Count * 50);

        for (int i = 0; i < armors.Count; i++)
        {
            GameObject temp = GameObject.Instantiate(card, armorList.GetComponent<RectTransform>());
            temp.GetComponent<CardController>().setup(armors[i], this);
            if (armors[i] == equippedArmor)
            {
                temp.GetComponent<CardController>().toggleItem();
            }
            temp.transform.Translate(Vector3.down * 50 * i);
            itemCards.Add(armors[i], temp);
        }
        armorList.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, armors.Count * 50);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateItem(IItem item)
    {
        if (item is Weapon)
        {
            itemCards[equippedWeapon].GetComponent<CardController>().toggleItem();
            equippedWeapon = item as Weapon;
            itemCards[equippedWeapon].GetComponent<CardController>().toggleItem();
            weaponLabel.GetComponent<TextMeshProUGUI>().text = equippedWeapon.getName();
        }
        else if (item is Armor)
        {
            itemCards[equippedArmor].GetComponent<CardController>().toggleItem();
            equippedArmor = item as Armor;
            itemCards[equippedArmor].GetComponent<CardController>().toggleItem();
            armorLabel.GetComponent<TextMeshProUGUI>().text = equippedArmor.getName();
        }
    }

    public void exitInventory()
    {
        InventoryTransitionController.equippedArmor = equippedArmor;
        InventoryTransitionController.equippedWeapon = equippedWeapon;
        SceneManager.LoadSceneAsync(loadScene);
    }
}
