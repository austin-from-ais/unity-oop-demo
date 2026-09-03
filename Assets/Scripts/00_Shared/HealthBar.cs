using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating world-space health bar. Builds its own tiny canvas at runtime so no prefab wiring is needed.
/// Call SetFraction(0..1) to update it.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Look")]
    public float heightAboveFeet = 2.1f;
    public float widthMeters     = 1.0f;
    public Color fillColor       = new Color(0.25f, 0.85f, 0.3f);
    public Color lowColor        = new Color(0.9f, 0.25f, 0.2f);
    public Color backColor       = new Color(0.1f, 0.1f, 0.1f, 0.85f);

    Transform     barRoot;
    RectTransform fillRect;
    Image         fillImage;
    Transform     cam;

    void Awake()
    {
        Build();
        SetFraction(1f);
    }

    void Build()
    {
        // Canvas root, parented so it is destroyed with the owner
        var root = new GameObject("HealthBar", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(transform, false);
        barRoot = root.transform;

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(100f, 12f);            // 100 x 12 "pixels"
        rootRect.localScale = Vector3.one * (widthMeters / 100f); // -> widthMeters wide

        // Background
        var back = new GameObject("Back", typeof(RectTransform), typeof(Image));
        back.transform.SetParent(root.transform, false);
        var backRect = back.GetComponent<RectTransform>();
        backRect.anchorMin = Vector2.zero;
        backRect.anchorMax = Vector2.one;
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        back.GetComponent<Image>().color = backColor;

        // Fill (anchored left, width driven by anchorMax.x)
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(root.transform, false);
        fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);
        fillImage = fill.GetComponent<Image>();
        fillImage.color = fillColor;
    }

    public void SetFraction(float fraction)
    {
        fraction = Mathf.Clamp01(fraction);
        fillRect.anchorMax = new Vector2(fraction, 1f);
        fillImage.color = Color.Lerp(lowColor, fillColor, fraction);
    }

    void LateUpdate()
    {
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam == null) return;

        barRoot.position = transform.position + Vector3.up * heightAboveFeet;
        barRoot.rotation = cam.rotation; // billboard
    }
}
