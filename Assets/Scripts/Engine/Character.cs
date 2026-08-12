using System.Collections;
using UnityEngine;

/// <summary>
/// 棋子角色基类 - 对应原版 Character : MonoBehaviour
/// 管理3D角色动画、材质切换、缩放特效
/// </summary>
public class Character : MonoBehaviour
{
    [Header("3D角色组件")]
    public Animator charAnimator;
    public GameObject[] materials;

    [Header("状态")]
    protected bool isblue;

    [Header("缩放动画")]
    private float scaleFactor;
    private float animationTime;
    private Vector3 originalScale;

    public virtual void Start()
    {
        originalScale = transform.localScale;
        charAnimator = GetComponent<Animator>();
    }

    /// <summary>
    /// 切换棋子材质/颜色（红蓝双方）
    /// </summary>
    public virtual void ChangeTextureFunc(string textureName, bool isblu = false)
    {
        isblue = isblu;
        foreach (var mat in materials)
        {
            mat.SetActive(false);
        }
        foreach (var mat in materials)
        {
            if (mat.name == textureName)
            {
                mat.SetActive(true);
                break;
            }
        }
    }

    /// <summary>
    /// 启动随机待机动画计时器
    /// </summary>
    public virtual void RandomAnimActionTimer(float minTimer, float maxTimer)
    {
        StartCoroutine(RandomAnimAction(minTimer, maxTimer));
    }

    private IEnumerator RandomAnimAction(float minTimer, float maxTimer)
    {
        while (true)
        {
            float timer = Random.Range(minTimer, maxTimer);
            yield return new WaitForSeconds(timer);
            RandomAnimActionSpeed(0.5f, 1.5f);
        }
    }

    public virtual void RandomAnimActionSpeed(float a, float b)
    {
        if (charAnimator != null)
        {
            float speed = Random.Range(a, b);
            charAnimator.speed = speed;
        }
    }

    /// <summary>
    /// 播放点击动画
    /// </summary>
    public virtual void OnClickPlayAnim(string name = "click")
    {
        if (charAnimator != null)
        {
            charAnimator.SetTrigger(name);
        }
    }

    public void OnClickPlaySoundCharacter(string soundTypeName)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAudio(0);
        }
    }

    public virtual void RandomAnimSpeed(float minSpeed, float maxSpeed)
    {
        if (charAnimator != null)
        {
            charAnimator.speed = Random.Range(minSpeed, maxSpeed);
        }
    }

    /// <summary>
    /// 缩放弹出特效
    /// </summary>
    private IEnumerator ScaleUpCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;
        Vector3 targetScale = originalScale * 1.2f;

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;
            if (t < 0.5f)
                transform.localScale = Vector3.Lerp(startScale, targetScale, t * 2f);
            else
                transform.localScale = Vector3.Lerp(targetScale, endScale, (t - 0.5f) * 2f);
            yield return null;
        }
        transform.localScale = endScale;
    }
}
