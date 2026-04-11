using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainUI : MonoBehaviour
{
    [SerializeField] private BlackScreenController BlackScreen;
    public void StartGame()
    {
        StartCoroutine(StartGameProcess());
    }

    private IEnumerator StartGameProcess()
    {
        Debug.Log("Start Game!");
        BlackScreen.FadeIn();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        BlackScreen.FadeOut();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
