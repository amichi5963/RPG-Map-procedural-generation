using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]

public class TilemapGenerator_Extend : MonoBehaviour
{
    [SerializeField] private List<SerializableKeyPair<SerializableKeyPair<float, float>, RuleTile>> Chips = default;
    [SerializeField] private RuleTile BridgeTile;
    [SerializeField] private RuleTile TownTile;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap seaTilemap;
    [SerializeField] Tilemap mountainTilemap;
    [SerializeField] Tilemap ColliderTilemap;
    [SerializeField] int MAP_SIZE_X = 7;
    [SerializeField] int MAP_SIZE_Y = 7;
    [SerializeField] int Magnification = 7;
    [SerializeField] int PercentageOfMt = 3;
    [SerializeField] int MaxIsle = 6;
    [SerializeField] int OutSea = 16;
    [SerializeField] float NoizMax = 0.2f;
    [SerializeField] float NoizMin = -0.2f;
    [SerializeField] float NoizCycle = -0.2f;
    [SerializeField] int MINIMUM_RANGE_WIDTH = 6;
    private int[,] map;
    private Mass[,] Massmap;
    private List<Range> rangeList;//島の矩形のリスト
    private List<Range> passList;//海のリスト
    private List<Range> bridgeList;//橋のリスト
    private List<Position> TownList;//城とか村とかのリスト

    void Start()
    {
        GenerateTilemap();
        DrawTilemap();
    }
    void Update()
    {
        //エンターキーが入力された場合「true」
        if (Input.GetKeyUp(KeyCode.Return))
        {
            GenerateTilemap();
            DrawTilemap();
        }
    }

