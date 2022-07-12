using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    UnitController hostController;

    void Awake()
    {
        hostController = this.GetComponent<UnitController>();
    }

    public Ability decideAction(List<Ability> availableAbilities)
    {
        List<Ability> validAbilities = new List<Ability>();
        foreach (Ability option in availableAbilities)
        {
            if (hostController.getActions()[1] >= option.getAPCost())
            {
                validAbilities.Add(option);
            }
        }
        return validAbilities[Random.Range(0,validAbilities.Count)];
    }
}