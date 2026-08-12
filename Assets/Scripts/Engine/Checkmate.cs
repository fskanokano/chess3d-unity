/// <summary>
/// 将军/将死检测 - 对应原版 Checkmate : MonoBehaviour
/// </summary>
using UnityEngine;

public class Checkmate : MonoBehaviour
{
    /// <summary>
    /// 判断是否被将死
    /// </summary>
    public static bool JudgeIfCheckmate(int[,] position, bool redSide)
    {
        // 检查己方是否还有合法走法
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int chessID = position[i, j];
                if (chessID == 0) continue;
                if (Rules.IsRed(chessID) != redSide) continue;

                // 获取该棋子的所有走法
                int[,] moves = MovingOfChess.GetRelatePos(position, i, j);
                for (int toX = 0; toX < 9; toX++)
                {
                    for (int toY = 0; toY < 10; toY++)
                    {
                        if (moves[toX, toY] != 0)
                        {
                            // 试走
                            int[,] temp = (int[,])position.Clone();
                            temp[toX, toY] = temp[i, j];
                            temp[i, j] = 0;

                            // 走完后若不被将军则不是死棋
                            if (!Rules.IsKingKill(temp, chessID))
                                return false;
                        }
                    }
                }
            }
        }
        return true; // 无合法走法 => 将死
    }

    /// <summary>
    /// 判断是否被将军
    /// </summary>
    public static bool JudgeIfCheck(int[,] position, bool redSide)
    {
        // 找到己方将/帅位置
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int chessID = position[i, j];
                if (chessID == 0) continue;
                if (Rules.IsRed(chessID) != redSide) continue;
                if (Mathf.Abs(chessID) % 10 != 5) continue; // 不是将/帅

                // 检查对方所有棋子是否能攻击到
                for (int k = 0; k < 9; k++)
                {
                    for (int l = 0; l < 10; l++)
                    {
                        int enemyID = position[k, l];
                        if (enemyID == 0) continue;
                        if (Rules.IsRed(enemyID) == redSide) continue;

                        int[,] moves = MovingOfChess.GetRelatePos(position, k, l);
                        if (moves[i, j] != 0)
                            return true;
                    }
                }
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取将/帅位置
    /// </summary>
    public static Vector2Int GetKingPosition(int[,] position, bool redSide)
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int chessID = position[i, j];
                if (chessID == 0) continue;
                if (Rules.IsRed(chessID) != redSide) continue;
                if (Mathf.Abs(chessID) % 10 == 5)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }
}
