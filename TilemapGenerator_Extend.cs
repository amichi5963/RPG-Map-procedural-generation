using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]

public class TilemapGenerator_Extend : MonoBehaviour
{
    [SerializeField] private List<SerializableKeyPair<float, RuleTile>> Chips = default;
    [SerializeField] private RuleTile BridgeTile;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tilemap seaTilemap;
    [SerializeField] Tilemap mountainTilemap;
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
    private float[,] heightmap;
    private List<Range> rangeList;//島の矩形のリスト
    private List<Range> passList;//海のリスト
    private List<Range> bridgeList;//橋のリスト

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

        CreateRange(MaxIsle);
        foreach (var range in rangeList)
        {
            int[,] isleMap = new MazeCreator_Extend((range.End.X - range.Start.X) * 2 + 4, (range.End.Y - range.Start.Y) * 2 + 4, PercentageOfMt).CreateMaze();
            for (int x = range.Start.X * 2; x < range.End.X * 2 + 4; x++)
            {
                for (int y = range.Start.Y * 2; y < range.End.Y * 2 + 4; y++)
                {
                    map[x, y] = isleMap[x - range.Start.X * 2, y - range.Start.Y * 2];
                }
            }
        }

        heightmap = new float[(MAP_SIZE_X + 1) * Magnification + OutSea * 2, (MAP_SIZE_Y + 1) * Magnification + OutSea * 2];

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
                        heightmap[X, Y] =
                            BilinearInterpolation((float)i / Magnification, (float)j / Magnification,
                            map[x, y], map[x, y + 1],
                            map[x + 1, y], map[x + 1, y + 1]);

                        //乱数でゆらがせる
                        if (heightmap[X, Y] != 0) heightmap[X, Y] += Mathf.PerlinNoise(X * NoizCycle, Y * NoizCycle) * (NoizMax - NoizMin) + NoizMin;

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
                            && heightmap[X, Y] < Chips[0].Key)
                        {
                            heightmap[X, Y] = 0;
                        }
                    }
                }
            }
        }
    }

    void DrawTilemap()
    {
        //基本的な地形(高低)の記述
        for (int i = 0; i < heightmap.GetLength(0); i++)
        {
            for (int j = 0; j < heightmap.GetLength(1); j++)
            {
                float height = heightmap[i, j];
                Vector3Int position = new Vector3Int(i, j, 0);
                int counter = 0;
                foreach (SerializableKeyPair<float, RuleTile> item in Chips)
                {
                    if (height > item.Key)
                    {
                        if(counter == 0)
                        mountainTilemap.SetTile(position, item.Value);
                        else if(counter==Chips.Count - 1)
                            seaTilemap.SetTile(position, item.Value);
                        else
                            tilemap.SetTile(position, item.Value);
                        break;
                    }
                }
            }
        }
        //橋の記述
        foreach (var range in bridgeList)
        {
            for (int i = range.Start.X * 2 * Magnification + Magnification * 3; i <= range.End.X * 2 * Magnification + Magnification * 3; i++)
            {
                for (int j = range.Start.Y * 2 * Magnification + Magnification * 3; j <= range.End.Y * 2 * Magnification + Magnification * 3; j++)
                {
                    //そのマスが通行不能である場合　橋をかける
                    if (heightmap[i, j] <= Chips[Chips.Count - 2].Key || heightmap[i, j] >= Chips[0].Key)
                    {
                        Vector3Int position = new Vector3Int(i, j, 0);
                        tilemap.SetTile(position, BridgeTile);
                    }
                }
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
                Range pass = new Range(range.Start.X - 1, devideIndex, range.End.X + 1, devideIndex);
                passList.Add(pass);
                CreateBridge(pass, isVertical);
                newRange = new Range(range.Start.X, devideIndex + 1, range.End.X, range.End.Y);
                range.End.Y = devideIndex - 1;
            }
            else
            {
                Range pass = new Range(devideIndex, range.Start.Y - 1, devideIndex, range.End.Y + 1);
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
            random = Pass.Start.X + RogueUtils.GetRandomInt(1, Pass.GetWidthX()-1);
            bridgeList.Add(new Range(random, Pass.Start.Y, random, Pass.Start.Y+1));
        }
        else
        {
            random = Pass.Start.Y + RogueUtils.GetRandomInt(1, Pass.GetWidthY()-1);
            bridgeList.Add(new Range(Pass.End.X, random, Pass.End.X + 1, random));
        }

    }

    public static float BilinearInterpolation(float x, float y, float q11, float q12, float q21, float q22)
    {
        //線形補完
        float r1 = (q21 - q11) * x + q11;
        float r2 = (q22 - q12) * x + q12;
        return (r2 - r1) * y + r1;
    }
}
