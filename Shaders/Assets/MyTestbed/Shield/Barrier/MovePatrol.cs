using UnityEngine;

public class MovePatrol : MonoBehaviour
{
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 endPos;

    [SerializeField] float speed = 1.0f;

    bool turn;

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, turn ? startPos : endPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, turn ? startPos : endPos) < 0.1f)
        {
            turn = !turn;
        }
    }
}
