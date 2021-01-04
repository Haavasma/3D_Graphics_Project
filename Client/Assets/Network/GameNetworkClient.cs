using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;


// package used to communicate with server
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

        private static Mutex mutex = new Mutex();


        private static Mutex channelMutex = new Mutex();

        private static Mutex inGameMutex = new Mutex();

        private Func<int> OnTransform = () => {return -1;}; 

        private Func<bool, bool> OnTurnChange = (bool value) => {return false;};

        Dictionary<string, FormattedTransform> transforms = new Dictionary<string, FormattedTransform>();

        public NetworkClient(){
        }

        // connects user to server, both udp and tcp part, with the host specified
        public void Connect(string host){
            tcpClient = new TcpClient(host, 8081);
            udpClient = new UdpClient(host, 8080);


            Thread readThread = new Thread(new ThreadStart(ReadUDP));
            readThread.Start();
            Thread readTCPThread = new Thread(new ThreadStart(ReadTCP));
            readTCPThread.Start();
        }

        //Sends a transform with id to the channel the instance is connected to
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
        
        // Sends queue message to server
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
        // Sends dequeue message to server
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

        // Tells server to end the game
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

        // constantly reads incoming udp messages
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

        // constantly reads tcp messages
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

        // sends ping on udp connection, signing that the client is still active
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

        // Handles incoming messages based on type
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

        // Ends the game, reseting values
        private void HandleEndGame()
        {
            channelMutex.WaitOne();
            channel = "";
            channelMutex.ReleaseMutex();
            mutex.WaitOne();
            transforms = new Dictionary<string, FormattedTransform>();
            mutex.ReleaseMutex();
        }
        
        // Sends endturn message to server on the current channel
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

        // updates onTransform which is called every time a transform message is received
        public void setOnTransform(Func<int> fun){
            OnTransform = fun;
        }

        // updates onTurnchange which is called every time turn is changed
        public void SetOnTurnChange(Func<bool, bool> fun)
        {
            OnTurnChange = fun;
        }

        // Gets the position of the gameobject with given id, returns the same position if it does not exist
        public Vector3 GetPosition(Vector3 pos, string id) {
            if(transforms.ContainsKey(id))
            {
                return transforms[id].position;
            }
            return pos;
        }

        // Gets the rotation of the gameobject with id, returns given rotation if it does not exist
        public Quaternion GetRotation(Quaternion rot, string id) {
            if(transforms.ContainsKey(id))
            {
                return transforms[id].rotation;
            }
            return rot;
        }

        // Gets the scale of gameobject with id, returns given scale if it does not exist
        public Vector3 GetScale(Vector3 scale, string id) {
            if(transforms.ContainsKey(id)){
                return transforms[id].scale;
            }
            return scale;
        }

        // Gets the velocity of gameobject with id, returns given velocity if it does not exist
        public Vector3 GetVelocity(Vector3 vel, string id)
        {
            if(transforms.ContainsKey(id)){
                return transforms[id].velocity;
            }
            return vel;
        }

        // returns the current channel given by the server
        public string getChannel()
        {
            return channel;
        }

        // Returns true if clients turn
        public bool getMyTurn()
        {
            return myTurn;
        }

        // returns true if client is in a gamechannel
        public bool InGame()
        {
            return channel != "";
        }

        // returns the result of game, -1 if unfinished, 0 if loss and 1 if win
        public int Result()
        {
            //returns -1 if not finished, 0 if loss and 1 if win
            return result;
        }

        // converts text to sendable bytes
        private byte[] toBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

    }
}