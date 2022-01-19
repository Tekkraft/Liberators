using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    MapController.actionType linkedAction;
    Ability linkedAbility;
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

    public void setupButton (Ability linkedAbility)
    {
        this.linkedAbility = linkedAbility;
        linkedAction = linkedAbility.getAbilityType();
        switch (linkedAction)
        {
            case MapController.actionType.MOVE:
                gameObject.GetComponentInChildren<SpriteRenderer>().sprite = moveSprite;
                break;
            case MapController.actionType.ATTACK:
                gameObject.GetComponentInChildren<SpriteRenderer>().sprite = moveSprite;
                break;
            case MapController.actionType.SUPPORT:
                gameObject.GetComponentInChildren<SpriteRenderer>().sprite = moveSprite;
                break;
        }
    }

    public void setButtonAction(MapController.actionType action)
    {
        linkedAction = action;
    }

    public void setAction()
    {
        mapController.setActionState(linkedAction);
    }

    public Ability getAbility()
    {
        return linkedAbility;
    }
}
