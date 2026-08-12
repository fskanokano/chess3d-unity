using UnityEngine;
using System.Collections;

/// <summary>
/// 游戏主控制器 - 1:1 还原原版游戏流程
/// 
/// 原版游戏流程:
/// 1. 启动 → Splash Screen (Unity Logo)
/// 2. 加载主菜单 (MainMenuPanel)
/// 3. 选择模式 (人机/人人)
/// 4. 人机模式: 选择难度 (简单/中等/困难)
/// 5. 选择执子颜色 (红/黑)
/// 6. 游戏开始 → 初始化棋盘 + 棋子
/// 7. 游戏循环: 点击棋子 → 显示可走位置 → 移动/吃子
/// 8. AI回合: 思考 → 自动走棋
/// 9. 将军/将死检测 → 结算弹窗
/// 10. 悔棋/重玩/返回菜单
/// 
/// 依赖组件:
/// - GameManager (棋盘状态 + 棋子创建 + 移动)
/// - AudioManager (音效播放)
/// - UIManager (UI 面板管理)
/// - SearchEngine (AI 搜索)
/// - ChessReseting (悔棋历史)
/// - Rules (走法规则)
/// - Checkmate (将军/将死检测)
/// - Character (棋子动画/材质)
/// - ChessOrGrid (棋盘格子交互)
/// </summary>
public partial class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("引用")]
    public GameManager gm;
    public AudioManager am;
    public UIManager ui;
    public SearchEngine ai;
    public ChessReseting reseting;

    [Header("游戏状态")]
    private bool isAIThinking = false;
    private int currentTurn = 0; // 0=红方, 1=黑方

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 查找所有管理器
        gm = FindFirstObjectByType<GameManager>();
        am = FindFirstObjectByType<AudioManager>();
        ui = FindFirstObjectByType<UIManager>();
        ai = FindFirstObjectByType<SearchEngine>();
        reseting = FindFirstObjectByType<ChessReseting>();

        // 初始化音效
        InitAudio();

        // 显示主菜单
        if (ui != null)
            ui.ShowMainMenu();

        Debug.Log("[GameController] 游戏初始化完成");
    }

    /// <summary>
    /// 初始化音效系统 - 加载所有 OGG 音效
    /// </summary>
    private void InitAudio()
    {
        if (am == null) return;

        // 加载音效 (从 Resources/Audio/)
        am.bgmAudio = Resources.Load<AudioClip>("Audio/bg01");
        am.moveAudio = Resources.Load<AudioClip>("Audio/ChessMove3");
        am.eatAudio = Resources.Load<AudioClip>("Audio/EatChess");
        am.attackAudio = Resources.Load<AudioClip>("Audio/attack2");
        am.winAudio = Resources.Load<AudioClip>("Audio/shengli");
        am.loseAudio = Resources.Load<AudioClip>("Audio/bai");
        am.clickAudio = Resources.Load<AudioClip>("Audio/Button");

        Debug.Log("[GameController] 音效加载完成");
    }

    /// <summary>
    /// 开始新游戏 - 从 UI 调用
    /// </summary>
    public void StartNewGame()
    {
        if (gm != null)
        {
            gm.ResetGame();
        }

        currentTurn = 0; // 红方先行
        isAIThinking = false;

        if (ui != null)
        {
            ui.UpdateTurnDisplay(true);
            ui.ShowAIThinking(false);
        }

        // 如果 AI 先手（玩家选黑）
        if (gm != null && gm.IsAI && !gm.playerIsRed)
        {
            StartCoroutine(AIFirstMove());
        }
    }

    /// <summary>
    /// AI 先手移动
    /// </summary>
    private IEnumerator AIFirstMove()
    {
        yield return new WaitForSeconds(1f);
        if (gm != null && gm.IsAI)
        {
            isAIThinking = true;
            if (ui != null) ui.ShowAIThinking(true);

            yield return new WaitForSeconds(0.5f);

            gm.AIRobot();
            isAIThinking = false;
            if (ui != null) ui.ShowAIThinking(false);

            // 播放 AI 棋子语音
            PlayPieceVoice(gm.chessBoard[gm.clickX, gm.clickY]);
        }
    }

    /// <summary>
    /// 玩家走棋完成后的回调
    /// </summary>
    public void OnPlayerMoveComplete()
    {
        currentTurn = 1 - currentTurn;

        // 检查将军/将死
        bool isRedTurn = currentTurn == 0;
        // Checkmate 为静态类，直接调用
        {
            if (Checkmate.JudgeIfCheckmate(gm.chessBoard, !isRedTurn))
            {
                // 将死！
                OnGameOver(isRedTurn);
                return;
            }
            if (Checkmate.JudgeIfCheck(gm.chessBoard, !isRedTurn))
            {
                // 被将军
                if (am != null) am.PlayAudio(3); // 攻击音效
                if (ui != null) ui.UpdateTurnDisplay(isRedTurn);
                return;
            }
        }

        // 更新回合显示
        if (ui != null)
            ui.UpdateTurnDisplay(isRedTurn);

        // AI 回合
        if (gm != null && gm.IsAI && !isAIThinking)
        {
            StartCoroutine(AIMove());
        }
    }
}
