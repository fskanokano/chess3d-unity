using UnityEngine;
using System.Collections;

public partial class GameController : MonoBehaviour
{
    /// <summary>
    /// AI 走棋协程
    /// </summary>
    private IEnumerator AIMove()
    {
        isAIThinking = true;
        if (ui != null) ui.ShowAIThinking(true);

        yield return new WaitForSeconds(1f); // 模拟思考时间

        if (gm != null)
        {
            gm.AIRobot();
        }

        isAIThinking = false;
        if (ui != null) ui.ShowAIThinking(false);

        // 播放 AI 棋子语音
        if (gm != null)
        {
            PlayPieceVoice(gm.chessBoard[gm.clickX, gm.clickY]);
        }

        currentTurn = 1 - currentTurn;

        // 检查将军/将死
        bool isRedTurn = currentTurn == 0;
        // Checkmate 为静态类
        {
            if (Checkmate.JudgeIfCheckmate(gm.chessBoard, !isRedTurn))
            {
                OnGameOver(!isRedTurn);
                yield break;
            }
            if (Checkmate.JudgeIfCheck(gm.chessBoard, !isRedTurn))
            {
                if (am != null) am.PlayAudio(3);
            }
        }

        if (ui != null)
            ui.UpdateTurnDisplay(isRedTurn);
    }

    /// <summary>
    /// 播放棋子语音
    /// </summary>
    private void PlayPieceVoice(int chessID)
    {
        if (gm == null || am == null) return;

        AudioClip voice = gm.GetChessVoice(chessID);
        if (voice != null)
        {
            am.PlayAudio(voice);
        }
    }

    /// <summary>
    /// 游戏结束
    /// </summary>
    private void OnGameOver(bool redWins)
    {
        if (gm != null) gm.isPlaying = false;

        bool playerWins = (gm.playerIsRed && redWins) || (!gm.playerIsRed && !redWins);

        if (ui != null)
            ui.ShowResult(playerWins);

        // 播放胜利/失败音乐
        if (am != null)
        {
            am.PlayAudio(playerWins ? 4 : 5);
        }

        // 记录结果
        var myData = FindFirstObjectByType<MyData>();
        if (myData != null)
        {
            myData.RecordGameResult(playerWins);
        }

        Debug.Log($"[GameController] 游戏结束 - {(playerWins ? "玩家胜利" : "玩家失败")}");
    }

    /// <summary>
    /// 悔棋
    /// </summary>
    public void Undo()
    {
        if (gm == null || reseting == null) return;

        if (am != null) am.PlayAudio(0);

        // 人机模式悔两步（玩家+AI），人人模式悔一步
        bool undone = reseting.UndoMove(gm);
        if (gm.IsAI)
        {
            reseting.UndoMove(gm);
        }

        if (ui != null)
        {
            ui.UpdateTurnDisplay(currentTurn == 0);
        }
    }

    /// <summary>
    /// 重新开始
    /// </summary>
    public void Replay()
    {
        if (am != null) am.PlayAudio(0);
        StartNewGame();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void BackToMenu()
    {
        if (am != null) am.PlayAudio(0);
        if (ui != null) ui.ShowMainMenu();
    }

    /// <summary>
    /// 切换音效开关
    /// </summary>
    public void ToggleSound(bool on)
    {
        if (am != null)
        {
            am.isMute = !on;
            PlayerPrefs.SetInt("isMute", on ? 0 : 1);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 切换 BGM
    /// </summary>
    public void ToggleBgm(bool on)
    {
        if (am != null)
        {
            am.isBgmMute = !on;
            PlayerPrefs.SetInt("isBgmMute", on ? 0 : 1);
            PlayerPrefs.Save();

            if (am.bgmSource != null)
            {
                if (on && !am.bgmSource.isPlaying)
                    am.bgmSource.Play();
                else if (!on)
                    am.bgmSource.Stop();
            }
        }
    }

    /// <summary>
    /// 选择模式 (0=人机, 1=人人)
    /// </summary>
    public void SelectMode(int mode)
    {
        if (am != null) am.PlayAudio(0);
        PlayerPrefs.SetInt("gameType", mode);
        PlayerPrefs.SetInt("isAI", mode == 0 ? 1 : 0);
        PlayerPrefs.Save();

        if (gm != null)
        {
            gm.IsAI = mode == 0;
            gm.gameType = mode;
        }

        if (ui != null)
        {
            if (mode == 0)
            {
                ui.modeSelectPanel?.SetActive(false);
                ui.difficultyPanel?.SetActive(true);
            }
            else
            {
                StartNewGame();
            }
        }
    }

    /// <summary>
    /// 选择难度 (0=简单, 1=中等, 2=困难)
    /// </summary>
    public void SelectDifficulty(int level)
    {
        if (am != null) am.PlayAudio(0);
        PlayerPrefs.SetInt("difficulty", level);
        PlayerPrefs.Save();

        if (gm != null)
        {
            gm.difficulty = level;
            // 难度影响搜索深度
            if (ai != null)
            {
                ai.searchDepth = level == 0 ? 2 : (level == 1 ? 4 : 6);
            }
        }

        if (ui != null)
            ui.difficultyPanel?.SetActive(false);

        // 选择执子颜色
        ShowColorSelect();
    }

    /// <summary>
    /// 显示执子颜色选择
    /// </summary>
    private void ShowColorSelect()
    {
        if (ui == null) return;

        // 简化：默认玩家执红
        PlayerPrefs.SetInt("playerIsRed", 1);
        PlayerPrefs.SetInt("isBlack", 0);
        PlayerPrefs.Save();

        if (gm != null)
        {
            gm.playerIsRed = true;
            gm.IsBlackPlayer = false;
        }

        StartNewGame();
    }
}
