using System.Collections;
using UnityEngine;

/// <summary>
/// 棋盘格子行为 - 对应原版 ChessOrGrid : MonoBehaviour
/// 处理点击、高亮、AI机器人逻辑
/// </summary>
public class ChessOrGrid : MonoBehaviour
{
    private int zX, zY;
    private bool isSelected, isCanMove, isCanEat;
    public bool isAI = false;

    [Header("高亮材质")]
    public Material canMoveMat, canEatMat;

    private float mTime;
    private bool isAuto;

    private void Start()
    {
        if (GameManager.instance != null)
        {
            isAI = GameManager.instance.IsAI;
        }
    }

    /// <summary>
    /// AI自动走棋协程
    /// </summary>
    private IEnumerator AIRobot()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.5f);
            // 等待玩家走完
            yield return new WaitUntil(() => !GameManager.instance.isPlaying);
            yield return new WaitForSeconds(0.5f);
            // 触发AI
            if (GameManager.instance != null && GameManager.instance.IsAI)
            {
                GameManager.instance.AIRobot();
            }
        }
    }

    private void Update()
    {
        if (isAuto && isAI && GameManager.instance != null)
        {
            if (!GameManager.instance.isPlaying && GameManager.instance.playerIsRed)
            {
                StartCoroutine(AIRobot());
                isAuto = false;
            }
        }
    }

    /// <summary>
    /// 格子被点击 - 处理移动/吃子逻辑
    /// </summary>
    private void OnMouseDown()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudio(0);
        }

        if (GameManager.instance == null) return;
        if (!GameManager.instance.isPlaying && isCanMove) return;

        int fromX = GameManager.instance.clickPointX;
        int fromY = GameManager.instance.clickPointY;

        // 计算从 chessOneID 到 chessTwoID 的对应关系
        int fromChess = GameManager.instance.chessBoard[fromX, fromY];
        int toChess = GameManager.instance.chessBoard[zX, zY];

        // 如果点击的是敌方棋子
        if (toChess != 0)
        {
            if (GameManager.instance.playerIsRed)
            {
                if (toChess > 0) // 黑方棋子
                {
                    GameManager.instance.chessTwoID = toChess;
                    GameManager.instance.clickX = zX;
                    GameManager.instance.clickY = zY;
                }
                else
                {
                    GameManager.instance.chessOneID = fromChess;
                }
            }
            else
            {
                if (toChess < 0) // 红方棋子
                {
                    GameManager.instance.chessTwoID = toChess;
                    GameManager.instance.clickX = zX;
                    GameManager.instance.clickY = zY;
                }
                else
                {
                    GameManager.instance.chessOneID = fromChess;
                }
            }
        }

        // 转换坐标
        int toXX = zX;
        int toYY = zY;
        int fromXX = fromX;
        int fromYY = fromY;

        // 旋转棋盘180度
        if (GameManager.instance.isChessBoardRotation180)
        {
            toXX = 8 - toXX;
            toYY = 9 - toYY;
            fromXX = 8 - fromXX;
            fromYY = 9 - fromYY;
        }

        GameManager.instance.clickX = zX;
        GameManager.instance.clickY = zY;

        // 判断是否可以移动
        if (isCanMove && isCanEat)
        {
            GameManager.instance.isPlaying = false;
            GameManager.instance.AIClickChess(zX, zY);
            GameManager.instance.MoveChess(fromXX, fromYY, toXX, toYY);
        }

        if (GameManager.instance.isPlaying && !isAI)
        {
            if (GameManager.instance.chessOneID != 0)
            {
                GameManager.instance.clickX = zX;
                GameManager.instance.clickY = zY;
                GameManager.instance.isPlaying = false;
                GameManager.instance.AIClickChess(zX, zY);
                GameManager.instance.MoveChess(fromXX, fromYY, toXX, toYY);
            }
        }

        // 清除高亮
        var grid = GameManager.instance.boardGrid[zX, zY];
        var render = grid.GetComponent<MeshRenderer>();
        GameManager.instance.ClearChessBoard();
        GameManager.instance.chessOneID = 0;
    }

    /// <summary>
    /// 重置选择状态
    /// </summary>
    public void ResetIsCanMove()
    {
        isSelected = false;
        isCanMove = false;
        isCanEat = false;
    }

    /// <summary>
    /// 标记为可移动
    /// </summary>
    public void SetIsCanMove(bool b)
    {
        isCanMove = b;
        isSelected = true;
        if (b)
        {
            var grid = GameManager.instance.boardGrid[zX, zY];
            var render = grid.GetComponent<MeshRenderer>();
            if (render != null && canMoveMat != null)
            {
                render.material = canMoveMat;
            }
        }
    }

    /// <summary>
    /// 标记为可吃子
    /// </summary>
    public void SetIsCanEat(bool b)
    {
        isCanEat = b;
        isSelected = true;
        if (b)
        {
            var grid = GameManager.instance.boardGrid[zX, zY];
            var render = grid.GetComponent<MeshRenderer>();
            if (render != null && canEatMat != null)
            {
                render.material = canEatMat;
            }
        }
    }

    public int GetZX() { return zX; }
    public int GetZY() { return zY; }
    public void SetZX(int x) { zX = x; }
    public void SetZY(int y) { zY = y; }

    public bool GetIsCanMove() { return isCanMove; }
    public bool GetIsCanEat() { return isCanEat; }
    public bool GetIsSelected() { return isSelected; }
}
