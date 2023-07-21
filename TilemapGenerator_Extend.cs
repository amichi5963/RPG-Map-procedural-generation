using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]

public class TilemapGenerator_Extend : MonoBehaviour
{
    [SerializeField] private List<GameObject> Biomes = default!;
    private List<List<SerializableKeyPair<SerializableKeyPair<float, float>, RuleTile>>> Chips;
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
    private List<Range> rangeList;//���̋�`�̃��X�g
    private List<Range> passList;//�C�̃��X�g
    private List<Range> bridgeList;//���̃��X�g
    private List<Position> TownList;//��Ƃ����Ƃ��̃��X�g

    void Start()
    {      // BiomeSetter�����Q�[���I�u�W�F�N�g���擾

        for (int i = 0; i < Biomes.Count; i++)
        {
            if (Biomes[i] != null)
            {
                BiomeSetter biomeSetter = Biomes[i].GetComponent<BiomeSetter>();
                if (biomeSetter != null)
                {
                    if (biomeSetter.Setting != null)
                        Chips.Add(biomeSetter.Setting);
                    else
                        Debug.LogWarning("Setting not found on Biomes[" + i + "] gameObject.");

                }
                else
                {
                    Debug.LogWarning("BiomeSetter component not found on Biomes[" + i + "] gameObject.");
                }
            }
            else
            {
                Debug.LogWarning("Biomes[" + i + "] is null.");
            }
        }
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
        //���敪�Ƌ��̐���
        CreateRange(MaxIsle);
        foreach (var range in rangeList)
        {
            //���̐���
            int[,] isleMap = new MazeCreator_Extend((range.End.X - range.Start.X + 2) * 2, (range.End.Y - range.Start.Y + 2) * 2, PercentageOfMt).CreateMaze();
            //���ɏ���Âz�u
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

            //�o�C�I�[���̌���
            int biome = Random.Range(0, Chips.Count);
            List<float> floats = new List<float>();
            for (var i = 0; i < Chips[biome].Count; i++)
            {
                floats.Add(Random.Range(Chips[biome][i].Key.Key, Chips[biome][i].Key.Value));
            }

            for (int i = (range.Start.X * 2) * Magnification + OutSea; i < (range.End.X * 2 + 4) * Magnification + OutSea; i++)
            {
                for (int j = (range.Start.Y * 2) * Magnification + OutSea; j < (range.End.Y * 2 + 4) * Magnification + OutSea; j++)
                {
                    Massmap[i, j].Threshold = floats;
                }
            }

        }

        //���x�̐���

        float ParlinX = Random.value, ParlinY = Random.value;
        float DesertOriginX = Random.value, DesertOriginY = Random.value;
        float SwampOriginX = Random.value, SwampOriginY = Random.value;
        for (int x = 0; x < MAP_SIZE_X - 1; x++)
        {
            for (int y = 0; y < MAP_SIZE_Y - 1; y++)
            {
                //�ʘH�ł���Ƃ���ɋ���o�^�@�ʂ�Ȃ��Ȃ����ꍇ���𐶐�
                if (x % 2 == 1 && y % 2 == 1)
                {
                    if (map[x, y] == 1 && map[x + 1, y] == 1)
                        bridgeList.Add(new Range(x / 2, y / 2, x / 2 + 1, y / 2));
                    if (map[x, y] == 1 && map[x, y + 1] == 1)
                        bridgeList.Add(new Range(x / 2, y / 2, x / 2, y / 2 + 1));
                }
                for (int i = 0; i < Magnification; i++)
                {
                    for (int j = 0; j < Magnification; j++)
                    {
                        int X = x * Magnification + i + OutSea, Y = y * Magnification + j + OutSea;
                        //���`�⊮�Ń}�b�v���g��
                        Massmap[X, Y].Height =
                            BilinearInterpolation((float)i / Magnification, (float)j / Magnification,
                            map[x, y], map[x, y + 1],
                            map[x + 1, y], map[x + 1, y + 1]);

                        //�����ł�炪����
                        if (Massmap[X, Y].Height != 0) Massmap[X, Y].Height += Mathf.PerlinNoise(X * NoizCycle + ParlinX, Y * NoizCycle + ParlinY) * (NoizMax - NoizMin) + NoizMin;
                    }
                }
            }
        }

        //�e���ɂ��Ēʂ点�����Ȃ����ɉ͐�𐶐� 
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
        //���̋L�q
        foreach (var range in bridgeList)
        {
            bool isVertical = range.GetWidthY() > 1;

            for (int i = (range.Start.X * 2 + 1) * Magnification + OutSea; i <= (range.End.X * 2 + 1) * Magnification + OutSea; i++)
            {
                for (int j = (range.Start.Y * 2 + 1) * Magnification + OutSea; j <= (range.End.Y * 2 + 1) * Magnification + OutSea; j++)
                {
                    //���̃}�X���ʍs�s�\�ł���ꍇ�@����������
                    if (!Massmap[i, j].CanWalk())
                    {
                        bool needs = true;
                        if (isVertical)
                        {
                            if (Massmap[i + 1, j - 1].CanWalk() && Massmap[i + 1, j].CanWalk() && Massmap[i + 1, j + 1].CanWalk())
                                needs = false;
                            else if (Massmap[i - 1, j - 1].CanWalk() && Massmap[i - 1, j].CanWalk() && Massmap[i - 1, j + 1].CanWalk())
                                needs = false;
                        }
                        else
                        {
                            if (Massmap[i - 1, j + 1].CanWalk() && Massmap[i, j + 1].CanWalk() && Massmap[i + 1, j + 1].CanWalk())
                                needs = false;
                            else if (Massmap[i - 1, j - 1].CanWalk() && Massmap[i, j - 1].CanWalk() && Massmap[i + 1, j - 1].CanWalk())
                                needs = false;
                        }
                        if (needs)
                            Massmap[i, j].hasBridge = true;
                    }
                }
            }
        }
        //���̐���
        //RoadLiner.GenerateRoad(Massmap, TownList[0], TownList[TownList.Count - 1]);
    }

