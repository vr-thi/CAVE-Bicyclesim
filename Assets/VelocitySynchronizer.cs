using UnityEngine;
using AwesomeSockets.Buffers;

namespace Cave
{
    public class InputVelocityMessage : ISynchroMessage
    {
        public float velocity = -1;
        

        public void Serialize(Buffer buffer)
        {
            
            Buffer.Add(buffer, velocity);
        }

        public void Deserialize(Buffer buffer)
        {
            
            velocity = Buffer.Get<float>(buffer);
        }

        public int GetLength()
        {
            return sizeof(float);
        }
    }

    class VelocitySynchronizer
    {

        public static float velocity = -1;
        

        public static void BuildMessage(InputVelocityMessage message)
        {
            velocity = BycicleBehaviour.speed*3.6f;
            message.velocity = velocity;
        }

        public static void ProcessMessage(InputVelocityMessage message)
        {
            velocity = message.velocity;
            BycicleBehaviour.speed = velocity;
        }
    }

}
