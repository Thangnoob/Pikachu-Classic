using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float lineDuration = 0.3f;

    public void DrawConnection(System.Collections.Generic.List<Vector2Int> path)
    {
        if (linePrefab == null || path == null || path.Count < 2)
            return;

        LineRenderer line = Instantiate(linePrefab);
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(path[i]);
            worldPos.z = -1f; // đưa line lên trên tile
            line.SetPosition(i, worldPos);
        }

        StartCoroutine(DestroyLineAfter(line, lineDuration));
    }

    private IEnumerator DestroyLineAfter(LineRenderer line, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (line != null)
        {
            Destroy(line.gameObject);
        }
    }
}
