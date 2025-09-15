using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(RectTransform))]
public class LayoutElementResizer : MonoBehaviour
{
    [SerializeField] private List<RectTransform> layoutParts;
    [SerializeField] private float extraVerticalPadding = 0f;
    
    private LayoutElement _layoutElement;
    private RectTransform _rectTransform;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        _layoutElement = GetComponent<LayoutElement>();
        _rectTransform = GetComponent<RectTransform>();
        
        Recalculate();
    }

    private void OnEnable() => Recalculate();
    private void OnTransformChildrenChanged() => Recalculate();
    private void OnRectTransformDimensionsChange() => Recalculate();

    [ContextMenu("Recalculate now")]
    public void Recalculate()
    {
        var sum = 0f;
        foreach (var rect in layoutParts)
            sum += LayoutUtility.GetPreferredHeight(rect);

        var finalHeight = Mathf.Max(0f, sum + extraVerticalPadding);
        _layoutElement.preferredHeight = finalHeight;
        
        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);

        _rectTransform.sizeDelta = new Vector2(_rectTransform.rect.width, _layoutElement.preferredHeight);
    }
}