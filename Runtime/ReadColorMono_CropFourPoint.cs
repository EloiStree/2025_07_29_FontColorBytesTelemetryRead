using System;
using UnityEngine;
using UnityEngine.Events;

public class ReadColorMono_CropFourPoint : MonoBehaviour
{
    public RenderTexture m_source;
    public RenderTexture m_crop;
    public int m_widthSource;
    public int m_heightSource;
    public int m_cropWidth;
    public int m_cropHeight;

    public Vector2 m_topLeftPercentLRTD = new Vector2(0.1f, 0.2f);
    public Vector2 m_topRightPercentLRTD = new Vector2(0.1f, 0.8f);
    public Vector2 m_bottomLeftPercentLRTD = new Vector2(0.9f, 0.2f);
    public Vector2 m_bottomRightPercentLRTD = new Vector2(0.9f, 0.8f);

    public Vector2Int m_topLeftInt;
    public Vector2Int m_topRightInt;
    public Vector2Int m_bottomLeftInt;
    public Vector2Int m_bottomRightInt;

    public UnityEvent<RenderTexture> m_onCropSizeChanged;

    public void SetRenderTexture(RenderTexture source)
    {
        if (m_source == source) return;
        m_source = source;
        Refresh();
    }


    [ContextMenu("refesh")]
    public void Refresh()
    {
        if (m_source == null) return;

        bool sizeChanged = m_widthSource != m_source.width || m_heightSource != m_source.height;
        if (sizeChanged)
        {
            m_widthSource = m_source.width;
            m_heightSource = m_source.height;

            // Convert percentage to pixel coordinates
            m_topLeftInt = new Vector2Int(
                (int)(m_topLeftPercentLRTD.y * m_widthSource),
                (int)(m_topLeftPercentLRTD.x * m_heightSource)
            );
            m_topRightInt = new Vector2Int(
                (int)(m_topRightPercentLRTD.y * m_widthSource),
                (int)(m_topRightPercentLRTD.x * m_heightSource)
            );
            m_bottomLeftInt = new Vector2Int(
                (int)(m_bottomLeftPercentLRTD.y * m_widthSource),
                (int)(m_bottomLeftPercentLRTD.x * m_heightSource)
            );
            m_bottomRightInt = new Vector2Int(
                (int)(m_bottomRightPercentLRTD.y * m_widthSource),
                (int)(m_bottomRightPercentLRTD.x * m_heightSource)
            );

            // Find cropping area
            FindMinMax(m_topLeftInt.x, m_topRightInt.x, m_bottomLeftInt.x, m_bottomRightInt.x, out int minX, out int maxX);
            FindMinMax(m_topLeftInt.y, m_topRightInt.y, m_bottomLeftInt.y, m_bottomRightInt.y, out int minY, out int maxY);

            m_cropWidth = maxX - minX;
            m_cropHeight = maxY - minY;

            // Create or resize crop texture
            if (m_crop == null || m_crop.width != m_cropWidth || m_crop.height != m_cropHeight)
            {
                if (m_crop != null) m_crop.Release();

                m_crop = new RenderTexture(m_cropWidth, m_cropHeight, 0);
                m_crop.Create();

                m_onCropSizeChanged?.Invoke(m_crop);
            }

            // Copy the region
            Graphics.SetRenderTarget(m_crop);
            GL.PushMatrix();
            GL.LoadPixelMatrix(minX, maxX, minY, maxY);
            Graphics.DrawTexture(new Rect(0, 0, m_cropWidth, m_cropHeight), m_source);
            GL.PopMatrix();
            Graphics.SetRenderTarget(null);

        }
    }

    private void FindMinMax(int x1, int x2, int x3, int x4, out int min, out int max)
    {
        min = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
        max = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
    }
}
