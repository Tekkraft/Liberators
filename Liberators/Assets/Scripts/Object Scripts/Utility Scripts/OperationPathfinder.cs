using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OperationPathfinder
{
    Tilemap map;
    Dictionary<Vector2Int, int> distanceMap = new Dictionary<Vector2Int, int>();
    Dictionary<Vector2Int, List<Vector2Int>> directionMap = new Dictionary<Vector2Int, List<Vector2Int>>();
    Dictionary<int, List<Vector2Int>> scanningQueue = new Dictionary<int, List<Vector2Int>>();

    public OperationPathfinder(Tilemap map)
    {
        this.map = map;
    }

    public void runCalculations(Vector2Int origin)
    {
        distanceMap.Clear();
        directionMap.Clear();
        scanningQueue.Clear();
        scanningQueue.Add(0, new List<Vector2Int>() { origin });
        directionMap.Add(origin, new List<Vector2Int>());
        int distance = 0;
        while (scanningQueue.Keys.Count > 0)
        {
            List<Vector2Int> scanList = new List<Vector2Int>();
            if (scanningQueue.ContainsKey(distance))
            {
                scanList.AddRange(scanningQueue[distance]);
            } else
            {
                distance++;
                continue;
            }
            for (int i = 0; i < scanList.Count; i++) {
                Vector2Int active = scanList[i];
                distanceMap.Add(active, distance);
                Vector2Int offset = new Vector2Int(1, 0);
                Vector2Int coords = active + offset;
                TerrainTileWorld tile = map.GetTile<TerrainTileWorld>(asVector3Int(coords));
                if (tile && tile.isPassable() && !tileSearchedOrQueued(coords))
                {
                    List<Vector2Int> directions = new List<Vector2Int>(directionMap[active]);
                    directions.Add(offset);
                    directionMap.Add(coords, directions);
                    int newDistance = distance + tile.getMovementFactor();
                    if (scanningQueue.ContainsKey(newDistance))
                    {
                        scanningQueue[newDistance].Add(coords);
                    } else
                    {
                        scanningQueue.Add(newDistance, new List<Vector2Int>() { coords });
                    }
                }
                offset = new Vector2Int(-1, 0);
                coords = active + offset;
                tile = map.GetTile<TerrainTileWorld>(asVector3Int(coords));
                if (tile && tile.isPassable() && !tileSearchedOrQueued(coords))
                {
                    List<Vector2Int> directions = new List<Vector2Int>(directionMap[active]);
                    directions.Add(offset);
                    directionMap.Add(coords, directions);
                    int newDistance = distance + tile.getMovementFactor();
                    if (scanningQueue.ContainsKey(newDistance))
                    {
                        scanningQueue[newDistance].Add(coords);
                    }
                    else
                    {
                        scanningQueue.Add(newDistance, new List<Vector2Int>() { coords });
                    }
                }
                offset = new Vector2Int(0, 1);
                coords = active + offset;
                tile = map.GetTile<TerrainTileWorld>(asVector3Int(coords));
                if (tile && tile.isPassable() && !tileSearchedOrQueued(coords))
                {
                    List<Vector2Int> directions = new List<Vector2Int>(directionMap[active]);
                    directions.Add(offset);
                    directionMap.Add(coords, directions);
                    int newDistance = distance + tile.getMovementFactor();
                    if (scanningQueue.ContainsKey(newDistance))
                    {
                        scanningQueue[newDistance].Add(coords);
                    }
                    else
                    {
                        scanningQueue.Add(newDistance, new List<Vector2Int>() { coords });
                    }
                }
                offset = new Vector2Int(0, -1);
                coords = active + offset;
                tile = map.GetTile<TerrainTileWorld>(asVector3Int(coords));
                if (tile && tile.isPassable() && !tileSearchedOrQueued(coords))
                {
                    List<Vector2Int> directions = new List<Vector2Int>(directionMap[active]);
                    directions.Add(offset);
                    directionMap.Add(coords, directions);
                    int newDistance = distance + tile.getMovementFactor();
                    if (scanningQueue.ContainsKey(newDistance))
                    {
                        scanningQueue[newDistance].Add(coords);
                    }
                    else
                    {
                        scanningQueue.Add(newDistance, new List<Vector2Int>() { coords });
                    }
                }
            }
            scanningQueue.Remove(distance);
            distance++;
        }
    }

    bool tileSearchedOrQueued(Vector2Int tile)
    {
        foreach (KeyValuePair<int, List<Vector2Int>> scan in scanningQueue)
        {
            if (scan.Value.Contains(tile))
            {
                return true;
            }
        }
        return distanceMap.ContainsKey(tile);
    }

    Vector3Int asVector3Int(Vector2Int tile)
    {
        return new Vector3Int(tile.x, tile.y, 0);
    }

    public List<Vector2Int> getPathToCell(Vector2Int target)
    {
        if (directionMap.ContainsKey(target))
        {
            return directionMap[target];
        }
        return null;
    }

    public double getDistanceToCell(Vector2Int target)
    {
        if (distanceMap.ContainsKey(target))
        {
            return distanceMap[target];
        }
        return -1f;
    }
}
