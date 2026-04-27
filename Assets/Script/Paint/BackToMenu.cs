using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    public void BackToMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }

    public void BackToMenuButton2()
    {
        var serial = FindObjectOfType<SerialReader>();

        if (serial != null)
            serial.StopSerial();

        SceneManager.LoadScene("Menu");
    }
}