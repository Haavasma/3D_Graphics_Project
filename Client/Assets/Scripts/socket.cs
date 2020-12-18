using UnityEngine;
using System.Collections.Generic;
using GameNetWorkClient;

public class Socket : MonoBehaviour
{
    // Start is called before the first frame update
    private NetworkClient client;

    private GameObject cubes;

    private GameController gameController;

    private float startTime;

    private float messageCD = 0.1f;

    public bool myTurn = false;

    void Start()
    {
        cubes = GameObject.Find("Cubes");
        client = new NetworkClient();
        client.Connect();
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
                if(Time.time - startTime > messageCD){
                    for (int i = 0; i < cubes.transform.childCount; i++)
                    {
                        client.SendTransform(cubes.transform.GetChild(i).name, cubes.transform.GetChild(i).transform);
                    }
                    startTime = Time.time;
                    Debug.Log("sending transforms");
                }
            }
            else
            {
                for (int i = 0; i < cubes.transform.childCount; i++)
                {
                    Transform newTransform = client.GetTransform(cubes.transform.GetChild(i));
                    Rigidbody rb = cubes.transform.GetChild(i).GetComponent<Rigidbody>();
                    rb.MovePosition(newTransform.position);
                    rb.MoveRotation(newTransform.rotation);
                    cubes.transform.GetChild(i).localScale = newTransform.localScale;
                    cubes.transform.GetChild(i).GetComponent<Rigidbody>().velocity = newTransform.GetComponent<Rigidbody>().velocity;
                }
            }
            if (client.getChannel() != "")
            {
                myTurn = client.getMyTurn();
            }
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
