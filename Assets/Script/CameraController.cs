using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;       
    public float rotateSpeed = 5f;      
    public float zoomSpeed = 10f;      

    private float yaw = 0f;
    private float pitch = 45f; 
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) 
        {
            yaw += Input.GetAxis("Mouse X") * rotateSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotateSpeed;
            pitch = Mathf.Clamp(pitch, 10f, 80f); 
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }


        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")) * moveSpeed * Time.deltaTime;
        transform.position += move;

  
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.position += transform.forward * scroll * zoomSpeed;
    }
}
