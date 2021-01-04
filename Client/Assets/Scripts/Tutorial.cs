using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controls the tutorial
public class Tutorial : MonoBehaviour
{
    private int step = 0;

    private GameController gameController;

    private Animator animator;

    private GameObject Pointer;

    private GameObject PracticeButton;

    private GameObject FindGameButton;

    private GameObject mainmenu;

    private GameObject practiceMenu;

    private GameObject goBackButton;

    private TextMeshProUGUI tutorialText;
    // Start is called before the first frame update
    private void Awake() {
        practiceMenu = GameObject.Find("PracticeMenu");
        goBackButton = GameObject.FindWithTag("GoBackButton");
    }

    // Destroys the gameobject if tutorial is already finished
    void Start()
    {
        if(PlayerPrefs.GetInt("Tutorial") == -1)
        {
            Destroy(gameObject);
            return;
        }
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.SetCanClickPieces(false);
        animator = GetComponent<Animator>();
        tutorialText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        Pointer = GameObject.Find("Pointer");
        PracticeButton = GameObject.FindWithTag("PracticeButton");
        FindGameButton = GameObject.Find("FindGame");
        mainmenu = GameObject.Find("MainMenu");
        setInteractableMenu(mainmenu, false, new string[]{"PracticeButton"});
    }

    // Sets the given menu's buttons interactable to given value, except for those with tags in exceptionTags
    void setInteractableMenu(GameObject menu, bool value, string[] exceptionTags)
    {
        for(int i = 0; i<menu.transform.childCount; i++)
        {
            Transform button = menu.transform.GetChild(i);
            button.GetComponent<Button>().interactable = value;

            if(exceptionTags == null || exceptionTags.Length == 0)
            {
                continue;
            }
            for (int j = 0; j<exceptionTags.Length; j++)
            {
                if(button.tag == exceptionTags[j])
                {
                    button.GetComponent<Button>().interactable = !value;
                    break;
                } 
            }
        }
    }

    void setInteractableMenu(GameObject menu, bool value)
    {
        setInteractableMenu(menu, value, null);
    }

    // Update is called once per frame
    void Update()
    {
        // if game is lost, reset the pieces
        if(gameController.GetGameLost())
        {
            gameController.ResetPieces();
        }
        switch(step)
        {
            case 0: // enter practice
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
                    tutorialText.text = "Use <sprite=2> + <sprite=1> to push a piece";
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
            
            case 6: // Exit practice
                Pointer.transform.position = Vector3.MoveTowards(Pointer.transform.position, 
                                                                goBackButton.transform.position + new Vector3(0.0f, -20.0f, 0.0f),
                                                                600f * Time.deltaTime);   
                
                Debug.Log(gameController.GetInPractice());
                if(!gameController.GetInPractice())
                {
                    tutorialText.text = "Press Find Game to look for an opponent";
                    nextStep();
                }
                break;

            case 7: // Find opponent
                Pointer.transform.position = Vector3.MoveTowards(Pointer.transform.position, 
                                                                FindGameButton.transform.position + new Vector3(0.0f, -20.0f, 0.0f),
                                                                600f * Time.deltaTime);
                if(gameController.GetInQueue())
                {
                    EndTutorial();
                }
                break;
                

            default:
                break; 
        }
    }

    // Changes animation and increments step number
    void nextStep()
    {
        step++;
        animator.SetTrigger("nextStep");
    }

    void EndTutorial()
    {
        Debug.Log("tutorial ended");
        // end tutorial
        PlayerPrefs.SetInt("Tutorial", -1);
        Destroy(gameObject);
    }
}
