using UnityEngine;

namespace GameNetWorkClient
{

    public class Message
    {
        public string type;

    }

    public class ChannelMessage : Message
    {
        public string channel;
    }

    public class NewGame : ChannelMessage
    {
        public bool myTurn;
    }

    public class GameEnd : ChannelMessage
    {
        public int result;
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