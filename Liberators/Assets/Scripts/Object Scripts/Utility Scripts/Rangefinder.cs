using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rangefinder
{
    GameObject attacker;
    int minRange;
    int maxRange;
    bool requiresLOS;
    MapController mapController;
    Dictionary<int, List<GameObject>> teamLists;
    Vector2 direction;

    public Rangefinder(GameObject attacker, int maxRange, int minRange, bool requiresLOS, MapController controller, Dictionary<int, List<GameObject>> teamLists, Vector2 direction)
    {
        this.attacker = attacker;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.requiresLOS = requiresLOS;
        this.teamLists = teamLists;
        this.mapController = controller;
        this.direction = direction;
    }

    //Conversions
    public List<GameObject> generateTargetsOfTeam(int teamNumber, bool beamMode)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(attacker, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (temp.GetComponent<UnitController>().getTeam() != teamNumber)
            {
                continue;
            }
            validTargets.Add(temp);
        }

        return validTargets;
    }

    public List<GameObject> generateTargetsNotOfTeam(int teamNumber, bool beamMode)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(attacker, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (temp.GetComponent<UnitController>().getTeam() == teamNumber)
            {
                continue;
            }
            validTargets.Add(temp);
        }

        return validTargets;
    }

    public List<Vector2Int> generateCoordsOfTeam(int teamNumber, bool beamMode)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(attacker, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (temp.GetComponent<UnitController>().getTeam() != teamNumber)
            {
                continue;
            }
            validTargets.Add(targetCoordsList[i]);
        }

        return validTargets;
    }

    public List<Vector2Int> generateCoordsNotOfTeam(int teamNumber, bool beamMode)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(attacker, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (temp.GetComponent<UnitController>().getTeam() == teamNumber)
            {
                continue;
            }
            validTargets.Add(targetCoordsList[i]);
        }

        return validTargets;
    }

    //Basic Rangefinding
    public List<Vector2Int> validTargetCoords(GameObject attacker, int maxRange, int minRange, bool losRequired)
    {
        List<GameObject> unitList = new List<GameObject>();
        for (int i = 0; i < teamLists.Count; i++)
        {
            unitList.AddRange(teamLists[i]);
        }
        for (int i = unitList.Count - 1; i >= 0; i--)
        {
            if ((losRequired && !checkLineOfSight(attacker, unitList[i])) || !inRange(mapController.tileGridPos(attacker.GetComponent<UnitController>().getUnitPos()), mapController.tileGridPos(unitList[i].GetComponent<UnitController>().getUnitPos()), maxRange, minRange))
            {
                unitList.Remove(unitList[i]);
            }
        }
        List<Vector2Int> coordsList = new List<Vector2Int>();
        for (int i = 0; i < unitList.Count; i++)
        {
            coordsList.Add(unitList[i].GetComponent<UnitController>().getUnitPos());
        }
        return coordsList;
    }

    public bool inRange(Vector2 attackerCoords, Vector2 targetCoords, int maxRange, int minRange)
    {
        float range = Mathf.Abs((attackerCoords - targetCoords).magnitude);
        return range <= maxRange && range >= minRange;
    }

    public List<Vector2Int> getTargetsInDirection(GameObject source, Vector2 direction)
    {
        List<Vector2Int> unitTargets = new List<Vector2Int>();

        Vector2 origin = mapController.tileGridPos(source.GetComponent<UnitController>().getUnitPos());
        RaycastHit2D[] beamHits = Physics2D.RaycastAll(origin, direction, direction.magnitude);
        foreach (RaycastHit2D hit in beamHits)
        {
            GameObject temp = hit.collider.gameObject;
            if (!temp.GetComponent<UnitController>() && requiresLOS)
            {
                break;
            }
            else if (temp.GetComponent<UnitController>())
            {
                unitTargets.Add(temp.GetComponent<UnitController>().getUnitPos());
            }
        }

        return unitTargets;
    }

    //Line of Sight Checks
    public bool checkLineOfSight(GameObject source, GameObject target)
    {
        Vector2 sourceCenter = mapController.tileGridPos(source.GetComponent<UnitController>().getUnitPos());
        Vector2 targetCenter = mapController.tileGridPos(target.GetComponent<UnitController>().getUnitPos());
        return !checkLineCollision(sourceCenter, targetCenter);
    }

    public bool checkLineOfSightAOE(Vector2 source, GameObject target)
    {
        Vector2 targetCenter = mapController.tileGridPos(target.GetComponent<UnitController>().getUnitPos());
        return !checkLineCollision(source, targetCenter);
    }

    public bool checkLineCollision(Vector2 origin, Vector2 target)
    {
        Vector2 direction = target - origin;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, direction.magnitude, mapController.lineOfSightLayer);
        return hit.collider != null;
    }
}