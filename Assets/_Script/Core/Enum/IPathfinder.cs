using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathfinder
{
    bool FindPath(Vector2Int start, Vector2Int goal, int maxTurns);
}
