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

    public Rangefinder(GameObject attacker, int maxRange, int minRange, bool requiresLOS, MapController controller, Dictionary<int, List<GameObject>> teamLists)
    {
        this.attacker = attacker;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.requiresLOS = requiresLOS;
        this.teamLists = teamLists;
        this.mapController = controller;
    }

    //Conversions
    public List<GameObject> generateTargetsOfTeam(int teamNumber)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
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

    public List<GameObject> generateTargetsNotOfTeam(int teamNumber)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
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

    public List<Vector2Int> generateCoordsOfTeam(int teamNumber)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
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

    public List<Vector2Int> generateCoordsNotOfTeam(int teamNumber)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList = validTargetCoords(attacker, maxRange, minRange, requiresLOS);
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
            Debug.Log(mapController);
            Debug.Log(mapController.tileGridPos(attacker.GetComponent<UnitController>().getUnitPos()));
            Debug.Log(mapController.tileGridPos(unitList[i].GetComponent<UnitController>().getUnitPos()));
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
        Debug.DrawRay(origin, direction);
        return hit.collider != null;
    }

}
