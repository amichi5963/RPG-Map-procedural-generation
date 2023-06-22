using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]

public class TilemapGenerator_Extend : MonoBehaviour
{
    [SerializeField] private List<SerializableKeyPair<float, RuleTile>> Chips = default;
    [SerializeField] Grid grid;
    [SerializeField] Tilemap tilemap;
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
    private List<Range> rangeList = new List<Range>();

    void Start()
    {
        GenerateTilemap();
        DrawTilemap();
    }
    void Update()
    {
        //�G���^�[�L�[�����͂��ꂽ�ꍇ�utrue�v
        if (Input.GetKeyUp(KeyCode.Return))
        {
            GenerateTilemap();
            DrawTilemap();
        }
    }

    void GenerateTilemap()
    {
        map = new int[MAP_SIZE_X, MAP_SIZE_Y];

        rangeList.Clear();
        CreateRange(MaxIsle);
        foreach (var range in rangeList)
        {
            int[,] isleMap = new MazeCreator_Extend(range.End.X - range.Start.X, range.End.Y - range.Start.Y, PercentageOfMt).CreateMaze();
            for (int x = range.Start.X; x < range.End.X; x++)
            {
                for (int y = range.Start.Y; y < range.End.Y; y++)
                {
                    map[x, y] = isleMap[x - range.Start.X, y - range.Start.Y];
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
                        //���`�⊮�Ń}�b�v���g��
                        heightmap[X, Y] =
                            BilinearInterpolation((float)i / Magnification, (float)j / Magnification,
                            map[x, y], map[x, y + 1],
                            map[x + 1, y], map[x + 1, y + 1]);

                        //�����ł�炪����
                        if (heightmap[X, Y] != 0) heightmap[X, Y] += Mathf.PerlinNoise(X * NoizCycle, Y * NoizCycle) * (NoizMax - NoizMin) + NoizMin;

                    }
                }
            }
        }
        //�e���ɂ��Ēʂ点�����Ȃ����ɉ͐�𐶐�
        foreach (var range in rangeList)
        {
            for (int x = range.Start.X; x < range.End.X; x++)
            {
                for (int y = range.Start.Y; y < range.End.Y; y++)
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
    }

    void DrawTilemap()
    {
        for (int i = 0; i < heightmap.GetLength(0); i++)
        {
            for (int j = 0; j < heightmap.GetLength(1); j++)
            {
                float height = heightmap[i, j];
                Vector3Int position = new Vector3Int(i, j, 0);
                foreach (SerializableKeyPair<float, RuleTile> item in Chips)
                {
                    if (height > item.Key)
                    {
                        tilemap.SetTile(position, item.Value);
                        break;
                    }
                }
            }
        }
    }

    public void CreateRange(int maxRoom)
    {
        // ���̃��X�g�̏����l�Ƃ��ă}�b�v�S�̂�����
        rangeList.Add(new Range(0, 0, MAP_SIZE_X - 1, MAP_SIZE_Y - 1));

        bool isDevided;
        do
        {
            // �c �� �� �̏��Ԃŕ�������؂��Ă����B�����؂�Ȃ�������I��
            isDevided = DevideRange(false);
            isDevided = DevideRange(true) || isDevided;

            // �������͍ő��搔�𒴂�����I��
            if (rangeList.Count >= maxRoom)
            {
                break;
            }
        } while (isDevided);

    }
    public bool DevideRange(bool isVertical)
    {
        bool isDevided = false;

        // ��悲�Ƃɐ؂邩�ǂ������肷��
        List<Range> newRangeList = new List<Range>();
        foreach (Range range in rangeList)
        {
            // ����ȏ㕪���ł��Ȃ��ꍇ�̓X�L�b�v
            if (isVertical && range.GetWidthY() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }
            else if (!isVertical && range.GetWidthX() < MINIMUM_RANGE_WIDTH * 2 + 1)
            {
                continue;
            }

            System.Threading.Thread.Sleep(1);

            // 40���̊m���ŕ������Ȃ�
            // �������A���̐���1�̎��͕K����������
            if (rangeList.Count > 1 && RogueUtils.RandomJadge(0.4f))
            {
                continue;
            }

            // ��������ŏ��̋��T�C�Y2���������A�c�肩�烉���_���ŕ����ʒu�����߂�
            int length = isVertical ? range.GetWidthY() : range.GetWidthX();
            int margin = length - MINIMUM_RANGE_WIDTH * 2;
            int baseIndex = isVertical ? range.Start.Y : range.Start.X;
            int devideIndex = baseIndex + MINIMUM_RANGE_WIDTH + RogueUtils.GetRandomInt(1, margin) - 1;

            // �������ꂽ���̑傫����ύX���A�V��������ǉ����X�g�ɒǉ�����
            // �����ɁA�����������E��ʘH�Ƃ��ĕۑ����Ă���
            Range newRange = new Range();
            if (isVertical)
            {
                newRange = new Range(range.Start.X, devideIndex + 1, range.End.X, range.End.Y);
                range.End.Y = devideIndex - 1;
            }
            else
            {
                newRange = new Range(devideIndex + 1, range.Start.Y, range.End.X, range.End.Y);
                range.End.X = devideIndex - 1;
            }

            // �ǉ����X�g�ɐV��������ޔ�����B
            newRangeList.Add(newRange);

            isDevided = true;
        }

        // �ǉ����X�g�ɑޔ����Ă������V��������ǉ�����B
        rangeList.AddRange(newRangeList);

        return isDevided;
    }

    public static float BilinearInterpolation(float x, float y, float q11, float q12, float q21, float q22)
    {
        //���`�⊮
        float r1 = (q21 - q11) * x + q11;
        float r2 = (q22 - q12) * x + q12;
        return (r2 - r1) * y + r1;
    }
}
