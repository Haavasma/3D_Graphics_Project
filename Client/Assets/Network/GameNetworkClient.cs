using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;


namespace GameNetWorkClient
{
    public class NetworkClient
    {
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        UdpClient udpClient;

        TcpClient tcpClient;    

        private string channel = "";

        private bool myTurn = false;

        private int result = -1;

        private float lastSentTime;

        private static Mutex mutex = new Mutex();


        private static Mutex channelMutex = new Mutex();

        private static Mutex inGameMutex = new Mutex();

        private Func<int> OnTransform = () => {return -1;}; 

        private Func<bool, bool> OnTurnChange = (bool value) => {return false;};

        Dictionary<string, FormattedTransform> transforms = new Dictionary<string, FormattedTransform>();

        public NetworkClient(){
            lastSentTime = Time.time;
        }

        public void Connect(string host){
            tcpClient = new TcpClient(host, 8081);
            udpClient = new UdpClient(host, 8080);


            Thread readThread = new Thread(new ThreadStart(ReadUDP));
            readThread.Start();
            Thread readTCPThread = new Thread(new ThreadStart(ReadTCP));
            readTCPThread.Start();
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
            result = -1;
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

        public void dequeue()
        {
            Thread dequeueThread = new Thread(new ThreadStart(() => {
                try
                {
                    Message msg = new Message();
                    msg.type = "dequeue";
                    byte[] jsonutf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));
                    NetworkStream stream= tcpClient.GetStream();

                    stream.Write(jsonutf8Bytes, 0, jsonutf8Bytes.Length);
                }
                catch
                {
                    Debug.Log("could not send dequeue");
                }
            }));
            dequeueThread.Start();
        }

        public void EndGame()
        {
            Thread endGameThread = new Thread(new ThreadStart(() =>{
                try
                {
                    ChannelMessage msg = new ChannelMessage();
                    msg.channel = channel;
                    msg.type = "gameLost";

                    byte[] jsonUtf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));

                    tcpClient.GetStream().Write(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);
                    Debug.Log("sending tcp package for end game");
                    HandleEndGame();
                }
                catch
                {
                    Debug.Log("could not send endgame-message");
                }
            }));
            endGameThread.Start();
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
                    //Debug.Log("Could not get message");
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
                OnTurnChange(myTurn);
                channelMutex.ReleaseMutex();
                Thread pingChannelThread = new Thread(new ThreadStart(PingChannel));
                pingChannelThread.Start();
            }
            else if (temp.type == "transform")
            {
                FormattedTransform tf = JsonUtility.FromJson<FormattedTransform>(message);
                if(tf.channel == channel)
                {
                    mutex.WaitOne();
                    transforms[tf.id] = tf;
                    OnTransform();
                    mutex.ReleaseMutex();
                }
            }
            else if (temp.type == "toggleTurn")
            {
                myTurn = !myTurn;
                OnTurnChange(myTurn);
            }
            else if(temp.type == "EndGame")
            {
                GameEnd g = JsonUtility.FromJson<GameEnd>(message);
                Debug.Log(message);
                result = g.result;
                Debug.Log(g.result);
                HandleEndGame();
            }
        }

        private void HandleEndGame()
        {
            channelMutex.WaitOne();
            channel = "";
            channelMutex.ReleaseMutex();
            mutex.WaitOne();
            transforms = new Dictionary<string, FormattedTransform>();
            mutex.ReleaseMutex();
        }
        
        public void EndTurn() {
            Thread endTurnThread = new Thread(new ThreadStart(() => {
                ChannelMessage msg = new ChannelMessage();
                msg.channel = channel;

                msg.type = "endTurn";
                byte[] jsonUtf8Bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));

                NetworkStream stream = tcpClient.GetStream();
                // Send the message to the connected TcpServer.
                stream.Write(jsonUtf8Bytes, 0, jsonUtf8Bytes.Length);

            }));
            endTurnThread.Start();
        }

        public void setOnTransform(Func<int> fun){
            OnTransform = fun;
        }

        public void SetOnTurnChange(Func<bool, bool> fun)
        {
            OnTurnChange = fun;
        }

        public Vector3 GetPosition(Vector3 pos, string id) {
            if(transforms.ContainsKey(id))
            {
                return transforms[id].position;
            }
            return pos;
        }

        public Quaternion GetRotation(Quaternion rot, string id) {
            if(transforms.ContainsKey(id))
            {
                return transforms[id].rotation;
            }
            return rot;
        }

        public Vector3 GetScale(Vector3 scale, string id) {
            if(transforms.ContainsKey(id)){
                return transforms[id].scale;
            }
            return scale;
        }

        public Vector3 GetVelocity(Vector3 vel, string id)
        {
            if(transforms.ContainsKey(id)){
                return transforms[id].velocity;
            }
            return vel;
        }

        public string getChannel()
        {
            return channel;
        }


        public bool getMyTurn()
        {
            return myTurn;
        }

        public bool InGame()
        {
            return channel != "";
        }

        public int Result()
        {
            //returns -1 if not finished, 0 if loss and 1 if win
            return result;
        }

        private byte[] toBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

    }
}