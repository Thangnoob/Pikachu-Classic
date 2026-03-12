using System.Collections.Generic;
using UnityEngine;

public class MatchPathfinder : MonoBehaviour
{

    public bool TryGetPath(Tile a, Tile b, out List<Vector2Int> path)
    {
        path = null;

        if (a == null || b == null || a == b || a.Type != b.Type)
            return false;

        Vector2Int posA = a.GridPos;
        Vector2Int posB = b.GridPos;

        // Đường thẳng (0 góc)
        if (posA.x == posB.x || posA.y == posB.y)
        {
            if (IsStraightLineClear(posA, posB))
            {
                path = new List<Vector2Int> { posA, posB };
                return true;
            }
        }

        // BFS tối đa 2 góc và lấy path
        return BFSWithMaxTurns(posA, posB, maxTurns: 2, out path);
    }

    public bool CanConnect(Tile a, Tile b)
    {
        return TryGetPath(a, b, out _);
    }

    private bool IsStraightLineClear(Vector2Int start, Vector2Int end)
    {
        if (start.x == end.x) // cùng cột
        {
            int min = Mathf.Min(start.y, end.y);
            int max = Mathf.Max(start.y, end.y);
            for (int r = min + 1; r < max; r++)
            {
                if (GridManager.Instance.GetTile(start.x, r) != null) return false;
            }
        }
        else if (start.y == end.y) // cùng hàng
        {
            int min = Mathf.Min(start.x, end.x);
            int max = Mathf.Max(start.x, end.x);
            for (int c = min + 1; c < max; c++)
            {
                if (GridManager.Instance.GetTile(c, start.y) != null) return false;
            }
        }
        return true;
    }

    private bool BFSWithMaxTurns(Vector2Int start, Vector2Int goal, int maxTurns, out List<Vector2Int> path)
    {
        path = null;

        var visited = new HashSet<string>();
        var parents = new Dictionary<string, string>();
        var queue = new Queue<(Vector2Int pos, int turns, Vector2Int dir, string key)>();

        Vector2Int startDir = Vector2Int.zero;
        string startKey = $"{start.x}_{start.y}_{startDir.x}_{startDir.y}_0";

        queue.Enqueue((start, 0, startDir, startKey));
        visited.Add(startKey);

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        while (queue.Count > 0)
        {
            var (current, turns, prevDir, currentKey) = queue.Dequeue();

            if (current == goal && turns <= maxTurns)
            {
                // reconstruct path
                var result = new List<Vector2Int>();
                string key = currentKey;
                while (true)
                {
                    string[] parts = key.Split('_');
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    result.Add(new Vector2Int(x, y));

                    if (!parents.ContainsKey(key))
                        break;

                    key = parents[key];
                }
                result.Reverse();
                path = result;
                return true;
            }

            for (int d = 0; d < 4; d++)
            {
                Vector2Int dir = new Vector2Int(dx[d], dy[d]);
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= GridManager.Instance.Cols || next.y < 0 || next.y >= GridManager.Instance.Rows)
                    continue;

                Tile tileAtNext = GridManager.Instance.GetTile(next.x, next.y);
                if (tileAtNext != null && next != goal) continue;

                int newTurns = turns;
                if (prevDir != Vector2Int.zero && prevDir != dir && prevDir != new Vector2Int(-dir.x, -dir.y))
                {
                    newTurns++;
                    if (newTurns > maxTurns) continue;
                }

                string nextKey = $"{next.x}_{next.y}_{dir.x}_{dir.y}_{newTurns}";
                if (visited.Contains(nextKey)) continue;

                visited.Add(nextKey);
                parents[nextKey] = currentKey;
                queue.Enqueue((next, newTurns, dir, nextKey));
            }
        }

        return false;
    }

    // Kiểm tra toàn bộ grid còn cặp nào match được không
    public bool HasAnyValidPair()
    {
        var activeTiles = GridManager.Instance.GetActiveTiles();
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