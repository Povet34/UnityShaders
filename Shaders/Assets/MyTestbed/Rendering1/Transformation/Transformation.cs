using UnityEngine;

namespace CLC.Rendering
{
    public abstract class Transformation : MonoBehaviour
    {
        public abstract Vector3 Apply(Vector3 point);
    }
}