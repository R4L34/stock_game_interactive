using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ButtonHoverExpand : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The GameObject to expand. Defaults to this GameObject if left empty.")]
    [SerializeField] private GameObject targetObject;

    [Header("Scale Settings")]
    [Tooltip("Multiplier applied to the original scale when hovered.")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;

    [Tooltip("How fast the object transitions between scales.")]
    [SerializeField] private float transitionSpeed = 10f;

    private Transform targetTransform;
    private Collider2D targetCollider2D;
    private Collider targetCollider3D;
    private SpriteRenderer targetSpriteRenderer;
    private Camera mainCamera;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        if (targetObject == null)
        {
            targetObject = gameObject;
        }

        targetTransform = targetObject.transform;
        originalScale = targetTransform.localScale;
        targetScale = originalScale;

        targetCollider2D = targetObject.GetComponent<Collider2D>();
        targetCollider3D = targetObject.GetComponent<Collider>();
        targetSpriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (targetTransform == null) return;

        bool isHovered = IsMouseOverTarget();
        targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;

        targetTransform.localScale = Vector3.Lerp(
            targetTransform.localScale,
            targetScale,
            Time.unscaledDeltaTime * transitionSpeed
        );
    }

    private bool IsMouseOverTarget()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return false;
        }

        if (!TryGetMouseScreenPosition(out Vector2 mouseScreenPos))
        {
            return false;
        }

        // 1. Check 2D Collider if attached
        if (targetCollider2D != null)
        {
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            return targetCollider2D.OverlapPoint(worldPoint);
        }

        // 2. Check 3D Collider if attached
        if (targetCollider3D != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
            return targetCollider3D.Raycast(ray, out _, Mathf.Infinity);
        }

        // 3. Fallback to SpriteRenderer bounds (works even without adding a Collider)
        if (targetSpriteRenderer != null && targetSpriteRenderer.sprite != null)
        {
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Bounds bounds = targetSpriteRenderer.bounds;
            return worldPoint.x >= bounds.min.x && worldPoint.x <= bounds.max.x &&
                   worldPoint.y >= bounds.min.y && worldPoint.y <= bounds.max.y;
        }

        return false;
    }

    private bool TryGetMouseScreenPosition(out Vector2 screenPosition)
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        screenPosition = Input.mousePosition;
        return true;
#else
        screenPosition = Vector2.zero;
        return false;
#endif
    }

    private void OnDisable()
    {
        if (targetTransform != null)
        {
            targetScale = originalScale;
            targetTransform.localScale = originalScale;
        }
    }
}
