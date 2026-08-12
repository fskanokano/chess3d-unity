/// <summary>
/// 象棋规则验证 - 对应原版 Rules : MonoBehaviour
/// 检查移动是否合法，判断棋子颜色
/// </summary>
using UnityEngine;

public class Rules : MonoBehaviour
{
    private void Start() { }

    /// <summary>
    /// 判断是否是红方棋子 (ID < 0 为红方)
    /// </summary>
    public static bool IsRed(int chessID)
    {
        return chessID < 0;
    }

    /// <summary>
    /// 判断是否是黑方棋子 (ID > 0 为黑方)
    /// </summary>
    public static bool IsBlack(int chessID)
    {
        return chessID > 0;
    }

    /// <summary>
    /// 判断两个棋子是否同侧
    /// </summary>
    public static bool IsSameSide(int chessOne, int chessTwo)
    {
        if (IsRed(chessOne) && IsRed(chessTwo)) return true;
        if (IsBlack(chessOne) && IsBlack(chessTwo)) return true;
        return false;
    }

    /// <summary>
    /// 验证移动是否合法 - 核心规则检查
    /// 检查移动后是否会被将军
    /// </summary>
    public static bool IsValidMove(int[,] position, int fromX, int fromY, int toX, int toY)
    {
        int fromChessID = position[fromX, fromY];
        int toChessID = position[toX, toY];

        // 不能吃自己的棋子
        if (IsSameSide(fromChessID, toChessID))
            return false;

        // 检查目标位置是否在合法走法中
        int[,] moveMap = MovingOfChess.GetRelatePos(position, fromX, fromY);

        // 如果目标位置没有被标记为可走，说明不合法
        if (toX < 0 || toX > 8 || toY < 0 || toY > 9)
            return false;

        // 检查走棋后是否被将军
        int[,] tempPos = (int[,])position.Clone();
        tempPos[toX, toY] = tempPos[fromX, fromY];
        tempPos[fromX, fromY] = 0;

        // 检查是否会被对面将军
        if (IsKingKill(tempPos, fromChessID))
            return false;

        return true;
    }

    /// <summary>
    /// 判断走棋后是否被将军
    /// </summary>
    public static bool IsKingKill(int[,] position, int chessID)
    {
        // 找到己方将/帅的位置
        int kingX = -1, kingY = -1;
        bool isRedSide = IsRed(chessID);

        for (int i = 3; i <= 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int id = position[i, j];
                if (id != 0 && IsRed(id) == isRedSide)
                {
                    if (Mathf.Abs(id) % 10 == 5) // 将/帅
                    {
                        kingX = i;
                        kingY = j;
                        break;
                    }
                }
            }
            if (kingX >= 0) break;
        }

        // 黑方将在上方
        if (!isRedSide)
        {
            for (int i = 3; i <= 5; i++)
            {
                for (int j = 7; j < 10; j++)
                {
                    int id = position[i, j];
                    if (id != 0 && IsBlack(id))
                    {
                        if (Mathf.Abs(id) % 10 == 5)
                        {
                            kingX = i;
                            kingY = j;
                            break;
                        }
                    }
                }
                if (kingX >= 0) break;
            }
        }

        if (kingX < 0) return true; // 将/帅已被吃，认为被将

        // 检查对方所有棋子是否能攻击到将/帅
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int id = position[i, j];
                if (id != 0 && IsRed(id) != isRedSide)
                {
                    int[,] moves = MovingOfChess.GetRelatePos(position, i, j);
                    if (moves[kingX, kingY] != 0)
                        return true; // 被将军
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查王对王（将帅面对面）
    /// </summary>
    public static bool KingsAreFacing(int[,] position)
    {
        int redKingX = -1, redKingY = -1;
        int blackKingX = -1, blackKingY = -1;

        for (int i = 3; i <= 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (Mathf.Abs(position[i, j]) % 10 == 5 && position[i, j] < 0)
                {
                    redKingX = i; redKingY = j;
                }
            }
            for (int j = 7; j < 10; j++)
            {
                if (Mathf.Abs(position[i, j]) % 10 == 5 && position[i, j] > 0)
                {
                    blackKingX = i; blackKingY = j;
                }
            }
        }

        if (redKingX < 0 || blackKingX < 0) return false;
        if (redKingX != blackKingX) return false;

        // 检查中间是否有棋子
        for (int j = redKingY + 1; j < blackKingY; j++)
        {
            if (position[redKingX, j] != 0)
                return false;
        }

        return true; // 将帅面对面
    }
}
