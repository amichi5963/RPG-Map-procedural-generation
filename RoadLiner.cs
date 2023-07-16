using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLiner : MonoBehaviour
{
    // A*アルゴリズムによる道の生成
    public static void GenerateRoad(Mass[,] massMap, Position start, Position goal)
    {
        int width = massMap.GetLength(0);
        int height = massMap.GetLength(1);

        // 移動コストを保持する配列
        int[,] cost = new int[width, height];

        // オープンリストとクローズドリスト
        var openList = new List<Position>();
        var closedList = new List<Position>();

        // 初期化
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cost[x, y] = int.MaxValue;
            }
        }

        // スタート地点の設定
        cost[start.X, start.Y] = 0;
        openList.Add(start);

        while (openList.Count > 0)
        {
            // オープンリストの中で最小コストのマスを取得
            Position current = GetMinCostPosition(openList, cost);

            // ゴールに到達した場合、終了
            if (current.X == goal.X && current.Y == goal.Y)
            {
                break;
            }

            // オープンリストから現在位置を削除し、クローズドリストに追加
            openList.Remove(current);
            closedList.Add(current);

            // 上下左右のマスを探索
            foreach (Position neighbor in GetNeighborPositions(current, width, height))
            {
                // クローズドリストに含まれている場合はスキップ
                if (closedList.Contains(neighbor))
                {
                    continue;
                }

                // 移動コストを計算
                int tentativeCost = cost[current.X, current.Y] + massMap[neighbor.X, neighbor.Y].Walkability();

                // オープンリストに含まれていないか、より低いコストの場合は更新
                if (!openList.Contains(neighbor) || tentativeCost < cost[neighbor.X, neighbor.Y])
                {
                    cost[neighbor.X, neighbor.Y] = tentativeCost;

                    // マスの道フラグを設定
                    massMap[neighbor.X, neighbor.Y].IsRoad = true;

                    // オープンリストに追加
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
    }

    // オープンリストの中で最小コストのマスを取得
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

    // 上下左右の隣接するマスの座標を取得
    private static List<Position> GetNeighborPositions(Position position, int width, int height)
    {
        List<Position> neighbors = new List<Position>();

        int x = position.X;
        int y = position.Y;

        if (x > 0)
        {
            neighbors.Add(new Position(x - 1, y)); // 左
        }
        if (x < width - 1)
        {
            neighbors.Add(new Position(x + 1, y)); // 右
        }
        if (y > 0)
        {
            neighbors.Add(new Position(x, y - 1)); // 上
        }
        if (y < height - 1)
        {
            neighbors.Add(new Position(x, y + 1)); // 下
        }

        return neighbors;
    }
}
