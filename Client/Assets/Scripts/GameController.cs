using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Piece;

    private GameObject focus;

    private GameObject cubes;

    // Start is called before the first frame update
    void Start()
    {
        cubes = GameObject.FindWithTag("CubeHolder");
        focus = GameObject.FindWithTag("CameraFocus");
        StartCoroutine("Spawn");
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Spawn()
    {
        for (int i = 0; i < 33; i++)
        {
            Debug.Log(Piece.transform.localScale);
            Vector3 spawnPoint = new Vector3();
            Quaternion rotation = Quaternion.identity;

            if (Mathf.Ceil(i / 3) % 2 == 0)
            {
                spawnPoint = new Vector3(0.0f,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y,
                                        ((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z);
                spawnPoint += focus.transform.position;
            }
            else
            {
                spawnPoint = new Vector3(((i % 3) * Piece.transform.localScale.z) - Piece.transform.localScale.z,
                                        Mathf.Ceil(i / 3) * Piece.transform.localScale.y,
                                        0.0f);
                spawnPoint += focus.transform.position;
                rotation = Quaternion.Euler(0, 90, 0);
            }
            GameObject piece = Instantiate(Piece, spawnPoint, rotation);

            piece.transform.parent = cubes.transform;
            
            if(i%3 == 2) {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}
