/// <summary>
/// UI管理器 - 对应原版 UIManager : MonoBehaviour
/// 管理所有UI面板：主菜单、模式选择、难度选择、游戏界面、结算弹窗
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("主菜单")]
    public GameObject mainMenuPanel;

    [Header("模式选择")]
    public GameObject modeSelectPanel;

    [Header("难度选择")]
    public GameObject difficultyPanel;

    [Header("游戏界面")]
    public GameObject gamePanel;
    public Text turnText;          // 回合提示
    public Text scoreText;         // 得分
    public Text aiThinkingText;    // AI思考中

    [Header("结算弹窗")]
    public GameObject resultPanel;
    public Text resultText;
    public Text resultSubText;

    [Header("设置")]
    public GameObject settingsPanel;
    public Toggle soundToggle;
    public Toggle bgmToggle;

    [Header("暂停")]
    public GameObject pausePanel;
    public Button undoButton;      // 悔棋
    public Button replayButton;    // 重玩
    public Button menuButton;      // 返回菜单

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowMainMenu();
    }

    /// <summary>
    /// 显示主菜单
    /// </summary>
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        modeSelectPanel?.SetActive(false);
        difficultyPanel?.SetActive(false);
        gamePanel?.SetActive(false);
        resultPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        pausePanel?.SetActive(false);
    }

    /// <summary>
    /// 点击开始游戏
    /// </summary>
    public void OnClickStart()
    {
        AudioManager.instance?.PlayAudio(0);
        mainMenuPanel?.SetActive(false);
        modeSelectPanel?.SetActive(true);
    }

    /// <summary>
    /// 选择模式: 0=单机(人机), 1=人人
    /// </summary>
    public void OnSelectMode(int mode)
    {
        AudioManager.instance?.PlayAudio(0);
        PlayerPrefs.SetInt("gameType", mode);

        if (mode == 0)
        {
            // 人机模式 - 先选难度
            modeSelectPanel?.SetActive(false);
            difficultyPanel?.SetActive(true);
        }
        else
        {
            // 人人模式 - 直接开始
            StartGame();
        }
    }

    /// <summary>
    /// 选择难度: 0=简单, 1=中等, 2=困难
    /// </summary>
    public void OnSelectDifficulty(int level)
    {
        AudioManager.instance?.PlayAudio(0);
        PlayerPrefs.SetInt("difficulty", level);
        difficultyPanel?.SetActive(false);
        StartGame();
    }

    /// <summary>
    /// 选择执子颜色: true=红, false=黑
    /// </summary>
    public void OnSelectColor(bool isRed)
    {
        AudioManager.instance?.PlayAudio(0);
        PlayerPrefs.SetInt("playerIsRed", isRed ? 1 : 0);
        PlayerPrefs.SetInt("isBlack", isRed ? 0 : 1);
        PlayerPrefs.Save();
        StartGame();
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        gamePanel?.SetActive(true);
        resultPanel?.SetActive(false);

        int mode = PlayerPrefs.GetInt("gameType", 0);
        if (mode == 0)
        {
            // 人机 - 显示颜色选择
            // (简化: 默认红方)
        }

        if (turnText != null)
            turnText.text = "红方走棋";

        // 通知GameManager开始
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetGame();
        }
    }

    /// <summary>
    /// 更新回合显示
    /// </summary>
    public void UpdateTurnDisplay(bool redTurn)
    {
        if (turnText != null)
        {
            turnText.text = redTurn ? "红方走棋" : "黑方走棋";
        }
    }

    /// <summary>
    /// AI思考显示
    /// </summary>
    public void ShowAIThinking(bool show)
    {
        if (aiThinkingText != null)
        {
            aiThinkingText.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// 显示游戏结果
    /// </summary>
    public void ShowResult(bool playerWin)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            if (resultText != null)
            {
                resultText.text = playerWin ? "胜利!" : "失败!";
            }
            if (resultSubText != null)
            {
                resultSubText.text = playerWin ? "你赢了这局象棋" : "再接再厉";
            }
        }

        // 播放音效
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudio(playerWin ? 4 : 5);
        }
    }

    /// <summary>
    /// 悔棋按钮
    /// </summary>
    public void OnClickUndo()
    {
        AudioManager.instance?.PlayAudio(0);
        if (GameManager.instance != null && GameManager.instance.reseting != null)
        {
            GameManager.instance.reseting.UndoMove(GameManager.instance);
        }
    }

    /// <summary>
    /// 重玩按钮
    /// </summary>
    public void OnClickReplay()
    {
        AudioManager.instance?.PlayAudio(0);
        resultPanel?.SetActive(false);
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetGame();
        }
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void OnClickMenu()
    {
        AudioManager.instance?.PlayAudio(0);
        resultPanel?.SetActive(false);
        gamePanel?.SetActive(false);
        ShowMainMenu();
    }

    /// <summary>
    /// 打开设置
    /// </summary>
    public void OnClickSettings()
    {
        AudioManager.instance?.PlayAudio(0);
        settingsPanel?.SetActive(true);
        if (soundToggle != null)
            soundToggle.isOn = PlayerPrefs.GetInt("isMute", 0) != 1;
        if (bgmToggle != null)
            bgmToggle.isOn = PlayerPrefs.GetInt("isBgmMute", 0) != 1;
    }

    /// <summary>
    /// 关闭设置
    /// </summary>
    public void OnCloseSettings()
    {
        AudioManager.instance?.PlayAudio(0);
        settingsPanel?.SetActive(false);
    }

    /// <summary>
    /// 音效开关
    /// </summary>
    public void OnToggleSound(bool on)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.isMute = !on;
            PlayerPrefs.SetInt("isMute", on ? 0 : 1);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 背景音乐开关
    /// </summary>
    public void OnToggleBgm(bool on)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.isBgmMute = !on;
            PlayerPrefs.SetInt("isBgmMute", on ? 0 : 1);
            PlayerPrefs.Save();
            if (AudioManager.instance.bgmSource != null)
            {
                if (on) AudioManager.instance.bgmSource.Play();
                else AudioManager.instance.bgmSource.Stop();
            }
        }
    }

    /// <summary>
    /// 暂停按钮
    /// </summary>
    public void OnClickPause()
    {
        AudioManager.instance?.PlayAudio(0);
        pausePanel?.SetActive(true);
    }

    /// <summary>
    /// 继续按钮
    /// </summary>
    public void OnClickResume()
    {
        AudioManager.instance?.PlayAudio(0);
        pausePanel?.SetActive(false);
    }
}
