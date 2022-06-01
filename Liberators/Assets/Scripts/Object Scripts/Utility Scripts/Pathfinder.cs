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
    Tilemap gameMap;

    public Pathfinder(Vector2Int origin, int maxRange, int minRange, Tilemap gameMap)
    {
        this.origin = origin;
        this.minRange = minRange;
        this.maxRange = maxRange;
        this.gameMap = gameMap;
    }

    public void changeParameters(Vector2Int origin, int maxRange, int minRange)
    {
        clearCalculator();
        this.origin = origin;
        this.minRange = minRange;
        this.maxRange = maxRange;
    }

    //passableState = true -> passable
    //passableState = false -> pathable
    public void calculate(bool passableState)
    {
        clearCalculator();
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
            TerrainTile tile = gameMap.GetTile<TerrainTile>(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0));
            if (tile && ((tile.isPassable() && passableState) || (tile.isPathable() && !passableState)) && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.x -= 2;
            tile = gameMap.GetTile<TerrainTile>(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0));
            if (tile && ((tile.isPassable() && passableState) || (tile.isPathable() && !passableState)) && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.x += 1;
            //+1/-1 in Y
            newCoords.y += 1;
            tile = gameMap.GetTile<TerrainTile>(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0));
            if (tile && ((tile.isPassable() && passableState) || (tile.isPathable() && !passableState)) && !searchedCoords.Contains(newCoords))
            {
                searchList.AddLast(new CellPair(newCoords, newDist));
            }
            newCoords.y -= 2;
            tile = gameMap.GetTile<TerrainTile>(new Vector3Int(newCoords.x - 1, newCoords.y - 1, 0));
            if (tile && ((tile.isPassable() && passableState) || (tile.isPathable() && !passableState)) && !searchedCoords.Contains(newCoords) )
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

    void clearCalculator()
    {
        searchList.Clear();
        searchedCoords.Clear();
        validCoords.Clear();
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
