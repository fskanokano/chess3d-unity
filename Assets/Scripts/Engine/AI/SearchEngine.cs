/// <summary>
/// AI搜索引擎 - 对应原版 SearchEngine : MonoBehaviour
/// NegaMax + AlphaBeta剪枝 + 渴望搜索 + 主变例 + 历史启发 + 归并排序走法排序
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class SearchEngine : MonoBehaviour
{
    [Header("搜索参数")]
    public int searchDepth = 5;          // 搜索深度（难度越高越深）
    public int SearchDepth
    {
        get { return searchDepth; }
        set { searchDepth = value; }
    }

    // 搜索的最佳走法
    public ChessReseting.Chess bestStep = new ChessReseting.Chess();
    public ChessReseting.Chess BestStep { get { return bestStep; } }

    // 历史启发表
    private Dictionary<ChessReseting.Chess, int> historyDic = new Dictionary<ChessReseting.Chess, int>();

    // 棋盘引用
    private int[,] board;
    private GameManager gm;

    // 走法列表 (用于排序)
    private List<ChessReseting.Chess> moves = new List<ChessReseting.Chess>();

    public SearchEngine()
    {
    }

    public void Init(GameManager gameManager)
    {
        gm = gameManager;
    }

    /// <summary>
    /// 搜索一步好棋 - 入口函数
    /// </summary>
    public ChessReseting.Chess SearchAGoodMove(int[,] position)
    {
        board = position;
        bestStep = new ChessReseting.Chess();
        historyDic.Clear();

        // 渴望搜索
        AspirationSearch();

        return bestStep;
    }

    /// <summary>
    /// 生成某方所有合法走法
    /// </summary>
    private List<ChessReseting.Chess> GenerateMoves(int[,] position, bool redSide)
    {
        moves.Clear();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int chessID = position[i, j];
                if (chessID == 0) continue;
                if (Rules.IsRed(chessID) != redSide) continue;

                int[,] relatePos = MovingOfChess.GetRelatePos(position, i, j);
                for (int toX = 0; toX < 9; toX++)
                {
                    for (int toY = 0; toY < 10; toY++)
                    {
                        if (relatePos[toX, toY] != 0)
                        {
                            // 验证走法合法（防止送将/王对王）
                            int[,] temp = (int[,])position.Clone();
                            temp[toX, toY] = temp[i, j];
                            temp[i, j] = 0;
                            if (Rules.IsKingKill(temp, chessID)) continue;

                            ChessReseting.Chess move = new ChessReseting.Chess();
                            move.fromX = i; move.fromY = j;
                            move.toX = toX; move.toY = toY;
                            move.moveChessID = chessID;
                            move.chessID = position[toX, toY];
                            moves.Add(move);
                        }
                    }
                }
            }
        }
        return moves;
    }

    /// <summary>
    /// 渴望搜索 - 用上次结果作为窗口
    /// </summary>
    private void AspirationSearch()
    {
        int lastScore = 0;
        int window = 50; // 初始窗口
        int alpha, beta;

        while (true)
        {
            alpha = lastScore - window;
            beta = lastScore + window;
            int score = AlphaBeta(searchDepth, alpha, beta);

            if (score <= alpha)
            {
                window += window / 2 + 50;
                continue;
            }
            if (score >= beta)
            {
                window += window / 2 + 50;
                continue;
            }
            break;
        }
    }

    /// <summary>
    /// AlphaBeta搜索 - 核心搜索函数
    /// </summary>
    private int AlphaBeta(int depth, int alpha, int beta)
    {
        // 叶子节点返回评估值
        if (depth <= 0)
        {
            return Eveluate(board, true);
        }

        // 生成走法
        bool redToMove = (searchDepth - depth) % 2 == 0;
        List<ChessReseting.Chess> moveList = GenerateMoves(board, redToMove);

        // 无走法 => 被将死或和棋
        if (moveList.Count == 0)
        {
            if (IsKingKilled(board, redToMove))
                return -10000 - depth; // 被将死，越早越差
            return 0; // 和棋
        }

        // 走法排序（历史启发 + 吃子优先）
        SortMoves(moveList, depth);

        int bestScore = int.MinValue;
        ChessReseting.Chess bestMove = null;

        foreach (var move in moveList)
        {
            // 执行走法
            int captured = MakeMove(move);

            // 递归搜索
            int score = -AlphaBeta(depth - 1, -beta, -alpha);

            // 撤销走法
            UnMakeMove(move, captured);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                // Beta截断 - 记录历史启发
                AddHistoryScore(move, depth);
                break;
            }
        }

        // 记录最佳走法
        if (depth == searchDepth && bestMove != null)
        {
            bestStep = bestMove;
        }

        return bestScore;
    }

    /// <summary>
    /// 主变例搜索 (PVS) - 更好的剪枝
    /// </summary>
    private int PrincipalVariation(int depth, int alpha, int beta)
    {
        if (depth <= 0)
        {
            return Eveluate(board, true);
        }

        bool redToMove = (searchDepth - depth) % 2 == 0;
        List<ChessReseting.Chess> moveList = GenerateMoves(board, redToMove);

        if (moveList.Count == 0)
        {
            if (IsKingKilled(board, redToMove))
                return -10000 - depth;
            return 0;
        }

        SortMoves(moveList, depth);

        int bestScore = int.MinValue;
        ChessReseting.Chess bestMove = null;
        bool firstMove = true;

        foreach (var move in moveList)
        {
            int captured = MakeMove(move);

            int score;
            if (firstMove)
            {
                score = -PrincipalVariation(depth - 1, -beta, -alpha);
            }
            else
            {
                // 先做零窗口搜索
                score = -PrincipalVariation(depth - 1, -alpha - 1, -alpha);
                if (score > alpha && score < beta)
                {
                    // 重新搜索
                    score = -PrincipalVariation(depth - 1, -beta, -alpha);
                }
            }
            firstMove = false;

            UnMakeMove(move, captured);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                AddHistoryScore(move, depth);
                break;
            }
        }

        if (depth == searchDepth && bestMove != null)
        {
            bestStep = bestMove;
        }

        return bestScore;
    }

    /// <summary>
    /// 执行走法，返回被吃棋子ID
    /// </summary>
    private int MakeMove(ChessReseting.Chess move)
    {
        int captured = board[move.toX, move.toY];
        board[move.toX, move.toY] = board[move.fromX, move.fromY];
        board[move.fromX, move.fromY] = 0;
        return captured;
    }

    /// <summary>
    /// 撤销走法
    /// </summary>
    private void UnMakeMove(ChessReseting.Chess move, int captured)
    {
        board[move.fromX, move.fromY] = board[move.toX, move.toY];
        board[move.toX, move.toY] = captured;
    }

    /// <summary>
    /// 判断某方是否被将死
    /// </summary>
    private bool IsKingKilled(int[,] position, bool redSide)
    {
        // 找将/帅
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int id = position[i, j];
                if (id == 0) continue;
                if (Rules.IsRed(id) != redSide) continue;
                if (Mathf.Abs(id) % 10 != 5) continue;

                // 检查是否被攻击
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
        return true; // 将/帅不见了
    }

    /// <summary>
    /// 走法排序 - 历史启发分数高的优先
    /// </summary>
    private void SortMoves(List<ChessReseting.Chess> moveList, int depth)
    {
        // 简单插入排序 (走法数量不多，用插入排序足够)
        for (int i = 1; i < moveList.Count; i++)
        {
            ChessReseting.Chess key = moveList[i];
            int keyScore = GetHistoryScore(key) + (key.chessID != 0 ? 10000 : 0);
            int j = i - 1;
            while (j >= 0 && GetHistoryScore(moveList[j]) + (moveList[j].chessID != 0 ? 10000 : 0) < keyScore)
            {
                moveList[j + 1] = moveList[j];
                j--;
            }
            moveList[j + 1] = key;
        }
    }

    /// <summary>
    /// 添加历史分数
    /// </summary>
    private void AddHistoryScore(ChessReseting.Chess move, int depth)
    {
        if (historyDic.ContainsKey(move))
            historyDic[move] += depth * depth;
        else
            historyDic[move] = depth * depth;
    }

    /// <summary>
    /// 获取历史分数
    /// </summary>
    private int GetHistoryScore(ChessReseting.Chess move)
    {
        if (historyDic.ContainsKey(move))
            return historyDic[move];
        return 0;
    }

    /// <summary>
    /// 局面评估 - 对应原版 Eveluate 函数
    /// 子力价值 + 位置价值
    /// </summary>
    private int Eveluate(int[,] position, bool redSide)
    {
        int score = 0;
        int blackScore = 0;
        int redScore = 0;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int id = position[i, j];
                if (id == 0) continue;

                int absID = Mathf.Abs(id);
                int type = absID % 10;
                int value = 0;

                switch (type)
                {
                    case 1: value = 900; break;  // 车
                    case 2: value = 400; break;  // 马
                    case 3: value = 200; break;  // 象
                    case 4: value = 150; break;  // 士
                    case 5: value = 10000; break; // 将
                    case 9: value = 900; break;  // 车
                    default:
                        if (absID >= 20 && absID <= 21 || absID >= 40 && absID <= 41)
                            value = 450; // 炮
                        else
                            value = 100; // 兵/卒
                        break;
                }

                if (Rules.IsRed(id))
                    redScore += value;
                else
                    blackScore += value;
            }
        }

        score = redScore - blackScore;
        if (!redSide) score = -score;
        return score;
    }

    /// <summary>
    /// 归并排序 - 对应原版 MergeSort
    /// </summary>
    private void MergeSort(ChessReseting.Chess[] move, int count, int depth, ChessReseting.Chess[] temp)
    {
        Sort(move, 0, count - 1, temp, depth);
    }

    private void Sort(ChessReseting.Chess[] move, int startIndex, int endIndex, ChessReseting.Chess[] temp, int depth)
    {
        if (startIndex >= endIndex) return;
        int mid = (startIndex + endIndex) / 2;
        Sort(move, startIndex, mid, temp, depth);
        Sort(move, mid + 1, endIndex, temp, depth);
        Merge(move, startIndex, mid, endIndex, temp, depth);
    }

    private void Merge(ChessReseting.Chess[] move, int startIndex, int mid, int endIndex, ChessReseting.Chess[] temp, int depth)
    {
        int i = startIndex, j = mid + 1, k = startIndex;
        while (i <= mid && j <= endIndex)
        {
            if (GetHistoryScore(move[i]) >= GetHistoryScore(move[j]))
                temp[k++] = move[i++];
            else
                temp[k++] = move[j++];
        }
        while (i <= mid) temp[k++] = move[i++];
        while (j <= endIndex) temp[k++] = move[j++];
        for (int t = startIndex; t <= endIndex; t++)
            move[t] = temp[t];
    }
}
