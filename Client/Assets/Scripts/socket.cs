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
            Dictionary<string, FormattedTransform> transforms = client.getTransforms();

            foreach (KeyValuePair<string, FormattedTransform> entry in transforms)
            {
                GameObject piece = GameObject.Find(entry.Key);
                piece.transform.position = entry.Value.position;
                piece.transform.rotation = entry.Value.rotation;
                piece.transform.localScale = entry.Value.scale;
            }
            /*
                        Transform piece = cubes.transform.GetChild(1).transform;

                        piece.position = new Vector3(transforms["Cube (1)"].position.x, transforms["Cube (1)"].position.y, transforms["Cube (1)"].position.z - 10.0f);
                        piece.rotation = transforms["Cube (1)"].rotation;
                        piece.localScale = transforms["Cube (1)"].scale;
                        */
        }
        if (client.getChannel() != "")
        {
            myTurn = client.getMyTurn();
        }
    }

}
