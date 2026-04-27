using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;
    public KeyCode switchKey = KeyCode.Tab;

    private int currentIndex = 0;

    void Start()
    {
        ActivateCamera(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(switchKey))
        {
            currentIndex++;
            if (currentIndex >= cameras.Length)
                currentIndex = 0;

            ActivateCamera(currentIndex);
        }
    }

    void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;
            cameras[i].gameObject.SetActive(i == index);
        }

        Debug.Log("[CameraSwitcher] Active camera: " + cameras[index].name);
    }
}