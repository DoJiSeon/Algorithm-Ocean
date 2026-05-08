using UnityEngine;

/// <summary>
/// Water가 항상 배 중심 아래에 위치하도록 따라다님.
/// Pivot이 모서리/끝쪽에 있는 메쉬도 자동 보정.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class SeaFollower : MonoBehaviour
{
    [SerializeField] private Transform target;       // Boat

    private Vector3 pivotOffset;  // pivot → mesh center 까지의 거리 (XZ만)

    private void Awake()
    {
        // 메쉬의 로컬 중심을 월드 스케일로 변환
        var meshFilter = GetComponent<MeshFilter>();
        Vector3 localCenter = meshFilter.sharedMesh.bounds.center;
        Vector3 worldOffset = transform.TransformVector(localCenter);

        // XZ 평면 기준 pivot 보정값
        pivotOffset = new Vector3(worldOffset.x, 0, worldOffset.z);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 배 위치에서 pivot 오프셋만큼 빼주면, 메쉬의 중심이 배 위치에 옴
        transform.position = new Vector3(
            target.position.x - pivotOffset.x,
            target.position.y,
            target.position.z - pivotOffset.z
        );
    }
}