using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Piece;

    [SerializeField] float fallSpeed = 0.000001f;

    private GameObject focus;

    private GameObject cubes;

    private List<GameObject> pieces;

    // Start is called before the first frame update
    void Start()
    {
        cubes = GameObject.FindWithTag("CubeHolder");
        focus = GameObject.FindWithTag("CameraFocus");
        pieces = new List<GameObject>();
        //StartCoroutine("Spawn");

        for (int i = 0; i < 54; i++)
        {
            Vector3 spawnPoint = new Vector3();
            Quaternion rotation = Quaternion.identity;

            if (Mathf.Ceil(i / 3) % 2 == 0)
            {
                spawnPoint = new Vector3(0.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y * 1.5f,
                                        (((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z) * 1.0f);
                spawnPoint += focus.transform.position;
            }
            else
            {
                spawnPoint = new Vector3((((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z) * 1.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y * 1.5f,
                                        0.0f);
                spawnPoint += focus.transform.position;
                rotation = Quaternion.Euler(0, 90, 0);
            }
            GameObject piece = Instantiate(Piece, spawnPoint, rotation);

            piece.GetComponent<Rigidbody>().useGravity = false;

            piece.transform.parent = cubes.transform;

            pieces.Add(piece);
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*
        float step = fallSpeed * Time.deltaTime;
        for (int i = 0; i < pieces.Count; i++)
        {
            Vector3 targetPos =  new Vector3(pieces[i].transform.position.x,
                                                                    Mathf.Ceil(i / 3) * Piece.transform.localScale.y,
                                                                    pieces[i].transform.position.z
                                                                );   
            if(pieces[i].transform.position.y <= targetPos.y) {
                pieces[i].GetComponent<Rigidbody>().useGravity = true;
                continue;
            }
            Debug.Log(targetPos);
            Debug.Log(pieces[i].transform.position);
            Debug.Log("");
            pieces[i].transform.position = Vector3.MoveTowards(pieces[i].transform.position, targetPos, step);
        }
        */
    }

    IEnumerator Spawn()
    {
        for (int i = 0; i < 33; i++)
        {
            Vector3 spawnPoint = new Vector3();
            Quaternion rotation = Quaternion.identity;

            if (Mathf.Ceil(i / 3) % 2 == 0)
            {
                spawnPoint = new Vector3(0.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y + 0.5f * Piece.transform.localScale.y,
                                        ((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z);
                spawnPoint += focus.transform.position;
            }
            else
            {
                spawnPoint = new Vector3(((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y + 0.5f * Piece.transform.localScale.y,
                                        0.0f);
                spawnPoint += focus.transform.position;
                rotation = Quaternion.Euler(0, 90, 0);
            }
            GameObject piece = Instantiate(Piece, spawnPoint, rotation);

            piece.transform.parent = cubes.transform;

            if (i % 3 == 2)
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
