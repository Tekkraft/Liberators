using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    UnitController hostController;
    List<Ability> disabledAbilities = new List<Ability>();
    List<Vector2Int> blockedTiles = new List<Vector2Int>();
    AIMode currentAIMode = AIMode.idle;
    int abilityStep = 1;

    void Awake()
    {
        hostController = this.GetComponent<UnitController>();
    }

    public Ability decideAction(List<Ability> availableAbilities)
    {
        List<Ability> validAbilities = new List<Ability>();
        foreach (Ability option in availableAbilities)
        {
            if (hostController.getActions()[1] >= option.getAPCost() && !disabledAbilities.Contains(option))
            {
                validAbilities.Add(option);
            }
        }
        if (validAbilities.Count == 0)
        {
            return null;
        }
        Ability returnValue;
        switch (currentAIMode)
        {
            case AIMode.attack:
                if (abilityStep < validAbilities.Count)
                {
                    returnValue = validAbilities[abilityStep];
                    abilityStep++;
                    return returnValue;
                } else
                {
                    abilityStep = 1;
                    return validAbilities[0];
                }

            case AIMode.flee:
                if (abilityStep < validAbilities.Count)
                {
                    returnValue = validAbilities[abilityStep];
                    abilityStep++;
                    return returnValue;
                } else
                {
                    abilityStep = 1;
                    return validAbilities[0];
                }

            default:
                return validAbilities[Random.Range(0, validAbilities.Count)];
        }
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
        return Vector2.zero;
    }

    public Vector2Int getTileTarget(CombatAbility activeAbility)
    {
        return Vector2Int.zero;
    }

    public Vector2Int getMoveTarget(MovementAbility activeAbility)
    {
        List<Vector2Int> validTiles = hostController.pathfinderValidCoords(activeAbility);
        if (validTiles.Count == 0)
        {
            return Vector2Int.zero;
        }
        float currentDistance;
        Vector2Int destination;
        bool skip;
        switch (currentAIMode)
        {
            case AIMode.attack:
                currentDistance = float.MaxValue;
                destination = new Vector2Int(int.MaxValue, int.MaxValue);
                skip = false;
                foreach (Vector2Int tile in validTiles)
                {
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }
                    if (blockedTiles.Contains(tile))
                    {
                        continue;
                    }
                    foreach (GameObject enemy in AICoordinator.seenEnemies)
                    {
                        float distance = Mathf.Abs((enemy.GetComponent<UnitController>().getUnitPos() - tile).magnitude);
                        if (distance == 0)
                        {
                            skip = true;
                            break;
                        }
                        if (distance < currentDistance)
                        {
                            currentDistance = distance;
                            destination = tile;
                        }
                    }
                }
                return destination;

            case AIMode.flee:
                currentDistance = float.MinValue;
                destination = new Vector2Int(int.MaxValue, int.MaxValue);
                skip = false;
                foreach (Vector2Int tile in validTiles)
                {
                    if (skip)
                    {
                        skip = false;
                        continue;
                    }
                    if (blockedTiles.Contains(tile))
                    {
                        continue;
                    }
                    foreach (GameObject enemy in AICoordinator.seenEnemies)
                    {
                        float distance = Mathf.Abs((enemy.GetComponent<UnitController>().getUnitPos() - tile).magnitude);
                        if (distance == 0)
                        {
                            skip = true;
                            break;
                        }
                        if (distance > currentDistance)
                        {
                            currentDistance = distance;
                            destination = tile;
                        }
                    }
                }
                return destination;

            default:
                return validTiles[Random.Range(0, validTiles.Count)];
        }
    }

    public void disableActions(Ability ability)
    {
        disabledAbilities.Add(ability);
    }

    public void resetActions()
    {
        disabledAbilities.Clear();
    }

    public void blockTiles(Vector2Int tile)
    {
        blockedTiles.Add(tile);
    }

    public void resetBlockedTiles()
    {
        disabledAbilities.Clear();
    }

    public void SetAIMode(AIMode mode)
    {
        currentAIMode = mode;
    }

    public AIMode GetAIMode()
    {
        return currentAIMode;
    }
}