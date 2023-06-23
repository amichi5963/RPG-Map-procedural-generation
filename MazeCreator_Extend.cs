using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public class MazeCreator_Extend : MonoBehaviour
{        // 2次元配列の迷路情報
    private int[,] Maze;
    public int Width { get; }
    public int Height { get; }
    public int PercentageOfMt { get; }

    // 乱数生成用
    private Random Random;
    // 現在拡張中の壁情報を保持
    private Stack<Cell> CurrentWallCells;
    //private Stack<int> CurrentWallIndex;
    // 壁の拡張を行う開始セルの情報
    private List<Cell> StartCells;

    // コンストラクタ
    public MazeCreator_Extend(int width, int height, int percentageOfMt)
    {
        // 5未満のサイズや偶数では生成できない
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;
        if (width < 5 || height < 5) throw new ArgumentOutOfRangeException();


        // 迷路情報を初期化
        this.Width = width;
        this.Height = height;
        this.PercentageOfMt = percentageOfMt;
        Maze = new int[width, height];
        StartCells = new List<Cell>();
        CurrentWallCells = new Stack<Cell>();
        this.Random = new Random();
    }

    public int[,] CreateMaze()
    {
        // 各マスの初期設定を行う
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                // 外周のみ壁にしておき、開始候補として保持
                if (x == 0 || y == 0 || x == this.Width - 1 || y == this.Height - 1)
                {
                    this.Maze[x, y] = Sea;
                }
                else
                {
                    this.Maze[x, y] = Grass;
                    // 外周ではない偶数座標を壁伸ばし開始点にしておく
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        // 開始候補座標
                        StartCells.Add(new Cell(x, y));
                    }
                }
            }
        }

        // 壁が拡張できなくなるまでループ
        while (StartCells.Count > 0)
        {
            // ランダムに開始セルを取得し、開始候補から削除
            var index = Random.Next(StartCells.Count);
            var cell = StartCells[index];
            StartCells.RemoveAt(index);
            var x = cell.X;
            var y = cell.Y;

            // すでに壁の場合は何もしない
            if (this.Maze[x, y] == Grass)
            {
                // 拡張中の壁情報を初期化
                CurrentWallCells.Clear();
                ExtendWall(x, y, Random.Next(PercentageOfMt) == 0 ? Sea : Mountain);
            }
        }
        return this.Maze;
    }

    // 指定座標から壁を生成拡張する
    private void ExtendWall(int x, int y, int chip)
    {
        // 伸ばすことができる方向(1マス先が通路で2マス先まで範囲内)
        // 2マス先が壁で自分自身の場合、伸ばせない
        var directions = new List<Direction>();
        if (this.Maze[x, y - 1] == Grass && !IsCurrentWall(x, y - 2))
            directions.Add(Direction.Up);
        if (this.Maze[x + 1, y] == Grass && !IsCurrentWall(x + 2, y))
            directions.Add(Direction.Right);
        if (this.Maze[x, y + 1] == Grass && !IsCurrentWall(x, y + 2))
            directions.Add(Direction.Down);
        if (this.Maze[x - 1, y] == Grass && !IsCurrentWall(x - 2, y))
            directions.Add(Direction.Left);

        // ランダムに伸ばす(2マス)
        if (directions.Count > 0)
        {
            // 壁を作成(この地点から壁を伸ばす)
            SetWall(x, y, chip);

            // 伸ばす先が通路の場合は拡張を続ける
            var isPath = false;
            var dirIndex = Random.Next(directions.Count);
            switch (directions[dirIndex])
            {
                case Direction.Up:
                    isPath = (this.Maze[x, y - 2] == Grass);
                    SetWall(x, --y, chip);
                    SetWall(x, --y, chip);
                    break;
                case Direction.Right:
                    isPath = (this.Maze[x + 2, y] == Grass);
                    SetWall(++x, y, chip);
                    SetWall(++x, y, chip);
                    break;
                case Direction.Down:
                    isPath = (this.Maze[x, y + 2] == Grass);
                    SetWall(x, ++y, chip);
                    SetWall(x, ++y, chip);
                    break;
                case Direction.Left:
                    isPath = (this.Maze[x - 2, y] == Grass);
                    SetWall(--x, y, chip);
                    SetWall(--x, y, chip);
                    break;
            }
            if (isPath)
            {
                // 既存の壁に接続できていない場合は拡張続行
                ExtendWall(x, y, chip);
            }
        }
        else
        {
            // すべて現在拡張中の壁にぶつかる場合、バックして再開
            var beforeCell = CurrentWallCells.Pop();
            ExtendWall(beforeCell.X, beforeCell.Y, chip);
        }
    }

    // 壁を拡張する
    private void SetWall(int x, int y, int chip)
    {
        this.Maze[x, y] = chip;
        if (x % 2 == 0 && y % 2 == 0)
        {
            CurrentWallCells.Push(new Cell(x, y));
        }
    }

    // 拡張中の座標かどうか判定
    private bool IsCurrentWall(int x, int y)
    {
        return CurrentWallCells.Contains(new Cell(x, y));
    }

    // 通路・壁情報
    const int Grass = 1;
    const int Sea = 0;
    const int Mountain = 2;

    // セル情報
    private struct Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Cell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    // 方向
    private enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
}
