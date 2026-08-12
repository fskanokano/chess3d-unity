using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 场景自动构建器 - 创建与原版 1:1 对应的场景层级
/// 
/// 原版场景结构:
/// Scene
/// ├── GameManager (GameManager + SearchEngine + ChessReseting + Rules + Checkmate)
/// ├── AudioManager (AudioManager + 2x AudioSource)
/// ├── UIManager (Canvas + 所有面板)
/// │   ├── MainMenuPanel (开始按钮)
/// │   ├── ModeSelectPanel (人机/人人)
/// │   ├── DifficultyPanel (简单/中等/困难)
/// │   ├── GamePanel (回合/分数/AI提示)
/// │   ├── ResultPanel (胜负弹窗)
/// │   ├── SettingsPanel (音效/BGM开关)
/// │   └── PausePanel (悔棋/重玩/菜单)
/// ├── Board (棋盘)
/// │   ├── RedParent (红方棋子容器)
/// │   └── BlackParent (黑方棋子容器)
/// └── Main Camera (CameraRotateAroundTarget)
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    [Header("配置")]
    public GameConfig config;

    [Header("是否自动构建")]
    public bool autoBuild = true;

    private void Start()
    {
        if (autoBuild && config != null)
        {
            BuildScene();
        }
    }

    /// <summary>
    /// 构建完整场景层级
    /// </summary>
    public void BuildScene()
    {
        Debug.Log("[SceneBuilder] 开始构建场景...");

        // 1. 创建 GameManager
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        var search = gmObj.AddComponent<SearchEngine>();
        var reseting = gmObj.AddComponent<ChessReseting>();
        var rules = gmObj.AddComponent<Rules>();
        var checkmate = gmObj.AddComponent<Checkmate>();
        gm.searchEngine = search;
        gm.reseting = reseting;

        // 2. 创建 AudioManager
        var audioObj = new GameObject("AudioManager");
        var am = audioObj.AddComponent<AudioManager>();
        audioObj.AddComponent<AudioSource>();
        audioObj.AddComponent<AudioSource>();
        audioObj.AddComponent<DontDestroyOnLoadMarker>();

        // 3. 创建棋盘
        var boardObj = new GameObject("Board");
        boardObj.transform.position = Vector3.zero;

        // 棋盘网格材质
        var boardRenderer = boardObj.AddComponent<MeshRenderer>();
        var boardFilter = boardObj.AddComponent<MeshFilter>();
        // 使用平面作为棋盘
        boardFilter.mesh = CreateBoardMesh();

        // 创建棋盘纹理材质
        if (config.qipanTex != null)
        {
            var boardMat = new Material(Shader.Find("Standard"));
            boardMat.mainTexture = config.qipanTex;
            boardMat.name = "BoardMaterial";
            boardRenderer.material = boardMat;
        }

        // 4. 创建红/黑父节点
        var redParent = new GameObject("RedParent");
        redParent.transform.SetParent(boardObj.transform);
        var blackParent = new GameObject("BlackParent");
        blackParent.transform.SetParent(boardObj.transform);

        gm.board = boardObj;
        gm.redParent = redParent;
        gm.blackParent = blackParent;

        // 5. 创建摄像机
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        var camRotate = camObj.AddComponent<CameraRotateAroundTarget>();
        camObj.transform.position = new Vector3(0, 500, -350);
        camObj.transform.rotation = Quaternion.Euler(45, 0, 0);
        cam.fieldOfView = 30;
        cam.nearClipPlane = 1f;
        cam.farClipPlane = 2000f;
        gm.cam = camObj;

        // 6. 创建 Canvas 和 UI
        BuildUI();

        Debug.Log("[SceneBuilder] 场景构建完成");
        Debug.Log($"  GameManager: {gmObj.name}");
        Debug.Log($"  AudioManager: {audioObj.name}");
        Debug.Log($"  Board: {boardObj.name}");
        Debug.Log($"  Camera: {camObj.name}");
    }

    /// <summary>
    /// 构建完整 UI 层级
    /// </summary>
    private void BuildUI()
    {
        // Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // UIManager
        var uiObj = new GameObject("UIManager");
        uiObj.transform.SetParent(canvasObj.transform);
        var ui = uiObj.AddComponent<UIManager>();

        // EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 主菜单面板
        var mainMenu = CreatePanel(canvasObj.transform, "MainMenuPanel", new Color(0.1f, 0.1f, 0.2f, 0.9f));
        CreateButton(mainMenu.transform, "StartButton", "开始游戏", new Vector2(0, 60));
        CreateButton(mainMenu.transform, "SettingsButton", "设置", new Vector2(0, -20));

        // 模式选择面板
        var modePanel = CreatePanel(canvasObj.transform, "ModeSelectPanel", new Color(0.1f, 0.1f, 0.2f, 0.9f));
        CreateButton(modePanel.transform, "PVEButton", "人机对战", new Vector2(0, 60));
        CreateButton(modePanel.transform, "PVPButton", "人人对战", new Vector2(0, -20));

        // 难度选择面板
        var diffPanel = CreatePanel(canvasObj.transform, "DifficultyPanel", new Color(0.1f, 0.1f, 0.2f, 0.9f));
        CreateButton(diffPanel.transform, "EasyButton", "简单", new Vector2(0, 100));
        CreateButton(diffPanel.transform, "MediumButton", "中等", new Vector2(0, 20));
        CreateButton(diffPanel.transform, "HardButton", "困难", new Vector2(0, -60));

        // 游戏面板
        var gamePanel = CreatePanel(canvasObj.transform, "GamePanel", new Color(0, 0, 0, 0));
        var turnText = CreateText(gamePanel.transform, "TurnText", "红方走棋", new Vector2(0, 400));
        var scoreText = CreateText(gamePanel.transform, "ScoreText", "得分: 0", new Vector2(350, 400));
        var aiText = CreateText(gamePanel.transform, "AIThinkingText", "AI思考中...", new Vector2(0, 0));
        aiText.gameObject.SetActive(false);

        // 结算面板
        var resultPanel = CreatePanel(canvasObj.transform, "ResultPanel", new Color(0, 0, 0, 0.8f));
        var resultText = CreateText(resultPanel.transform, "ResultText", "胜利!", new Vector2(0, 60));
        var subText = CreateText(resultPanel.transform, "ResultSubText", "你赢了这局象棋", new Vector2(0, 0));
        CreateButton(resultPanel.transform, "ReplayButton", "重玩", new Vector2(0, -60));
        CreateButton(resultPanel.transform, "MenuButton", "返回菜单", new Vector2(0, -120));

        // 设置面板
        var settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", new Color(0.1f, 0.1f, 0.2f, 0.95f));
        CreateToggle(settingsPanel.transform, "SoundToggle", "音效", new Vector2(0, 60));
        CreateToggle(settingsPanel.transform, "BGMToggle", "背景音乐", new Vector2(0, 0));
        CreateButton(settingsPanel.transform, "CloseSettingsButton", "关闭", new Vector2(0, -60));

        // 暂停面板
        var pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0, 0, 0, 0.8f));
        CreateButton(pausePanel.transform, "UndoButton", "悔棋", new Vector2(0, 60));
        CreateButton(pausePanel.transform, "ReplayButton2", "重玩", new Vector2(0, 0));
        CreateButton(pausePanel.transform, "ResumeButton", "继续", new Vector2(0, -60));
        CreateButton(pausePanel.transform, "MenuButton2", "返回菜单", new Vector2(0, -120));

        // 绑定 UIManager 引用
        ui.mainMenuPanel = mainMenu;
        ui.modeSelectPanel = modePanel;
        ui.difficultyPanel = diffPanel;
        ui.gamePanel = gamePanel;
        ui.resultPanel = resultPanel;
        ui.settingsPanel = settingsPanel;
        ui.pausePanel = pausePanel;
        ui.turnText = turnText;
        ui.scoreText = scoreText;
        ui.aiThinkingText = aiText;
        ui.resultText = resultText;
        ui.resultSubText = subText;

        // 初始状态：只显示主菜单
        mainMenu.SetActive(true);
        modePanel.SetActive(false);
        diffPanel.SetActive(false);
        gamePanel.SetActive(false);
        resultPanel.SetActive(false);
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);

        Debug.Log("[SceneBuilder] UI 构建完成 - 7个面板");
    }

    // === UI 工具方法 ===

    private GameObject CreatePanel(Transform parent, string name, Color bgColor)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        if (bgColor.a > 0)
        {
            var img = panel.AddComponent<Image>();
            img.color = bgColor;
        }

        return panel;
    }

    private Button CreateButton(Transform parent, string name, string text, Vector2 pos)
    {
        var btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(200, 50);

        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.8f, 0.9f);

        var btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.7f);
        btn.colors = colors;

        // 按钮文字
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        var txt = textObj.AddComponent<Text>();
        txt.text = text;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 24;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return btn;
    }

    private Toggle CreateToggle(Transform parent, string name, string label, Vector2 pos)
    {
        var toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        var rect = toggleObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(200, 40);

        // Toggle 背景
        var bg = new GameObject("Background");
        bg.transform.SetParent(toggleObj.transform, false);
        var bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.anchoredPosition = new Vector2(-70, 0);
        bgRect.sizeDelta = new Vector2(40, 40);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f);

        // Checkmark
        var check = new GameObject("Checkmark");
        check.transform.SetParent(bg.transform, false);
        var checkRect = check.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = new Vector2(-10, -10);
        var checkImg = check.AddComponent<Image>();
        checkImg.color = Color.green;

        // Label
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(-30, 0);
        labelRect.offsetMax = Vector2.zero;
        var labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.fontSize = 20;

        var toggle = toggleObj.AddComponent<Toggle>();
        toggle.graphic = checkImg;
        toggle.targetGraphic = bgImg;
        toggle.isOn = true;

        return toggle;
    }

    private Text CreateText(Transform parent, string name, string content, Vector2 pos)
    {
        var textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        var rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(400, 60);

        var txt = textObj.AddComponent<Text>();
        txt.text = content;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 28;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 描边
        var outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        return txt;
    }

    /// <summary>
    /// 创建棋盘平面网格
    /// </summary>
    private Mesh CreateBoardMesh()
    {
        var mesh = new Mesh();
        mesh.name = "BoardPlane";

        // 9列 x 10行的棋盘
        float width = 8 * 69.9f;  // 8个间隔
        float height = 9 * 71.8f; // 9个间隔

        mesh.vertices = new Vector3[]
        {
            new Vector3(-width/2, 0, -height/2),
            new Vector3(width/2, 0, -height/2),
            new Vector3(width/2, 0, height/2),
            new Vector3(-width/2, 0, height/2)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