    void DrawTilemap()
    {
        tilemap.ClearAllTiles();
        seaTilemap.ClearAllTiles();
        mountainTilemap.ClearAllTiles();
        ColliderTilemap.ClearAllTiles();

        //��{�I�Ȓn�`(����)�̋L�q
        for (int i = 0; i < Massmap.GetLength(0); i++)
        {
            for (int j = 0; j < Massmap.GetLength(1); j++)
            {
                Vector3Int position = new Vector3Int(i, j, 0);

                if (Massmap[i, j].hasBridge)
                {
                    tilemap.SetTile(position, BridgeTile);
                    seaTilemap.SetTile(position, Chips[0][Chips.Count - 1].Value);
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
                    int biome = Massmap[i, j].Biome;
                    switch (counter)
                    {
                        case 0:
                            mountainTilemap.SetTile(position, Chips[biome][0].Value);
                            break;
                        case -1:
                            seaTilemap.SetTile(position, Chips[biome][Chips.Count - 1].Value);
                            break;
                        default:
                            if (counter == Chips.Count - 1)
                                seaTilemap.SetTile(position, Chips[biome][Chips.Count - 1].Value);

                            else
                                tilemap.SetTile(position, Chips[biome][counter].Value);
                            break;
                    }
                }
            }
        }
        //��̋L�q
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
        // ���̃��X�g�̏����l�Ƃ��ă}�b�v�S�̂�����
        rangeList.Add(new Range(0, 0, MAP_SIZE_X / 2 - 2, MAP_SIZE_Y / 2 - 2));

        bool isDevided;
        do
        {
            bool ran = Random.Range(0, 2) == 1;
            // �c �� �� �̏��Ԃŕ�������؂��Ă����B�����؂�Ȃ�������I��
            isDevided = DevideRange(false);
            // �������͍ő��搔�𒴂�����I��
            if (isDevided && rangeList.Count >= maxRoom)
            {
                break;
            }
            isDevided = DevideRange(true) || isDevided;

            // �������͍ő��搔�𒴂�����I��
            if (rangeList.Count >= maxRoom)
            {
                break;
            }
        } while (isDevided);

        Debug.Log(rangeList.Count);
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

            // �ǉ����X�g�ɐV��������ޔ�����B
            newRangeList.Add(newRange);

            isDevided = true;
        }

