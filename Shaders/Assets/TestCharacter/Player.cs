using UnityEngine;


namespace Test
{
    public class Player : MonoBehaviour
    {
        GroundMovement movement;
        public float speed = 5f;

        // Start is called before the first frame update
        void Start()
        {
            movement = new GroundMovement(transform, speed);
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            movement.Move(direction);
        }
    }
}
