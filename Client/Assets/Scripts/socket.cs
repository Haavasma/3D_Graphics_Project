using UnityEngine;
using System.Collections.Generic;
using GameNetWorkClient;
using System;

public class Socket : MonoBehaviour
{
    // Start is called before the first frame update
    private NetworkClient client;

    private GameObject cubes;

    private Dictionary<string, GameObject> pieces;

    private GameController gameController;

    private float startTime;

    private float messageCD = 0.2f;

    public bool myTurn = false;

    private bool first = true;


    void Start()
    {
        pieces = new Dictionary<string, GameObject>();
        cubes = GameObject.Find("Cubes");
        for (int i = 0; i < cubes.transform.childCount; i++)
        {
            GameObject piece = cubes.transform.GetChild(i).gameObject;
            pieces[piece.name] = piece;
        }
        client = new NetworkClient();
        client.Connect("35.228.141.165");
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameController.inGame)
        {
            if (myTurn)
            {
                if (first)
                {
                    updateExactPos();
                    first = false;
                }
                if (Time.time - startTime > messageCD)
                {
                    for (int i = 0; i < cubes.transform.childCount; i++)
                    {
                        client.SendTransform(cubes.transform.GetChild(i).name, cubes.transform.GetChild(i).transform);
                    }
                    startTime = Time.time;
                }
            }
            else
            {
                updateTransforms();
                first = true;
            }
            if (client.getChannel() != "")
            {
                myTurn = client.getMyTurn();
            }
        }
    }

    private void updateTransforms()
    {
        for (int i = 0; i < cubes.transform.childCount; i++)
        {
            Transform piece = cubes.transform.GetChild(i);
            Rigidbody rb = cubes.transform.GetChild(i).GetComponent<Rigidbody>();
            Vector3 velocity = client.GetVelocity(rb.velocity, piece.name);
            Vector3 newPos = client.GetPosition(piece.position, piece.name);
            Quaternion newRot = client.GetRotation(piece.rotation, piece.name);
            float distance = (newPos - piece.position).magnitude;
            float angDistance = Quaternion.Angle(piece.rotation, newRot);
            rb.MovePosition(Vector3.MoveTowards(piece.position, newPos, 15f * Time.deltaTime * distance));
            rb.MoveRotation(Quaternion.RotateTowards(piece.rotation, newRot, 10f * Time.deltaTime * angDistance));
            piece.localScale = client.GetScale(piece.localScale, piece.name);
            rb.velocity = client.GetVelocity(rb.velocity, piece.name);
        }
    }

    private void updateExactPos()
    {
        for (int i = 0; i < cubes.transform.childCount; i++)
        {
            Transform piece = cubes.transform.GetChild(i);
            Rigidbody rb = cubes.transform.GetChild(i).GetComponent<Rigidbody>();
            Vector3 velocity = client.GetVelocity(rb.velocity, piece.name);
            Vector3 newPos = client.GetPosition(piece.position, piece.name);
            Quaternion newRot = client.GetRotation(piece.rotation, piece.name);
            rb.MovePosition(newPos);
            rb.MoveRotation(newRot);
            piece.localScale = client.GetScale(piece.localScale, piece.name);
            rb.velocity = client.GetVelocity(rb.velocity, piece.name);
        }
    }

    public void EndTurn()
    {
        Debug.Log("ending turn from network controller");
        client.EndTurn();
    }

    public void Queue()
    {
        client.queue();
    }

    public void deQueue()
    {
        client.dequeue();
    }
    public void EndGame()
    {
        Debug.Log("ending game");
        client.EndGame();
    }

    public bool InGame()
    {
        return client.InGame();
    }

    public int GetResult()
    {
        return client.Result();
    }
}
