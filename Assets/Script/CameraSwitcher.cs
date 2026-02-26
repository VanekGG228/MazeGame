using UnityEngine;

public class FreeCamera : MonoBehaviour
{
    public float mouseSensitivity = 3f;
    public float moveSpeed = 5f;
    public float fastMultiplier = 2f;

    float yaw;
    float pitch;

    void Start()
    {
        // ВАЖНО: берём текущий поворот камеры
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Move()
    {
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed *= fastMultiplier;

        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.Space)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) dir += Vector3.down;

        transform.position += dir * speed * Time.deltaTime;
    }
}
