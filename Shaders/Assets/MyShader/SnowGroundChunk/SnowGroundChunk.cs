using UnityEngine;

public class SnowGroundChunk : MonoBehaviour
{
    public struct ChunkParam
    {
        public int index;
        public Vector2 center;
        public Material snowMaterial;
    }

    public int index;
    public Vector2 center;
    [SerializeField] Camera renderCam;
    [SerializeField] MeshRenderer meshRenderer;
    RenderTexture renderTexture;
    private Material instanceSnowMaterial;

    public void Init(ChunkParam param)
    {
        // 인덱스 부여
        index = param.index;
        center = param.center;

        transform.position = new Vector3(center.x, 0, center.y);

        // 2048x2048 렌더 텍스처 생성
        renderTexture = new RenderTexture(2048, 2048, 24);
        renderTexture.Create();

        // renderCam에 렌더 텍스처 적용
        if (renderCam != null)
        {
            renderCam.targetTexture = renderTexture;
        }

        // snowMaterial 인스턴스 생성 및 렌더 텍스처 적용
        instanceSnowMaterial = Instantiate(param.snowMaterial);
        instanceSnowMaterial.SetTexture("_SplatMap", renderTexture);

        meshRenderer.material = instanceSnowMaterial;
    }
}
