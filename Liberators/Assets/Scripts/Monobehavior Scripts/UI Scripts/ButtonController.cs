using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    ActionType linkedAction;
    Ability linkedAbility;
    GameObject linkedUnit;
    Grid mainGrid;
    MapController mapController;
    BattleController battleController;

    public bool fixedAction;
    public Ability fixedAbility;
    public Sprite moveSprite;
    public Sprite combatSprite;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
        if (fixedAction)
        {
            setupButton(fixedAbility, null);
        }
    }

    void Update()
    {
        if (gameObject.GetComponent<Button>().interactable || fixedAction)
        {
            gameObject.GetComponent<Button>().interactable = battleController.GetTurnPhase() == TurnPhase.MAIN;
        }
    }

    public void setupButton(Ability linkedAbility, GameObject linkedUnit)
    {
        this.linkedUnit = linkedUnit;
        this.linkedAbility = linkedAbility;
        if (linkedAbility.getSprite())
        {
            gameObject.transform.GetChild(0).GetComponent<Image>().sprite = linkedAbility.getSprite();
            return;
        }
        linkedAction = linkedAbility.getAbilityType();
        switch (linkedAction)
        {
            case ActionType.MOVE:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = moveSprite;
                break;
            case ActionType.COMBAT:
                gameObject.transform.GetChild(0).GetComponent<Image>().sprite = combatSprite;
                break;
        }
    }

    public void setAction()
    {
        battleController.SetActionState(linkedUnit, linkedAbility);
    }

    public Ability getAbility()
    {
        return linkedAbility;
    }
}
