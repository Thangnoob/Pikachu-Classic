using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingBFS : IPathfinder
{
    private readonly GridQueryService _gridService;
    // Dùng mảng 3 chiều hoặc Dictionary với Key là int để tránh tạo rác (GC)
    private readonly HashSet<int> _visited = new HashSet<int>();

    public PathfindingBFS(GridQueryService gridService) => _gridService = gridService;

    public bool FindPath(Vector2Int start, Vector2Int goal, int maxTurns)
    {
        _visited.Clear();
        var queue = new Queue<PathNode>();
        queue.Enqueue(new PathNode(start, Vector2Int.zero, 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var dir in DirectionUtils.Directions)
            {
                int nextTurns = current.Turns;
                if (current.Direction != Vector2Int.zero && current.Direction != dir)
                    nextTurns++;

                if (nextTurns > maxTurns) continue;

                Vector2Int next = current.Position + dir;

                // Thuật toán Pikachu: Đi thẳng tối đa có thể trước khi rẽ
                while (_gridService.IsInsideExtendedGrid(next))
                {
                    if (next == goal) return true;
                    if (_gridService.IsBlocked(next, goal)) break;

                    // Tạo key duy nhất từ x, y, dir, turns (Bit-masking cơ bản)
                    int key = (next.x + 1) | ((next.y + 1) << 8) | (nextTurns << 16);
                    if (!_visited.Contains(key))
                    {
                        _visited.Add(key);
                        queue.Enqueue(new PathNode(next, dir, nextTurns));
                    }
                    next += dir;
                }
            }
        }
        return false;
    }
}
