using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerTriggerFull : MonoBehaviour
{
    [Header("UI")]
    public GameObject winText;
    public GameObject loseText;
    public float animationTime = 0.5f;
    public float displayTime = 2f;

    [Header("Falling")]
    public float sinkSpeed = 2f; // скорость падения в яму
    public float pullSpeed = 3f; // притяжение к центру

    private bool finished = false;
    private Transform targetBall;
    private bool falling = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished) return;

        if (other.CompareTag("Finish"))
        {
            finished = true;
            rb.isKinematic = true; // останавливаем физику шарика
            Debug.Log("ПОБЕДА!");
            if (winText != null)
            {
                winText.SetActive(true);
                winText.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateUI(winText));
            }
        }
        else if (other.CompareTag("DeadZone"))
        {
            finished = true;
            rb.isKinematic = true; // отключаем физику, чтобы управлять падением
            targetBall = transform;
            falling = true;

            if (loseText != null)
            {
                loseText.SetActive(true);
                loseText.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateUI(loseText));
            }

            Debug.Log("ПРОИГРЫШ!");
        }
    }

    void Update()
    {
        if (falling && targetBall != null)
        {
            // Тянем к центру зоны (по X,Z)
            targetBall.position = Vector3.MoveTowards(
                targetBall.position,
                new Vector3(targetBall.position.x, targetBall.position.y, targetBall.position.z),
                pullSpeed * Time.deltaTime
            );

            // Опускаем вниз
            targetBall.position += Vector3.down * sinkSpeed * Time.deltaTime;
        }
    }

    System.Collections.IEnumerator AnimateUI(GameObject text)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        // Анимация роста текста
        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / animationTime);
            yield return null;
        }

        text.transform.localScale = endScale;

        // Держим текст на экране
        yield return new WaitForSeconds(displayTime);

        // Перезапуск уровня
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
