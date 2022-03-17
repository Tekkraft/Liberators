using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculatorController : MonoBehaviour
{
    Grid mainGrid;
    MapController mapController;
    Canvas uiCanvas;
    UIController uiController;
    Vector2Int calculatorPosition;

    GameObject linkedUnit;
    UnitController linkedController;
    Ability linkedAbility;

    public void inintalizeCalculator(GameObject unit, Ability ability, Vector2Int calcPos)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        uiController = uiCanvas.GetComponent<UIController>();
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
            if (unitController.inRange(linkedAbility.getAbilityRadiusType(), linkedAbility.getAbilityRadii()[0], linkedAbility.getAbilityRadii()[1], unitController.getUnitPos() - calculatorPosition))
            {
                hitUnits.Add(unit);
            }
        }
        return hitUnits;
    }

    void modifyEffect()
    {

    }
}