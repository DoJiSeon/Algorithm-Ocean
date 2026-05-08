using UnityEngine;
using UnityEngine.InputSystem;  // 추가

public class ShipController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask seaLayer;

    private Camera cam;
    private Vector3 targetPos;
    private bool isMoving;

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
        // 변경: Input.GetMouseButtonDown(0) → Mouse.current
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        // 변경: Input.mousePosition → Mouse.current.position.ReadValue()
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, seaLayer))
        {
            targetPos = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            isMoving = true;
        }
    }

    private void Move()
    {
        // (동일)
        if (!isMoving) return;
        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector3 dir = (targetPos - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            isMoving = false;
    }

    public void StopMoving()
    {
        isMoving = false;
        targetPos = transform.position;
    }
}