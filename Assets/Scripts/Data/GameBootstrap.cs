using UnityEngine;

/// <summary>
/// 游戏自举器 - 挂在 Main 场景的 GameBootstrap 物体上
/// 负责在运行时自动构建完整游戏场景并启动。
/// 即使场景中没有任何预配置，也能在运行时 1:1 还原原版游戏。
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("自动构建")]
    public bool autoBuild = true;

    private void Start()
    {
        if (!autoBuild) return;

        Debug.Log("[GameBootstrap] 启动游戏...");

        // 动态添加 SceneBuilder（如果场景中还没有）
        if (FindFirstObjectByType<SceneBuilder>() == null)
        {
            gameObject.AddComponent<SceneBuilder>();
            Debug.Log("[GameBootstrap] 已添加 SceneBuilder");
        }

        // SceneBuilder 会在 Start 中自动 BuildScene()
        // GameController 则在场景构建完成后接管游戏流程

        // 尝试加载 GameConfig（若存在）
        var config = Resources.Load<GameConfig>("GameConfig");
        if (config != null)
        {
            Debug.Log("[GameBootstrap] 找到 GameConfig.asset");
        }
        else
        {
            Debug.Log("[GameBootstrap] 未找到 GameConfig.asset，资源将通过 Resources.Load 动态加载");
        }
    }
}
