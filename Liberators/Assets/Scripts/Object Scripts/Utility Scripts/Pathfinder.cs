using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder
{
    LinkedList<CellPair> searchList = new LinkedList<CellPair>();
    List<Vector2Int> searchedCoords = new List<Vector2Int>();
    List<Vector2Int> validCoords = new List<Vector2Int>();
    Vector2Int origin;
    int minRange;
    int maxRange;
    MapController controller;

    public Pathfinder(Vector2Int origin, int maxRange, int minRange, MapController controller)
    {
        this.origin = origin;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.controller = controller;
        calculate();
    }

    public void calculate()
    {
        searchList.AddFirst(new CellPair(origin, 0));
        while (searchList.Count > 0)
        {
            CellPair temp = searchList.First.Value;
            searchList.Remove(temp);
            searchedCoords.Add(temp.getCoords());
            if (temp.getDistance() >= minRange && temp.getDistance() <= maxRange)
            {
                validCoords.Add(temp.getCoords());
            }
            if (temp.getDistance() >= maxRange)
            {
                continue;
            }
            int newDist = temp.getDistance() + 1;
            Vector2Int newCoords = temp.getCoords();
            //+1/- 1 in X
            newCoords.x += 1;
            TerrainTile tile = controller.getTileAtPos(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0)) as TerrainTile;
            if (tile && tile.isPassable() && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.x -= 2;
            tile = controller.getTileAtPos(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0)) as TerrainTile;
            if (tile && tile.isPassable() && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.x += 1;
            //+1/-1 in Y
            newCoords.y += 1;
            tile = controller.getTileAtPos(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0)) as TerrainTile;
            if (tile && tile.isPassable() && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.y -= 2;
            tile = controller.getTileAtPos(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0)) as TerrainTile;
            if (tile && tile.isPassable() && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
        }
    }

    public List<Vector2Int> getValidCoords()
    {
        return validCoords;
    }

    public bool checkCoords(Vector2Int coords)
    {
        return validCoords.Contains(coords);
    }
}

class CellPair
{
    Vector2Int coords;
    int distance;

    public CellPair(Vector2Int coords, int distance)
    {
        this.coords = coords;
        this.distance = distance;
    }

    public Vector2Int getCoords()
    {
        return coords;
    }

    public int getDistance()
    {
        return distance;
    }
}
