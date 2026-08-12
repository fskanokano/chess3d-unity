/// <summary>
/// 音频管理器 - 对应原版 AudioManager : MonoBehaviour
/// 管理所有音效播放
/// </summary>
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("音效来源")]
    public AudioSource audioSource;
    public AudioSource bgmSource;

    [Header("音效")]
    public AudioClip clickAudio;      // 点击
    public AudioClip moveAudio;       // 移动
    public AudioClip eatAudio;        // 吃子
    public AudioClip attackAudio;     // 攻击
    public AudioClip winAudio;        // 胜利
    public AudioClip loseAudio;       // 失败
    public AudioClip bgmAudio;        // 背景音乐

    [Header("开关")]
    public bool isMute;
    public bool isBgmMute;

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
        }
    }

    private void Start()
    {
        // 读取设置
        isMute = PlayerPrefs.GetInt("isMute", 0) == 1;
        isBgmMute = PlayerPrefs.GetInt("isBgmMute", 0) == 1;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (bgmSource == null)
            bgmSource = GetComponents<AudioSource>()?.Length > 1 ? GetComponents<AudioSource>()[1] : null;

        if (bgmSource != null && bgmAudio != null)
        {
            bgmSource.clip = bgmAudio;
            bgmSource.loop = true;
            if (!isBgmMute)
                bgmSource.Play();
        }
    }

    /// <summary>
    /// 播放音效 - 对应原版 PlayAudio
    /// </summary>
    public void PlayAudio(int index)
    {
        if (isMute) return;
        if (audioSource == null) return;

        switch (index)
        {
            case 0: // 点击
                if (clickAudio != null) audioSource.PlayOneShot(clickAudio);
                break;
            case 1: // 移动
                if (moveAudio != null) audioSource.PlayOneShot(moveAudio);
                break;
            case 2: // 吃子
                if (eatAudio != null) audioSource.PlayOneShot(eatAudio);
                break;
            case 3: // 攻击
                if (attackAudio != null) audioSource.PlayOneShot(attackAudio);
                break;
            case 4: // 胜利
                if (winAudio != null) audioSource.PlayOneShot(winAudio);
                break;
            case 5: // 失败
                if (loseAudio != null) audioSource.PlayOneShot(loseAudio);
                break;
        }
    }

    /// <summary>
    /// 播放指定音效
    /// </summary>
    public void PlayAudio(AudioClip clip)
    {
        if (isMute) return;
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 切换静音
    /// </summary>
    public void ToggleMute()
    {
        isMute = !isMute;
        PlayerPrefs.SetInt("isMute", isMute ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 切换BGM
    /// </summary>
    public void ToggleBgm()
    {
        isBgmMute = !isBgmMute;
        PlayerPrefs.SetInt("isBgmMute", isBgmMute ? 1 : 0);
        PlayerPrefs.Save();

        if (bgmSource != null)
        {
            if (isBgmMute) bgmSource.Stop();
            else if (bgmAudio != null && !bgmSource.isPlaying) bgmSource.Play();
        }
    }
}
