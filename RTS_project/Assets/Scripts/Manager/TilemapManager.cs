using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : SingletonManager<TilemapManager>
{
    public Tilemap WalkableTilemap;
    public Tilemap PlacementTilemap;
    public Tilemap UnreachableTilemap;
    public Tilemap BuildingAreaTilemap;

    private PathFinding m_PathFinding;

    private void Start()
    {
        m_PathFinding = new(this);
    }
    public List<Node> FindPath(Vector3 _startPosition, Vector3 _endPosition) => m_PathFinding.FindPath(_startPosition, _endPosition);

    public Node FindNode(Vector3 _position) => m_PathFinding.FindNode(_position);

    public bool CanPlaceBuinding(Vector3Int _position)
    {
        return BuildingAreaTilemap.HasTile(_position) && !IsPlaceOverUnreachbleArea(_position);
    }

    private bool IsPlaceOverUnreachbleArea(Vector3Int _position)
    {
        return UnreachableTilemap.HasTile(_position) || IsPlaceAreaOverObstacle(_position);
    }

    public bool IsPlaceAreaOverObstacle(Vector3Int _position)
    {
        Vector3 tileSize = WalkableTilemap.cellSize;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(_position + tileSize * .5f, tileSize * .9f, 0);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == "BlueUnit"|| collider.gameObject.tag == "RedUnit")
            {
                return true;
            }
        }
        return false;
    }

    public bool CanWalkAtTile(Vector3Int _position)
    {
        return WalkableTilemap.HasTile(_position) && !UnreachableTilemap.HasTile(_position);
    }
}