    void GenerateTilemap()
    {
        map = new int[MAP_SIZE_X, MAP_SIZE_Y];
        rangeList = new List<Range>();
        passList = new List<Range>();
        bridgeList = new List<Range>();
        TownList = new List<Position>();
        Massmap = new Mass[(MAP_SIZE_X + 1) * Magnification + OutSea * 2, (MAP_SIZE_Y + 1) * Magnification + OutSea * 2];
        for (int i = 0; i < Massmap.GetLength(0); i++)
        {
            for (int j = 0; j < Massmap.GetLength(1); j++)
            {
                Massmap[i, j] = new Mass(i, j);
            }
        }
        //島区分と橋の生成
        CreateRange(MaxIsle);
        foreach (var range in rangeList)
        {
            List<float> floats = new List<float>();
            for (var i = 0; i < Chips.Count; i++)
            {
                floats.Add(Random.Range(Chips[i].Key.Key, Chips[i].Key.Value));
            }

            for (int i = (range.Start.X * 2) * Magnification + OutSea; i < (range.End.X * 2+4) * Magnification + OutSea; i++)
            {
                for (int j = (range.Start.Y * 2) * Magnification + OutSea; j < (range.End.Y * 2+4) * Magnification + OutSea; j++)
                {
                    Debug.Log(i+","+j);
                    Massmap[i, j].Threshold = floats;
                }
            }

            //島の生成
            int[,] isleMap = new MazeCreator_Extend((range.End.X - range.Start.X + 2) * 2, (range.End.Y - range.Start.Y + 2) * 2, PercentageOfMt).CreateMaze();
            //島に城を二つづつ配置
            int[,] meiro = new int[isleMap.GetLength(0), isleMap.GetLength(1)];
            for (int i = 0; i < isleMap.GetLength(0); i++)
            {
                for (int j = 0; j < isleMap.GetLength(1); j++)
                {
                    meiro[i, j] = isleMap[i, j] == 1 ? 0 : 1;
                }
            }
            Position start = search_far(new Position(Random.Range(0, (isleMap.GetLength(0) - 1) / 2) * 2 + 1, Random.Range(0, (isleMap.GetLength(1) - 1) / 2) * 2 + 1), meiro);
            Position goal = search_far(start, meiro);
            start.X = (start.X + (range.Start.X * 2)) * Magnification + OutSea;
            start.Y = (start.Y + (range.Start.Y * 2)) * Magnification + OutSea;
            goal.X = (goal.X + (range.Start.X * 2)) * Magnification + OutSea;
            goal.Y = (goal.Y + (range.Start.Y * 2)) * Magnification + OutSea;
            TownList.Add(start);
            TownList.Add(goal);
            for (int x = range.Start.X * 2; x < range.End.X * 2 + 4; x++)
            {
                for (int y = range.Start.Y * 2; y < range.End.Y * 2 + 4; y++)
                {
                    map[x, y] = isleMap[x - range.Start.X * 2, y - range.Start.Y * 2];
                }
            }
        }

        //高度の生成

        float ParlinX = Random.value, ParlinY = Random.value;
        float DesertOriginX = Random.value, DesertOriginY = Random.value;
        float SwampOriginX = Random.value, SwampOriginY = Random.value;
        for (int x = 0; x < MAP_SIZE_X - 1; x++)
        {
            for (int y = 0; y < MAP_SIZE_Y - 1; y++)
            {
                for (int i = 0; i < Magnification; i++)
                {
                    for (int j = 0; j < Magnification; j++)
                    {
                        int X = x * Magnification + i + OutSea, Y = y * Magnification + j + OutSea;
                        //線形補完でマップを拡大
                        Massmap[X, Y].Height =
                            BilinearInterpolation((float)i / Magnification, (float)j / Magnification,
                            map[x, y], map[x, y + 1],
                            map[x + 1, y], map[x + 1, y + 1]);

                        //乱数でゆらがせる
                        if (Massmap[X, Y].Height != 0) Massmap[X, Y].Height += Mathf.PerlinNoise(X * NoizCycle + ParlinX, Y * NoizCycle + ParlinY) * (NoizMax - NoizMin) + NoizMin;
                    }
                }
            }
        }

        //各島について通らせたくない所に河川を生成
        for (int x = 0; x < MAP_SIZE_X - 1; x++)
        {
            for (int y = 0; y < MAP_SIZE_Y - 1; y++)
            {
                for (int i = 0; i < Magnification; i++)
                {
                    for (int j = 0; j < Magnification; j++)
                    {
                        int X = x * Magnification + i + OutSea, Y = y * Magnification + j + OutSea;

                        if (((map[x, y] != 1 && map[x + 1, y] != 1 && j == 0) || (map[x, y] != 1 && map[x, y + 1] != 1 && i == 0))
                            && Massmap[X, Y].getTerrainType() != 0)
                        {
                            Massmap[X, Y].Height = 0f;
                        }
                    }
                }
            }
        }
        //橋の記述
        foreach (var range in bridgeList)
        {
            for (int i = (range.Start.X * 2+1) * Magnification + OutSea; i <= (range.End.X * 2+1) * Magnification + OutSea; i++)
            {
                for (int j = (range.Start.Y * 2+1) * Magnification + OutSea; j <= (range.End.Y * 2+1) * Magnification + OutSea; j++)
                {
                    //そのマスが通行不能である場合　橋をかける
                    if (!Massmap[i, j].CanWalk())
                    {
                        Massmap[i, j].hasBridge = true;
                    }
                }
            }
        }
        //道の生成
        //RoadLiner.GenerateRoad(Massmap, TownList[0], TownList[TownList.Count - 1]);
    }

    void DrawTilemap()
    {
        tilemap.ClearAllTiles();
        seaTilemap.ClearAllTiles();
        mountainTilemap.ClearAllTiles();
        ColliderTilemap.ClearAllTiles();

        //基本的な地形(高低)の記述
        for (int i = 0; i < Massmap.GetLength(0); i++)
        {
            for (int j = 0; j < Massmap.GetLength(1); j++)
            {
                Vector3Int position = new Vector3Int(i, j, 0);

                if (Massmap[i, j].hasBridge)
                {
                    tilemap.SetTile(position, BridgeTile);
                    seaTilemap.SetTile(position, Chips[Chips.Count - 1].Value);
                    continue;
                }
                else if (Massmap[i, j].IsRoad)
                {
                    Debug.Log("Road");
                    tilemap.SetTile(position, BridgeTile);
                    continue;
                }
                else
                {
                    int counter = Massmap[i, j].getTerrainType();
                    switch (counter)
                    {
                        case 0:
                            mountainTilemap.SetTile(position, Chips[0].Value);
                            break;
                        case -1:
                            seaTilemap.SetTile(position, Chips[Chips.Count - 1].Value);
                            break;
                        default:
                            if (counter == Chips.Count - 1)
                                seaTilemap.SetTile(position, Chips[Chips.Count - 1].Value);

                            else
                                tilemap.SetTile(position, Chips[counter].Value);
                            break;
                    }
                }
            }
        }
        //城の記述
        foreach (var pos in TownList)
        {
            int i = pos.X;
            int j = pos.Y;
            Vector3Int position = new Vector3Int(i, j, 0);
            var positionArray = new[]
            {
                position,
                new Vector3Int( 1, 0, 0 )+position,
                new Vector3Int( 1, 1, 0 )+position,
                new Vector3Int( 0, 1, 0 )+position
            };
            foreach (var p in positionArray)
            {
                tilemap.SetTile(p, TownTile);
            }
        }
    }

