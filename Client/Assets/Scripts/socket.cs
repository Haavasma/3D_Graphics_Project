using UnityEngine;
using System.Collections.Generic;
using GameNetWorkClient;

public class socket : MonoBehaviour
{
    // Start is called before the first frame update
    private NetworkClient client;

    private GameObject cubes;

    public bool myTurn = false;

    void Start()
    {
        cubes = GameObject.Find("Cubes");
        client = new NetworkClient();
        client.queue();
    }

    // Update is called once per frame
    void Update()
    {
        if (myTurn)
        {
            for (int i = 0; i < cubes.transform.childCount; i++)
            {
                client.SendTransform(cubes.transform.GetChild(i).name, cubes.transform.GetChild(i).transform);
            }
        }
        else
        {
            
            for (int i = 0; i < cubes.transform.childCount; i++)
            {
               Transform newTransform =  client.GetTransform(cubes.transform.GetChild(i));
               cubes.transform.GetChild(i).position = newTransform.position;
               cubes.transform.GetChild(i).rotation = newTransform.rotation;
               cubes.transform.GetChild(i).localScale = newTransform.localScale;
               cubes.transform.GetChild(i).GetComponent<Rigidbody>().velocity = newTransform.GetComponent<Rigidbody>().velocity;
            }
        }
        if (client.getChannel() != "")
        {
            myTurn = client.getMyTurn();
        }
    }

    public void EndTurn(){
        client.EndTurn();
    }

}
