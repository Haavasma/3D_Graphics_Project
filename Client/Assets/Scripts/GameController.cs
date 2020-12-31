using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject Piece;

    private GameObject Ground;

    public GameObject Stabilizer;

    private bool canClickPieces = false;

    private bool inPractice = false;

    [SerializeField] int amountOfPieces = 54;

    [SerializeField] GameObject ResultText;

    [SerializeField] GameObject findingGameText;

    GameObject LeaveGameButton;

    private GameObject fgText;

    private AudioSource audioSource;

    private AudioClip ding;

    private AudioClip success;

    private AudioClip clock;

    private Vector3 firstSpawnPoint;

    private GameObject cameraFocus;

    private GameObject cubes;

    private Dictionary<int, Vector3> initialPositions = new Dictionary<int, Vector3>();

    private Dictionary<int, Quaternion> initialRotations = new Dictionary<int, Quaternion>();

    private List<GameObject> pieces;

    private Socket nwController;

    private UIController uIController;

    private roamCamera roamCam;

    private int piecesDropped = 0;

    private bool gameEnd = false;

    private bool inQueue = false;

    private bool gameLost = false;

    public bool inGame = false;

    void Awake()
    {
    }
    void Start()
    {
        LeaveGameButton = GameObject.Find("LeaveGame");
        LeaveGameButton.SetActive(false);
        nwController = GameObject.Find("NetworkController").GetComponent<Socket>();
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        UnityEngine.Object pPrefab = Resources.Load("Prefabs/Ground");
        roamCam = GameObject.Find("Camera").GetComponent<roamCamera>();
        cameraFocus = GameObject.FindWithTag("CameraFocus");
        firstSpawnPoint = GameObject.FindWithTag("SpawnPoint").transform.position;
        Ground = (GameObject)GameObject.Instantiate(pPrefab, firstSpawnPoint - new Vector3(0.0f, 0.7f, 0.0f), Quaternion.identity);
        clock = (AudioClip)Resources.Load("Sound/SFX/clock");
        ding = (AudioClip)Resources.Load("Sound/SFX/ding");
        success = (AudioClip)Resources.Load("Sound/SFX/success");
        audioSource = GetComponent<AudioSource>();
        SetUpGame(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(inGame && Input.GetKeyDown(KeyCode.Escape)){
            uIController.toggleInGameMenu();
        }
        if(initialPositions.Keys.Count != amountOfPieces && pieces.TrueForAll((GameObject p)=>{return !p.GetComponent<DragToMove>().falling;})){
            Debug.Log("YOYOYO");
            pieces.ForEach((GameObject p) => {
                int number = int.Parse(p.name);
                initialPositions[number] = p.transform.position;
                initialRotations[number] = p.transform.rotation;
                if(number<amountOfPieces - 3){
                    p.GetComponent<DragToMove>().SetClickable(true);
                }
            });
        } else if(nwController.GetResult() == 1 && inGame){ 
            HandleWin();
        }
    }

    private void SetUpGame(){
        initialPositions = new Dictionary<int, Vector3>();
        cubes = GameObject.FindWithTag("CubeHolder");
        pieces = new List<GameObject>();

        float currMass = Piece.GetComponent<Rigidbody>().mass;

        cameraFocus.transform.position = CalculateFocusPosition(amountOfPieces, Piece.transform.localScale.y, firstSpawnPoint);

        for (int i = 0; i < amountOfPieces; i++)
        {
            Vector3 spawnPoint = new Vector3();
            Quaternion rotation = Quaternion.identity;

            if (Mathf.Ceil(i / 3) % 2 == 0)
            {
                spawnPoint = new Vector3(0.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y * 1.5f,
                                        (((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z) * 1.0f);
                spawnPoint += firstSpawnPoint;
            }
            else
            {
                spawnPoint = new Vector3((((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z) * 1.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y * 1.5f,
                                        0.0f);
                spawnPoint += firstSpawnPoint;
                rotation = Quaternion.Euler(0, 90, 0);
            }
            GameObject piece = Instantiate(Piece, spawnPoint, rotation);

            Rigidbody pieceRB = piece.GetComponent<Rigidbody>();

            pieceRB.useGravity = false;

            pieceRB.mass = currMass;

            piece.transform.parent = cubes.transform;

            piece.name = "" + i;
            
            if(i < 3){
                piece.gameObject.tag = "BottomPiece";
                pieceRB.useGravity = true;
            }

            pieces.Add(piece);
            if (i % 3 == 2)
            {
                currMass *= 0.85f;  
            }

        }
    }

    private void HandleGameEnd()
    {
        ResultText.SetActive(true);
        gameEnd = true;
        inGame = false;
        uIController.SetTurnTextActive(false);
        LeaveGameButton.SetActive(true);
    }

    public void HandleLose(){
        if((!nwController.myTurn || !inGame) && !gameLost){
            gameLost = true;
            Debug.Log("skipping lose cos not myturn or not ingame");
            return;
        }else if(!gameEnd && inGame && nwController.myTurn) {
            Debug.Log("losing");
            Lose();
        }
        //Play losing sound;
    }

    private void Lose()
    {
        Debug.Log("YOU LOSE");
        pieces.ForEach((GameObject p) => {
            p.GetComponent<DragToMove>().SetClickable(false);
        });
        nwController.EndGame();
        ResultText.GetComponent<Text>().text = "YOU LOSE";
        HandleGameEnd();
    }

    public void HandleWin(){
        if(!gameEnd){
            Debug.Log("YOU WIN");
            ResultText.GetComponent<Text>().text = "YOU WIN";
            HandleGameEnd();
        }
    }

    public void FindGame(GameObject findGameButton){
        if(findGameButton.GetComponentInChildren<Text>().text == "Find game"){
            findGameButton.GetComponentInChildren<Text>().text = "Cancel";
            nwController.Queue();
            inQueue = true;
            findingGameText.SetActive(true);
            StartCoroutine(pollGameFound(findGameButton));
            StartCoroutine(TextAnimation(findingGameText, new string[3]{"Looking for opponent.", "Looking for opponent..", "Looking for opponent..."}, 0.5f));
        } else {
            findGameButton.GetComponentInChildren<Text>().text = "Find game";
            Destroy(fgText);
            inQueue = false;
            findingGameText.SetActive(false);
            nwController.deQueue();
        }
    }

    public void SetPractice(bool value)
    {
        inPractice = value;
    }

    public void SetCanClickPieces(bool value)
    {
        canClickPieces = value;
        if(!value)
        {
            inPractice = false;
        }
    }

    public bool GetCanClickPieces()
    {
        return canClickPieces;
    }

    public bool GetInPractice()
    {
        return inPractice;
    }

    public void ResetPieces(){
        gameLost = false;
        if(initialPositions.Keys.Count==pieces.Count && !inGame){
            piecesDropped = 0;
            Debug.Log("setting positions");
            pieces.ForEach((GameObject p) => {
                int pieceNo = int.Parse(p.name);
                Rigidbody rb = p.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                p.transform.position = initialPositions[pieceNo];
                p.transform.rotation = initialRotations[pieceNo];
                if (pieceNo < 3){
                    p.tag = "BottomPiece";
                    p.GetComponent<DragToMove>().SetClickable(true);
                } else if (pieceNo >= amountOfPieces - 3){
                    p.GetComponent<DragToMove>().SetClickable(false);
                } else {
                    p.GetComponent<DragToMove>().SetClickable(true);
                    p.tag = "Untagged";
                }
            });
        }
    }

    public void EndTurn(){
        audioSource.PlayOneShot(ding);
        piecesDropped++;
        if(!inGame || !nwController.myTurn){
            return;
        }        
        canClickPieces = false;
        Debug.Log("ending turn");
        StartCoroutine(EndTurnAfterSeconds(5));
        // play some animation for ending turn or w/e
    }

    public void LeaveGame(){
        if(!gameEnd){
            Lose();
        }
        Debug.Log("Leaving game");
        ResultText.SetActive(false);
        uIController.SetInGameMenuActive(false);
        uIController.SetMenusActive(true);
        SetUp();
        Debug.Log("setting gameEnd to false");
        gameEnd = false;
    }

    public void SetUp()
    {
        foreach(GameObject p in pieces)
        {
            Destroy(p);
        }
        SetUpGame();
    }

    public int GetPiecesDropped()
    {
        return piecesDropped;
    }

    public bool GetGameLost()
    {
        return gameLost;
    }

    Vector3 CalculateFocusPosition(int pieces, float pieceHeight, Vector3 startPos){
        float height = (pieces/(3*2))*pieceHeight;
        return new Vector3(startPos.x, startPos.y += height, startPos.z);
    }


    void SpawnStabilizer(Vector3 center, float width){
        for(int i = 0; i<4; i++) {
            GameObject stabilizer = Instantiate(Stabilizer, center + new Vector3((float)(((i+1)%2) * (i<2 ? 1 : -1)) * width*1.01f , 0.0f, (float)((i%2) * (i<2 ? 1 : - 1)) * width*1.01f), Quaternion.Euler(0, 90*-i, 0));
            stabilizer.transform.localScale += new Vector3(0.0f, 0.0f, 0.6f);
            stabilizer.tag = "BottomPiece";
        }
    }

    IEnumerator EndTurnAfterSeconds(int seconds){
        audioSource.PlayOneShot(clock);
        yield return new WaitForSeconds(seconds);
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        nwController.EndTurn();
        canClickPieces = true;
    }

    IEnumerator pollGameFound(GameObject findGameButton){
        while(true){
            Debug.Log("polling");
            if(nwController.InGame()){
                findGameButton.GetComponentInChildren<Text>().text = "Find game";
                foreach(GameObject p in pieces)
                {
                    Destroy(p);
                }
                SetUpGame();
                inGame = true;
                uIController.SetMenusActive(false);
                roamCam.StopRoam();
                canClickPieces = true;
                uIController.SetTurnTextActive(true);
                findingGameText.SetActive(false);
                break;
            } else if(!inQueue){
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator TextAnimation(GameObject text, string[] states, float intervalTime)
    {
        Text textText = text.GetComponent<Text>();
        int counter = 0;
        while(text.activeSelf)
        {
            textText.text = states[counter%states.Length];
            counter++;
            yield return new WaitForSeconds(intervalTime);
        }
    }
}
