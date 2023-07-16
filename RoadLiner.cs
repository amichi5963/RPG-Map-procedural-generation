using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLiner : MonoBehaviour
{
    // A*�A���S���Y���ɂ�铹�̐���
    public static void GenerateRoad(Mass[,] massMap, Position start, Position goal)
    {
        int width = massMap.GetLength(0);
        int height = massMap.GetLength(1);

        // �ړ��R�X�g��ێ�����z��
        int[,] cost = new int[width, height];

        // �I�[�v�����X�g�ƃN���[�Y�h���X�g
        var openList = new List<Position>();
        var closedList = new List<Position>();

        // ������
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cost[x, y] = int.MaxValue;
            }
        }

        // �X�^�[�g�n�_�̐ݒ�
        cost[start.X, start.Y] = 0;
        openList.Add(start);

        while (openList.Count > 0)
        {
            // �I�[�v�����X�g�̒��ōŏ��R�X�g�̃}�X���擾
            Position current = GetMinCostPosition(openList, cost);

            // �S�[���ɓ��B�����ꍇ�A�I��
            if (current.X == goal.X && current.Y == goal.Y)
            {
                break;
            }

            // �I�[�v�����X�g���猻�݈ʒu���폜���A�N���[�Y�h���X�g�ɒǉ�
            openList.Remove(current);
            closedList.Add(current);

            // �㉺���E�̃}�X��T��
            foreach (Position neighbor in GetNeighborPositions(current, width, height))
            {
                // �N���[�Y�h���X�g�Ɋ܂܂�Ă���ꍇ�̓X�L�b�v
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                // �ړ��R�X�g���v�Z
                int tentativeCost = cost[current.X, current.Y] + massMap[neighbor.X, neighbor.Y].Walkability();

                // �I�[�v�����X�g�Ɋ܂܂�Ă��Ȃ����A���Ⴂ�R�X�g�̏ꍇ�͍X�V
                if (!openList.Contains(neighbor) || tentativeCost < cost[neighbor.X, neighbor.Y])
                {
                    cost[neighbor.X, neighbor.Y] = tentativeCost;

                    // �}�X�̓��t���O��ݒ�
                    massMap[neighbor.X, neighbor.Y].IsRoad = true;

                    // �I�[�v�����X�g�ɒǉ�
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
    }

    // �I�[�v�����X�g�̒��ōŏ��R�X�g�̃}�X���擾
    private static Position GetMinCostPosition(List<Position> openList, int[,] cost)
    {
        Position minCostPosition = openList[0];
        int minCost = cost[minCostPosition.X, minCostPosition.Y];

        foreach (Position position in openList)
        {
            int positionCost = cost[position.X, position.Y];
            if (positionCost < minCost)
            {
                minCost = positionCost;
                minCostPosition = position;
            }
        }

        return minCostPosition;
    }

    // �㉺���E�̗אڂ���}�X�̍��W���擾
    private static List<Position> GetNeighborPositions(Position position, int width, int height)
    {
        List<Position> neighbors = new List<Position>();

        int x = position.X;
        int y = position.Y;

        if (x > 0)
        {
            neighbors.Add(new Position(x - 1, y)); // ��
        }
        if (x < width - 1)
        {
            neighbors.Add(new Position(x + 1, y)); // �E
        }
        if (y > 0)
        {
            neighbors.Add(new Position(x, y - 1)); // ��
        }
        if (y < height - 1)
        {
            neighbors.Add(new Position(x, y + 1)); // ��
        }

        return neighbors;
    }
}
