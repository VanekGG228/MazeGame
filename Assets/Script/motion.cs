//using System.Collections;
//using System.Collections.Generic;

//using UnityEngine;

//public class motion : MonoBehaviour
//{
//    public float speed = 10f;
//    Rigidbody rb;

//    SerialReader serial;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        serial = FindObjectOfType<SerialReader>();
//    }

//    void FixedUpdate()
//    {
//        if (serial != null)
//        {
//            Vector2 tilt = serial.GetTilt();

//            float x = tilt.x;
//            float y = tilt.y;

//            rb.AddForce(new Vector3(x * speed, 0, y * speed));
//        }
//    }
//    private void OnCollisionEnter(Collision collision)
//    {
//        StatisticsManager.Instance.AddCollision();
//    }
//}

////using System.Collections;
////using System.Collections.Generic;
////using UnityEngine;
////public class motion : MonoBehaviour
////{
////    public float speed = 10F; Rigidbody rd; float xinput; float yinput;

////    void Start() { }
////    private void Awake()
////    {
////        rd = GetComponent<Rigidbody>();
////    }
////    public void FixedUpdate()
////    {
////        xinput = Input.GetAxis("Horizontal");
////        yinput = Input.GetAxis("Vertical");
////        rd.AddForce(xinput * speed, 0, yinput * speed);
////    }
////    private void OnCollisionEnter(Collision collision)
////    {
////        StatisticsManager.Instance.AddCollision();
////    }
////}