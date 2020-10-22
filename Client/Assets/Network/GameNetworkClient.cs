using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime;


namespace GameNetWorkClient
{
    public class NetworkClient
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient;

        TcpClient tcpClient;    

        private string channel = "";

        private bool myTurn = false;

        private static Mutex mutex = new Mutex();

        private static Mutex channelMutex = new Mutex();

        Dictionary<string, FormattedTransform> transforms = new Dictionary<string, FormattedTransform>();
        public NetworkClient()
        {
            udpClient = new UdpClient("localhost", 8080);
            tcpClient = new TcpClient("localhost", 8081);

            Thread readThread = new Thread(new ThreadStart(ReadUDP));
            readThread.Start();
            Thread readTCPThread = new Thread(new ThreadStart(ReadTCP));
            readTCPThread.Start();
            Thread pingChannelThread = new Thread(new ThreadStart(PingChannel));
            pingChannelThread.Start();
        }

        public void SendTransform(string objectId, Transform message)
        {
            try
            {
                byte[] jsonUtf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(new FormattedTransform(objectId, "transform", channel, message)));

                udpClient.Send(jsonUtf8Bytes, jsonUtf8Bytes.Length);
            }
            catch
            {
                Debug.Log("could not send message");
            }
        }

        public void queue()
        {
            Thread queueThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    Message msg = new Message();
                    msg.type = "queue";
                    byte[] jsonUtf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));

                     NetworkStream stream = tcpClient.GetStream();

                    // Send the message to the connected TcpServer.
                    stream.Write(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);
                }
                catch
                {
                    Debug.Log("could not send message");
                }
            }));
            queueThread.Start();
        }

        public string getChannel()
        {
            return channel;
        }


        public bool getMyTurn()
        {
            return myTurn;
        }

        private void ReadUDP()
        {
            while (true)
            {
                try
                {
                    // Blocks until a message returns on this socket from a remote host.
                    byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    HandleRead(returnData);
                }
                catch
                {
                    Debug.Log("Could not get message");
                }
            }
        }

        private void ReadTCP()
        {
            while (true)
            {
                try
                {
                    byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                    tcpClient.GetStream().Read(bytes, 0, tcpClient.ReceiveBufferSize);
                    string returnData = Encoding.UTF8.GetString(bytes);
                    HandleRead(returnData);
                }
                catch
                {
                    Debug.Log("Could not get message");
                }
            }
        }

        private void PingChannel()
        {
            while(true)
            {
                try
                {
                    if(channel != "") {
                        ChannelMessage msg = new ChannelMessage();
                        msg.type = "ping";
                        msg.channel = channel;
                        byte[] jsonUtf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));
                        udpClient.Send(jsonUtf8Bytes, jsonUtf8Bytes.Length);
                    }
                }
                catch
                {
                    Debug.Log("Could not send message");
                }
                Thread.Sleep(2000);
            }
        }

        private void HandleRead(string message)
        {
            Message temp = JsonUtility.FromJson<Message>(message);

            if (temp.type == "newGame")
            {
                NewGame game = JsonUtility.FromJson<NewGame>(message);
                channelMutex.WaitOne();
                channel = game.channel;
                myTurn = game.myTurn;
                channelMutex.ReleaseMutex();
            }
            else if (temp.type == "transform")
            {
                FormattedTransform tf = JsonUtility.FromJson<FormattedTransform>(message);
                mutex.WaitOne();
                transforms[tf.id] = tf;
                mutex.ReleaseMutex();
            }
            else if (temp.type == "toggleTurn")
            {
                myTurn = !myTurn;
            }
        }

        public Dictionary<string, FormattedTransform> getTransforms()
        {
            return transforms;
        }

        private byte[] toBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

    }
}


public class Message
{
    public string type;

}

public class ChannelMessage : Message {
    public string channel;
}

public class NewGame : ChannelMessage
{
    public bool myTurn;
}

public class Velocity : Message
{
    public string id;

    public Vector3 velocity;
}

public class FormattedTransform : ChannelMessage
{
    public string id;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public FormattedTransform(string objectId, string msgType, string _channel, Transform tf)
    {
        id = objectId;
        type = msgType;
        channel = _channel;
        position = tf.position;
        rotation = tf.rotation;
        scale = tf.localScale;
    }

    public FormattedTransform()
    {

    }
}