    public void CreateRange(int maxRoom)
    {
        // 区画のリストの初期値としてマップ全体を入れる
        rangeList.Add(new Range(0, 0, MAP_SIZE_X / 2 - 2, MAP_SIZE_Y / 2 - 2));

        bool isDevided;
        do
        {
            // 縦 → 横 の順番で部屋を区切っていく。一つも区切らなかったら終了
            isDevided = DevideRange(false);
            isDevided = DevideRange(true) || isDevided;

            // もしくは最大区画数を超えたら終了
            if (rangeList.Count >= maxRoom)
            {
                break;
            }
        } while (isDevided);
    }
    public bool DevideRange(bool isVertical)
    {
        bool isDevided = false;

        // 区画ごとに切るかどうか判定する
        List<Range> newRangeList = new List<Range>();
        foreach (Range range in rangeList)
        {
            // これ以上分割できない場合はスキップ
            if (isVertical && range.GetWidthY() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }
            else if (!isVertical && range.GetWidthX() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }

            System.Threading.Thread.Sleep(1);

            // 40％の確率で分割しない
            // ただし、区画の数が1つの時は必ず分割する
            if (rangeList.Count > 1 && RogueUtils.RandomJadge(0.4f))
            {
                continue;
            }

            // 長さから最少の区画サイズ2つ分を引き、残りからランダムで分割位置を決める
            int length = isVertical ? range.GetWidthY() : range.GetWidthX();
            int margin = length - MINIMUM_RANGE_WIDTH * 2;
            int baseIndex = isVertical ? range.Start.Y : range.Start.X;
            int devideIndex = baseIndex + MINIMUM_RANGE_WIDTH + RogueUtils.GetRandomInt(1, margin) - 1;

            // 分割された区画の大きさを変更し、新しい区画を追加リストに追加する
            // 同時に、分割した境界を通路として保存しておく
            Range newRange = new Range();
            if (isVertical)
            {
                Range pass = new Range(range.Start.X - 1, devideIndex, range.End.X, devideIndex);
                passList.Add(pass);
                CreateBridge(pass, isVertical);
                newRange = new Range(range.Start.X, devideIndex + 1, range.End.X, range.End.Y);
                range.End.Y = devideIndex - 1;
            }
            else
            {
                Range pass = new Range(devideIndex, range.Start.Y - 1, devideIndex, range.End.Y);
                passList.Add(pass);
                CreateBridge(pass, isVertical);
                newRange = new Range(devideIndex + 1, range.Start.Y, range.End.X, range.End.Y);
                range.End.X = devideIndex - 1;
            }

            // 追加リストに新しい区画を退避する。
            newRangeList.Add(newRange);

            isDevided = true;
        }

        // 追加リストに退避しておいた新しい区画を追加する。
        rangeList.AddRange(newRangeList);

        return isDevided;
    }

    private void CreateBridge(Range Pass, bool isVertical)
    {
        int random;
        if (isVertical)
        {
            random = Pass.Start.X + RogueUtils.GetRandomInt(1, Pass.GetWidthX());
            bridgeList.Add(new Range(random, Pass.Start.Y, random, Pass.Start.Y + 1));
        }
        else
        {
            random = Pass.Start.Y + RogueUtils.GetRandomInt(1, Pass.GetWidthY());
            bridgeList.Add(new Range(Pass.End.X, random, Pass.End.X + 1, random));
        }

    }

