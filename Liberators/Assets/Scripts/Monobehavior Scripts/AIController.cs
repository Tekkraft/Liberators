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
        if (validAbilities.Count == 0)
        {
            return null;
        }
        return validAbilities[Random.Range(0, validAbilities.Count)];
    }

    public GameObject getGameObjectTarget(CombatAbility activeAbility, List<GameObject> targets)
    {
        if (targets.Count == 0)
        {
            return null;
        }
        return targets[Random.Range(0, targets.Count)];
    }

    public Vector2 getPointTarget(CombatAbility activeAbility)
    {
        float maxRange = activeAbility.getAbilityRanges()[0];
        float minRange = activeAbility.getAbilityRanges()[1];
        Vector2 randomOffset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(minRange, maxRange);
        return (Vector2)hostController.getUnitPos() + randomOffset;
    }

    public Vector2Int getTileTarget(CombatAbility activeAbility)
    {
        int maxRange = activeAbility.getAbilityRanges()[0];
        int minRange = activeAbility.getAbilityRanges()[1];
        int horizontal = Random.Range(minRange, maxRange + 1);
        int vertical = Random.Range(minRange, maxRange + 1);
        return hostController.getUnitPos() + new Vector2Int(horizontal, vertical);
    }

    public Vector2Int getMoveTarget(MovementAbility activeAbility)
    {
        List<Vector2Int> validTiles = hostController.pathfinderValidCoords(activeAbility);
        return validTiles[Random.Range(0,validTiles.Count)];
    }
}