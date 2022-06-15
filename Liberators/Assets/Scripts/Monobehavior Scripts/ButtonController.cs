using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    actionType linkedAction;
    Ability linkedAbility;
    GameObject linkedUnit;
    Grid mainGrid;
    MapController mapController;

    public bool fixedAction = true;
    public Ability fixedAbility;
    public Sprite moveSprite;
    public Sprite attackSprite;
    public Sprite supportSprite;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        if (fixedAction)
        {
            setupButton(fixedAbility, null);
        }
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
            case actionType.MOVE:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = moveSprite;
                break;
            case actionType.ATTACK:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = attackSprite;
                break;
            case actionType.SUPPORT:
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
