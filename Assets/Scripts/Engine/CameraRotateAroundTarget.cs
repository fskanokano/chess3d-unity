/// <summary>
/// 相机环绕 - 对应原版 CameraRotateAroundTarget : MonoBehaviour
/// 相机围绕棋盘旋转，支持缩放
/// </summary>
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotateAroundTarget : MonoBehaviour
{
    [Header("目标")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0, 0, 0);

    [Header("旋转")]
    public float rotateSpeed = 1.5f;
    public float currentAngle = 0f;
    public float maxAngle = 60f;

    [Header("缩放")]
    public float zoomSpeed = 2f;
    public float minDistance = 10f;
    public float maxDistance = 30f;
    public float currentDistance = 20f;

    [Header("平滑")]
    public float smoothTime = 0.1f;
    private Vector3 velocity = Vector3.zero;

    private float targetAngle;
    private float targetDistance;
    private float pitchAngle = 30f;

    private void Start()
    {
        if (target == null)
            target = GameObject.Find("Board")?.transform;

        targetAngle = currentAngle;
        targetDistance = currentDistance;

        if (target != null)
        {
            transform.position = target.position + GetOrbitPosition(currentAngle, currentDistance, pitchAngle);
            transform.LookAt(target.position + targetOffset);
        }
    }

    private void Update()
    {
        if (target == null) return;

        // 触摸/鼠标拖动旋转
        if (Input.touchCount == 1 || Input.GetMouseButton(0))
        {
            // 检查是否在UI上
            if (!IsPointerOverUI())
            {
                float deltaX = 0f;
                if (Input.touchCount == 1)
                    deltaX = Input.GetTouch(0).deltaPosition.x;
                else
                    deltaX = Input.GetAxis("Mouse X");

                targetAngle += deltaX * rotateSpeed;
            }
        }

        // 双指缩放
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            targetDistance -= difference * zoomSpeed * 0.01f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            targetDistance -= scroll * zoomSpeed * 2f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // 平滑移动
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothTime * 10f);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, smoothTime * 10f);

        Vector3 targetPos = target.position + targetOffset + GetOrbitPosition(currentAngle, currentDistance, pitchAngle);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        transform.LookAt(target.position + targetOffset);
    }

    /// <summary>
    /// 获取轨道位置
    /// </summary>
    private Vector3 GetOrbitPosition(float angle, float distance, float pitch)
    {
        float rad = angle * Mathf.Deg2Rad;
        float pitchRad = pitch * Mathf.Deg2Rad;

        float y = distance * Mathf.Sin(pitchRad);
        float horizontal = distance * Mathf.Cos(pitchRad);

        return new Vector3(
            horizontal * Mathf.Sin(rad),
            y,
            horizontal * Mathf.Cos(rad)
        );
    }

    /// <summary>
    /// 判断是否点击在UI上
    /// </summary>
    private bool IsPointerOverUI()
    {
#if ENABLE_INPUT_SYSTEM
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#else
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#endif
    }

    /// <summary>
    /// 重置相机位置
    /// </summary>
    public void ResetCamera()
    {
        targetAngle = 0f;
        targetDistance = 20f;
    }
}
