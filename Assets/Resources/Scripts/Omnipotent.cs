using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Omnipotent : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadingAsync(Constants.Scenes.TitleScreen));
    }

    // Update is called once per frame
    void Update()
    {
        if (!SceneManager.GetActiveScene().name.Equals(Constants.Scenes.InitialScene.ToString()) && !SceneManager.GetActiveScene().name.Equals(Constants.Scenes.Loading.ToString()))
        {
            QuickSceneSwitch();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    IEnumerator LoadingAsync(Constants.Scenes scene)
    {
        Debug.Log("loading started");

        // load the loading screen
        AsyncOperation operationLoading = SceneManager.LoadSceneAsync(Constants.Scenes.Loading.ToString());
        operationLoading.allowSceneActivation = false;

        while (!operationLoading.isDone)
        {
            if (operationLoading.progress >= 0.9f)
            {
                operationLoading.allowSceneActivation = true;
            }

            yield return null;
        }

        // find the progress bar
        Image _progressBar = GameObject.FindWithTag(Constants.Tags.UILoadingFillBar.ToString()).GetComponent<Image>();

        // load the new scene
        AsyncOperation operationMain = SceneManager.LoadSceneAsync(scene.ToString());
        operationMain.allowSceneActivation = false;

        while (!operationMain.isDone)
        {
            if(_progressBar != null)
            {
                _progressBar.fillAmount = operationMain.progress;
            }

            if (operationMain.progress >= 0.9f)
            {
                operationMain.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Switch scenes with the press of a button. Debug function
    /// </summary>
    void QuickSceneSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(LoadingAsync(Constants.Scenes.SebastianScene));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(LoadingAsync(Constants.Scenes.LevelAdjusted));
        }
    }
}
