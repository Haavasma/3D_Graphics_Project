using UnityEngine;

// Classes used in gamenetworkclient to convert from json-messages to objects
namespace GameNetWorkClient
{
    // normal message containing a type
    public class Message
    {
        public string type;

    }

    // message that also contains a channel
    public class ChannelMessage : Message
    {
        public string channel;
    }

    // Class for getting a newgame message, containing your turn
    public class NewGame : ChannelMessage
    {
        public bool myTurn;
    }

    // Class for getting the result of ended game
    public class GameEnd : ChannelMessage
    {
        public int result;
    }

    // class for receiving transform data
    public class FormattedTransform : ChannelMessage
    {
        public string id;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Vector3 velocity;

        public FormattedTransform(string objectId, string msgType, string _channel, Transform tf)
        {
            id = objectId;
            type = msgType;
            channel = _channel;
            position = tf.position;
            rotation = tf.rotation;
            scale = tf.localScale;
            velocity = tf.GetComponent<Rigidbody>().velocity;
        }

        public FormattedTransform()
        {

        }
    }
}