/// <summary>
/// 游戏主管理器 - 对应原版 GameManager : MonoBehaviour (Singleton)
/// 管理棋盘状态、棋子创建/移动/吃子、AI调用、音效触发
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("游戏状态")]
    public int chessOneID = 0;
    public int chessTwoID = 0;
    public int chessOneType, chessTwoType;
    public int chessOneX, chessOneY, chessTwoX, chessTwoY;
    public bool isPlaying;
    public int clickX, clickY, clickPointX, clickPointY;
    public bool IsAI, IsBlackPlayer;
    public bool playerIsRed;
    public int playerType;
    public int gameType;
    public int difficulty;

    [Header("棋盘")]
    public float gridWidth = 69.9f;
    public float gridHeight = 71.8f;
    public int[,] chessBoard;
    public GameObject[,] boardGrid;

    [Header("预制体")]
    public GameObject chessPrefab;
    public GameObject gridPrefab;
    public GameObject redParent, blackParent;

    [Header("棋子预制体")]
    public GameObject bingPrefab, chePrefab, maPrefab, paoPrefab, qinHuangPrefab;
    public GameObject shiPrefab, shuaiPrefab, xiangPrefab;

    [Header("场景")]
    public GameObject board;
    public bool isChessBoardRotation180;

    [Header("引用")]
    public GameObject cam;
    public SearchEngine searchEngine;
    public ChessReseting reseting;

    // 棋子总数量
    private int chessCount = 32;

    // 棋子GameObject列表
    private List<GameObject> chessList;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        board = GameObject.Find("Board");
        boardGrid = new GameObject[9, 10];
        chessBoard = new int[9, 10];
        chessList = new List<GameObject>();

        // 从MyData读取设置
        IsAI = PlayerPrefs.GetInt("isAI", 1) == 1;
        IsBlackPlayer = PlayerPrefs.GetInt("isBlack", 0) == 1;
        gameType = PlayerPrefs.GetInt("gameType", 0);
        difficulty = PlayerPrefs.GetInt("difficulty", 1);
        playerIsRed = PlayerPrefs.GetInt("playerIsRed", 1) == 1;
        isChessBoardRotation180 = PlayerPrefs.GetInt("rot180", 0) == 1;

        InitGrid();
        InitChess();
        SetChessID();
    }

    /// <summary>
    /// 初始化棋盘格子
    /// </summary>
    public void InitGrid()
    {
        float startX = -gridWidth * 4;
        float startY = -gridHeight * 4.5f;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 pos = new Vector3(
                    board.transform.position.x + startX + i * gridWidth,
                    board.transform.position.y,
                    board.transform.position.z + startY + j * gridHeight
                );

                if (isChessBoardRotation180)
                {
                    pos.x = board.transform.position.x + (gridWidth * 4 - i * gridWidth);
                    pos.z = board.transform.position.z + (gridHeight * 4.5f - j * gridHeight);
                }

                boardGrid[i, j] = Instantiate(gridPrefab, pos, Quaternion.identity);
                boardGrid[i, j].transform.SetParent(board.transform);
                boardGrid[i, j].GetComponent<ChessOrGrid>().SetZX(i);
                boardGrid[i, j].GetComponent<ChessOrGrid>().SetZY(j);
            }
        }
    }

    /// <summary>
    /// 初始化棋盘数据 (红方在下)
    /// </summary>
    public void InitChess()
    {
        // 清空棋盘
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 10; j++)
                chessBoard[i, j] = 0;

        // 黑方（上方）- 正值
        chessBoard[0, 9] = 31; // 车
        chessBoard[1, 9] = 32; // 马
        chessBoard[2, 9] = 33; // 象
        chessBoard[3, 9] = 34; // 士
        chessBoard[4, 9] = 35; // 将
        chessBoard[5, 9] = 36; // 士
        chessBoard[6, 9] = 37; // 象
        chessBoard[7, 9] = 38; // 马
        chessBoard[8, 9] = 39; // 车
        chessBoard[1, 7] = 40; // 炮
        chessBoard[7, 7] = 41; // 炮
        chessBoard[0, 6] = 42; // 卒
        chessBoard[2, 6] = 43; // 卒
        chessBoard[4, 6] = 44; // 卒
        chessBoard[6, 6] = 45; // 卒
        chessBoard[8, 6] = 46; // 卒

        // 红方（下方）- 负值
        chessBoard[0, 0] = -11; // 车
        chessBoard[1, 0] = -12; // 马
        chessBoard[2, 0] = -13; // 相
        chessBoard[3, 0] = -14; // 仕
        chessBoard[4, 0] = -15; // 帅
        chessBoard[5, 0] = -16; // 仕
        chessBoard[6, 0] = -17; // 相
        chessBoard[7, 0] = -18; // 马
        chessBoard[8, 0] = -19; // 车
        chessBoard[1, 2] = -20; // 炮
        chessBoard[7, 2] = -21; // 炮
        chessBoard[0, 3] = -22; // 兵
        chessBoard[2, 3] = -23; // 兵
        chessBoard[4, 3] = -24; // 兵
        chessBoard[6, 3] = -25; // 兵
        chessBoard[8, 3] = -26; // 兵
    }

    /// <summary>
    /// 根据棋盘数据创建棋子GameObject
    /// </summary>
    public void CreateChess()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                int id = chessBoard[i, j];
                if (id == 0) continue;

                GameObject prefab = GetPrefabByID(id);
                if (prefab == null) continue;

                Vector3 pos = boardGrid[i, j].transform.position;
                pos.y += 2f; // 棋子略微抬高

                GameObject chess = Instantiate(prefab, pos, Quaternion.identity);
                chessList.Add(chess);

                // 设置父节点（红/黑分组）
                if (id < 0)
                    chess.transform.SetParent(redParent?.transform);
                else
                    chess.transform.SetParent(blackParent?.transform);

                // 设置材质（红/黑颜色区分）
                var character = chess.GetComponent<Character>();
                if (character != null)
                {
                    character.ChangeTextureFunc(id < 0 ? "red" : "blue", id > 0);
                }
            }
        }
    }

    /// <summary>
    /// 根据棋子ID获取对应预制体
    /// </summary>
    private GameObject GetPrefabByID(int id)
    {
        int absId = Mathf.Abs(id);
        int type = absId % 10; // 个位数代表类型

        switch (type)
        {
            case 1: return chePrefab;     // 车
            case 2: return maPrefab;      // 马
            case 3: return xiangPrefab;   // 象/相
            case 4: return shiPrefab;     // 士/仕
            case 5: return shuaiPrefab;   // 将/帅
            case 6: return shiPrefab;     // 士/仕
            case 7: return xiangPrefab;   // 象/相
            case 8: return maPrefab;      // 马
            case 9: return chePrefab;     // 车
            case 0: return paoPrefab;     // 炮 (0,1,2十位)
            default:
                if (absId >= 22 && absId <= 26) return bingPrefab; // 兵
                if (absId >= 42 && absId <= 46) return bingPrefab; // 卒
                return paoPrefab;
        }
    }

    /// <summary>
    /// 设置棋子初始ID映射
    /// </summary>
    public void SetChessID()
    {
        isPlaying = true;
    }

    /// <summary>
    /// 移动棋子 - 对应原版 MoveChess
    /// </summary>
    public void MoveChess(int fromX, int fromY, int toX, int toY)
    {
        // 保存移动历史（用于悔棋）
        if (reseting != null)
        {
            ChessReseting.Chess move = new ChessReseting.Chess();
            move.fromX = fromX; move.fromY = fromY;
            move.toX = toX; move.toY = toY;
            move.chessID = chessBoard[toX, toY];
            reseting.AddMove(move);
        }

        int chessID = chessBoard[fromX, fromY];
        chessBoard[fromX, fromY] = 0;
        chessBoard[toX, toY] = chessID;

        // 移动对应的GameObject
        if (chessTwoID != 0)
        {
            // 吃子 - 播放攻击动画
            EatChess(toX, toY);
        }

        // 播放移动音效
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudio(1);
        }

        isPlaying = true;
    }

    /// <summary>
    /// 吃子处理
    /// </summary>
    private void EatChess(int x, int y)
    {
        int chessID = chessBoard[x, y];
        // 找到被吃的棋子并销毁
        // (实际实现中需要根据chessID找到对应GameObject)
    }

    /// <summary>
    /// AI点击棋子时记录位置
    /// </summary>
    public void AIClickChess(int x, int y)
    {
        clickPointX = x;
        clickPointY = y;
    }

    /// <summary>
    /// AI自动走棋
    /// </summary>
    public void AIRobot()
    {
        if (searchEngine == null) return;

        ChessReseting.Chess bestMove = searchEngine.SearchAGoodMove(chessBoard);
        if (bestMove != null)
        {
            int fromX = bestMove.fromX, fromY = bestMove.fromY;
            int toX = bestMove.toX, toY = bestMove.toY;
            MoveChess(fromX, fromY, toX, toY);
        }
    }

    /// <summary>
    /// 清除棋盘高亮显示
    /// </summary>
    public void ClearChessBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                var grid = boardGrid[i, j];
                if (grid != null)
                {
                    var chessOrGrid = grid.GetComponent<ChessOrGrid>();
                    if (chessOrGrid != null)
                    {
                        chessOrGrid.ResetIsCanMove();
                    }
                    // 恢复默认材质
                    var render = grid.GetComponent<MeshRenderer>();
                    if (render != null)
                    {
                        render.material = null; // 恢复默认
                    }
                }
            }
        }
    }

    /// <summary>
    /// 显示棋子可移动位置
    /// </summary>
    public void ShowCanMoveChess(int[,] position)
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (position[i, j] != 0 && boardGrid[i, j] != null)
                {
                    var grid = boardGrid[i, j].GetComponent<ChessOrGrid>();
                    if (grid != null)
                    {
                        if (chessBoard[i, j] != 0)
                            grid.SetIsCanEat(true);
                        else
                            grid.SetIsCanMove(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 显示可移动和可吃子的棋子
    /// </summary>
    public void ShowCanMoveChess(int[,] canMove, int[,] canEat)
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardGrid[i, j] == null) continue;
                var grid = boardGrid[i, j].GetComponent<ChessOrGrid>();
                if (grid == null) continue;

                if (canMove[i, j] == 1)
                    grid.SetIsCanMove(true);
                if (canEat[i, j] == 1)
                    grid.SetIsCanEat(true);
            }
        }
    }

    /// <summary>
    /// 刷新棋盘显示 - 悔棋后调用
    /// 根据 chessBoard 数据重新生成所有棋子 GameObject
    /// </summary>
    public void RefreshBoard()
    {
        // 清除所有现有棋子
        foreach (var chess in chessList)
        {
            if (chess != null) Destroy(chess);
        }
        chessList.Clear();

        // 清除所有格子
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardGrid[i, j] != null)
                    Destroy(boardGrid[i, j]);
            }
        }

        // 重新初始化格子和棋子
        InitGrid();
        CreateChess();
        isPlaying = true;
    }

    /// <summary>
    /// 重置游戏
    /// </summary>
    public void ResetGame()
    {
        // 清除所有棋子
        foreach (var chess in chessList)
        {
            if (chess != null) Destroy(chess);
        }
        chessList.Clear();

        // 清除所有格子
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (boardGrid[i, j] != null)
                    Destroy(boardGrid[i, j]);
            }
        }

        // 重新初始化
        InitGrid();
        InitChess();
        CreateChess();
        isPlaying = true;

        // 清空悔棋历史
        if (reseting != null)
            reseting.ResetChess();
    }

    /// <summary>
    /// 获取棋子语音音效
    /// </summary>
    public AudioClip GetChessVoice(int chessID)
    {
        int absId = Mathf.Abs(chessID);
        int type = absId % 10;
        string voiceName = "";

        switch (type)
        {
            case 1: case 9: voiceName = "ju"; break;   // 车
            case 2: case 8: voiceName = "ma"; break;   // 马
            case 3: case 7: voiceName = "xiang"; break; // 象
            case 4: case 6: voiceName = "shi"; break;   // 士
            case 5: voiceName = "shuai"; break;          // 帅
            case 0: voiceName = "pao"; break;            // 炮
            default: voiceName = "zhu"; break;           // 卒
        }

        return Resources.Load<AudioClip>($"Audio/juese/{voiceName}");
    }
}
