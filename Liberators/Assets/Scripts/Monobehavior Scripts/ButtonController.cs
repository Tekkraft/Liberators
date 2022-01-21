using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    MapController.actionType linkedAction;
    Ability linkedAbility;
    GameObject linkedUnit;
    Grid mainGrid;
    MapController mapController;

    public Sprite moveSprite;
    public Sprite attackSprite;
    public Sprite supportSprite;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setupButton (Ability linkedAbility, GameObject linkedUnit)
    {
        this.linkedUnit = linkedUnit;
        this.linkedAbility = linkedAbility;
        linkedAction = linkedAbility.getAbilityType();
        switch (linkedAction)
        {
            case MapController.actionType.MOVE:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = moveSprite;
                break;
            case MapController.actionType.ATTACK:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = attackSprite;
                break;
            case MapController.actionType.SUPPORT:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = supportSprite;
                break;
        }
    }

    public void setAction()
    {
        mapController.setActionState(linkedUnit, linkedAbility);
    }

    public Ability getAbility()
    {
        return linkedAbility;
    }
}
