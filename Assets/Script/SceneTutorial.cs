using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SceneTutorial : MonoBehaviour
{
    [Header("Tutorial ID")]
    public string tutorialID = "Tutorial";

    [Header("UI")]
    public Image tutorialImage;

    public Button btnSkip;
    public Button btnNext;
    public Button btnReady;

    [Header("Pages")]
    public Sprite[] tutorialPages;

    private int currentPage = 0;

    [Header("Animation")]
    public CanvasGroup canvasGroup;

    public Transform tutorialWindow;

    public float fadeDuration = 0.25f;
    public float popDuration = 0.25f;

    void Start()
    {
        // Sudah pernah lihat tutorial?
        if (PlayerPrefs.GetInt(tutorialID, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        Time.timeScale = 0f;

        ShowPage();
        StartCoroutine(PlayOpenAnimation());
    }

    public void NextPage()
    {
        currentPage++;

        if (currentPage >= tutorialPages.Length)
        {
            FinishTutorial();
            return;
        }

        ShowPage();
        StartCoroutine(PlayOpenAnimation());
    }

    public void SkipTutorial()
    {
        StartCoroutine(CloseTutorial());
    }

    public void FinishTutorial()
    {
        StartCoroutine(CloseTutorial());
    }

    void ShowPage()
    {
        if (tutorialPages.Length == 0)
            return;

        tutorialImage.sprite =
            tutorialPages[currentPage];

        // Hanya 1 gambar
        if (tutorialPages.Length == 1)
        {
            btnSkip.gameObject.SetActive(false);
            btnNext.gameObject.SetActive(false);
            btnReady.gameObject.SetActive(true);

            return;
        }

        // Halaman terakhir
        if (currentPage == tutorialPages.Length - 1)
        {
            btnSkip.gameObject.SetActive(false);
            btnNext.gameObject.SetActive(false);
            btnReady.gameObject.SetActive(true);
        }
        else
        {
            btnSkip.gameObject.SetActive(true);
            btnNext.gameObject.SetActive(true);
            btnReady.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayOpenAnimation()
    {
        canvasGroup.alpha = 0f;

        tutorialWindow.localScale =
            Vector3.one * 0.8f;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / fadeDuration;

            canvasGroup.alpha =
                Mathf.Lerp(0f, 1f, t);

            tutorialWindow.localScale =
                Vector3.Lerp(
                    Vector3.one * 0.8f,
                    Vector3.one * 1.05f,
                    t
                );

            yield return null;
        }

        timer = 0f;

        Vector3 startScale =
            tutorialWindow.localScale;

        while (timer < 0.1f)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / 0.1f;

            tutorialWindow.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.one,
                    t
                );

            yield return null;
        }

        tutorialWindow.localScale =
            Vector3.one;
    }

    IEnumerator CloseTutorial()
    {
        PlayerPrefs.SetInt(tutorialID, 1);
        PlayerPrefs.Save();

        float timer = 0f;

        Vector3 startScale =
            tutorialWindow.localScale;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                timer / fadeDuration;

            canvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    t
                );

            tutorialWindow.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.one * 0.8f,
                    t
                );

            yield return null;
        }

        Time.timeScale = 1f;

        gameObject.SetActive(false);
    }
}