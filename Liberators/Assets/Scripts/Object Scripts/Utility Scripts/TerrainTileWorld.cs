using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainTileWorld : Tile
{
    public string tileName;

    public List<Sprite> spriteList;
    public bool single;

    //Can units move past this tile?
    public bool passable = true;

    public int movementCost = 10;

    public string getTileName()
    {
        return tileName;
    }

    public bool isPassable()
    {
        return passable;
    }

    public int getMovementCost()
    {
        return movementCost;
    }

    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        tilemap.RefreshTile(location);
        if (tilemap.GetTile<TerrainTile>(location + new Vector3Int(1, 0, 0)))
        {
            tilemap.RefreshTile(location + new Vector3Int(1, 0, 0));
        }
        if (tilemap.GetTile<TerrainTile>(location + new Vector3Int(-1, 0, 0)))
        {
            tilemap.RefreshTile(location + new Vector3Int(-1, 0, 0));
        }
        if (tilemap.GetTile<TerrainTile>(location + new Vector3Int(0, 1, 0)))
        {
            tilemap.RefreshTile(location + new Vector3Int(0, 1, 0));
        }
        if (tilemap.GetTile<TerrainTile>(location + new Vector3Int(0, -1, 0)))
        {
            tilemap.RefreshTile(location + new Vector3Int(0, -1, 0));
        }
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        if (single)
        {
            return;
        }

        int borderCase = 0;
        if (!(tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(1, 0, 0)) && tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(1, 0, 0)).tileName.Equals(tileName)))
        {
            borderCase += 1;
        }
        if (!(tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(-1, 0, 0)) && tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(-1, 0, 0)).tileName.Equals(tileName)))
        {
            borderCase += 2;
        }
        if (!(tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(0, 1, 0)) && tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(0, 1, 0)).tileName.Equals(tileName)))
        {
            borderCase += 4;
        }
        if (!(tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(0, -1, 0)) && tilemap.GetTile<TerrainTileWorld>(position + new Vector3Int(0, -1, 0)).tileName.Equals(tileName)))
        {
            borderCase += 8;
        }

        tileData.sprite = spriteList[borderCase];
    }

    //"Borrowed" from Unity Documentation
#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a TerrainTile Asset
    [MenuItem("Assets/Create/TerrainTileWorld")]
    public static void CreateTerrainTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save World Terrain Tile", "New World Terrain Tile", "Asset", "Save World Terrain Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerrainTileWorld>(), path);
    }
#endif
}
