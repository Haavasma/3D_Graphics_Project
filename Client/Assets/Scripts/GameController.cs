using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Piece;

    private GameObject Ground;

    public GameObject Stabilizer;

    [SerializeField] int amountOfPieces = 54;

    private Vector3 firstSpawnPoint;

    private GameObject cameraFocus;

    private GameObject cubes;

    private Dictionary<int, Vector3> initialPositions = new Dictionary<int, Vector3>();

    private Dictionary<int, Quaternion> initialRotations = new Dictionary<int, Quaternion>();

    private List<GameObject> pieces;

    // Start is called before the first frame update
    void Start()
    {
        SetUpGame(); 
    }

    // Update is called once per frame
    void Update()
    {
        if(initialPositions.Keys.Count != amountOfPieces && pieces.TrueForAll((GameObject p)=>{return !p.GetComponent<DragToMove>().falling;})){
            Debug.Log("YOYOYO");
            pieces.ForEach((GameObject p) => {
                int number = int.Parse(p.name);
                initialPositions[int.Parse(p.name)] = p.transform.position;
                initialRotations[int.Parse(p.name)] = p.transform.rotation;
                if(number<amountOfPieces - 3){
                    p.GetComponent<DragToMove>().SetClickable(true);
                }
            });
        }
    }

    private void SetUpGame(){
        UnityEngine.Object pPrefab = Resources.Load("Prefabs/Ground");
        Ground = (GameObject)GameObject.Instantiate(pPrefab, firstSpawnPoint - new Vector3(0.0f, 0.7f, 0.0f), Quaternion.identity);
        cubes = GameObject.FindWithTag("CubeHolder");
        cameraFocus = GameObject.FindWithTag("CameraFocus");
        firstSpawnPoint = GameObject.FindWithTag("SpawnPoint").transform.position;
        pieces = new List<GameObject>();
        //StartCoroutine("Spawn");

        //SpawnStabilizer(firstSpawnPoint, Piece.transform.localScale.z * 3.0f);

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
            }

            pieces.Add(piece);
            if (i % 3 == 2)
            {
                currMass *= 0.85f;  
            }

        }
    }

    public void HandleLose(){
        /*if(!GameObject.Find("NetworkController").GetComponent<Socket>().myTurn){
            return;
        }*/

        Debug.Log("YOU LOSE");
        pieces.ForEach((GameObject p) => {
            p.GetComponent<DragToMove>().SetClickable(false);
        });
        //play some loss animation
    }

    public void ResetPieces(){
        Debug.Log("Clicked reset");
        Debug.Log(initialPositions.Keys);
        if(initialPositions.Keys.Count==pieces.Count){
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
        GameObject.Find("NetWorkController").GetComponent<Socket>().EndTurn();
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

}
