using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Netkraft.Messaging
{
    //MessageTypes
    public interface IUnreliableMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnReceive(ClientConnection Context);
        /// <summary>
        /// This is a callback method for when a message is sent from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is sent, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnSend(ClientConnection Context);
    }
    public interface IUnreliableAcknowledgedMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnReceive(ClientConnection Context);
        /// <summary>
        /// This is a callback method for when a message is sent from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is sent, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnSend(ClientConnection Context);
        void OnAcknowledgment(ClientConnection Context);
    }
    public interface IReliableMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnReceive(ClientConnection Context);
        /// <summary>
        /// This is a callback method for when a message is sent from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is sent, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnSend(ClientConnection Context);
    }
    public interface IReliableAcknowledgedMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnReceive(ClientConnection Context);
        /// <summary>
        /// This is a callback method for when a message is sent from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is sent, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnSend(ClientConnection Context);
        void OnAcknowledgment(ClientConnection Context);
    }
    public static class Message
    {
        private static Dictionary<Type, Type> _typeToChannelType = new Dictionary<Type, Type>();
        private static Dictionary<Type, ushort> _typeToMessageID = new Dictionary<Type, ushort>();
        private static Dictionary<ushort, Type> _IDToMessageType = new Dictionary<ushort, Type>();
        private static MemoryStream _copyStream = new MemoryStream();

        static Message()
        {
            Console.WriteLine("Inizilaze message system");
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> messages = new List<Type>();
            foreach (Assembly A in assemblies)
            {
                foreach (Type T in A.GetTypes())
                {
                    if (typeof(IUnreliableMessage).IsAssignableFrom(T))
                        _typeToChannelType.Add(T, typeof(IUnreliableMessage));
                    else if (typeof(IUnreliableAcknowledgedMessage).IsAssignableFrom(T))
                        _typeToChannelType.Add(T, typeof(IUnreliableAcknowledgedMessage));
                    else if (typeof(IReliableMessage).IsAssignableFrom(T))
                        _typeToChannelType.Add(T, typeof(IReliableMessage));
                    else if (typeof(IReliableAcknowledgedMessage).IsAssignableFrom(T))
                        _typeToChannelType.Add(T, typeof(IReliableAcknowledgedMessage));
                    else
                        continue;
                    messages.Add(T);
                }
            }
            foreach (Type t in messages)
            {
                Console.WriteLine("Inizialized: " + t.Name);
                ushort id = (ushort)_typeToMessageID.Keys.Count;
                _typeToMessageID.Add(t, id);
                _IDToMessageType.Add(id, t);
            }
        }
        //Static public interface!
        /// <summary>
        /// Read a message from a stream.
        /// An object of the read message type will be assigned and called OnReceive with the data retrived.
        /// This message object is a volatile pointer and can not be stored withoutmaking a deep copy first. 
        /// </summary>
        /// <param name="stream">Stream to read message from</param>
        /// <param name="context">NetkraftClient this message was received at and the endPoint that sent it</param>
        public static object ReadMessage(Stream stream, ClientConnection context)
        {
            BinaryReader reader = new BinaryReader(stream);
            ushort id = reader.ReadUInt16();
            Type messageType = _IDToMessageType[id];
            return WritableSystem.Read(stream, messageType);
        }
        /// <summary>
        /// Write a message to a stream.
        /// This includes the main header, other data related to the message channel and message body.
        /// </summary>
        /// <param name="message">Message to write to steam</param>
        /// <param name="stream">Stream to write message into. Message will be writen into the stream at the current position of the stream.</param>
        public static void WriteMessage(Stream stream, object message)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(_typeToMessageID[message.GetType()]);
            WritableSystem.Write(stream, message);
        }
        /// <summary>
        /// Get the channel type for this object.
        /// </summary>
        /// <param name="message">Message object</param>
        /// <returns>Channel type or null if object id not a messageType</returns>
        public static Type GetChannelType(object message)
        {
            return _typeToChannelType.ContainsKey(message.GetType()) ? _typeToChannelType[message.GetType()] : null;
        }
        //Callbacks to user created messages.
        /// <summary>
        /// Creates a deep copy of the calling message.
        /// This copy can be stored knowing it will not change or be alterd by the system.
        /// </summary>
        /// <returns>A stand alone deep copy of the calling message</returns>
        public static object CopyMessage(object message)
        {
            _copyStream.Seek(0, SeekOrigin.Begin); //Seek zero.
            WritableSystem.Write(_copyStream, message);//Write this message to copyStream.
            _copyStream.Seek(0, SeekOrigin.Begin); //Seek zero.
            return WritableSystem.Read(_copyStream, message.GetType());//Return copy read from stream.
        }
    }
}
