using UnityEngine;

public class CutoutObject : MonoBehaviour
{
    Camera mainCam;
    [SerializeField] Transform targetObject;
    [SerializeField] LayerMask wawllMask;

    void Start()
    {
        mainCam = GetComponent<Camera>();
    }

    private void Update()
    {
        var cutoutPos = mainCam.WorldToViewportPoint(targetObject.position);
        cutoutPos.y /= (Screen.width / Screen.height);

        var offset = targetObject.position - mainCam.transform.position;
        var hits = Physics.RaycastAll(mainCam.transform.position, offset, offset.magnitude, wawllMask);

        for(int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var renderer = hit.transform.GetComponent<Renderer>();

            for(int m = 0; m < renderer.materials.Length; m++)
            {
                var mat = renderer.materials[m];
                mat.SetVector("_CutoutPosition", cutoutPos);
                mat.SetFloat("_CutoutSize", 0.1f);
                mat.SetFloat("_FalloffSize", 0.05f);
            }
        }
    }
}
