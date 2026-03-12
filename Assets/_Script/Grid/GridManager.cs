using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private TileTypeGenerator typeGenerator;
    [SerializeField] private MatchPathfinder matchPathfinder;

    [Header("Grid Settings")]
    [SerializeField] private float margin = 0.4f;
    [SerializeField] private Vector2 gridOffset = Vector2.zero;

    private Tile[,] grid;
    private List<Tile> tileList = new List<Tile>();
    private int cols, rows;
    private float tileWidth, tileHeight;
    private float startX, startY;

    public int Cols => cols;
    public int Rows => rows;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int cols, int rows)
    {
        this.cols = cols + 2;
        this.rows = rows + 2;

        grid = new Tile[this.cols, this.rows];

        GridLayoutCalculator.Calculate(this.cols, this.rows, margin, out tileWidth, out tileHeight, out startX, out startY, gridOffset);
        GenerateGrid(cols, rows);
    }

    private void GenerateGrid(int playableCols, int playableRows)
    {
        ClearGrid();

        List<int> types = typeGenerator.GenerateTypes(playableCols * playableRows);
        int index = 0;

        for (int row = 1; row <= playableRows; row++)
        {
            for (int col = 1; col <= playableCols; col++)
            {
                SpawnTile(col, row, types[index]);
                index++;
            }
        }
    }

    private void SpawnTile(int col, int row, int type)
    {
        Vector3 pos = GridLayoutCalculator.GetWorldPosition(col, row, tileWidth, tileHeight, startX, startY);
        GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);

        SpriteRenderer sr = tileObj.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteWidth = sr.sprite.bounds.size.x;
            float spriteHeight = sr.sprite.bounds.size.y;
            float scaleX = tileWidth / spriteWidth;
            float scaleY = tileHeight / spriteHeight;
            tileObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }

        Tile tile = tileObj.GetComponent<Tile>();
        tile.SetGridPos(new Vector2Int(col, row));
        tile.SetType(type, typeGenerator.GetTileSprites());

        grid[col, row] = tile;
        tileList.Add(tile);
    }

    private void ClearGrid()
    {
        foreach (var tile in tileList)
            if (tile != null) Destroy(tile.gameObject);
        tileList.Clear();
    }

    public Tile GetTile(int col, int row)
    {
        if (col < 0 || col >= cols || row < 0 || row >= rows) return null;
        return grid[col, row];
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return GridLayoutCalculator.GetWorldPosition(
            gridPos.x,
            gridPos.y,
            tileWidth,
            tileHeight,
            startX,
            startY
        );
    }

    public List<Tile> GetActiveTiles() => tileList;

    public void RemoveTile(Tile tile)
    {
        if (tile == null) return;
        Vector2Int pos = tile.GridPos;
        grid[pos.x, pos.y] = null;
        tileList.Remove(tile);
        tile.gameObject.SetActive(false); 
    }

    public void ShuffleGrid(bool ensureValidPair = false)
    {
        var remaining = GetActiveTiles();
        if (remaining.Count < 2) return;

        // Lấy danh sách vị trí hiện tại của các tile còn lại
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (var t in remaining) positions.Add(t.GridPos);

        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            ShuffleList(positions); // Fisher-Yates

            // Di chuyển tile sang vị trí mới
            for (int i = 0; i < remaining.Count; i++)
            {
                Tile tile = remaining[i];
                Vector2Int oldPos = tile.GridPos;
                Vector2Int newPos = positions[i];

                grid[oldPos.x, oldPos.y] = null;
                tile.transform.position = GridLayoutCalculator.GetWorldPosition(newPos.x, newPos.y, tileWidth, tileHeight, startX, startY);
                tile.SetGridPos(newPos);
                grid[newPos.x, newPos.y] = tile;
            }

            attempts++;
        }
        while (ensureValidPair && !matchPathfinder.HasAnyValidPair() && attempts < maxAttempts);

        if (ensureValidPair && attempts >= maxAttempts)
            Debug.LogWarning("Không tìm thấy layout có match sau 20 lần shuffle!");
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}