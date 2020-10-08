using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime;


namespace GameNetWorkClient
{
    public class SocketClient
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient;

        private int channel = -1;

        private static Mutex mutex = new Mutex();

        private static Mutex channelMutex = new Mutex();

        Dictionary<string, FormattedTransform> transforms = new Dictionary<string, FormattedTransform>();
        public SocketClient()
        {
            udpClient = new UdpClient("localhost", 8080);

            Thread readThread = new Thread(new ThreadStart(Read));
            readThread.Start();
        }

        public void SendTransform(string objectId, Transform message)
        {
            try
            {
                byte[] jsonUtf8Bytes = toBytes(JsonUtility.ToJson(new FormattedTransform(objectId,"transform", channel, message)));

                udpClient.Send(jsonUtf8Bytes, jsonUtf8Bytes.Length);
            }
            catch
            {
                Debug.Log("could not send message");
            }
        }

        public int getChannel(){
            return channel;
        }

        public void queue()
        {
            try
            {
                Message msg = new Message();
                msg.type = "queue";
                byte[] jsonUtf8Bytes = toBytes(JsonUtility.ToJson(msg));

                udpClient.Send(jsonUtf8Bytes, jsonUtf8Bytes.Length);
            }
            catch
            {
                Debug.Log("could not send message");
            }
        }

        public void Read()
        {
            while(true){
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

        public void HandleRead(string message){
            Message temp =  JsonUtility.FromJson<Message>(message);
            Debug.Log(temp.type);

            if(temp.type == "newGame"){
                NewGame game = JsonUtility.FromJson<NewGame>(message);
                channelMutex.WaitOne();
                channel = game.channel;
                channelMutex.ReleaseMutex();
            } else if(temp.type == "transform"){
                FormattedTransform tf = JsonUtility.FromJson<FormattedTransform>(message);
                mutex.WaitOne();
                transforms[tf.id] = tf;
                mutex.ReleaseMutex();
            }
        }

        public Dictionary<string, FormattedTransform> getTransforms() {
            return transforms;
        }

        private byte[] toBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

    }
}


public class Message{
    public string type;

}

public class NewGame:Message{
    public int channel;

}

public class FormattedTransform: Message
{
    public string id;
    public int channel;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public FormattedTransform(string objectId, string msgType, int _channel, Transform tf)
    {
        id = objectId;
        type = msgType;
        channel = _channel;
        position = tf.position;
        rotation = tf.rotation;
        scale = tf.localScale;
    }

    public FormattedTransform(){

    }
}