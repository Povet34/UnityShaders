using UnityEngine;

public class CubeAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct CubeProperties
    {
        public Color baseColor;         // �⺻ ����
        public float brightnessFactor;  // ��� ���
        public float positionOffset;    // X�� ��ġ ������
        public float rotationAngle;     // ȸ�� ����
        public float random;            // random
    }

    public CubeProperties cubeData;   // CubeProperties ������
    public Material targetMaterial;   // ��� ��Ƽ����
    public float speed = 2f;          // �ִϸ��̼� �ӵ�
    public float range = 10f;         // ����

    private ComputeBuffer constantBuffer;  // ComputeBuffer ����

    private Transform[] children;

    void OnEnable()
    {
        // ����ü ũ�� ��� �� ComputeBuffer ����
        int bufferSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CubeProperties));
        Debug.Log($"bufferSize {bufferSize}");
        constantBuffer = new ComputeBuffer(1, bufferSize);

        children = GetComponentsInChildren<Transform>();
        foreach(var child in children)
        {
            if (child != transform) // �ڱ� �ڽ��� ����
            {
                child.transform.position = Random.insideUnitSphere * range;
            }
        }
    }

    void OnDisable()
    {
        // ComputeBuffer ����
        if (constantBuffer != null)
        {
            constantBuffer.Release();
            constantBuffer = null;
        }
    }

    void Update()
    {
        // CubeProperties ������ ������Ʈ (�ð��� ���� �ִϸ��̼�)
        float colorLerpFactor = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        cubeData.baseColor = Color.Lerp(Color.black, Color.red, colorLerpFactor);
        cubeData.brightnessFactor = Mathf.Lerp(0.5f, 1.5f, (Mathf.Sin(Time.time * speed * 0.7f) + 1f) * 0.5f);
        cubeData.positionOffset = Mathf.Sin(Time.time * speed * 0.5f) * range;
        cubeData.rotationAngle = Time.time * speed * 60f;
        cubeData.random = Random.Range(0f, 1f);

        // ComputeBuffer�� ������ ����
        constantBuffer.SetData(new CubeProperties[] { cubeData });

        // ���̴��� ��� ���� ����
        if (targetMaterial != null)
        {
            targetMaterial.SetConstantBuffer("CubePropertiesBuffer", constantBuffer, 0, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CubeProperties)));
        }
    }
}
