﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
/*
Radial Layout Group by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
public class RadialLayout : LayoutGroup
{
    [SerializeField]
    protected float radius;
    [SerializeField]
    protected bool clockwise;
    [Range(0f, 360f)]
    [SerializeField]
    protected float minAngle;
    [Range(0f, 360f)]
    [SerializeField]
    protected float maxAngle = 360f;
    [Range(0f, 360f)]
    [SerializeField]
    protected float startAngle;
    [SerializeField]
    protected bool customRebuild;

    [Header("Child rotation")]
    [Range(0f, 360f)]
    [SerializeField]
    protected float startElementAngle;
    [SerializeField]
    protected bool rotateElements;

    [Header("Child width")]
    [SerializeField]
    protected bool expandChildWidth;
    [SerializeField]
    protected float childWidthFactor = 1f;
    [Range(0f, 360f)]
    [SerializeField]
    protected float maxWidthFactor = 360f;
    [SerializeField]
    protected bool childWidthFromRadius;
    [SerializeField]
    protected float childWidthRadiusFactor = 0.01f;

    [Header("Child height")]
    [SerializeField]
    protected bool expandChildHeight;
    [SerializeField]
    protected float childHeight = 100f;
    [SerializeField]
    protected bool childHeightFromRadius;
    [SerializeField]
    protected float childHeightRadiusFactor = 0.0025f;

    #region Properties

    public float Radius
    {
        get
        {
            return radius;
        }
        set
        {
            if (radius != value)
            {
                radius = value;
                OnValueChanged();
            }
        }
    }

    public bool Clockwise
    {
        get
        {
            return clockwise;
        }
        set
        {
            if (clockwise != value)
            {
                clockwise = value;
                OnValueChanged();
            }
        }
    }

    public float MinAngle
    {
        get
        {
            return minAngle;
        }
        set
        {
            if (minAngle != value)
            {
                minAngle = value;
                OnValueChanged();
            }
        }
    }

    public float MaxAngle
    {
        get
        {
            return maxAngle;
        }
        set
        {
            if (maxAngle != value)
            {
                maxAngle = value;
                OnValueChanged();
            }
        }
    }

    public float StartAngle
    {
        get
        {
            return startAngle;
        }
        set
        {
            if (startAngle != value)
            {
                startAngle = value;
                OnValueChanged();
            }
        }
    }

    public bool CustomRebuild
    {
        get
        {
            return customRebuild;
        }
        set
        {
            if (customRebuild != value)
            {
                customRebuild = value;
                OnValueChanged();
            }
        }
    }

    public bool ExpandChildWidth
    {
        get
        {
            return expandChildWidth;
        }
        set
        {
            if (expandChildWidth != value)
            {
                expandChildWidth = value;
                OnValueChanged();
            }
        }
    }

    public float ChildWidthFactor
    {
        get
        {
            return childWidthFactor;
        }
        set
        {
            if (childWidthFactor != value)
            {
                childWidthFactor = value;
                OnValueChanged();
            }
        }
    }

    public bool ChildWidthFromRadius
    {
        get
        {
            return childWidthFromRadius;
        }
        set
        {
            if (childWidthFromRadius != value)
            {
                childWidthFromRadius = value;
                OnValueChanged();
            }
        }
    }

    public float ChildWidthRadiusFactor
    {
        get
        {
            return childWidthRadiusFactor;
        }
        set
        {
            if (childWidthRadiusFactor != value)
            {
                childWidthRadiusFactor = value;
                OnValueChanged();
            }
        }
    }

    public bool ExpandChildHeight
    {
        get
        {
            return expandChildHeight;
        }
        set
        {
            if (expandChildHeight != value)
            {
                expandChildHeight = value;
                OnValueChanged();
            }
        }
    }

    public float ChildHeight
    {
        get
        {
            return childHeight;
        }
        set
        {
            if (childHeight != value)
            {
                childHeight = value;
                OnValueChanged();
            }
        }
    }

    public bool ChildHeightFromRadius
    {
        get
        {
            return childHeightFromRadius;
        }
        set
        {
            if (childHeightFromRadius != value)
            {
                childHeightFromRadius = value;
                OnValueChanged();
            }
        }
    }

    public float ChildHeightRadiusFactor
    {
        get
        {
            return childHeightRadiusFactor;
        }
        set
        {
            if (childHeightRadiusFactor != value)
            {
                childHeightRadiusFactor = value;
                OnValueChanged();
            }
        }
    }

    public RectTransform SelfTransform
    {
        get
        {
            return rectTransform;
        }
    }

    public void OnValueChanged()
    {
        if (customRebuild)
        {
            CalculateRadial();
        }
    }

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        CalculateRadial();
    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }

    public override void CalculateLayoutInputVertical()
    {
        CalculateRadial();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        CalculateRadial();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        //CalculateRadial();
    }
#endif

    public void CalculateRadial()
    {
        int activeChildCount = 0;
        List<RectTransform> childList = new List<RectTransform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            LayoutElement childLayout = child.GetComponent<LayoutElement>();
            if (child == null || !child.gameObject.activeSelf || (childLayout != null && childLayout.ignoreLayout))
            {
                continue;
            }
            childList.Add(child);
            activeChildCount++;
        }

        m_Tracker.Clear();
        if (activeChildCount == 0)
        {
            return;
        }

        rectTransform.sizeDelta = new Vector2(radius, radius) * 2f;
        float sAngle = ((360f) / activeChildCount) * (activeChildCount - 1f);

        float anglOffset = minAngle;
        if (anglOffset > sAngle)
        {
            anglOffset = sAngle;
        }

        float buff = sAngle - anglOffset;

        float maxAngl = 360f - maxAngle;
        if (maxAngl > sAngle)
        {
            maxAngl = sAngle;
        }

        if (anglOffset > sAngle)
        {
            anglOffset = sAngle;
        }

        buff = sAngle - anglOffset;

        float fOffsetAngle = ((buff - maxAngl)) / (activeChildCount - 1f);
        float fAngle = startAngle + anglOffset;
        float countWidthFactor = fOffsetAngle < maxWidthFactor ? fOffsetAngle : maxWidthFactor;

        bool expandChilds = expandChildWidth | expandChildHeight;
        DrivenTransformProperties drivenTransformProperties = DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot;
        if (expandChildWidth)
        {
            drivenTransformProperties |= DrivenTransformProperties.SizeDeltaX;
        }
        if (expandChildHeight)
        {
            drivenTransformProperties |= DrivenTransformProperties.SizeDeltaY;
        }
        if (rotateElements)
        {
            drivenTransformProperties |= DrivenTransformProperties.Rotation;
        }

        if (clockwise)
        {
            fOffsetAngle *= -1f;
        }

        for (int i = 0; i < childList.Count; i++)
        {
            RectTransform child = childList[i];
            if (child != null && child.gameObject.activeSelf)
            {
                //Adding the elements to the tracker stops the user from modifiying their positions via the editor.
                m_Tracker.Add(this, child, drivenTransformProperties);
                Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                child.localPosition = vPos * radius;
                //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);

                float elementAngle = startElementAngle;
                if (rotateElements)
                {
                    elementAngle += fAngle;
                    child.localEulerAngles = new Vector3(0f, 0f, elementAngle);
                }
                else
                {
                    child.localEulerAngles = new Vector3(0f, 0f, elementAngle);
                }

                if (expandChilds)
                {
                    Vector2 expandSize = child.sizeDelta;
                    if (expandChildWidth)
                    {
                        expandSize.x = childWidthFromRadius ? (radius * childWidthRadiusFactor) * countWidthFactor * childWidthFactor : countWidthFactor * childWidthFactor;
                    }
                    if (expandChildHeight)
                    {
                        expandSize.y = childHeightFromRadius ? (radius * childHeightRadiusFactor) * childHeight : childHeight;
                    }
                    child.sizeDelta = expandSize;
                }

                fAngle += fOffsetAngle;
            }

        }

    }

}