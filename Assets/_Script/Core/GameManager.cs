using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GridManager gridManager;
    [SerializeField] private MatchPathfinder matchPathfinder;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float lineDuration = 0.3f;

    private Tile firstSelected;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gridManager.Initialize(16, 9);
    }

    public void OnTileClicked(Tile clicked)
    {
        if (firstSelected == null)
        {
            firstSelected = clicked;
            firstSelected.SetSelected(true);
            Debug.Log($"Đã chọn tile: {clicked.GridPos}");
            return;
        }

        if (firstSelected == clicked)
        {
            firstSelected.SetSelected(false);
            firstSelected = null;
            return;
        }

        // tạm thời highlight tile thứ hai
        clicked.SetSelected(true);

        // Kiểm tra match
        if (matchPathfinder.TryGetPath(firstSelected, clicked, out var path))
        {
            Debug.Log("MATCH SUCCESS!");

            // Vẽ line đỏ theo path
            DrawConnection(path);

            gridManager.RemoveTile(firstSelected);
            gridManager.RemoveTile(clicked);

            // Nếu hết cặp → shuffle đảm bảo còn đường
            if (!matchPathfinder.HasAnyValidPair())
            {
                Debug.Log("Hết cặp → Auto Shuffle...");
                gridManager.ShuffleGrid(true); // ← ensure valid pair
            }
        }
        else
        {
            Debug.Log("Không nối được");
            // Nếu không nối được thì bỏ chọn tile thứ hai
            clicked.SetSelected(false);
        }

        if (firstSelected != null)
        {
            firstSelected.SetSelected(false);
        }

        firstSelected = null;
    }

    private void DrawConnection(System.Collections.Generic.List<Vector2Int> path)
    {
        if (linePrefab == null || path == null || path.Count < 2)
            return;

        LineRenderer line = Instantiate(linePrefab);
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = gridManager.GridToWorld(path[i]);
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