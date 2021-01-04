using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UIController : MonoBehaviour
{
    public AudioMixer mixer;
    private GameObject mainMenu;

    private GameObject PracticeMenu;

    private GameObject InGameMenu;

    private GameObject settingsMenu;

    private GameObject turnText;

    private GameController gameController;

    private Texture2D pointCursor;

    private Texture2D dragCursor;

    private Texture2D handCursor;

    private AudioSource audioSource;

    private AudioClip highlightClick;

    private AudioClip click;

    private AudioClip woosh;

    private int amountOfMenuButtons;

    private float main_menu_min_x = 0.3f;
    
    private float main_menu_max_x = 0.7f;

    private float main_menu_y_interval_size = 0.5f;

    private float main_menu_btn_height = 0.1f;
    // Start is called before the first frame update
    void Start()
    {    
        SetUpPlayerPrefs();
        PracticeMenu = GameObject.Find("PracticeMenu");
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        mainMenu = GameObject.Find("MainMenu");
        InGameMenu = GameObject.Find("InGameMenu");
        settingsMenu = GameObject.Find("SettingsMenu");
        turnText = GameObject.Find("TurnText");
        turnText.SetActive(false);
        PracticeMenu.SetActive(false);
        InGameMenu.SetActive(false);
        settingsMenu.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        highlightClick = (AudioClip)Resources.Load("Sound/UI/Highlight_click");
        click = (AudioClip)Resources.Load("Sound/UI/Click");
        woosh = (AudioClip)Resources.Load("Sound/SFX/Woosh_1");
        SetUpMenu(mainMenu);
        SetUpMenu(InGameMenu, new string[]{"Background"});
        SetUpCursors();
    }

    // sets up the given menu's buttons to correct positions
    private void SetUpMenu(GameObject menu, string[] exceptionTags)
    {
        int amountOfButtons = menu.transform.childCount;
        int step = 0;
        bool skip = false;
        float size_between_btns = (main_menu_y_interval_size - amountOfButtons*main_menu_btn_height)/(amountOfButtons-1-exceptionTags.Length);
        for (int i = 0; i < amountOfButtons; i++) 
        {
            skip = false;
            RectTransform rt = menu.transform.GetChild(amountOfButtons - 1 - i).GetComponent<RectTransform>();
            for (int j = 0; j<exceptionTags.Length; j++)
            {
                if(rt.tag == exceptionTags[j])
                {
                    skip = true;
                    break;
                }
            }
            if(skip)
            {
                continue;
            }
            rt.anchorMin = new Vector2(main_menu_min_x, main_menu_y_interval_size/2 + step*(main_menu_btn_height + size_between_btns));
            rt.anchorMax = new Vector2(main_menu_max_x, main_menu_y_interval_size/2 + main_menu_btn_height + step*(main_menu_btn_height + size_between_btns));
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            step++;
        }
    }

    private void SetUpMenu(GameObject menu)
    {
        SetUpMenu(menu, new string[]{});
    }

    // loads cursors and sets the cursor to handcursor
    private void SetUpCursors()
    {
        pointCursor = (Texture2D)Resources.Load("Cursors/Point");
        dragCursor = (Texture2D)Resources.Load("Cursors/Drag");
        handCursor = (Texture2D)Resources.Load("Cursors/Hand");
        Cursor.SetCursor(handCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    // loads the playerprefs for sound and activates the on sliders
    private void SetUpPlayerPrefs()
    {
        string[] keys = {"SoundEffectVol", "MasterVol", "MusicVol"}; 

        foreach(string key in keys)
        {
            float value = PlayerPrefs.GetFloat(key);
            mixer.SetFloat(key, value);
            Debug.Log(GameObject.FindGameObjectWithTag(key).GetComponent<Slider>().value);
            GameObject.FindGameObjectWithTag(key).GetComponent<Slider>().value = value;
        }
    }

    // sets sfx sound level to given float
    public void SetSFXlvl(float lvl)
    {
        Debug.Log("setting sfx to " + lvl);
        mixer.SetFloat("SoundEffectVol", lvl);
        PlayerPrefs.SetFloat("SoundEffectVol", lvl);
    }
    
    // Sets the Master sound level to given float
    public void SetMasterlvl(float lvl)
    {
        Debug.Log("setting master level to " + lvl);
        mixer.SetFloat("MasterVol", lvl);
        PlayerPrefs.SetFloat("MasterVol", lvl);
    }

    // Sets the Music sound level to given float
    public void SetMusiclvl(float lvl)
    {
        mixer.SetFloat("MusicVol", lvl);
        PlayerPrefs.SetFloat("MusicVol", lvl);
    }
    // Sets the cursor to drag cursor
    public void SetDragCursor()
    {
        Cursor.SetCursor(dragCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    // Sets the cursor to pointing cursor
    public void SetPointCursor()
    {
        Cursor.SetCursor(pointCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    // Sets the cursor to hand cursor
    public void SetHandCursor()
    {
        Cursor.SetCursor(handCursor, new Vector3(100f, 0.0f, 0.0f), CursorMode.Auto);
    }

    // plays highlight sound
    public void PlayHighlightSound()
    {
        SetPointCursor();
        audioSource.PlayOneShot(highlightClick);
    }
    // plays click sound
    public void PlayClickSound()
    {
        audioSource.PlayOneShot(click);
    }

    // Toggles practice mode
    public void SetPractice(bool value){
        mainMenu.GetComponent<Animator>().SetBool("Practice", value);
        PracticeMenu.SetActive(value);
        gameController.SetCanClickPieces(value);
        gameController.SetPractice(value);
    }

    // toggles main menu, if turned off, activate practice trigger on animator, and set pieces clickable
    public void SetMenusActive(bool value)
    {
        mainMenu.SetActive(value);
        mainMenu.GetComponent<Animator>().SetTrigger("Activate");
        if(!value)
        {
            PracticeMenu.SetActive(value);
            mainMenu.GetComponent<Animator>().SetBool("Practice", value);
            gameController.SetCanClickPieces(value);
        }
    }
    // Sets Settings menu's active state to given value
    public void SetSettingsActive(bool value){
        settingsMenu.SetActive(value);
    }

    // Sets ingame menu's active state to given value
    public void SetInGameMenuActive(bool value)
    {
        InGameMenu.SetActive(value);
    }

    // sets text to your turn or their turn from given value
    public void SetTurnText(bool value)
    {
        if(value)
        {
            turnText.GetComponent<Text>().text = "YOUR TURN";
            turnText.GetComponent<Animation>().Play();
            audioSource.PlayOneShot(woosh);
        } else 
        {
            turnText.GetComponent<Text>().text = "THEIR TURN";
            turnText.GetComponent<Animation>().Play();
        }
    }
    // Sets turn text active state to given value
    public void SetTurnTextActive(bool value)
    {
        turnText.SetActive(value);
    }

    // toggles in game menu
    public void toggleInGameMenu()
    {
        InGameMenu.SetActive(!InGameMenu.activeSelf);
    }

    // Quits the application
    public void ExitGame()
    {
        Application.Quit();
    }

}
