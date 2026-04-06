using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerTriggerFull : MonoBehaviour
{
    private Rigidbody rb;

    private bool finished = false;
    private bool falling = false;

    [Header("Falling")]
    public float sinkSpeed = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;

        Debug.Log($"[TriggerEnter] Player entered trigger: {other.name} at position {transform.position}");

        if (other.CompareTag("Finish"))
        {
            finished = true;

            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            Debug.Log("[TriggerEnter] Player reached Finish!");
            EventManager.RaiseLevelFinished();
        }

        if (other.CompareTag("DeadZone"))
        {
            finished = true;

            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;

            //falling = true;

            Debug.Log("[TriggerEnter] Player fell into DeadZone!");
            EventManager.RaiseLevelFailed();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[CollisionEnter] Player collided with: {collision.collider.name} at position {transform.position}");

        if (collision.collider.transform.parent != null &&
            collision.collider.transform.parent.name == "LevelContent")
        {
            StatisticsManager.Instance?.RegisterCollision();
            Debug.Log("[CollisionEnter] Collision registered in StatisticsManager.");
        }
    }

    void Update()
    {
        if (falling)
        {
            transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
        }
    }
}