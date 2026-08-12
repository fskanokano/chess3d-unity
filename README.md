# 中国象棋3D天下无敌 — 逆向 Unity 项目对接文档

## 项目简介
**逆向目标**: Unity 6000.0.28f1 + IL2CPP  
**原包**: `com.chineseschess.dayz` (108MB)  
**引擎**: Unity URP 14 / InputSystem

---

## 目录结构

```
UnityProject/Assets/
├── Scripts/
│   ├── Engine/
│   │   ├── GameManager.cs          # 主管理器（棋盘创建/走法/移动）
│   │   ├── GameSetup.cs            # NEW 启动配置器（资源绑定）
│   │   ├── GameController.cs       # NEW 游戏主控制器（流程控制）
│   │   ├── ChessMaterialFactory.cs # NEW 材质工厂（红/黑纹理绑定）
│   │   ├── SceneBuilder.cs         # NEW 场景构建器（UI+场景层级）
│   │   ├── Character.cs            # 棋子基类（动画/材质切换）
│   │   ├── MovingOfChess.cs        # 各类走法规则（车马象士将炮兵）
│   │   ├── Rules.cs                # 规则验证（同侧/合法性/王对王）
│   │   ├── Checkmate.cs            # 将死检测
│   │   ├── ChessOrGrid.cs          # 棋盘格子（点击/高亮/吃子）
│   │   ├── ChessReseting.cs        # 悔棋/历史
│   │   ├── CameraRotateAroundTarget.cs # 摄像机环绕
│   │   ├── Pieces.cs               # 棋子子类（兵马炮仕帅相等）
│   │   └── AI/SearchEngine.cs      # AI（NegaMax+AlphaBeta+渴望搜索）
│   ├── UI/
│   │   ├── UIManager.cs            # 7个面板管理
│   │   ├── MyGameUI.cs             # 游戏内UI
│   │   └── SceneLoader.cs          # 场景加载
│   ├── Data/
│   │   ├── GameConfig.cs           # NEW ScriptableObject资源配置
│   │   ├── MyData.cs               # 存档（PlayerPrefs）
│   │   └── DontDestroyOnLoadMarker.cs # 场景持久化标记
│   └── Ads/
│       ├── AppOpenAdController.cs  # 开屏广告
│       ├── MyAdsInitialize.cs      # AdMob初始化
│       └── RewardedAdController.cs # 激励视频
└── Resources/
    ├── Audio/      # 19个 OGG（bg01/ChessMove3/EatChess/JiangJun/7个棋子语音等）
    ├── Fonts/      # 2个 TTF（刘梦吟书法/方正流行体繁）
    ├── Textures/   # 211张（含棋子纹理 pao_r/b, xiang_r/b等；最高2444px）
    ├── Sprites/    # 28个棋子精灵（2048x2048图集切割）
    ├── Meshes/     # 282个 GLB（兵6.5MB 帅4.5MB 士1.4MB 马1.2MB 车1.0MB等）
    ├── Materials/  # 材质定义
    ├── Shaders/    # 11个 Shader
    └── Effects/    # 特效
```

---

## 对接配置

### 1. 资源整合（已完成）
```bash
python3 integrate_assets.py
# → 643个资源文件自动同步到 Resources/
```

### 2. GameConfig（ScriptableObject）
在 Unity 中：
1. `右键 > Create > Chess3D > GameConfig`
2. 拖入 7个棋子预制体、13个纹理、19个音效、精灵图集
3. 保存为 `Assets/Resources/GameConfig.asset`

门拥有 `GetTextureByID()` / `GetVoiceByID()` 便捷方法。

### 3. 自动启动（两种方案）

**方案 A — Inspector 配置**
```csharp
// 场景根节点挂载 GameSetup
// 拖入 GameConfig + GameManager + AudioManager + UIManager + Board + Camera
// Awake 时自动绑定所有引用
```

**方案 B — 运行时动态创建（推荐用于测试）**
```csharp
// 空场景挂载 SceneBuilder（config=GameConfig）
// autoBuild=true 时自动创建完整场景层级（GameManager/AudioManager/Canvas/Board等）
```

**方案 C — 单一入口（完整游戏流程）**
```csharp
// 空场景挂载 GameController（可配合 SceneBuilder 共享引用）
// 管理: 模式选择→难度→颜色→对局→AI→将军→结算→悔棋
```

---

## 编译验证

```bash
# dotnet 模拟Unity环境编译（排除 Editor 依赖更好结果）
dotnet build GameCore.csproj  # 核心脚本: 0错误 ✓
dotnet build GameFull.csproj  # 全部脚本: 需 Unity 真实环境
```

核心18个脚本已验证 0 编译错误。

---

## 流日志

| 步骤 | 状态 | 备注 |
|------|------|------|
| APK解包 | ✓ | jdgui + AssetRipper |
| IL2CPP dump | ✓ | Il2CppDumper + dummy DLL |
| GLB/OGG/PNG提取 | ✓ | AssetRipper + glibc兼容运行 |
| Unity重构脚本 | ✓ | GameManager/SearchEngine/规则/网络验证跳过 |
| 资源导入 | ✓ | 643文件自动映射 |
| 脚本编译 | ✓ | 核心0错误 / 辅助需Unity |
| 场景搭建 | WIP | SceneBuilder / 需在Unity Editor内验证 |
| Unity运行时 | WIP | 需在真机/桌面Unity Editor测试 |

---

## 下一步

1. 将项目在 Unity 6 (6000.0.28f1) 或 Unity 2022.3 LTS 中打开
2. 导入 `com.unity.render-pipelines.universal` 14.0.11（如用2022.3）或 17.x（如用Unity6）
3. 创建 `GameConfig.asset` 并绑定所有纹理/预制体/音效
4. 挂载 `SceneBuilder` 或 `GameSetup+GameController` 到空场景
5. Play 按钮进入主菜单 → 人机/人人 → 开始对局

---

*逆向于 2026-08-12 | Alpine arm64 PRoot 环境*
