/// <summary>
/// 得分系统 - 对应原版 MyGameUI : MonoBehaviour
/// 管理游戏得分显示
/// </summary>
using UnityEngine;
using UnityEngine.UI;

public class MyGameUI : MonoBehaviour
{
    public static MyGameUI instance;

    [Header("得分显示")]
    public Text scoreText;
    public Text bestScoreText;

    private int currentScore;
    private int bestScore;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt("bestScore", 0);
        UpdateScoreDisplay();
    }

    /// <summary>
    /// 增加得分
    /// </summary>
    public void AddScore(int value)
    {
        currentScore += value;
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("bestScore", bestScore);
            PlayerPrefs.Save();
        }
        UpdateScoreDisplay();
    }

    /// <summary>
    /// 设置得分
    /// </summary>
    public void SetScore(int value)
    {
        currentScore = value;
        UpdateScoreDisplay();
    }

    public int GetCurrentScore() { return currentScore; }
    public int GetBestScore() { return bestScore; }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
        if (bestScoreText != null)
            bestScoreText.text = bestScore.ToString();
    }

    /// <summary>
    /// 重置得分
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
}
