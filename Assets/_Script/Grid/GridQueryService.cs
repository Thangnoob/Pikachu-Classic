using UnityEngine;

public class GridQueryService
{
    private GridManager gridManager;

    public GridQueryService(GridManager grid)
    {
        gridManager = grid;
    }

    public bool IsInsideExtendedGrid(Vector2Int pos)
    {
        return pos.x >= -1 &&
               pos.x <= gridManager.Cols &&
               pos.y >= -1 &&
               pos.y <= gridManager.Rows;
    }

    public bool IsBlocked(Vector2Int pos, Vector2Int goal)
    {
        Tile tile = gridManager.GetTile(pos.x, pos.y);

        if (tile == null) return false;

        return pos != goal;
    }
}