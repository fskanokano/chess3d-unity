/// <summary>
/// 游戏数据管理 - 对应原版 MyData : MonoBehaviour
/// 存储得分、设置等数据
/// </summary>
using UnityEngine;

public class MyData : MonoBehaviour
{
    public static MyData instance;

    [Header("得分数据")]
    public int bestScore;      // 最高分
    public int totalGames;     // 总局数
    public int winCount;       // 胜局数
    public int loseCount;      // 败局数

    [Header("设置")]
    public bool isAI;          // 是否人机模式
    public bool playerIsRed;   // 玩家是否红方
    public int difficulty;     // 难度
    public bool soundOn;       // 音效
    public bool bgmOn;         // 背景音乐

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 加载存档数据
    /// </summary>
    public void LoadData()
    {
        bestScore = PlayerPrefs.GetInt("bestScore", 0);
        totalGames = PlayerPrefs.GetInt("totalGames", 0);
        winCount = PlayerPrefs.GetInt("winCount", 0);
        loseCount = PlayerPrefs.GetInt("loseCount", 0);
        isAI = PlayerPrefs.GetInt("isAI", 1) == 1;
        playerIsRed = PlayerPrefs.GetInt("playerIsRed", 1) == 1;
        difficulty = PlayerPrefs.GetInt("difficulty", 1);
        soundOn = PlayerPrefs.GetInt("isMute", 0) == 0;
        bgmOn = PlayerPrefs.GetInt("isBgmMute", 0) == 0;
    }

    /// <summary>
    /// 保存所有数据
    /// </summary>
    public void SaveData()
    {
        PlayerPrefs.SetInt("bestScore", bestScore);
        PlayerPrefs.SetInt("totalGames", totalGames);
        PlayerPrefs.SetInt("winCount", winCount);
        PlayerPrefs.SetInt("loseCount", loseCount);
        PlayerPrefs.SetInt("isAI", isAI ? 1 : 0);
        PlayerPrefs.SetInt("playerIsRed", playerIsRed ? 1 : 0);
        PlayerPrefs.SetInt("difficulty", difficulty);
        PlayerPrefs.SetInt("isMute", soundOn ? 0 : 1);
        PlayerPrefs.SetInt("isBgmMute", bgmOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 记录一局结果
    /// </summary>
    public void RecordGameResult(bool win)
    {
        totalGames++;
        if (win) winCount++;
        else loseCount++;
        SaveData();
    }
}
