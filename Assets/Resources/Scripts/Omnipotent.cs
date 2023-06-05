using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Omnipotent : MonoBehaviour
{
    //inputs 
    private PlayerInputActions playerActions;

    private InputAction inputClick;
    private InputAction inputSubmit;
    private InputAction inputPause;
    private InputAction inputNavigation;
    private InputAction inputPoint;

    public SettingsValues sValues;
    [HideInInspector] public bool IsPaused;

    float[] bufferTimers;

    private Constants.MenuStates menuState;

    // UI
    public Canvas TitleCanvas;
    public Canvas GameplayCanvas;

    private bool IsLoadingScene => (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.InitialScene.ToString()) || SceneManager.GetActiveScene().name.Equals(Constants.Scenes.Loading.ToString()));
    private bool loadingComplete;

    private Canvas loadedUI;
    private Image GameplayHealthRadial;
    private TMPro.TextMeshProUGUI GameplayHealthText;
    private EventSystem eventSys;

    private int pauseIndex = 1;
    private int deathIndex = 2;
    private int victoryIndex = 3;
    private int titleIndex = 1;

    private int menuValue;
    private int lastMenuValue;
    private Vector2 lastMousePos = Constants.DefaultMousePos;

    private AudioSource aSource;
    public AudioClip MenuSoundNavigate;
    public AudioClip MenuSoundConfirm;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        playerActions = new PlayerInputActions();
        bufferTimers = GlobalScript.Instance.GenerateEnumList(typeof(Constants.UIInputs));
        eventSys = this.GetComponent<EventSystem>();
        aSource = this.GetComponent<AudioSource>();

        titleIndex = TitleCanvas.gameObject.transform.Find(Constants.UIElements.TitleMenu.ToString()).GetSiblingIndex();
        deathIndex = GameplayCanvas.gameObject.transform.Find(Constants.UIElements.DeathMenu.ToString()).GetSiblingIndex();
        pauseIndex = GameplayCanvas.gameObject.transform.Find(Constants.UIElements.PauseMenu.ToString()).GetSiblingIndex();
        victoryIndex = GameplayCanvas.gameObject.transform.Find(Constants.UIElements.VictoryMenu.ToString()).GetSiblingIndex();
    }

    // Start is called before the first frame update
    void Start()
    {
        lastMousePos = inputNavigation.ReadValue<Vector2>();
        SwitchScene(Constants.Scenes.TitleScreen);
    }

    // Update is called once per frame
    void Update()
    {
        BufferUpdate();

        if (!IsLoadingScene)
        {
            QuickSceneSwitch();
        }

        if (IsLoadingScene)
        {
            ResetValues();
        }
        else if (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {
            Time.timeScale = 1;

            menuState = Constants.MenuStates.Title;

            MenuControls();
        }
        else
        {
            UpdateGameplayUI();

            MenuControls();
        }

        UpdateLastMousePos();
    }

    /// <summary>
    /// Checks if the player is using the mouse. Deselects the menu button if they are and the mouse isn't positioned on the button
    /// </summary>
    void UpdateLastMousePos()
    {
        Vector2 inputMouse = inputPoint.ReadValue<Vector2>();
        bool onButton = false;

        PointerEventData pointerEventData = new PointerEventData(eventSys) { position = inputMouse };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        GameObject resultObj = null;

        if (raycastResults.Any())
        {
            foreach (RaycastResult result in raycastResults)
            {
                if (result.gameObject.tag.Equals(Constants.Tags.UIButton.ToString()))
                {
                    onButton = true;
                    resultObj = result.gameObject;
                }
            }
        }

        if (onButton)
        {
            menuValue = resultObj.transform.GetSiblingIndex();
            lastMenuValue = menuValue;
        }
        else
        {
            if (inputMouse != lastMousePos && lastMousePos != Constants.DefaultMousePos)
            {
                menuValue = -1;
            }
        }

        lastMousePos = inputMouse;
    }

    void ResetValues()
    {
        IsPaused = false;
        menuState = Constants.MenuStates.None;
        Time.timeScale = 1;
    }

    void BufferUpdate()
    {
        for (int i = 0; i < bufferTimers.Length; i++)
        {
            if (bufferTimers[i] > 0)
            {
                bufferTimers[i] -= Time.unscaledDeltaTime;
            }
        }
    }

    #region Inputs

    private void OnEnable()
    {
        inputClick = playerActions.UI.Click;
        inputClick.Enable();
        inputClick.performed += InputClick;

        inputSubmit = playerActions.UI.Submit;
        inputSubmit.Enable();
        inputSubmit.performed += InputSubmit;

        inputPause = playerActions.UI.Pause;
        inputPause.Enable();
        inputPause.performed += InputPause;

        inputNavigation = playerActions.UI.Navigate;
        inputNavigation.Enable();

        inputPoint = playerActions.UI.Point;
        inputPoint.Enable();
    }

    private void OnDisable()
    {
        inputClick.Disable();
        inputSubmit.Disable();
        inputPause.Disable();
        inputNavigation.Disable();
        inputPoint.Disable();
    }

    private void InputSubmit(InputAction.CallbackContext context)
    {
        if (IsLoadingScene)
        {

        }
        else
        {
            GetMenuAction();
        }
    }

    private void InputClick(InputAction.CallbackContext context)
    {
        if (IsLoadingScene)
        {

        }
        else
        {
            GetMenuAction();
        }
    }

    private void InputPause(InputAction.CallbackContext context)
    {
        if (IsLoadingScene)
        {
        }
        else if (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {
            //SwitchScene(Constants.Scenes.LevelAdjusted);
        }
        else
        {
            ChangePauseState();
        }
    }
    #endregion

    void ChangePauseState()
    {
        if (menuState == Constants.MenuStates.Pause || menuState == Constants.MenuStates.None)
        {
            aSource.PlayOneShot(MenuSoundConfirm);
            IsPaused = !IsPaused;
            loadedUI.transform.GetChild(pauseIndex).gameObject.SetActive(IsPaused);

            menuValue = 0;
            lastMenuValue = menuValue;

            if (IsPaused)
            {
                Time.timeScale = 0;

                menuState = Constants.MenuStates.Pause;

                if (loadedUI.transform.GetChild(pauseIndex) == null)
                {
                }
            }
            else
            {
                Time.timeScale = 1;

                menuState = Constants.MenuStates.None;

                if (loadedUI.transform.GetChild(pauseIndex) != null)
                {
                    //Destroy(loadedPause);
                }
            }
        }
    }

    public void SetVictoryState()
    {
        menuState = Constants.MenuStates.Victory;
        loadedUI.transform.GetChild(victoryIndex).gameObject.SetActive(true);
    }

    void MenuControls()
    {
        if (loadingComplete && menuState != Constants.MenuStates.None)
        {
            int menuItems = 0;
            int currentIndex = 0;

            switch (menuState)
            {
                case Constants.MenuStates.Pause:
                    currentIndex = pauseIndex;
                    menuItems = loadedUI.transform.GetChild(currentIndex).childCount - 2;
                    break;
                case Constants.MenuStates.Death:
                    currentIndex = deathIndex;
                    menuItems = loadedUI.transform.GetChild(currentIndex).childCount - 2;
                    break;
                case Constants.MenuStates.Title:
                    currentIndex = titleIndex;
                    menuItems = loadedUI.transform.GetChild(currentIndex).childCount - 1;
                    break;
                case Constants.MenuStates.Victory:
                    //currentIndex = victoryIndex;
                    //menuItems = loadedUI.transform.GetChild(currentIndex).childCount - 1;
                    break;
            }

            Vector2 nav = inputNavigation.ReadValue<Vector2>();

            if (bufferTimers[(int)Constants.UIInputs.Navigate] <= 0 && menuItems > 0)
            {
                if (nav.y > 0)
                {
                    if (menuValue < 0)
                    {
                        menuValue = lastMenuValue;
                    }
                    else
                    {
                        menuValue--;
                        if (menuValue < 0)
                        {
                            menuValue = menuItems;
                        }
                        lastMenuValue = menuValue;
                    }

                    aSource.PlayOneShot(MenuSoundNavigate);
                    bufferTimers[(int)Constants.UIInputs.Navigate] = sValues.BufferLeniency;
                }
                else if (nav.y < 0)
                {
                    if (menuValue < 0)
                    {
                        menuValue = lastMenuValue;
                    }
                    else
                    {
                        menuValue++;
                        if (menuValue > menuItems)
                        {
                            menuValue = 0;
                        }
                        lastMenuValue = menuValue;
                    }

                    aSource.PlayOneShot(MenuSoundNavigate);
                    bufferTimers[(int)Constants.UIInputs.Navigate] = sValues.BufferLeniency;
                }
            }

            if (nav.y == 0)
            {
                bufferTimers[(int)Constants.UIInputs.Navigate] = 0;
            }

            for (int i = 0; i <= menuItems; i++)
            {
                VisualizeMenuOption(loadedUI.transform.GetChild(currentIndex).GetChild(i), menuValue, i);
            }
        }
    }

    void VisualizeMenuOption(Transform option, int currentIndex, int currentOption)
    {
        option.GetComponent<Image>().color = (currentIndex == currentOption) ? Color.white : new Color32(255, 255, 255, 100);
    }

    void GetMenuAction()
    {
        string optionName = "";

        if (menuValue >= 0)
        {
            switch (menuState)
            {
                case Constants.MenuStates.Pause:
                    optionName = loadedUI.transform.GetChild(pauseIndex).GetChild(menuValue).name;
                    break;
                case Constants.MenuStates.Death:
                    optionName = loadedUI.transform.GetChild(deathIndex).GetChild(menuValue).name;
                    break;
                case Constants.MenuStates.Title:
                    optionName = loadedUI.transform.GetChild(titleIndex).GetChild(menuValue).name;
                    break;
                case Constants.MenuStates.Victory:
                    //optionName = loadedUI.transform.GetChild(victoryIndex).GetChild(menuValue).name;
                    break;
            }
        }

        if (optionName.Equals(""))
        {

        }
        else
        {
            aSource.PlayOneShot(MenuSoundConfirm);
        }

        if (optionName.Equals(Constants.MenuOptions.Button_Start.ToString()))
        {
            SwitchScene(Constants.Scenes.LevelAdjusted);
        }
        else if (optionName.Equals(Constants.MenuOptions.Button_Resume.ToString()))
        {
            ChangePauseState();
        }
        else if (optionName.Equals(Constants.MenuOptions.Button_Restart.ToString()))
        {
            SwitchScene(SceneManager.GetActiveScene().name);
        }
        else if (optionName.Equals(Constants.MenuOptions.Button_ReturnToTitle.ToString()))
        {
            SwitchScene(Constants.Scenes.TitleScreen);
        }
        else if (optionName.Equals(Constants.MenuOptions.Button_Quit.ToString()))
        {
            Application.Quit();
        }
    }

    void UpdateGameplayUI()
    {
        Cursor.lockState = (menuState != (Constants.MenuStates.None)) ? CursorLockMode.Locked : CursorLockMode.None;

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

        if(UIHealthCurrent <= 0)
        {
            menuState = Constants.MenuStates.Death;
            loadedUI.transform.GetChild(deathIndex).gameObject.SetActive(true);
        }
    }

    [HideInInspector] public int UIHealthMax;
    [HideInInspector] public int UIHealthCurrent;

    public void UIHealthUpdate(int max, int current)
    {
        UIHealthMax = max;
        UIHealthCurrent = current;
    }

    #region Scenes
    public void SwitchScene(string scene)
    {
        StartCoroutine(LoadingAsync(scene));
    }

    public void SwitchScene(Constants.Scenes scene)
    {
        SwitchScene(scene.ToString());
    }

    void UnloadUI()
    {
        loadingComplete = false;
        menuValue = 0;
        loadedUI = null;
        GameplayHealthRadial = null;
        GameplayHealthText = null;
    }

    void LoadUI()
    {
        if (IsLoadingScene)
        {

        }
        else if (SceneManager.GetActiveScene().name.Equals(Constants.Scenes.TitleScreen.ToString()))
        {
            loadedUI = GameObject.Instantiate(TitleCanvas);
        }
        else
        {
            loadedUI = GameObject.Instantiate(GameplayCanvas);

            GameplayHealthRadial = loadedUI.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            GameplayHealthText = loadedUI.transform.GetChild(0).GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();

            loadedUI.transform.GetChild(pauseIndex).gameObject.SetActive(false);
            loadedUI.transform.GetChild(deathIndex).gameObject.SetActive(false);
            loadedUI.transform.GetChild(victoryIndex).gameObject.SetActive(false);
        }

        loadingComplete = true;
    }

    IEnumerator LoadingAsync(string scene)
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
            if (_progressBar != null)
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

    public void LoadNextScene(Constants.Scenes scene)
    {
        LoadNextScene(scene.ToString());
    }

    public void LoadNextScene(string scene)
    {
        if (scene.Equals(Constants.Scenes.LevelAdjusted.ToString()))
        {
            SwitchScene(Constants.Scenes.TitleScreen);
        }
        else
        {

        }
    }

    /// <summary>
    /// Switch scenes with the press of a button. Debug function
    /// </summary>
    void QuickSceneSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchScene(Constants.Scenes.SebastianScene);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchScene(Constants.Scenes.LevelAdjusted);
        }
    }
    #endregion

    public void UpdateReticle(bool hasObject, bool aimTargetIsEnemy)
    {
        loadedUI.transform.Find("Reticle").gameObject.SetActive(hasObject);

        loadedUI.transform.Find("Reticle").GetComponent<Image>().color = (aimTargetIsEnemy) ? Color.red : Color.white;
    }
}
