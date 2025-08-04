using System;
using UnityEngine;

public class ReadColorMono_InspectorPctFourPerspectivePoints : ReadColorMono_AbstractFourPerspectivePoints
{

    [Range(0f, 1f)]
    public float m_topLeftHorizontalX;
    [Range(0f, 1f)]
    public float m_topLeftVerticalY;
    [Range(0f, 1f)]
    public float m_topRightHorizontalX;
    [Range(0f, 1f)]
    public float m_topRightVerticalY;
    [Range(0f, 1f)]
    public float m_bottomLeftHorizontalX;
    [Range(0f, 1f)]
    public float m_bottomLeftVerticalY;
    [Range(0f, 1f)]
    public float m_bottomRightHorizontalX;
    [Range(0f, 1f)]
    public float m_bottomRightVerticalY;


    public override void GetPointsPercent(out Vector2 topLeft, out Vector2 topRight, out Vector2 downLeft, out Vector2 downRight)
    {
        topLeft = new Vector2(m_topLeftHorizontalX, m_topLeftVerticalY);
        topRight = new Vector2(m_topRightHorizontalX, m_topRightVerticalY);
        downLeft = new Vector2(m_bottomLeftHorizontalX, m_bottomLeftVerticalY);
        downRight = new Vector2(m_bottomRightHorizontalX, m_bottomRightVerticalY);
    }
}
