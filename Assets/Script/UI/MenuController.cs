using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject profilePanel;  
    public GameObject scrollView;
    public GameObject graphArea;
    public GameObject statsText;
    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void OpenRedactor()
    {
        SceneManager.LoadScene("Redactor");
    }

    public void BackToMenu()
    {
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenProfile()
    {
        mainMenuPanel.SetActive(false);
        profilePanel.SetActive(true);
    }

    public void BackFromProfile()
    {

        if (graphArea.activeSelf || statsText.gameObject.activeSelf)
        {
            // Скрываем график и статистику
            graphArea.SetActive(false);
            statsText.gameObject.SetActive(false);
     

            scrollView.SetActive(true);
        }
        else
        {
            // Иначе возвращаемся в главное меню
            profilePanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}