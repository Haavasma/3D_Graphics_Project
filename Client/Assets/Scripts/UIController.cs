using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private GameObject mainMenu;

    private GameObject PracticeMenu;

    private GameController gameController;

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
        amountOfMenuButtons = mainMenu.transform.childCount;
        PracticeMenu.SetActive(false);

        float size_between_btns = (main_menu_y_interval_size - amountOfMenuButtons*main_menu_btn_height)/(amountOfMenuButtons-1);
        for (int i = 0; i < amountOfMenuButtons; i++) 
        {
            RectTransform rt = mainMenu.transform.GetChild(amountOfMenuButtons - 1 - i).GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(main_menu_min_x, main_menu_y_interval_size/2 + i*(main_menu_btn_height + size_between_btns));
            rt.anchorMax = new Vector2(main_menu_max_x, main_menu_y_interval_size/2 + main_menu_btn_height + i*(main_menu_btn_height + size_between_btns));
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    public void SetPractice(bool value){
        mainMenu.GetComponent<Animator>().SetBool("Practice", value);
        PracticeMenu.SetActive(value);
    }

    public void SetMenusActive(bool value)
    {
        mainMenu.SetActive(value);
        if(!value)
        {
            PracticeMenu.SetActive(value);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
