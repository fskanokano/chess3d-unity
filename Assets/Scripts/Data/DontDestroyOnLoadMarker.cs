using UnityEngine;

/// <summary>
/// 标记 DontDestroyOnLoad（对应原版 DontDestroyOnLoad 辅助脚本）
/// 挂载后该物体不会在场景切换时销毁
/// </summary>
public class DontDestroyOnLoadMarker : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
