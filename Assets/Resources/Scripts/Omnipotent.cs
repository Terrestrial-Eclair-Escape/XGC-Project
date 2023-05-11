using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Omnipotent : MonoBehaviour
{
    // Inputs
    private PlayerInputActions playerActions;

    private InputAction inputClick;
    private InputAction inputSubmit;


    // UI
    public Canvas GameplayCanvas;

    private bool IsLoadingScene => (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.InitialScene.ToString()) || SceneManager.GetActiveScene().name.Equals(Constants.Scenes.Loading.ToString()));

    private Canvas loadedUI;
    private Image GameplayHealthRadial;
    private TMPro.TextMeshProUGUI GameplayHealthText;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        playerActions = new PlayerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadingAsync(Constants.Scenes.TitleScreen));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsLoadingScene)
        {
            QuickSceneSwitch();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        if (IsLoadingScene)
        {

        }
        else if (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {
            //StartCoroutine(LoadingAsync(Constants.Scenes.LevelAdjusted));
        }
        else
        {
            UpdateGameplayUI();
        }
    }

    private void OnEnable()
    {
        inputClick = playerActions.UI.Click;
        inputClick.Enable();

        inputSubmit = playerActions.Player.Jump;
        inputSubmit.Enable();
        inputSubmit.performed += InputSubmit;
    }

    private void OnDisable()
    {
        inputClick.Disable();
        inputSubmit.Disable();
    }

    private void InputSubmit(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {
            StartCoroutine(LoadingAsync(Constants.Scenes.LevelAdjusted));
        }
    }

    IEnumerator LoadingAsync(Constants.Scenes scene)
    {
        UnloadUI();

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

        LoadUI();
    }

    void UnloadUI()
    {
        GameplayHealthRadial = null;
        GameplayHealthText = null;
    }

    void LoadUI()
    {
        if (IsLoadingScene)
        {

        } 
        else if(SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {

        }
        else
        {
            LoadGameplayUI();
        }
    }

    void LoadGameplayUI()
    {
        loadedUI = GameObject.Instantiate(GameplayCanvas);

        GameplayHealthRadial = loadedUI.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        GameplayHealthText = loadedUI.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
    }

    void UpdateGameplayUI()
    {

        if (GameplayHealthRadial != null)
        {
            if (UIHealthMax != 0)
            {
                GameplayHealthRadial.fillAmount = (float)UIHealthCurrent / (float)UIHealthMax;
            }
        }
        else
        {
            GameplayHealthRadial = loadedUI.transform.GetChild(0).GetChild(0).GetComponent<Image>();
        }


        if (GameplayHealthText != null)
        {
            GameplayHealthText.text = UIHealthCurrent.ToString();
        }
        else
        {
            GameplayHealthText = loadedUI.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        }
    }

    public int UIHealthMax;
    public int UIHealthCurrent;

    public void UIHealthUpdate(int max, int current)
    {
        UIHealthMax = max;
        UIHealthCurrent = current;
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
