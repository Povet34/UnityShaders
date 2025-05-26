using UnityEngine;

public class CubeAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct CubeProperties
    {
        public Color baseColor;         // 기본 색상
        public float brightnessFactor;  // 밝기 계수
        public float positionOffset;    // X축 위치 오프셋
        public float rotationAngle;     // 회전 각도
        public float random;            // random
    }

    public CubeProperties cubeData;   // CubeProperties 데이터
    public Material targetMaterial;   // 대상 머티리얼
    public float speed = 2f;          // 애니메이션 속도
    public float range = 10f;         // 범위

    private ComputeBuffer constantBuffer;  // ComputeBuffer 선언

    private Transform[] children;

    void OnEnable()
    {
        // 구조체 크기 계산 및 ComputeBuffer 생성
        int bufferSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CubeProperties));
        Debug.Log($"bufferSize {bufferSize}");
        constantBuffer = new ComputeBuffer(1, bufferSize);

        children = GetComponentsInChildren<Transform>();
        foreach(var child in children)
        {
            if (child != transform) // 자기 자신은 제외
            {
                child.transform.position = Random.insideUnitSphere * range;
            }
        }
    }

    void OnDisable()
    {
        // ComputeBuffer 해제
        if (constantBuffer != null)
        {
            constantBuffer.Release();
            constantBuffer = null;
        }
    }

    void Update()
    {
        // CubeProperties 데이터 업데이트 (시간에 따른 애니메이션)
        float colorLerpFactor = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        cubeData.baseColor = Color.Lerp(Color.black, Color.red, colorLerpFactor);
        cubeData.brightnessFactor = Mathf.Lerp(0.5f, 1.5f, (Mathf.Sin(Time.time * speed * 0.7f) + 1f) * 0.5f);
        cubeData.positionOffset = Mathf.Sin(Time.time * speed * 0.5f) * range;
        cubeData.rotationAngle = Time.time * speed * 60f;
        cubeData.random = Random.Range(0f, 1f);

        // ComputeBuffer에 데이터 전달
        constantBuffer.SetData(new CubeProperties[] { cubeData });

        // 셰이더에 상수 버퍼 전달
        if (targetMaterial != null)
        {
            targetMaterial.SetConstantBuffer("CubePropertiesBuffer", constantBuffer, 0, System.Runtime.InteropServices.Marshal.SizeOf(typeof(CubeProperties)));
        }
    }
}
