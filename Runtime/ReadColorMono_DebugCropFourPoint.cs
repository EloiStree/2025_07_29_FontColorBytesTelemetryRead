using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class ReadColorMono_AbstractFourPerspectivePoints : MonoBehaviour {

    public abstract void GetPointsPercent(out Vector2 topLeft, out Vector2 topRight, out Vector2 downLeft, out Vector2 downRight);

}




public class ReadColorMono_DebugCropFourPoint : MonoBehaviour
{
    public ReadColorMono_AbstractFourPerspectivePoints m_perspectivePoints;
    public RenderTexture m_source;
    public Texture2D m_debugPoints;
    public int m_widthSource;
    public int m_heightSource;
    public int m_cropWidth;
    public int m_cropHeight;

    public float m_rescaleTexture = 0.6f;





    public Vector2Int m_topLeftInt;
    public Vector2Int m_topRightInt;
    public Vector2Int m_bottomLeftInt;
    public Vector2Int m_bottomRightInt;

    public Vector2Int m_previsouTopLeftInt;
    public Vector2Int m_previsouTopRightInt;
    public Vector2Int m_previsouBottomLeftInt;
    public Vector2Int m_previsouBottomRightInt;


    public UnityEvent<Texture2D> m_onCropSizeChanged;

    public void SetRenderTexture(RenderTexture source)
    {
        if (m_source == source) return;

        if (m_source == null || m_debugPoints==null) { 
            m_widthSource = 0;
            m_heightSource =0;
        }
        m_source = source;
        Refresh();
    }

    
    [ContextMenu("Refesh")]
    public void Refresh()
    {
        if (m_source == null) return;

        bool wasReset = false;
        bool sizeChanged = m_widthSource != m_source.width || m_heightSource != m_source.height;
        if (sizeChanged || m_debugPoints == null)
        {
            if (m_debugPoints != null)
            {
                DestroyImmediate(m_debugPoints);
                m_debugPoints = null;
            }

            m_widthSource = m_source.width;
            m_heightSource = m_source.height;
            m_debugPoints = new Texture2D(
                Mathf.RoundToInt(m_widthSource * m_rescaleTexture),
                Mathf.RoundToInt(m_heightSource * m_rescaleTexture),
                TextureFormat.RGBA32,
                false
            );
            wasReset = true;
            m_debugPoints.wrapMode = TextureWrapMode.Clamp;
            m_debugPoints.filterMode = FilterMode.Point;
            m_debugPoints.Apply();
        }

        // Store previous values before updating
        m_previsouTopLeftInt = m_topLeftInt;
        m_previsouTopRightInt = m_topRightInt;
        m_previsouBottomLeftInt = m_bottomLeftInt;
        m_previsouBottomRightInt = m_bottomRightInt;

        int dw = m_debugPoints.width;
        int dh = m_debugPoints.height;
        // Get the four perspective points as percent values
        Vector2 m_topLeftPercentLRTD, m_topRightPercentLRTD, m_bottomLeftPercentLRTD, m_bottomRightPercentLRTD;
        m_perspectivePoints.GetPointsPercent(
            out m_topLeftPercentLRTD,
            out m_topRightPercentLRTD,
            out m_bottomLeftPercentLRTD,
            out m_bottomRightPercentLRTD
        );

        m_topLeftInt = new Vector2Int(
            (int)(m_topLeftPercentLRTD.x * dw),
            (int)((1 - m_topLeftPercentLRTD.y) * dh)
        );
        m_topRightInt = new Vector2Int(
            (int)(m_topRightPercentLRTD.x * dw),
            (int)((1 - m_topRightPercentLRTD.y) * dh)
        );
        m_bottomLeftInt = new Vector2Int(
            (int)(m_bottomLeftPercentLRTD.x * dw),
            (int)((1 - m_bottomLeftPercentLRTD.y) * dh)
        );
        m_bottomRightInt = new Vector2Int(
            (int)(m_bottomRightPercentLRTD.x * dw),
            (int)((1 - m_bottomRightPercentLRTD.y) * dh)
        );

        bool onePointChangeSpace =
            m_topLeftInt != m_previsouTopLeftInt ||
            m_topRightInt != m_previsouTopRightInt ||
            m_bottomLeftInt != m_previsouBottomLeftInt ||
            m_bottomRightInt != m_previsouBottomRightInt;

        // Find cropping area
        FindMinMax(m_topLeftInt.x, m_topRightInt.x, m_bottomLeftInt.x, m_bottomRightInt.x, out int minX, out int maxX);
        FindMinMax(m_topLeftInt.y, m_topRightInt.y, m_bottomLeftInt.y, m_bottomRightInt.y, out int minY, out int maxY);

        m_cropWidth = maxX - minX;
        m_cropHeight = maxY - minY;

        if (onePointChangeSpace || wasReset)
        {
            if (m_debugPoints != null)
            {
                Color32[] pixels = m_debugPoints.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32(0, 0, 0, 0);
                m_debugPoints.SetPixels32(pixels);
            }
            // Draw vertical lines at minX and maxX (3 pixels wide)
            for (int y = 0; y < m_heightSource; y++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int xMin = minX + dx;
                    int xMax = maxX + dx;
                    if (xMin >= 0 && xMin < m_widthSource)
                        m_debugPoints.SetPixel(xMin, y, Color.green);
                    if (xMax >= 0 && xMax < m_widthSource)
                        m_debugPoints.SetPixel(xMax, y, Color.green);
                }
            }

            // Draw horizontal lines at minY and maxY (3 pixels wide)
            for (int x = 0; x < m_widthSource; x++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int yMin = minY + dy;
                    int yMax = maxY + dy;
                    if (yMin >= 0 && yMin < m_heightSource)
                        m_debugPoints.SetPixel(x, yMin, Color.green);
                    if (yMax >= 0 && yMax < m_heightSource)
                        m_debugPoints.SetPixel(x, yMax, Color.green);
                }
            }

            // Draw 5x5 squares for each point: TL=red, TR=green, BL=blue, BR=magenta
            DrawSquare(m_topLeftInt, Color.red, dw, dh);
            DrawSquare(m_topRightInt, Color.green, dw, dh);
            DrawSquare(m_bottomLeftInt, Color.blue, dw, dh);
            DrawSquare(m_bottomRightInt, Color.magenta, dw, dh);

            m_debugPoints.Apply();
            m_onCropSizeChanged.Invoke(m_debugPoints);
        }
    }
    public int m_pointRadius = 3;

    // Helper to draw a 5x5 square centered at pos with color
    private void DrawSquare(Vector2Int pos, Color color, int width , int height)
    {
        int half = m_pointRadius;
        for (int dx = -half; dx <= half; dx++)
        {
            for (int dy = -half; dy <= half; dy++)
            {
                int x = pos.x + dx;
                int y = pos.y + dy;
                if (x >= 0 && x < m_widthSource && y >= 0 && y < m_heightSource)
                {
                    m_debugPoints.SetPixel(x, y, color);
                }
            }
        }
    }

    private void Update()
    {
        Refresh();
    }

    private void FindMinMax(int x1, int x2, int x3, int x4, out int min, out int max)
    {
        min = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
        max = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
    }
}
