using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculatorController : MonoBehaviour
{
    Grid mainGrid;
    MapController mapController;
    Canvas uiCanvas;
    Vector2Int calculatorPosition;

    GameObject linkedUnit;
    UnitController linkedController;
    Ability linkedAbility;

    public void inintalizeCalculator(GameObject unit, Ability ability, Vector2Int calcPos)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        linkedUnit = unit;
        linkedController = linkedUnit.GetComponent<UnitController>();
        linkedAbility = ability;
        calculatorPosition = calcPos;
    }

    public List<GameObject> getAffectedUnits()
    {
        List<GameObject> hitUnits = new List<GameObject>();
        foreach (GameObject unit in mapController.getUnits())
        {
            UnitController unitController = unit.GetComponent<UnitController>();
            float distance = Mathf.Abs((mapController.gridTilePos(calculatorPosition) - linkedController.getUnitPos()).magnitude);
            if (distance <= linkedAbility.getAbilityRadii()[0] && distance >= linkedAbility.getAbilityRadii()[1])
            {
                if (mapController.checkLineOfSightAOE(mapController.gridTilePos(calculatorPosition),unit) || !linkedAbility.getLOSRequirement())
                {
                    hitUnits.Add(unit);
                }
            }
        }
        return hitUnits;
    }

    void modifyEffect()
    {

    }
}
