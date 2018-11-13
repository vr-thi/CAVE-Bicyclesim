using UnityEngine;
using AwesomeSockets.Buffers;
using System;


namespace Cave
{
    public class InputTimeMessage : ISynchroMessage
    {
        public float deltaTime;
        public float time;
        public int timeHour;
        public int timeMinute;

        public void Serialize(AwesomeSockets.Buffers.Buffer buffer)
        {
            AwesomeSockets.Buffers.Buffer.Add(buffer, deltaTime);
            AwesomeSockets.Buffers.Buffer.Add(buffer, time);
            AwesomeSockets.Buffers.Buffer.Add(buffer, timeHour);
            AwesomeSockets.Buffers.Buffer.Add(buffer, timeMinute);
        }

        public void Deserialize(AwesomeSockets.Buffers.Buffer buffer)
        {
            deltaTime = AwesomeSockets.Buffers.Buffer.Get<float>(buffer);
            time = AwesomeSockets.Buffers.Buffer.Get<float>(buffer);
            timeHour = AwesomeSockets.Buffers.Buffer.Get<int>(buffer);
            timeMinute = AwesomeSockets.Buffers.Buffer.Get<int>(buffer);
        }

        public int GetLength()
        {
            return sizeof(float) * 2 + sizeof(int) * 2;
        }
    }

    class TimeSynchronizer
    {

        public static float deltaTime;
        public static float time;
        public static int timeMinute;
        public static int timeHour;
        private static DateTime dateTime;

        public static void BuildMessage(InputTimeMessage message)
        {
            deltaTime = Time.deltaTime;
            time = Time.time;
            dateTime = DateTime.Now;
            timeMinute = dateTime.Minute;
            timeHour = dateTime.Hour;
            message.deltaTime = deltaTime;
            message.time = time;
            message.timeHour = timeHour;
            message.timeMinute = timeMinute;
        }

        public static void ProcessMessage(InputTimeMessage message)
        {
            deltaTime = message.deltaTime;
            time = message.time;
            timeHour = message.timeHour;
            timeMinute = message.timeMinute;
        }
    }

}
