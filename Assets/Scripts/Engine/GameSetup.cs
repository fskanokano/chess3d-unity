using UnityEngine;
using System.IO;

/// <summary>
/// 游戏启动配置器 - 在场景加载时自动配置所有资源引用
/// 对应原版中通过 Inspector 手动配置的所有 Prefab/AudioClip/Material 引用
/// 
/// 使用方法:
/// 1. 将此脚本挂载到场景根对象
/// 2. 在 Inspector 中拖入 GameConfig ScriptableObject
/// 3. 运行时自动完成所有资源绑定
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("资源配置")]
    public GameConfig config;

    [Header("场景对象")]
    public GameManager gameManager;
    public AudioManager audioManager;
    public UIManager uiManager;

    [Header("场景层级")]
    public GameObject board;
    public Camera mainCamera;

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("[GameSetup] GameConfig 未配置！请在 Inspector 中拖入 GameConfig.asset");
            return;
        }

        SetupGameManager();
        SetupAudioManager();
        SetupCamera();
        Debug.Log("[GameSetup] 游戏资源配置完成");
    }

    /// <summary>
    /// 配置 GameManager 的所有预制体引用
    /// </summary>
    private void SetupGameManager()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;

        // 棋子预制体
        gameManager.bingPrefab = config.bingPrefab;
        gameManager.chePrefab = config.chePrefab;
        gameManager.maPrefab = config.maPrefab;
        gameManager.paoPrefab = config.paoPrefab;
        gameManager.shiPrefab = config.shiPrefab;
        gameManager.shuaiPrefab = config.shuaiPrefab;
        gameManager.xiangPrefab = config.xiangPrefab;

        // 棋盘
        if (config.boardPrefab != null && board != null)
        {
            var boardModel = Instantiate(config.boardPrefab, board.transform.position, board.transform.rotation);
            boardModel.transform.SetParent(board.transform);
            boardModel.transform.localPosition = Vector3.zero;
            boardModel.transform.localScale = Vector3.one;
        }

        Debug.Log("[GameSetup] GameManager 配置完成 - 7个棋子预制体已绑定");
    }

    /// <>
    /// 配置 AudioManager 的所有音效引用
    /// </summary>
    private void SetupAudioManager()
    {
        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null) return;

        // 确保有两个 AudioSource（一个音效，一个BGM）
        if (audioManager.GetComponent<AudioSource>() == null)
            audioManager.gameObject.AddComponent<AudioSource>();
        var sources = audioManager.GetComponents<AudioSource>();
        if (sources.Length < 2)
            audioManager.gameObject.AddComponent<AudioSource>();

        // 重新获取引用
        audioManager.audioSource = audioManager.GetComponent<AudioSource>();
        var allSources = audioManager.GetComponents<AudioSource>();
        if (allSources.Length > 1)
            audioManager.bgmSource = allSources[1];

        // 音效
        audioManager.clickAudio = config.clickClip;
        audioManager.moveAudio = config.moveClip;
        audioManager.eatAudio = config.eatClip;
        audioManager.attackAudio = config.attackClip;
        audioManager.winAudio = config.winClip;
        audioManager.loseAudio = config.loseClip;
        audioManager.bgmAudio = config.bgmClip;

        // 配置 AudioSource
        if (audioManager.bgmSource != null && config.bgmClip != null)
        {
            audioManager.bgmSource.clip = config.bgmClip;
            audioManager.bgmSource.loop = true;
            audioManager.bgmSource.playOnAwake = false;
            if (!audioManager.isBgmMute)
                audioManager.bgmSource.Play();
        }

        Debug.Log("[GameSetup] AudioManager 配置完成 - 7个音效 + BGM 已绑定");
    }

    /// <summary>
    /// 配置摄像机
    /// </summary>
    private void SetupCamera()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null) return;

        // 3D 棋盘视角设置
        mainCamera.transform.position = new Vector3(0, 500, -350);
        mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
        mainCamera.orthographic = false;
        mainCamera.fieldOfView = 30;

        // 将摄像机引用传给 GameManager
        if (gameManager != null)
            gameManager.cam = mainCamera.gameObject;

        Debug.Log("[GameSetup] 摄像机配置完成");
    }

    /// <summary>
    /// 运行时动态加载资源（备选方案）
    /// </summary>
    public static class RuntimeLoader
    {
        /// <summary>
        /// 从 Resources 加载棋子预制体
        /// </summary>
        public static GameObject LoadPiecePrefab(string pieceName)
        {
            string path = $"Meshes/{pieceName}";
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[RuntimeLoader] 未找到预制体: {path}");
                // 尝试备选路径
                prefab = Resources.Load<GameObject>($"Prefabs/{pieceName}");
            }
            return prefab;
        }

        /// <summary>
        /// 从 Resources 加载纹理
        /// </summary>
        public static Texture2D LoadTexture(string texName)
        {
            var tex = Resources.Load<Texture2D>($"Textures/{texName}");
            if (tex == null)
                tex = Resources.Load<Texture2D>($"Sprites/{texName}");
            return tex;
        }

        /// <summary>
        /// 从 Resources 加载音效
        /// </summary>
        public static AudioClip LoadAudio(string clipName)
        {
            return Resources.Load<AudioClip>($"Audio/{clipName}");
        }
    }
}
