/// <summary>
/// 悔棋/走法历史 - 对应原版 ChessReseting : MonoBehaviour
/// 保存每一步走法，支持悔棋
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class ChessReseting : MonoBehaviour
{
    [System.Serializable]
    public class Chess
    {
        public int fromX, fromY;  // 起始位置
        public int toX, toY;      // 目标位置
        public int chessID;       // 被吃的棋子ID (0=无)
        public int moveChessID;   // 移动的棋子ID

        public Chess()
        {
            fromX = 0; fromY = 0;
            toX = 0; toY = 0;
            chessID = 0;
            moveChessID = 0;
        }
    }

    private Stack<Chess> moveHistory = new Stack<Chess>();

    /// <summary>
    /// 记录一步走法
    /// </summary>
    public void AddMove(Chess move)
    {
        moveHistory.Push(move);
    }

    /// <summary>
    /// 悔棋 - 撤销最后一步
    /// </summary>
    public bool UndoMove(GameManager gm)
    {
        if (moveHistory.Count == 0) return false;

        Chess lastMove = moveHistory.Pop();

        // 恢复棋盘状态
        int[,] board = gm.chessBoard;
        board[lastMove.fromX, lastMove.fromY] = lastMove.moveChessID;
        board[lastMove.toX, lastMove.toY] = lastMove.chessID;

        // 如果AI模式，撤销两步（玩家+AI）
        if (gm.IsAI && moveHistory.Count > 0)
        {
            Chess aiMove = moveHistory.Pop();
            board[aiMove.fromX, aiMove.fromY] = aiMove.moveChessID;
            board[aiMove.toX, aiMove.toY] = aiMove.chessID;
        }

        // 刷新棋盘显示
        gm.RefreshBoard();

        return true;
    }

    /// <summary>
    /// 清空历史
    /// </summary>
    public void ResetChess()
    {
        moveHistory.Clear();
    }

    public int GetHistoryCount()
    {
        return moveHistory.Count;
    }
}