        // �ǉ����X�g�ɑޔ����Ă������V��������ǉ�����B
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

    // �㉺���E�V���b�t��
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

    //������Hmeiro�̒n�_p����ł������_������(���D��) meiro��0��ʘH�A1��ǂƂ���B
    Position search_far(Position p, int[,] meiro)
    {
        int[,] meiro_2 = (int[,])meiro.Clone();
        int w = meiro_2.GetLength(0), h = meiro_2.GetLength(1);
        Position n = new Position();
        int sx, sy;

        tilemap.ClearAllTiles();
        Position[,] direction = new Position[w, h]; // �H���Ă����������L������z��(�v�f��d[4]�̂ǂꂩ)
        Position[] d = new Position[4]{new Position(0, 1),
                    new Position(0, -1),
                    new Position(1, 0),
                    new Position(-1, 0)}; // �i�ޕ���	
        Queue<Position> path = new Queue<Position>();
        // �X�^�[�g��ǉ�
        path.Enqueue(p);

        // �J��Ԃ��T��
        while (path.Count != 0)
        {
            n = path.Dequeue();
            meiro_2[n.X, n.Y] = 2; // �Ή�������W��T���ς݂ɂ���
            shuffle(d); // �����_�������������邽�߃V���b�t��
            for (int i = 0; i < 4; i++)
            {
                sx = n.X + d[i].X; // �אڃ}�X��x���W
                sy = n.Y + d[i].Y; // �אڃ}�X��y���W
                if (meiro_2[sx, sy] == 0)
                { // �אڃ}�X�����T���̓��̂Ƃ�
                    path.Enqueue(new Position(sx, sy));
                }
            }
        }
        return n;
    }

    //���D��T��
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
        Position[,] direction = new Position[w, h]; // �H���Ă����������L������z��(�v�f��d[4]�̂ǂꂩ)
        Position[] d = new Position[4]{new Position(0, 1),
                    new Position(0, -1),
                    new Position(1, 0),
                    new Position(-1, 0)}; // �i�ޕ���	
        Queue<Position> path = new Queue<Position>();
        // �X�^�[�g��ǉ�
        path.Enqueue(start);

        // �J��Ԃ��T��
        while (path.Count != 0)
        {
            n = path.Dequeue();
            // printf("search = (%d, %d)\n", n.x, n.y);
            // printf ("%d\n", pathend);
            meiro[n.X, n.X] = 2; // �Ή�������W��T���ς݂ɂ���
            if (n.X == goal.X && n.Y == goal.Y)
            { // �S�[������
                while (n.X != start.X || n.Y != start.Y)
                {
                    // �l���X�V
                    sx = n.X;
                    sy = n.Y;
                    // printf("PATH = (%d, %d)\n", n.x, n.y);
                    // �}�[�L���O
                    meiro[n.X, n.Y] = 3;
                    // ��������߂�
                    n.X -= direction[sx, sy].X;
                    n.Y -= direction[sx, sy].Y;
                }
                return meiro;
            }
            else
            { // �T������
                for (int i = 0; i < 4; i++)
                {
                    sx = n.X + d[i].X; // �אڃ}�X��x���W
                    sy = n.Y + d[i].Y; // �אڃ}�X��y���W
                    if (meiro[sx, sy] == 0)
                    { // �אڃ}�X�����T���̓��̂Ƃ�
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
        //���`�⊮
        float r1 = (q21 - q11) * x + q11;
        float r2 = (q22 - q12) * x + q12;
        return (r2 - r1) * y + r1;
    }
}
