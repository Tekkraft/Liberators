using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rangefinder
{
    int minRange;
    int maxRange;
    bool requiresLOS;
    MapController mapController;
    Dictionary<int, List<GameObject>> teamLists;
    Vector2 direction;

    public Rangefinder(int maxRange, int minRange, bool requiresLOS, MapController controller, Dictionary<int, List<GameObject>> teamLists, Vector2 direction)
    {
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.requiresLOS = requiresLOS;
        this.teamLists = teamLists;
        this.mapController = controller;
        this.direction = direction;
    }

    //Conversions
    public List<GameObject> generateTargetsOfTeam(Vector2 originCoords, List<int> targetTeams, bool beamMode)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(originCoords, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(originCoords, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (targetTeams.Contains(temp.GetComponent<UnitController>().getTeam()))
            {
                validTargets.Add(temp);
            }
        }

        return validTargets;
    }

    public List<GameObject> generateTargetsNotOfTeam(Vector2 originCoords, List<int> targetTeams, bool beamMode)
    {
        List<GameObject> validTargets = new List<GameObject>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(originCoords, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(originCoords, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (!targetTeams.Contains(temp.GetComponent<UnitController>().getTeam()))
            {
                validTargets.Add(temp);
            }
        }

        return validTargets;
    }

    public List<Vector2Int> generateCoordsOfTeam(Vector2 originCoords, List<int> targetTeams, bool beamMode)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(originCoords, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(originCoords, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (targetTeams.Contains(temp.GetComponent<UnitController>().getTeam()))
            {
                validTargets.Add(targetCoordsList[i]);
            }
        }

        return validTargets;
    }

    public List<Vector2Int> generateCoordsNotOfTeam(Vector2 originCoords, List<int> targetTeams, bool beamMode)
    {
        List<Vector2Int> validTargets = new List<Vector2Int>();

        List<Vector2Int> targetCoordsList;
        if (beamMode)
        {
            targetCoordsList = getTargetsInDirection(originCoords, direction);
        }
        else
        {
            targetCoordsList = validTargetCoords(originCoords, maxRange, minRange, requiresLOS);
        }

        for (int i = 0; i < targetCoordsList.Count; i++)
        {
            GameObject temp = mapController.getUnitFromCoords(targetCoordsList[i]);
            if (!targetTeams.Contains(temp.GetComponent<UnitController>().getTeam()))
            {
                validTargets.Add(targetCoordsList[i]);
            }
        }

        return validTargets;
    }

    //Basic Rangefinding
    public List<Vector2Int> validTargetCoords(Vector2 originCoords, int maxRange, int minRange, bool losRequired)
    {
        List<GameObject> unitList = new List<GameObject>();
        for (int i = 0; i < teamLists.Count; i++)
        {
            unitList.AddRange(teamLists[i]);
        }
        for (int i = unitList.Count - 1; i >= 0; i--)
        {
            if ((losRequired && checkLineCollision(originCoords, unitList[i].GetComponent<UnitController>().getUnitPos())) || !inRange(mapController.tileGridPos(originCoords), mapController.tileGridPos(unitList[i].GetComponent<UnitController>().getUnitPos()), maxRange, minRange))
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

    public List<Vector2Int> getTargetsInDirection(Vector2 originCoords, Vector2 direction)
    {
        List<Vector2Int> unitTargets = new List<Vector2Int>();

        Vector2 origin = mapController.tileGridPos(originCoords);
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
    public bool checkLineCollision(Vector2 originPos, Vector2 targetPos)
    {
        Vector2 origin = mapController.tileGridPos(originPos);
        Vector2 target = mapController.tileGridPos(targetPos);
        Vector2 direction = target - origin;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, direction.magnitude, mapController.lineOfSightLayer);
        if (hit.collider)
        {
            Debug.DrawRay(origin, direction, Color.red, 100);
        } else
        {
            Debug.DrawRay(origin, direction, Color.white, 100);
        }
        return hit.collider != null;
    }
}