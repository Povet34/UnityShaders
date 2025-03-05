using UnityEngine;

namespace Test
{
    public class GroundMovement
    {
        private Transform playerTransform;
        private float speed;

        public GroundMovement(Transform playerTransform, float speed)
        {
            this.playerTransform = playerTransform;
            this.speed = speed;
        }

        public void Move(Vector3 direction)
        {
            playerTransform.Translate(direction * speed * Time.deltaTime);
        }
    }
}