    // 上下左右シャッフル
    Position[] shuffle(Position[] direction)
    {
        int j;
        Position tmp;
        for (int i = 0; i < 4; i++)
        {
            j = Random.Range(0, 3);
            tmp = direction[i];
            direction[i] = direction[j];
            direction[j] = tmp;
        }
        return direction;
    }

    //ある迷路meiroの地点pから最も遠い点を検索(幅優先) meiroは0を通路、1を壁とする。
    Position search_far(Position p, int[,] meiro)
    {
        int[,] meiro_2 = (int[,])meiro.Clone();
        int w = meiro_2.GetLength(0), h = meiro_2.GetLength(1);
        Position n = new Position();
        int sx, sy;

        tilemap.ClearAllTiles();
        Position[,] direction = new Position[w, h]; // 辿ってきた方向を記憶する配列(要素はd[4]のどれか)
        Position[] d = new Position[4]{new Position(0, 1),
                    new Position(0, -1),
                    new Position(1, 0),
                    new Position(-1, 0)}; // 進む方向	
        Queue<Position> path = new Queue<Position>();
        // スタートを追加
        path.Enqueue(p);

        // 繰り返し探索
        while (path.Count != 0)
        {
            n = path.Dequeue();
            meiro_2[n.X, n.Y] = 2; // 対応する座標を探索済みにする
            shuffle(d); // ランダム性を持たせるためシャッフル
            for (int i = 0; i < 4; i++)
            {
                sx = n.X + d[i].X; // 隣接マスのx座標
                sy = n.Y + d[i].Y; // 隣接マスのy座標
                if (meiro_2[sx, sy] == 0)
                { // 隣接マスが未探索の道のとき
                    path.Enqueue(new Position(sx, sy));
                }
            }
        }
        return n;
    }

    //幅優先探索
    int[,] search_meiro(int w, int h, Position start, Position goal, int[,] isleMap)
    {
        int[,] meiro = new int[isleMap.GetLength(0), isleMap.GetLength(1)];
        for (int i = 0; i < isleMap.GetLength(0); i++)
        {
            for (int j = 0; j < isleMap.GetLength(1); j++)
            {
                meiro[i, j] = isleMap[i, j] == 1 ? 0 : 1;
            }
        }
        Position n;
        int sx, sy;
        Position[,] direction = new Position[w, h]; // 辿ってきた方向を記憶する配列(要素はd[4]のどれか)
        Position[] d = new Position[4]{new Position(0, 1),
                    new Position(0, -1),
                    new Position(1, 0),
                    new Position(-1, 0)}; // 進む方向	
        Queue<Position> path = new Queue<Position>();
        // スタートを追加
        path.Enqueue(start);

        // 繰り返し探索
        while (path.Count != 0)
        {
            n = path.Dequeue();
            // printf("search = (%d, %d)\n", n.x, n.y);
            // printf ("%d\n", pathend);
            meiro[n.X, n.X] = 2; // 対応する座標を探索済みにする
            if (n.X == goal.X && n.Y == goal.Y)
            { // ゴール処理
                while (n.X != start.X || n.Y != start.Y)
                {
                    // 値を更新
                    sx = n.X;
                    sy = n.Y;
                    // printf("PATH = (%d, %d)\n", n.x, n.y);
                    // マーキング
                    meiro[n.X, n.Y] = 3;
                    // 来た道を戻る
                    n.X -= direction[sx, sy].X;
                    n.Y -= direction[sx, sy].Y;
                }
                return meiro;
            }
            else
            { // 探索処理
                for (int i = 0; i < 4; i++)
                {
                    sx = n.X + d[i].X; // 隣接マスのx座標
                    sy = n.Y + d[i].Y; // 隣接マスのy座標
                    if (meiro[sx, sy] == 0)
                    { // 隣接マスが未探索の道のとき
                        path.Enqueue(new Position(sx, sy));
                        direction[sx, sy] = d[i];
                    }
                }
            }
        }
        Debug.Log("I cannot find the goal...");
        return meiro;
    }
    public static float BilinearInterpolation(float x, float y, float q11, float q12, float q21, float q22)
    {
        //線形補完
        float r1 = (q21 - q11) * x + q11;
        float r2 = (q22 - q12) * x + q12;
        return (r2 - r1) * y + r1;
    }
}
