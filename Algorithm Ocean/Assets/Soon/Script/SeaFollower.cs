// Assets/Soon/Script/SeaFollower.cs
using UnityEngine;

/// <summary>
/// Sea Plane이 항상 배 아래에 있도록 따라다님.
/// 배는 자유롭게 이동하지만 Sea 끝에 도달하지 않음.
/// </summary>
public class SeaFollower : MonoBehaviour
{
    [SerializeField] private Transform target;       // Boat
    [SerializeField] private float yOffset = 0f;     // Sea Y 좌표 (보통 0)

    private void LateUpdate()
    {       
        if (target == null) return;

        // X, Z만 따라가고 Y는 고정
        transform.position = new Vector3(
            target.position.x,
            yOffset,
            target.position.z
        );
    }
}