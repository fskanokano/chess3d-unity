/// <summary>
/// 动画运行速度控制 - 对应原版 MyAnimRunSpeed : MonoBehaviour
/// 控制角色动画播放速度
/// </summary>
using UnityEngine;

public class MyAnimRunSpeed : MonoBehaviour
{
    [Header("动画组件")]
    public Animator animator;

    [Header("速度控制")]
    public float minSpeed = 0.8f;
    public float maxSpeed = 1.2f;
    public bool randomizeOnStart = true;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (randomizeOnStart && animator != null)
        {
            animator.speed = Random.Range(minSpeed, maxSpeed);
        }
    }

    /// <summary>
    /// 设置动画速度
    /// </summary>
    public void SetAnimSpeed(float speed)
    {
        if (animator != null)
            animator.speed = speed;
    }

    /// <summary>
    /// 播放指定动画
    /// </summary>
    public void PlayAnim(string stateName)
    {
        if (animator != null)
            animator.Play(stateName);
    }

    /// <summary>
    /// 触发动画参数
    /// </summary>
    public void SetTrigger(string triggerName)
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }
}
