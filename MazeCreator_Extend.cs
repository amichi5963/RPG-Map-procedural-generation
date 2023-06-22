using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = System.Random;

public class MazeCreator_Extend : MonoBehaviour
{        // 2�����z��̖��H���
    private int[,] Maze;
    public int Width { get; }
    public int Height { get; }
    public int PercentageOfMt { get; }

    // ���������p
    private Random Random;
    // ���݊g�����̕Ǐ���ێ�
    private Stack<Cell> CurrentWallCells;
    //private Stack<int> CurrentWallIndex;
    // �ǂ̊g�����s���J�n�Z���̏��
    private List<Cell> StartCells;

    // �R���X�g���N�^
    public MazeCreator_Extend(int width, int height, int percentageOfMt)
    {
        // 5�����̃T�C�Y������ł͐����ł��Ȃ�
        if (width < 5 || height < 5) throw new ArgumentOutOfRangeException();
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        // ���H����������
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
        // �e�}�X�̏����ݒ���s��
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                // �O���̂ݕǂɂ��Ă����A�J�n���Ƃ��ĕێ�
                if (x == 0 || y == 0 || x == this.Width - 1 || y == this.Height - 1)
                {
                    this.Maze[x, y] = Sea;
                }
                else
                {
                    this.Maze[x, y] = Grass;
                    // �O���ł͂Ȃ��������W��ǐL�΂��J�n�_�ɂ��Ă���
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        // �J�n�����W
                        StartCells.Add(new Cell(x, y));
                    }
                }
            }
        }

        // �ǂ��g���ł��Ȃ��Ȃ�܂Ń��[�v
        while (StartCells.Count > 0)
        {
            // �����_���ɊJ�n�Z�����擾���A�J�n��₩��폜
            var index = Random.Next(StartCells.Count);
            var cell = StartCells[index];
            StartCells.RemoveAt(index);
            var x = cell.X;
            var y = cell.Y;

            // ���łɕǂ̏ꍇ�͉������Ȃ�
            if (this.Maze[x, y] == Grass)
            {
                // �g�����̕Ǐ���������
                CurrentWallCells.Clear();
                ExtendWall(x, y, Random.Next(PercentageOfMt) == 0 ? Sea : Mountain);
            }
        }
        return this.Maze;
    }

    // �w����W����ǂ𐶐��g������
    private void ExtendWall(int x, int y, int chip)
    {
        // �L�΂����Ƃ��ł������(1�}�X�悪�ʘH��2�}�X��܂Ŕ͈͓�)
        // 2�}�X�悪�ǂŎ������g�̏ꍇ�A�L�΂��Ȃ�
        var directions = new List<Direction>();
        if (this.Maze[x, y - 1] == Grass && !IsCurrentWall(x, y - 2))
            directions.Add(Direction.Up);
        if (this.Maze[x + 1, y] == Grass && !IsCurrentWall(x + 2, y))
            directions.Add(Direction.Right);
        if (this.Maze[x, y + 1] == Grass && !IsCurrentWall(x, y + 2))
            directions.Add(Direction.Down);
        if (this.Maze[x - 1, y] == Grass && !IsCurrentWall(x - 2, y))
            directions.Add(Direction.Left);

        // �����_���ɐL�΂�(2�}�X)
        if (directions.Count > 0)
        {
            // �ǂ��쐬(���̒n�_����ǂ�L�΂�)
            SetWall(x, y, chip);

            // �L�΂��悪�ʘH�̏ꍇ�͊g���𑱂���
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
                // �����̕ǂɐڑ��ł��Ă��Ȃ��ꍇ�͊g�����s
                ExtendWall(x, y, chip);
            }
        }
        else
        {
            // ���ׂČ��݊g�����̕ǂɂԂ���ꍇ�A�o�b�N���čĊJ
            var beforeCell = CurrentWallCells.Pop();
            ExtendWall(beforeCell.X, beforeCell.Y, chip);
        }
    }

    // �ǂ��g������
    private void SetWall(int x, int y, int chip)
    {
        this.Maze[x, y] = chip;
        if (x % 2 == 0 && y % 2 == 0)
        {
            CurrentWallCells.Push(new Cell(x, y));
        }
    }

    // �g�����̍��W���ǂ�������
    private bool IsCurrentWall(int x, int y)
    {
        return CurrentWallCells.Contains(new Cell(x, y));
    }

    // �ʘH�E�Ǐ��
    const int Grass = 1;
    const int Sea = 0;
    const int Mountain = 2;

    // �Z�����
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

    // ����
    private enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }
}
