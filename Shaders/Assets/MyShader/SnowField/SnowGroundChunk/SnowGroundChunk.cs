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
        // �ε��� �ο�
        index = param.index;
        center = param.center;

        transform.position = new Vector3(center.x, 0, center.y);

        // 2048x2048 ���� �ؽ�ó ����
        renderTexture = new RenderTexture(2048, 2048, 24);
        renderTexture.Create();

        // renderCam�� ���� �ؽ�ó ����
        if (renderCam != null)
        {
            renderCam.targetTexture = renderTexture;
        }

        // snowMaterial �ν��Ͻ� ���� �� ���� �ؽ�ó ����
        instanceSnowMaterial = Instantiate(param.snowMaterial);
        instanceSnowMaterial.SetTexture("_SplatMap", renderTexture);

        meshRenderer.material = instanceSnowMaterial;
    }
}
