using UnityEngine;

public class MeshMerger : MonoBehaviour
{
    public MeshFilter[] meshFilters; // ������ MeshFilter �迭
    public bool mergeSubMeshes = true; // ���� �޽� ���� ����

    void Start()
    {
        MergeMeshes();
    }

    void MergeMeshes()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine, mergeSubMeshes);

        mf.mesh = mesh;

        // ���յ� �޽ø� ����� ���� �޽� ����
        foreach (MeshFilter m in meshFilters)
        {
            Destroy(m.gameObject);
        }
    }
}
