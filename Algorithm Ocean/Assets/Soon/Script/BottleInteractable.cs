using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class BottleInteractable : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;

    // TODO: ShortsData 연결되면 string → ShortsData로 교체
    private string dummyData;
    private bool isPicked;

    public UnityEvent<string> OnPicked;

    public void Initialize(string data)
    {
        dummyData = data;
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactRange;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPicked) return;
        if (!other.CompareTag("Boat")) return;

        Pick();
    }

    private void Pick()
    {
        isPicked = true;

        // 배 정지
        var ship = GameObject.FindWithTag("Boat");
        if (ship != null) ship.GetComponent<ShipController>()?.StopMoving();

        Debug.Log($"[Bottle] Auto-picked: {dummyData}");
        OnPicked?.Invoke(dummyData);
        gameObject.SetActive(false);
    }
}