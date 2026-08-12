/// <summary>
/// 棋子移动规则 - 对应原版 MovingOfChess : MonoBehaviour
/// 每种棋子的所有合法走法生成
/// 坐标系统: x=0~8 (列), y=0~9 (行), 红方在下
/// </summary>
using System;
using UnityEngine;

public class MovingOfChess : MonoBehaviour
{
    private void Start() { }

    // 兵/卒过河判断
    private static readonly int RED_RIVER = 4;   // 红方过河线 (y >= 5)
    private static readonly int BLACK_RIVER = 5; // 黑方过河线 (y <= 4)

    /// <summary>
    /// 获取某位置棋子的所有合法走法 (返回9x10矩阵)
    /// 调用对应的棋子走法生成函数
    /// </summary>
    public static int[,] GetRelatePos(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];
        if (chessID == 0) return result;

        int absID = Mathf.Abs(chessID);
        int type = absID % 10;

        switch (type)
        {
            case 1: return GetCheMove(position, fromX, fromY);      // 车
            case 2: return GetMaMove(position, fromX, fromY);       // 马
            case 3: return GetXiangMove(position, fromX, fromY);    // 象/相
            case 4: return GetShiMove(position, fromX, fromY);      // 士/仕
            case 5: return GetShuaiMove(position, fromX, fromY);    // 将/帅
            case 9: return GetCheMove(position, fromX, fromY);      // 车
            case 0:
                // 炮: 十位为2是炮 (20,21,40,41)
                if (absID >= 20 && absID <= 21 || absID >= 40 && absID <= 41)
                    return GetPaoMove(position, fromX, fromY);
                return result;
            default:
                // 兵/卒 (22~26, 42~46)
                if (absID >= 22 && absID <= 26)
                    return GetRedBingMove(position, fromX, fromY);
                if (absID >= 42 && absID <= 46)
                    return GetBlackBingMove(position, fromX, fromY);
                return result;
        }
    }

    /// <summary>
    /// 车走法：横竖任意距离，不能越子
    /// </summary>
    public static int[,] GetCheMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];

        // 四个方向: 上(-x? 实际是y方向), 下, 左, 右
        // 沿x正方向
        for (int i = fromX + 1; i < 9; i++)
        {
            if (position[i, fromY] == 0)
                result[i, fromY] = 1;
            else
            {
                if (!Rules.IsSameSide(chessID, position[i, fromY]))
                    result[i, fromY] = 1;
                break;
            }
        }
        // 沿x负方向
        for (int i = fromX - 1; i >= 0; i--)
        {
            if (position[i, fromY] == 0)
                result[i, fromY] = 1;
            else
            {
                if (!Rules.IsSameSide(chessID, position[i, fromY]))
                    result[i, fromY] = 1;
                break;
            }
        }
        // 沿y正方向
        for (int j = fromY + 1; j < 10; j++)
        {
            if (position[fromX, j] == 0)
                result[fromX, j] = 1;
            else
            {
                if (!Rules.IsSameSide(chessID, position[fromX, j]))
                    result[fromX, j] = 1;
                break;
            }
        }
        // 沿y负方向
        for (int j = fromY - 1; j >= 0; j--)
        {
            if (position[fromX, j] == 0)
                result[fromX, j] = 1;
            else
            {
                if (!Rules.IsSameSide(chessID, position[fromX, j]))
                    result[fromX, j] = 1;
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// 马走法：日字形，注意蹩马腿
    /// </summary>
    public static int[,] GetMaMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];

        // 8个马腿方向
        int[,] legOffsets = new int[,]
        {
            { 1, 0 },   // 右
            { -1, 0 },  // 左
            { 0, 1 },   // 上
            { 0, -1 },  // 下
        };
        int[,] moveOffsets = new int[,]
        {
            { 1, 2 }, { 2, 1 },     // 右上
            { -1, 2 }, { -2, 1 },   // 左上
            { 1, -2 }, { 2, -1 },   // 右下
            { -1, -2 }, { -2, -1 }, // 左下
        };

        for (int d = 0; d < 4; d++)
        {
            int legX = fromX + legOffsets[d, 0];
            int legY = fromY + legOffsets[d, 1];
            if (legX < 0 || legX > 8 || legY < 0 || legY > 9) continue;
            if (position[legX, legY] != 0) continue; // 蹩马腿

            for (int m = d * 2; m < d * 2 + 2; m++)
            {
                int toX = fromX + moveOffsets[m, 0];
                int toY = fromY + moveOffsets[m, 1];
                if (toX < 0 || toX > 8 || toY < 0 || toY > 9) continue;
                if (position[toX, toY] == 0 || !Rules.IsSameSide(chessID, position[toX, toY]))
                {
                    result[toX, toY] = 1;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 象/相走法：田字形，不能过河，注意塞象眼
    /// </summary>
    public static int[,] GetXiangMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];
        bool isRed = Rules.IsRed(chessID);

        int[,] offsets = new int[,]
        {
            { 2, 2 }, { -2, 2 }, { 2, -2 }, { -2, -2 }
        };

        for (int k = 0; k < offsets.GetLength(0); k++)
        {
            int toX = fromX + offsets[k, 0];
            int toY = fromY + offsets[k, 1];
            if (toX < 0 || toX > 8 || toY < 0 || toY > 9) continue;

            // 不能过河
            if (isRed && toY > 4) continue;   // 红相只能在y<=4
            if (!isRed && toY < 5) continue;  // 黑象只能在y>=5

            // 塞象眼
            int eyeX = fromX + offsets[k, 0] / 2;
            int eyeY = fromY + offsets[k, 1] / 2;
            if (position[eyeX, eyeY] != 0) continue;

            if (position[toX, toY] == 0 || !Rules.IsSameSide(chessID, position[toX, toY]))
            {
                result[toX, toY] = 1;
            }
        }
        return result;
    }

    /// <summary>
    /// 士/仕走法：九宫格内斜走一步
    /// </summary>
    public static int[,] GetShiMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];
        bool isRed = Rules.IsRed(chessID);

        int[,] offsets = new int[,]
        {
            { 1, 1 }, { -1, 1 }, { 1, -1 }, { -1, -1 }
        };

        for (int k = 0; k < offsets.GetLength(0); k++)
        {
            int toX = fromX + offsets[k, 0];
            int toY = fromY + offsets[k, 1];
            if (toX < 3 || toX > 5) continue; // 九宫格x范围3~5

            // 九宫格y范围
            if (isRed && (toY < 0 || toY > 2)) continue;    // 红仕y 0~2
            if (!isRed && (toY < 7 || toY > 9)) continue;   // 黑士y 7~9

            if (position[toX, toY] == 0 || !Rules.IsSameSide(chessID, position[toX, toY]))
            {
                result[toX, toY] = 1;
            }
        }
        return result;
    }

    /// <summary>
    /// 将/帅走法：九宫格内上下左右走一步
    /// </summary>
    public static int[,] GetShuaiMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];
        bool isRed = Rules.IsRed(chessID);

        int[,] offsets = new int[,]
        {
            { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 }
        };

        for (int k = 0; k < offsets.GetLength(0); k++)
        {
            int toX = fromX + offsets[k, 0];
            int toY = fromY + offsets[k, 1];
            if (toX < 3 || toX > 5) continue;

            if (isRed && (toY < 0 || toY > 2)) continue;    // 红帅y 0~2
            if (!isRed && (toY < 7 || toY > 9)) continue;   // 黑将y 7~9

            if (position[toX, toY] == 0 || !Rules.IsSameSide(chessID, position[toX, toY]))
            {
                result[toX, toY] = 1;
            }
        }

        // 将帅面对面 - 可以直接吃对面将
        // (飞将规则: 如果对面将同列且中间无子，可以直线吃掉)
        int enemyKingY = isRed ? 9 : 0;
        if (fromX >= 3 && fromX <= 5)
        {
            bool canSee = true;
            int step = isRed ? 1 : -1;
            for (int j = fromY + step; j != enemyKingY + (isRed ? 0 : 1); j += step)
            {
                if (position[fromX, j] != 0)
                {
                    canSee = false;
                    break;
                }
            }
            if (canSee)
            {
                int enemyY = enemyKingY;
                if (position[fromX, enemyY] != 0)
                {
                    int enemyID = position[fromX, enemyY];
                    if (Mathf.Abs(enemyID) % 10 == 5 && !Rules.IsSameSide(chessID, enemyID))
                    {
                        result[fromX, enemyY] = 1;
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 炮走法：直线移动（不能吃子）或隔子打（必须隔一子）
    /// </summary>
    public static int[,] GetPaoMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];

        // 四个方向
        // x正方向
        int screen = 0; // 0=移动, 1=隔子
        for (int i = fromX + 1; i < 9; i++)
        {
            if (position[i, fromY] == 0)
            {
                if (screen == 0) result[i, fromY] = 1;
            }
            else
            {
                if (screen == 0)
                {
                    screen = 1; // 找到炮架
                }
                else
                {
                    // 隔子打
                    if (!Rules.IsSameSide(chessID, position[i, fromY]))
                        result[i, fromY] = 1;
                    break;
                }
            }
        }
        // x负方向
        screen = 0;
        for (int i = fromX - 1; i >= 0; i--)
        {
            if (position[i, fromY] == 0)
            {
                if (screen == 0) result[i, fromY] = 1;
            }
            else
            {
                if (screen == 0) screen = 1;
                else
                {
                    if (!Rules.IsSameSide(chessID, position[i, fromY]))
                        result[i, fromY] = 1;
                    break;
                }
            }
        }
        // y正方向
        screen = 0;
        for (int j = fromY + 1; j < 10; j++)
        {
            if (position[fromX, j] == 0)
            {
                if (screen == 0) result[fromX, j] = 1;
            }
            else
            {
                if (screen == 0) screen = 1;
                else
                {
                    if (!Rules.IsSameSide(chessID, position[fromX, j]))
                        result[fromX, j] = 1;
                    break;
                }
            }
        }
        // y负方向
        screen = 0;
        for (int j = fromY - 1; j >= 0; j--)
        {
            if (position[fromX, j] == 0)
            {
                if (screen == 0) result[fromX, j] = 1;
            }
            else
            {
                if (screen == 0) screen = 1;
                else
                {
                    if (!Rules.IsSameSide(chessID, position[fromX, j]))
                        result[fromX, j] = 1;
                    break;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 红兵走法：只能前进，过河后可左右
    /// </summary>
    public static int[,] GetRedBingMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];

        // 前进（y增加方向）
        int toY = fromY + 1;
        if (toY <= 9)
        {
            if (position[fromX, toY] == 0 || !Rules.IsSameSide(chessID, position[fromX, toY]))
                result[fromX, toY] = 1;
        }

        // 过河后可以左右
        if (fromY >= 5)
        {
            if (fromX > 0)
            {
                if (position[fromX - 1, fromY] == 0 || !Rules.IsSameSide(chessID, position[fromX - 1, fromY]))
                    result[fromX - 1, fromY] = 1;
            }
            if (fromX < 8)
            {
                if (position[fromX + 1, fromY] == 0 || !Rules.IsSameSide(chessID, position[fromX + 1, fromY]))
                    result[fromX + 1, fromY] = 1;
            }
        }
        return result;
    }

    /// <summary>
    /// 黑卒走法：只能前进（y减小方向），过河后可左右
    /// </summary>
    public static int[,] GetBlackBingMove(int[,] position, int fromX, int fromY)
    {
        int[,] result = new int[9, 10];
        int chessID = position[fromX, fromY];

        // 前进（y减小方向）
        int toY = fromY - 1;
        if (toY >= 0)
        {
            if (position[fromX, toY] == 0 || !Rules.IsSameSide(chessID, position[fromX, toY]))
                result[fromX, toY] = 1;
        }

        // 过河后可以左右
        if (fromY <= 4)
        {
            if (fromX > 0)
            {
                if (position[fromX - 1, fromY] == 0 || !Rules.IsSameSide(chessID, position[fromX - 1, fromY]))
                    result[fromX - 1, fromY] = 1;
            }
            if (fromX < 8)
            {
                if (position[fromX + 1, fromY] == 0 || !Rules.IsSameSide(chessID, position[fromX + 1, fromY]))
                    result[fromX + 1, fromY] = 1;
            }
        }
        return result;
    }

    /// <summary>
    /// 王对王检查 - 将帅不能面对面
    /// </summary>
    public static bool IsKingFacingKing(int[,] position, int fromX, int fromY, int toX, int toY)
    {
        int[,] temp = (int[,])position.Clone();
        temp[toX, toY] = temp[fromX, fromY];
        temp[fromX, fromY] = 0;
        return Rules.KingsAreFacing(temp);
    }
}
