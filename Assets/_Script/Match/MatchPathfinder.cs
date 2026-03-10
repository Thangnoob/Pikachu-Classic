using System.Collections.Generic;
using UnityEngine;

public class MatchPathfinder : MonoBehaviour
{
    [SerializeField] private GridManager gridManager; 
    public bool CanConnect(Tile a, Tile b)
    {
        if (a == null || b == null || a == b || a.Type != b.Type) return false;

        Vector2Int posA = a.GridPos;
        Vector2Int posB = b.GridPos;

        // Đường thẳng (0 góc)
        if (posA.x == posB.x || posA.y == posB.y)
        {
            if (IsStraightLineClear(posA, posB)) return true;
        }

        // BFS tối đa 2 góc
        return BFSWithMaxTurns(posA, posB, maxTurns: 2);
    }

    private bool IsStraightLineClear(Vector2Int start, Vector2Int end)
    {
        if (start.x == end.x) // cùng cột
        {
            int min = Mathf.Min(start.y, end.y);
            int max = Mathf.Max(start.y, end.y);
            for (int r = min + 1; r < max; r++)
            {
                if (gridManager.GetTile(start.x, r) != null) return false;
            }
        }
        else if (start.y == end.y) // cùng hàng
        {
            int min = Mathf.Min(start.x, end.x);
            int max = Mathf.Max(start.x, end.x);
            for (int c = min + 1; c < max; c++)
            {
                if (gridManager.GetTile(c, start.y) != null) return false;
            }
        }
        return true;
    }

    private bool BFSWithMaxTurns(Vector2Int start, Vector2Int goal, int maxTurns)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<(Vector2Int pos, int turns, Vector2Int? prevDir)>();

        queue.Enqueue((start, 0, null));
        visited.Add($"{start.x}_{start.y}_0");

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        while (queue.Count > 0)
        {
            var (current, turns, prevDir) = queue.Dequeue();

            if (current == goal && turns <= maxTurns) return true;

            for (int d = 0; d < 4; d++)
            {
                Vector2Int dir = new Vector2Int(dx[d], dy[d]);
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= gridManager.Cols || next.y < 0 || next.y >= gridManager.Rows)
                    continue;

                Tile tileAtNext = gridManager.GetTile(next.x, next.y);
                if (tileAtNext != null && next != goal) continue;

                int newTurns = turns;
                if (prevDir.HasValue && prevDir.Value != dir && prevDir.Value != new Vector2Int(-dir.x, -dir.y))
                {
                    newTurns++;
                    if (newTurns > maxTurns) continue;
                }

                string key = $"{next.x}_{next.y}_{newTurns}";
                if (visited.Contains(key)) continue;

                visited.Add(key);
                queue.Enqueue((next, newTurns, dir));
            }
        }
        return false;
    }

    // Kiểm tra toàn bộ grid còn cặp nào match được không
    public bool HasAnyValidPair()
    {
        var activeTiles = gridManager.GetActiveTiles();
        for (int i = 0; i < activeTiles.Count; i++)
        {
            for (int j = i + 1; j < activeTiles.Count; j++)
            {
                if (CanConnect(activeTiles[i], activeTiles[j]))
                    return true;
            }
        }
        return false;
    }
}