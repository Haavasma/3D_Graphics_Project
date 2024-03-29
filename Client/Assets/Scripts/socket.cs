﻿using UnityEngine;
using System.Collections.Generic;
using GameNetWorkClient;
using System;


// network controller, uses the client package to decide state of the game
public class Socket : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] string ipAddress = "localhost";
    private NetworkClient client;

    private GameObject cubes;

    private Dictionary<string, GameObject> pieces;

    private GameController gameController;

    private UIController uIController;

    private float startTime;

    private float messageCD = 0.2f;

    public bool myTurn = false;

    private bool first = true;

    private bool turnUpdated;


    void Start()
    {
        // creates pieces dictionary based on name of gameobject
        pieces = new Dictionary<string, GameObject>();
        cubes = GameObject.Find("Cubes");
        for (int i = 0; i < cubes.transform.childCount; i++)
        {
            GameObject piece = cubes.transform.GetChild(i).gameObject;
            pieces[piece.name] = piece;
        }
        // connects to the server
        client = new NetworkClient();
        // replace with "localhost" to connect to locally running version of server
        client.Connect(ipAddress);
        client.SetOnTurnChange((bool value) => {
            turnUpdated = true;
            myTurn = value;
            return true;
        });
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        uIController = GameObject.Find("UIController").GetComponent<UIController>();
        startTime = Time.time;
    }

    // Update is called once per frame
    // sends out transform of pieces if inside game and client has the turn
    // updates the transforms of pieces if client does not have the turn
    void Update()
    {
        if (gameController.inGame)
        {
            if(turnUpdated)
            {
                uIController.SetTurnText(myTurn);
                Debug.Log("updating turn: " + myTurn);
                turnUpdated = false;
            }
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
        }
    }

    // Moves the pieces towards the state provided by the server via the client package
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

    // moves the pieces to the exact state provided by the server
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

    // tells the client to end the turn
    public void EndTurn()
    {
        Debug.Log("ending turn from network controller");
        client.EndTurn();
    }

    // tells the client to queue for a game
    public void Queue()
    {
        client.queue();
    }
    
    // tells the client to stop looking for a game
    public void deQueue()
    {
        client.dequeue();
    }

    // tells the client to end the game (losing)
    public void EndGame()
    {
        Debug.Log("ending game");
        client.EndGame();
    }

    // returns true if client is in a game
    public bool InGame()
    {
        return client.InGame();
    }

    // returns the result of the game (-1 if unfinished, 0 if loss, 1 if victory)
    public int GetResult()
    {
        return client.Result();
    }
}
