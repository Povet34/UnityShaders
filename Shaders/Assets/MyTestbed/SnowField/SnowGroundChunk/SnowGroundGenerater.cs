using System.Collections.Generic;
using UnityEngine;

public class SnowGroundGenerater : MonoBehaviour
{
    [SerializeField] Material snowMaterial;
    [SerializeField] SnowGroundChunk snowGroundChunkPrefab;
    [SerializeField] int gridSize = 5;

    [SerializeField] int offsetSize = 19;

    List<SnowGroundChunk> snowGroundChunks = new List<SnowGroundChunk>();

    //private void OnValidate()
    //{
    //    Debug.Log("ddd");
    //    DestroyChunks();
    //    GenerateChunk();
    //}

    private void Start()
    {
        GenerateChunk();
    }

    void DestroyChunks()
    {
        foreach (var chunk in snowGroundChunks)
        {
            Destroy(chunk.gameObject);
        }
        snowGroundChunks.Clear();
    }


    void GenerateChunk()
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
                    center = new Vector2(x * offsetSize, y * offsetSize),
                    snowMaterial = snowMaterial
                });

                snowGroundChunks.Add(snowGroundChunk);
            }
        }
    }
}
