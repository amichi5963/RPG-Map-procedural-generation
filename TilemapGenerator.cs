using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    [SerializeField] RuleTile WallMap;
    [SerializeField] RuleTile SeaMap;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    [SerializeField] int MAP_SIZE_X=30;
    [SerializeField] int MAP_SIZE_Y=20;
    [SerializeField] int MAX_ROOM_NUMBER=6;
    private int[,] map;

    void Start()
    {
        GenerateTilemap();
        ConnectTiles();
    }
    void Update()
    {
        //エンターキーが入力された場合「true」
        if (Input.GetKey(KeyCode.Return))
        {
            GenerateTilemap();
            ConnectTiles();
        }
    }

    void GenerateTilemap()
    {
        map = new MapGenerator().GenerateMap(MAP_SIZE_X, MAP_SIZE_Y, MAX_ROOM_NUMBER);
        // タイルの配置
        for (int x = 0; x < MAP_SIZE_X; x++)
        {
            for (int y = 0; y < MAP_SIZE_Y; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (map[x,y]==0)
                tilemap.SetTile(position, WallMap);
                else
                tilemap.SetTile(position, SeaMap);
            }
        }
    }

    void ConnectTiles()
    {
        // タイルの自動接続を有効化
        tilemap.CompressBounds();
        tilemap.RefreshAllTiles();
    }
}
