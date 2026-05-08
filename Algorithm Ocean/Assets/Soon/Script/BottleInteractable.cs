using UnityEngine;
using UnityEngine.Events;
using AlgorithmOcean.Dohyeon;

[RequireComponent(typeof(SphereCollider))]
public class BottleInteractable : MonoBehaviour
{
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private bool pickAutomaticallyOnEnter = true;

    private SubmitData contentData;
    private ShortsPlaybackUI playbackUI;
    private Transform boatTarget;
    private bool isPlayerInRange;
    private bool isPicked;

    public UnityEvent<string> OnPicked;
    public SubmitData ContentData => contentData;

    public void Initialize(SubmitData data, ShortsPlaybackUI targetPlaybackUI, Transform targetBoat)
    {
        contentData = data;
        playbackUI = targetPlaybackUI;
        boatTarget = targetBoat;
        isPicked = false;
        isPlayerInRange = false;

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = interactRange;

        SetPromptVisible(false);
    }

    private void Update()
    {
        if (isPicked || boatTarget == null)
        {
            return;
        }

        bool isInRange = Vector3.Distance(transform.position, boatTarget.position) <= interactRange;
        if (isInRange == isPlayerInRange)
        {
            return;
        }

        isPlayerInRange = isInRange;
        SetPromptVisible(isPlayerInRange);

        if (isPlayerInRange && pickAutomaticallyOnEnter)
        {
            Pick();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPicked) return;
        if (!IsBoatCollider(other)) return;

        isPlayerInRange = true;
        SetPromptVisible(true);

        if (pickAutomaticallyOnEnter)
        {
            Pick();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsBoatCollider(other)) return;

        isPlayerInRange = false;
        SetPromptVisible(false);
    }

    private void OnMouseDown()
    {
        if (!isPlayerInRange) return;

        Pick();
    }

    private void Pick()
    {
        if (contentData == null || string.IsNullOrWhiteSpace(contentData.youtube))
        {
            Debug.LogWarning("[Bottle] Cannot pick because SubmitData.youtube is empty. Check Firebase submissions or repository fallback data.", this);
            return;
        }

        isPicked = true;
        SetPromptVisible(false);

        var ship = GameObject.FindWithTag("Boat");
        if (ship != null) ship.GetComponent<ShipController>()?.StopMoving();

        string shortsUrl = contentData.youtube;
        string categories = contentData.categories != null
            ? string.Join(", ", contentData.categories)
            : string.Empty;
        Debug.Log($"[Bottle] Picked categories='{categories}', youtube='{shortsUrl}'");
        OnPicked?.Invoke(shortsUrl);
        playbackUI?.Open(shortsUrl);
        gameObject.SetActive(false);
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(visible);
        }
    }

    private static bool IsBoatCollider(Collider other)
    {
        if (other.CompareTag("Boat"))
        {
            return true;
        }

        if (other.GetComponentInParent<ShipController>() != null)
        {
            return true;
        }

        Transform root = other.transform.root;
        return root != null && root.CompareTag("Boat");
    }
}
