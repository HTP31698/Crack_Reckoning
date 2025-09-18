using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform safeAreaRect;
    private Canvas canvas;
    private Rect lastSafeArea;

    void Start()
    {
        safeAreaRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        OnRectTransformDimensionsChange();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (GetSafeArea() != lastSafeArea && canvas != null)
        {
            lastSafeArea = GetSafeArea();
            UpdateSizeToSafeArea();
        }
    }

    private void UpdateSizeToSafeArea()
    {
        var safeArea = GetSafeArea();
        var inverseSize = new Vector2(1f, 1f) / canvas.pixelRect.size;
        var newAnchorMin = Vector2.Scale(safeArea.position, inverseSize);
        var newAnchorMax = Vector2.Scale(safeArea.position + safeArea.size, inverseSize);

        safeAreaRect.anchorMin = newAnchorMin;
        safeAreaRect.anchorMax = newAnchorMax;

        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
    }

    Rect GetSafeArea()
    {
#if UNITY_EDITOR
        return new Rect(0, 0, 600, 800); // 에디터용 시뮬레이션
#else
        Rect sa = Screen.safeArea;
        if (Screen.height > Screen.width) // 세로 모드
            return sa;
        else // 가로 모드 반환 시 회전
            return new Rect(sa.y, sa.x, sa.height, sa.width);
#endif
    }
}
