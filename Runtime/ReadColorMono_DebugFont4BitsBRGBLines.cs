using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class ReadColorMono_DebugFont4BitsBRGBLines : MonoBehaviour
{
    public ReadColorMono_AbstractFourPerspectivePoints m_perspectivePoints;
    public NativeArray2DColor32WH m_source;
    public Texture2D m_debugPoints;
    public int m_widthSource;
    public int m_heightSource;
    public int m_cropWidth;
    public int m_cropHeight;

    public float m_rescaleTexture = 0.6f;

    public Texture2D m_textureFourLine;
    public UnityEvent<Texture2D> m_onFourLineCreated;
    public Vector2Int m_topLeftInt;
    public Vector2Int m_topRightInt;
    public Vector2Int m_bottomLeftInt;
    public Vector2Int m_bottomRightInt;

    public Vector2Int m_previsouTopLeftInt;
    public Vector2Int m_previsouTopRightInt;
    public Vector2Int m_previsouBottomLeftInt;
    public Vector2Int m_previsouBottomRightInt;

    public FourLinePctInt m_fourLines;
    [System.Serializable]
    public class FourLinePctInt
    {


        [Header("Percent")]
        public Vector2 m_leftSquare0;
        public Vector2 m_leftSquare1;
        public Vector2 m_leftSquare2;
        public Vector2 m_leftSquare3;
        public Vector2 m_rightSquare0;
        public Vector2 m_rightSquare1;
        public Vector2 m_rightSquare2;
        public Vector2 m_rightSquare3;
        [Header("Int")]
        public Vector2Int m_leftSquare0Int;
        public Vector2Int m_leftSquare1Int;
        public Vector2Int m_leftSquare2Int;
        public Vector2Int m_leftSquare3Int;
        public Vector2Int m_rightSquare0Int;
        public Vector2Int m_rightSquare1Int;
        public Vector2Int m_rightSquare2Int;
        public Vector2Int m_rightSquare3Int;
    }


    public Color m_lineColor;


    public void GetVector2Int(Vector2 source, int width, int height, out Vector2Int result)
    {
        // Clamp source.x and source.y to [0,1] to avoid out-of-bounds
        float x = Mathf.Clamp01(source.x);
        float y = Mathf.Clamp01(source.y);
        int ix = Mathf.RoundToInt(x * width);
        int iy = Mathf.RoundToInt((1f - y) * height);
        result = new Vector2Int(ix, iy);
    }
    public
        int segmentCount = 300;
    public void FetchPointsPctConcerned(Vector2 startPct, Vector2 endPct, int width, int height, ref List<Vector2Int> points)
    {
        GetVector2Int(startPct, width, height, out Vector2Int startInt);
        GetVector2Int(endPct, width, height, out Vector2Int endInt);

        points ??= new List<Vector2Int>();
        points.Clear();

        Vector2 direction = endPct - startPct;
        float mag = direction.magnitude;

        if (points.Capacity < segmentCount)
            points.Capacity = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / (segmentCount - 1);
            Vector2 lerpPct = Vector2.Lerp(startPct, endPct, t);
            GetVector2Int(lerpPct, width, height, out Vector2Int pixel);

            if (points.Count > i)
                points[i] = pixel;
            else
                points.Add(pixel);
        }

    }

    public void FetchColorBetween(List<Vector2Int> points, ref List<Color32> result, NativeArray2DColor32WH source)
    {
        source.GetNativeArray(out NativeArray<Color32> colors, out int width, out int height);
        if (result == null)
            result = new List<Color32>(points.Count);
        if (result.Count != points.Count)
        {
            result.Clear();
            for (int i = 0; i < points.Count; i++)
                result.Add(new Color32(1,0,1,1));
        }
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int point = points[i];
            int index1D = point.x + point.y * width;
            if (index1D >= 0 && index1D < colors.Length)
            {
                result[i] = colors[index1D];
            }
            else { 
                result[i] = new Color32(0, 1, 0, 1);
            }
        }
    }

    [ContextMenu("Refresh List of Points")]
    public void RefreshListOfPoints() {

        m_perspectivePoints.GetPointsPercent(out Vector2 topLeft,
            out Vector2 topRight, out _, out _);
        FetchPointsPctConcerned(m_fourLines.m_leftSquare0, m_fourLines.m_rightSquare0, m_source.m_width, m_source.m_height, ref m_linePoints0);
        FetchPointsPctConcerned(m_fourLines.m_leftSquare1, m_fourLines.m_rightSquare1, m_source.m_width, m_source.m_height, ref m_linePoints1);
        FetchPointsPctConcerned(m_fourLines.m_leftSquare2, m_fourLines.m_rightSquare2, m_source.m_width, m_source.m_height, ref m_linePoints2);
        FetchPointsPctConcerned(m_fourLines.m_leftSquare3, m_fourLines.m_rightSquare3, m_source.m_width, m_source.m_height, ref m_linePoints3);
    }

    private void Awake()
    {
        InvokeRepeating("RefreshListOfPoints", 1, 1);
    }

    public void DrawLineInTexture(Vector2Int start, Vector2Int end, Color color )
    {

        Texture2D t = m_debugPoints;
        if (t == null) return;

        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int colorCount = 0;
        int index1d = 0;
        while (true)
        {
            if (x0 >= 0 && x0 < t.width && y0 >= 0 && y0 < t.height)
                t.SetPixel(x0, y0, color);


            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public int m_squareCount;

    public UnityEvent<Texture2D> m_onCropSizeChanged;

    public List<Vector2Int> m_linePoints0 = new();
    public List<Vector2Int> m_linePoints1 = new();
    public List<Vector2Int> m_linePoints2 = new();
    public List<Vector2Int> m_linePoints3 = new();


    public List<Color32> m_line0=new ();
    public List<Color32> m_line1=new ();
    public List<Color32> m_line2=new ();
    public List<Color32> m_line3=new ();

    public void SetRenderTexture(NativeArray2DColor32WH source)
    {
        if (m_source == source) return;

        if (m_source == null || m_debugPoints == null)
        {
            m_widthSource = 0;
            m_heightSource = 0;
        }
        m_source = source;
        Refresh();
    }


    [ContextMenu("Refesh")]
    public void Refresh()
    {


        if (m_source == null) return;
        m_source.GetWidth(out int width);
        m_source.GetHeight(out int height);
        if (width<=0 || height<=0) return;




        bool wasReset = false;
        bool sizeChanged = m_widthSource != m_source.m_width || m_heightSource != m_source.m_height;
        if (sizeChanged || m_debugPoints == null)
        {
            if (m_debugPoints != null)
            {
                DestroyImmediate(m_debugPoints);
                m_debugPoints = null;
            }

            m_widthSource = m_source.m_width;
            m_heightSource = m_source.m_height;
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


            if (m_textureFourLine != null) { 
                DestroyImmediate(m_textureFourLine);
            }


            m_textureFourLine = new Texture2D(segmentCount, 4, TextureFormat.ARGB32, false,false);
            m_textureFourLine.wrapMode = TextureWrapMode.Clamp;
            m_textureFourLine.filterMode = FilterMode.Point;
            m_textureFourLine.anisoLevel = 0;
            m_textureFourLine.Apply();
            m_onFourLineCreated.Invoke( m_textureFourLine );


            RefreshListOfPoints();
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


            float line0 = 1f / 8f;
            float line1 = 3f / 8f;
            float line2 = 5f / 8f;
            float line3 = 7f / 8f;

            
            LerpPoint(m_topLeftPercentLRTD, m_bottomLeftPercentLRTD, line0,   out m_fourLines.m_leftSquare0 , out m_fourLines.m_leftSquare0Int   );
            LerpPoint(m_topLeftPercentLRTD, m_bottomLeftPercentLRTD  , line1, out m_fourLines.m_leftSquare1 , out m_fourLines.m_leftSquare1Int  );
            LerpPoint(m_topLeftPercentLRTD, m_bottomLeftPercentLRTD  , line2, out m_fourLines.m_leftSquare2 , out m_fourLines.m_leftSquare2Int  );
            LerpPoint(m_topLeftPercentLRTD, m_bottomLeftPercentLRTD  , line3, out m_fourLines.m_leftSquare3 , out m_fourLines.m_leftSquare3Int  );
            LerpPoint(m_topRightPercentLRTD, m_bottomRightPercentLRTD, line0, out m_fourLines.m_rightSquare0, out m_fourLines.m_rightSquare0Int );
            LerpPoint(m_topRightPercentLRTD, m_bottomRightPercentLRTD, line1, out m_fourLines.m_rightSquare1, out m_fourLines.m_rightSquare1Int );
            LerpPoint(m_topRightPercentLRTD, m_bottomRightPercentLRTD, line2, out m_fourLines.m_rightSquare2, out m_fourLines.m_rightSquare2Int );
            LerpPoint(m_topRightPercentLRTD, m_bottomRightPercentLRTD, line3, out m_fourLines.m_rightSquare3, out m_fourLines.m_rightSquare3Int );




            DrawLineInTexture( m_fourLines.m_leftSquare0Int,  m_fourLines.m_rightSquare0Int,Color.yellow );
            DrawLineInTexture( m_fourLines.m_leftSquare1Int,  m_fourLines.m_rightSquare1Int,Color.yellow );
            DrawLineInTexture( m_fourLines.m_leftSquare2Int,  m_fourLines.m_rightSquare2Int,Color.yellow );
            DrawLineInTexture(m_fourLines.m_leftSquare3Int,   m_fourLines.m_rightSquare3Int, Color.yellow );


            DrawSquare(m_topLeftInt, Color.red, dw, dh, m_pointRadius);
            DrawSquare(m_topRightInt, Color.green, dw, dh, m_pointRadius);
            DrawSquare(m_bottomLeftInt, Color.blue, dw, dh, m_pointRadius);
            DrawSquare(m_bottomRightInt, Color.magenta, dw, dh, m_pointRadius);

            DrawSquare( m_fourLines.m_leftSquare0Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare( m_fourLines.m_leftSquare1Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare( m_fourLines.m_leftSquare2Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare(m_fourLines.m_leftSquare3Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare( m_fourLines.m_rightSquare0Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare( m_fourLines.m_rightSquare1Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare( m_fourLines.m_rightSquare2Int, Color.yellow, dw, dh, m_pointRadius);
            DrawSquare (m_fourLines.m_rightSquare3Int, Color.yellow, dw, dh, m_pointRadius);
            
            //if (m_squareCount > 1)
            //{
            //    float piece = 1f / m_squareCount;
            //    float halfPiece = piece / 2f;
            //    for (int squareI = 0; squareI < m_squareCount; squareI++)
            //    {
            //        float t = halfPiece + piece * squareI;
                   
            //        Vector2Int pointLerp = LerpPoint(leftSquare0, rightSquare0, t);
            //        DrawSquare(pointLerp, Color.cyan, dw, dh, m_linePointRadius);
            //    }
            //}


            m_debugPoints.Apply();
            m_onCropSizeChanged.Invoke(m_debugPoints);


            if (m_linePoints0.Count <= 0)
            {
                RefreshListOfPoints();
            }
            FetchColorBetween(m_linePoints0, ref m_line0, m_source);
            //FetchColorBetween(leftSquare1, rightSquare1, ref m_line1 , m_source);
            //FetchColorBetween(leftSquare2, rightSquare2, ref m_line2 , m_source);
            //FetchColorBetween(leftSquare3, rightSquare3, ref m_line3 , m_source);
        }

        FetchColorBetween(m_linePoints0, ref m_line0, m_source);
        FetchColorBetween(m_linePoints1, ref m_line1, m_source);
        FetchColorBetween(m_linePoints2, ref m_line2, m_source);
        FetchColorBetween(m_linePoints3, ref m_line3, m_source);
        for (int i = 0; i < segmentCount; i++)
        {
            m_textureFourLine.SetPixel( i,3, m_line0[i]);
            m_textureFourLine.SetPixel( i,2, m_line1[i]);
            m_textureFourLine.SetPixel( i,1, m_line2[i]);
            m_textureFourLine.SetPixel( i,0, m_line3[i]);
        }
        m_textureFourLine.Apply();
    }

    private Vector2Int LerpPoint(Vector2 startPercent, Vector2 endPercent, double t)
    {
        int dw = m_debugPoints != null ? m_debugPoints.width : 0;
        int dh = m_debugPoints != null ? m_debugPoints.height : 0;
        float lerpX = (float)(startPercent.x + (endPercent.x - startPercent.x) * t);
        float lerpY = (float)(startPercent.y + (endPercent.y - startPercent.y) * t);
        int x = Mathf.RoundToInt(lerpX * dw);
        int y = Mathf.RoundToInt((1f - lerpY) * dh);
        return new Vector2Int(x, y);
    }

    private void  LerpPoint(Vector2 startPercent, Vector2 endPercent, double t, out Vector2 newPointPercent, out Vector2Int newPointInt)
    {
        int dw = m_debugPoints != null ? m_debugPoints.width : 0;
        int dh = m_debugPoints != null ? m_debugPoints.height : 0;
        float lerpX = (float)(startPercent.x + (endPercent.x - startPercent.x) * t);
        float lerpY = (float)(startPercent.y + (endPercent.y - startPercent.y) * t);
        int x = Mathf.RoundToInt(lerpX * dw);
        int y = Mathf.RoundToInt((1f - lerpY) * dh);
       

        newPointPercent = new Vector2(lerpX, lerpY); 
        newPointInt= new Vector2Int(x, y);
    }

    public int m_pointRadius = 3;
    public int m_linePointRadius = 2;

    // Helper to draw a 5x5 square centered at pos with color
    private void DrawSquare(Vector2Int pos, Color color, int width, int height, int radius=2)
    {
        int half = radius;
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

    public bool m_useUpdate=true;
    private void Update()
    {
        if (m_useUpdate)
            Refresh();
    }

    private void FindMinMax(int x1, int x2, int x3, int x4, out int min, out int max)
    {
        min = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
        max = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4));
    }

}
