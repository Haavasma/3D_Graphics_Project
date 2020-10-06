using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime;

public class socket : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UdpClient udpClient = new UdpClient("localhost", 8080);

        try{
            byte[] sendBytes = toBytes("yoyoyoHELLOOOasd halsdlaks dlak asd asd asd sdad dwqej lkaj sldkj alskdj alksjd lakjsO");

            Debug.Log(sendBytes.Length);
            udpClient.Send(sendBytes, sendBytes.Length);
        } catch {
            Debug.Log("could not send message");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public byte[] toBytes(string text) {
            return Encoding.UTF8.GetBytes(text);
        }
}
