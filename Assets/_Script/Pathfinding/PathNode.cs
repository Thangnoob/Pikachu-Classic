using UnityEngine;

public struct PathNode 
{
    public Vector2Int Position;
    public Vector2Int Direction;
    public int Turns;

    public PathNode(Vector2Int pos, Vector2Int dir, int turns)
    {
        Position = pos;
        Direction = dir;
        Turns = turns;
    }
}
