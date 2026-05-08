using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothTime = 0.6f;     // 감속 시간 (클수록 부드럽게)
    [SerializeField] private LayerMask seaLayer;

    private Camera cam;
    private Vector3 targetPos;
    private Vector3 velocity;                              // SmoothDamp용
    private Vector3 lastMoveDirection;
    private bool isMoving;
    private bool isStopping;

    private void Awake()
    {
        cam = Camera.main;
        targetPos = transform.position;
    }

    private void Update()
    {
        HandleClick();
        Move();
    }

    private void HandleClick()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es != null && es.IsPointerOverGameObject()) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, seaLayer))
        {
            targetPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            isMoving = true;
            isStopping = false;
        }
    }

    private void Move()
    {
        if (!isMoving) return;

        // SmoothDamp: 목표에 가까워질수록 자동으로 감속
        transform.position = Vector3.SmoothDamp(
            transform.position, targetPos, ref velocity, smoothTime, moveSpeed);

        // 회전: 현재 속도 방향을 바라봄 (목표 방향 X)
        Vector3 dir = velocity;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = dir.normalized;

            if (!isStopping)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(lastMoveDirection), 5f * Time.deltaTime);
            }
        }

        // 거의 멈췄으면 종료
        if (Vector3.Distance(transform.position, targetPos) < 0.05f && velocity.sqrMagnitude < 0.01f)
        {
            isMoving = false;
            isStopping = false;
            velocity = Vector3.zero;
        }
    }

    public void StopMoving()
    {
        // 자동 픽업 시 사용. 즉시 멈추진 않고 감속 시작
        //targetPos = transform.position;
        // 또는 더 자연스럽게: 현재 위치 살짝 앞으로
        isStopping = true;

        Vector3 stopDirection = lastMoveDirection.sqrMagnitude > 0.01f
            ? lastMoveDirection
            : transform.forward;

        targetPos = transform.position + stopDirection * 0.5f;
    }
}