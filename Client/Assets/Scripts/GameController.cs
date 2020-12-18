using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject Piece;

    private GameObject Ground;

    public GameObject Stabilizer;

    public bool canClickPieces = true;

    [SerializeField] int amountOfPieces = 54;

    [SerializeField] GameObject WinText;

    [SerializeField] GameObject LoseText;

    private Vector3 firstSpawnPoint;

    private GameObject cameraFocus;

    private GameObject cubes;

    private Dictionary<int, Vector3> initialPositions = new Dictionary<int, Vector3>();

    private Dictionary<int, Quaternion> initialRotations = new Dictionary<int, Quaternion>();

    private List<GameObject> pieces;

    private Socket nwController;

    private bool gameEnd = false;

    private bool inQueue = false;

    public bool inGame = false;

    // Start is called before the first frame update
    void Start()
    {
        nwController = GameObject.Find("NetworkController").GetComponent<Socket>();
        UnityEngine.Object pPrefab = Resources.Load("Prefabs/Ground");
        Ground = (GameObject)GameObject.Instantiate(pPrefab, firstSpawnPoint - new Vector3(0.0f, 0.7f, 0.0f), Quaternion.identity);
        SetUpGame(); 
    }

    // Update is called once per frame
    void Update()
    {
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
        } else if(nwController.GetResult() == 1){ 
            HandleWin();
        }
    }

    private void SetUpGame(){
        initialPositions = new Dictionary<int, Vector3>();
        cubes = GameObject.FindWithTag("CubeHolder");
        cameraFocus = GameObject.FindWithTag("CameraFocus");
        firstSpawnPoint = GameObject.FindWithTag("SpawnPoint").transform.position;
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

    public void HandleLose(){
        if(!nwController.myTurn || !inGame){
            return;
        }else if(!gameEnd) {
            Debug.Log("YOU LOSE");
            pieces.ForEach((GameObject p) => {
                p.GetComponent<DragToMove>().SetClickable(false);
            });
            nwController.EndGame();
            LoseText.SetActive(true);
            //play some loss animation
            gameEnd = true;
            inGame = false;
        }
    }

    public void HandleWin(){
        if(!gameEnd){
            Debug.Log("YOU WIN");
            WinText.SetActive(true);
            gameEnd = true;
            inGame = false;
        }
    }

    public void FindGame(){
        GameObject findGameButton = GameObject.Find("FindGame");
        if(findGameButton.GetComponentInChildren<Text>().text == "Find game"){
            findGameButton.GetComponentInChildren<Text>().text = "Cancel";
            nwController.Queue();
            inQueue = true;
            StartCoroutine(pollGameFound());
        } else {
            findGameButton.GetComponentInChildren<Text>().text = "Find game";
            inQueue = false;
            nwController.deQueue();
        }
    }

    public void ResetPieces(){
        if(initialPositions.Keys.Count==pieces.Count && !inGame){
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
        if(!inGame || !nwController.myTurn){
            return;
        }        
        canClickPieces = false;
        Debug.Log("ending turn");
        StartCoroutine(EndTurnAfterSeconds(5));
        // play some animation for ending turn or w/e
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
        yield return new WaitForSeconds(seconds);
        nwController.EndTurn();
        canClickPieces = true;
    }

    IEnumerator pollGameFound(){
        while(true){
            Debug.Log("polling");
            if(nwController.InGame()){
                GameObject gamefoundText = Instantiate((GameObject)Resources.Load("Prefabs/Informative"), Vector3.zero, Quaternion.identity);
                gamefoundText.transform.parent = GameObject.Find("Canvas").transform;
                RectTransform rt = gamefoundText.GetComponent<RectTransform>();
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                gamefoundText.GetComponent<Text>().text = "Game found!";
                GameObject.Find("FindGame").SetActive(false);
                GameObject.Find("ResetButton").SetActive(false);
                foreach(GameObject p in pieces){
                    Destroy(p);
                }
                SetUpGame();
                inGame = true;
                break;
            } else if(!inQueue){
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
