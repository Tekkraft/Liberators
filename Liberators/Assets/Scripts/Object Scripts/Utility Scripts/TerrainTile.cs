using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainTile : Tile
{
    public string tileName;

    [SerializeField]
    List<Sprite> spriteList;

    [SerializeField]
    bool single;

    //Can units move past this tile?
    public bool passable = true;

    public int hitBonus;
    public int avoidBonus;

    [SerializeField]
    List<string> matchFlags;

    [SerializeField]
    string idFlag;

    public string getTileName()
    {
        return tileName;
    }

    public bool isPassable()
    {
        return passable;
    }

    public int[] getBonuses()
    {
        return new int[] { hitBonus, avoidBonus };
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

        //Add to border case if tile absent
        int borderCase = 0;
        if (!(tilemap.GetTile<TerrainTile>(position + new Vector3Int(1, 0, 0)) && matchFlags.Contains(tilemap.GetTile<TerrainTile>(position + new Vector3Int(1, 0, 0)).GetIDFlag())))
        {
            borderCase += 1;
        }
        if (!(tilemap.GetTile<TerrainTile>(position + new Vector3Int(-1, 0, 0)) && matchFlags.Contains(tilemap.GetTile<TerrainTile>(position + new Vector3Int(-1, 0, 0)).GetIDFlag())))
        {
            borderCase += 2;
        }
        if (!(tilemap.GetTile<TerrainTile>(position + new Vector3Int(0, 1, 0)) && matchFlags.Contains(tilemap.GetTile<TerrainTile>(position + new Vector3Int(0, 1, 0)).GetIDFlag())))
        {
            borderCase += 4;
        }
        if (!(tilemap.GetTile<TerrainTile>(position + new Vector3Int(0, -1, 0)) && matchFlags.Contains(tilemap.GetTile<TerrainTile>(position + new Vector3Int(0, -1, 0)).GetIDFlag())))
        {
            borderCase += 8;
        }

        tileData.sprite = spriteList[borderCase];
    }

    public List<string> GetMatchFlags()
    {
        return matchFlags;
    }

    public string GetIDFlag()
    {
        return idFlag;
    }

    //"Borrowed" from Unity Documentation
#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a TerrainTile Asset
    [MenuItem("Assets/Create/TerrainTile")]
    public static void CreateTerrainTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Terrain Tile", "New Terrain Tile", "Asset", "Save Terrain Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerrainTile>(), path);
    }
    #endif
}
