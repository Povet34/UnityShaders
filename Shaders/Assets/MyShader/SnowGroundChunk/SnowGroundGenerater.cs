using UnityEngine;

public class SnowGroundGenerater : MonoBehaviour
{
    [SerializeField] Material snowMaterial;
    [SerializeField] SnowGroundChunk snowGroundChunkPrefab;
    [SerializeField] int gridSize = 5;

    private void Start()
    {
        int index = 0;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                var snowGroundChunk = Instantiate(snowGroundChunkPrefab, transform);
                snowGroundChunk.Init(new SnowGroundChunk.ChunkParam
                {
                    index = index++,
                    center = new Vector2(x * 10f, y * 10f), // 격자 간격을 10으로 설정
                    snowMaterial = snowMaterial
                });
            }
        }
    }
}
