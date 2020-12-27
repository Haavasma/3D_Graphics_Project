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

    private AudioSource audioSource;

    private AudioClip highlightClick;

    private AudioClip click;

    private int amountOfMenuButtons;

    private float main_menu_min_x = 0.3f;
    
    private float main_menu_max_x = 0.7f;

    private float main_menu_y_interval_size = 0.5f;

    private float main_menu_btn_height = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
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

        SetUpMenu(mainMenu);
        SetUpMenu(InGameMenu);
        SetUpCursors();
    }

    private void SetUpMenu(GameObject menu)
    {
        int amountOfButtons = menu.transform.childCount;
        float size_between_btns = (main_menu_y_interval_size - amountOfButtons*main_menu_btn_height)/(amountOfButtons-1);
        for (int i = 0; i < amountOfButtons; i++) 
        {
            RectTransform rt = menu.transform.GetChild(amountOfButtons - 1 - i).GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(main_menu_min_x, main_menu_y_interval_size/2 + i*(main_menu_btn_height + size_between_btns));
            rt.anchorMax = new Vector2(main_menu_max_x, main_menu_y_interval_size/2 + main_menu_btn_height + i*(main_menu_btn_height + size_between_btns));
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    private void SetUpCursors()
    {
        pointCursor = (Texture2D)Resources.Load("Cursors/Point");
        dragCursor = (Texture2D)Resources.Load("Cursors/Drag");
        Cursor.SetCursor(pointCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    public void SetSFXlvl(float lvl)
    {
        Debug.Log("setting sfx to " + lvl);
        mixer.SetFloat("SoundEffectVol", lvl);
    }

    public void SetMasterlvl(float lvl)
    {
        Debug.Log("setting master level to " + lvl);
        mixer.SetFloat("MasterVol", lvl);
    }

    public void SetMusiclvl(float lvl)
    {
        mixer.SetFloat("MusicVol", lvl);
    }
    public void SetDragCursor()
    {
        Cursor.SetCursor(dragCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    public void SetPointCursor()
    {
        Cursor.SetCursor(pointCursor, new Vector3(40f, 0.0f, 0.0f), CursorMode.Auto);
    }

    public void PlayHighlightSound()
    {
        audioSource.PlayOneShot(highlightClick);
    }

    public void PlayClickSound()
    {
        audioSource.PlayOneShot(click);
    }

    public void SetPractice(bool value){
        mainMenu.GetComponent<Animator>().SetBool("Practice", value);
        PracticeMenu.SetActive(value);
        gameController.canClickPieces = value;
    }

    public void SetMenusActive(bool value)
    {
        mainMenu.SetActive(value);
        mainMenu.GetComponent<Animator>().SetTrigger("Activate");
        if(!value)
        {
            PracticeMenu.SetActive(value);
            mainMenu.GetComponent<Animator>().SetBool("Practice", value);
            gameController.canClickPieces = value;
        }
    }

    public void SetSettingsActive(bool value){
        settingsMenu.SetActive(value);
    }

    public void SetInGameMenuActive(bool value)
    {
        InGameMenu.SetActive(value);
    }

    public void SetTurnText(bool value)
    {
        if(value)
        {
            turnText.GetComponent<Text>().text = "YOUR TURN";
            turnText.GetComponent<Animation>().Play();
        } else 
        {
            turnText.GetComponent<Text>().text = "THEIR TURN";
            turnText.GetComponent<Animation>().Play();
        }
    }

    public void SetTurnTextActive(bool value)
    {
        turnText.SetActive(value);
    }

    public void toggleInGameMenu()
    {
        InGameMenu.SetActive(!InGameMenu.activeSelf);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
