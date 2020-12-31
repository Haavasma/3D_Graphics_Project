using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tutorial : MonoBehaviour
{
    private int step = 0;

    private GameController gameController;

    private Animator animator;

    private GameObject Pointer;

    private GameObject PracticeButton;

    private GameObject mainmenu;

    private GameObject practiceMenu;

    private GameObject goBackButton;

    private TextMeshProUGUI tutorialText;
    // Start is called before the first frame update
    private void Awake() {
        practiceMenu = GameObject.Find("PracticeMenu");
        goBackButton = GameObject.FindWithTag("GoBackButton");
    }

    void Start()
    {
        /*if(PlayerPrefs.GetInt("Tutorial") == -1)
        {
            Destroy(gameObject);
        }
        */
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.SetCanClickPieces(false);
        animator = GetComponent<Animator>();
        tutorialText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        Pointer = GameObject.Find("Pointer");
        PracticeButton = GameObject.FindWithTag("PracticeButton");
        mainmenu = GameObject.Find("MainMenu");
        setInteractableMenu(mainmenu, false);
    }

    void setInteractableMenu(GameObject menu, bool value)
    {
        for(int i = 0; i<menu.transform.childCount; i++)
        {
            Transform button = menu.transform.GetChild(i);
            if(button.tag != "PracticeButton")
            {
                button.GetComponent<Button>().interactable = value;
            } 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(gameController.GetGameLost())
        {
            gameController.ResetPieces();
        }
        switch(step)
        {
            case 0:
                Pointer.transform.position = Vector3.MoveTowards(Pointer.transform.position, 
                                                                PracticeButton.transform.position + new Vector3(0.0f, -20.0f, 0.0f),
                                                                200f * Time.deltaTime);
                if(gameController.GetInPractice())
                {
                    nextStep();
                    tutorialText.text = "Use <sprite=0> to move the camera";
                    Pointer.SetActive(false);
                    setInteractableMenu(mainmenu, true);
                    setInteractableMenu(practiceMenu, false);
                }
                break;

            case 1: // move camera
                if(Input.GetMouseButtonDown(1))
                {
                    gameController.SetCanClickPieces(true);
                    nextStep();
                    tutorialText.text = "Hold <sprite=1> to drag a piece";
                }
                break;
            case 2: // move piece
                if(Input.GetMouseButtonDown(0))
                {
                    nextStep();
                    tutorialText.text = "Drop one piece on the ground with <sprite=1>";
                }
                break;
            case 3: // drop piece on ground
                if(gameController.GetPiecesDropped()>0)
                {
                    nextStep();
                    tutorialText.text = "Use <sprite=2> + <sprite=3> to push a piece";
                }
                break;
            case 4: // push piece
                if(Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
                {
                    nextStep();
                    tutorialText.text = "Drop 4 pieces without tower falling";
                }
                break;

            case 5: //drop 4 pieces on the ground without the tower falling
                tutorialText.text = "Drop " + (4 - gameController.GetPiecesDropped()) + " more pieces without tower falling";
                if(gameController.GetPiecesDropped() > 3)
                {
                    setInteractableMenu(practiceMenu, true);

                    tutorialText.text = "Well Done!";

                    Pointer.SetActive(true);

                    nextStep();

                    // end tutorial
                    PlayerPrefs.SetInt("Tutorial", -1);
                }
                break;
            
            case 6:
                Pointer.transform.position = Vector3.MoveTowards(Pointer.transform.position, 
                                                                goBackButton.transform.position + new Vector3(0.0f, -20.0f, 0.0f),
                                                                200f * Time.deltaTime);   
                
                Debug.Log(gameController.GetInPractice());
                if(!gameController.GetInPractice())
                {
                    Debug.Log("tutorial ended");
                    // end tutorial
                    PlayerPrefs.SetInt("Tutorial", -1);
                    Destroy(gameObject);
                }
                break; 

            default:
                break; 
        }
    }

    void nextStep()
    {
        step++;
        animator.SetTrigger("nextStep");
    }
}
