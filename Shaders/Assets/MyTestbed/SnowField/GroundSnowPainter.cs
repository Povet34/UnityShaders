using UnityEngine;

/// <summary> 
/// ���� �ؽ��ĸ� �̿��� ���� �� �ױ�
/// </summary>
public class GroundSnowPainter : MonoBehaviour
{
    [SerializeField]
    private Material targetMaterial; // ���� �ؽ��ĸ� ���� �ؽ��ķ� ������ ��� ���׸���

    [SerializeField, Range(0.01f, 1f)]
    private float brushSize = 0.1f;

    [SerializeField, Range(0.01f, 1f)]
    private float pileBrushIntensity = 0.1f;

    [SerializeField, Range(0.01f, 1f)]
    private float eraserBrushIntensity = 0.1f;

    [SerializeField] // �ν����� Ȯ�ο�
    private RenderTexture snowRenderTexture; // �귯�÷� �׷��� ��� ���� �ؽ���

    private Texture2D whiteBrushTexture; // Painter
    private Texture2D blackBrushTexture; // Eraser

    private const int Resolution = 1024;

    private void Awake()
    {
        //snowRenderTexture = new RenderTexture(Resolution, Resolution, 0);
        snowRenderTexture.filterMode = FilterMode.Point;
        //snowRenderTexture.Create();

        targetMaterial.mainTexture = snowRenderTexture;

        whiteBrushTexture = CreateBrushTexture(Color.white, pileBrushIntensity);
        blackBrushTexture = CreateBrushTexture(Color.black, eraserBrushIntensity);
    }

    private void OnDestroy()
    {
        if (snowRenderTexture) Destroy(snowRenderTexture);
        if (whiteBrushTexture) Destroy(whiteBrushTexture);
        if (blackBrushTexture) Destroy(blackBrushTexture);
    }

    private Texture2D CreateBrushTexture(Color color, float intensity)
    {
        int res = Resolution / 2;
        float hRes = res * 0.5f;
        float sqrSize = hRes * hRes;

        Texture2D texture = new Texture2D(res, res);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                // Sqaure Length From Center
                float sqrLen = (hRes - x) * (hRes - x) + (hRes - y) * (hRes - y);
                float alpha = Mathf.Max(sqrSize - sqrLen, 0f) / sqrSize;

                // Soft
                alpha = Mathf.Pow(alpha, 2f);

                color.a = alpha * intensity;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    /// <summary> ���� �ؽ��Ŀ� �귯�� �ؽ��ķ� �׸��� </summary>
    private void PaintBrush(Texture2D brush, Vector2 uv, float size)
    {
        RenderTexture.active = snowRenderTexture;         // �������� ���� Ȱ�� ���� �ؽ��� �ӽ� �Ҵ�
        GL.PushMatrix();                                  // ��Ʈ���� ���
        GL.LoadPixelMatrix(0, Resolution, Resolution, 0); // �˸��� ũ��� �ȼ� ��Ʈ���� ����

        float brushPixelSize = brushSize * Resolution * size;
        uv.x *= Resolution;
        uv.y *= Resolution;

        // ���� �ؽ��Ŀ� �귯�� �ؽ��ĸ� �̿��� �׸���
        Graphics.DrawTexture(
            new Rect(
                uv.x - brushPixelSize * 0.5f,
                (snowRenderTexture.height - uv.y) - brushPixelSize * 0.5f,
                brushPixelSize,
                brushPixelSize
            ),
            brush
        );

        GL.PopMatrix();              // ��Ʈ���� ����
        RenderTexture.active = null; // Ȱ�� ���� �ؽ��� ����
    }

    /// <summary> �� �ױ� </summary>
    public void PileSnow(Vector3 contactPoint)
    {
        float snowSize = UnityEngine.Random.Range(0.5f, 2.0f);
        Paint(contactPoint, snowSize, true);
    }

    /// <summary> �� ����� </summary>zzzzz
    public void ClearSnow(Vector3 contactPoint, float size)
    {
        Paint(contactPoint, size, false);
    }

    /// <summary> �� �ױ� or ����� </summary>
    private void Paint(in Vector3 contactPoint, float size = 1f, bool pileOrClear = true)
    {
        // ���� �ε��� 3D ��ǥ�κ��� 2D UV ��ǥ ���
        // Plane�� scale 1�� ��ǥ 10�̹Ƿ� 10���� ������
        Vector3 normalizedVec3 = (contactPoint - transform.position) / 10f;
        normalizedVec3.x /= transform.lossyScale.x;
        normalizedVec3.z /= transform.lossyScale.z;

        Vector2 uv = new Vector2(normalizedVec3.x + 0.5f, normalizedVec3.z + 0.5f);

        // UV ���� �ٱ��̸� ����
        if (uv.x < 0f || uv.y < 0f || uv.x > 1f || uv.y > 1f)
            return;

        uv = Vector2.one - uv; // ��ǥ ����

        // 1. �ױ�
        if (pileOrClear)
        {
            PaintBrush(whiteBrushTexture, uv, size);
        }
        // 2. �����
        else
        {
            PaintBrush(blackBrushTexture, uv, size);
        }
    }
}
