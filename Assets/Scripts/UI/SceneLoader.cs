/// <summary>
/// 场景加载器 - 对应原版 SceneLoader : MonoBehaviour
/// 支持带进度条的异步场景加载
/// </summary>
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    [Header("进度显示")]
    public Slider progressSlider;
    public Text progressText;
    public GameObject loadingPanel;

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

    /// <summary>
    /// 异步加载场景（带进度条）
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// 直接加载场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressSlider != null)
                progressSlider.value = progress;
            if (progressText != null)
                progressText.text = (int)(progress * 100) + "%";

            if (operation.progress >= 0.9f)
            {
                // 模拟加载完成
                if (progressSlider != null)
                    progressSlider.value = 1f;
                if (progressText != null)
                    progressText.text = "100%";
                yield return new WaitForSeconds(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}
