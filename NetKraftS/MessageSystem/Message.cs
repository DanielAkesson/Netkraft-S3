using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Netkraft.Messaging
{
    //MessageTypes
    /// <summary>
    /// Any class or struct that inherits this Interface will automaticlly have the <see cref="Writable"/> attribute and can be sent between <see cref="NetkraftClient"/>'s.
    /// <para>This message type is not guaranteed to be delivered to end-client</para>
    /// </summary>
    public interface IUnreliableMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a <see cref="NetkraftClient"/>.
        /// The object this method is called on will have the correct context and data.
        /// <para>This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made. </para>
        /// </summary>
        void OnReceive(ClientConnection context);
    }
    /// <summary>
    /// Any class or struct that inherits this Interface will automaticlly have the <see cref="Writable"/> attribute and can be sent between <see cref="NetkraftClient"/>'s.
    /// <para>This message type is guaranteed to be delivered to end-client and will therefore be resent until acknowledgement is confirmed.</para>
    /// <remarks>Note: This message is not slower then <see cref="IUnreliableMessage"/> however, it's resent multiple times and should be avoided if the amount of data sent is a concern</remarks>
    /// </summary>
    public interface IReliableMessage
    {
        /// <summary>
        /// This is a callback method for when a message is read from a NetkraftClient.
        /// The object this method is called on will have the correct context and data.
        /// <para></para>
        /// This object is reused everytime a message of this class is received, meaning it's a volatile pointer and can not be stored.
        /// If you want this message object stored <see cref="CopyMessage"/> that will allow deep copies of messages to be made.
        /// </summary>
        void OnReceive(ClientConnection context);
    }
    /// <summary>
    /// Any class or struct that inherits this Interface has to inherit <see cref="IReliableMessage"/> or <see cref="IUnreliableMessage"/> interface too.
    /// <para>This message will be acknowledged and <see cref="OnAcknowledgment(ClientConnection)"/> will be called once the end-client has recvied the message.</para>
    /// </summary>
    public interface IAcknowledged
    {
        /// <summary>
        /// OnAcknowledgment is called once this message has been confirmed to have been received by the end client
        /// <paramref name="context"/>
        /// </summary>
        /// <param name="context">The client that received the message</param>
        void OnAcknowledgment(ClientConnection context);
    }

    public static class Message
    {
        private static Dictionary<Type, ushort> _typeToMessageID = new Dictionary<Type, ushort>();
        private static Dictionary<ushort, Type> _IDToMessageType = new Dictionary<ushort, Type>();
        private static MemoryStream _copyStream = new MemoryStream();

        static Message()
        {
            //Console.WriteLine("Inizilaze message system");
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> messages = new List<Type>();
            foreach (Assembly a in assemblies)
            {
                foreach (Type t in a.GetTypes())
                {
                    if (!typeof(IUnreliableMessage).IsAssignableFrom(t) && !typeof(IReliableMessage).IsAssignableFrom(t))
                        continue;
                    messages.Add(t);
                }
            }
            foreach (Type t in messages)
            {
                ushort id = (ushort)_typeToMessageID.Keys.Count; //TODO: make this more reliable since both clients need all messages in the same order.
                _typeToMessageID.Add(t, id);
                _IDToMessageType.Add(id, t);
            }
        }
        //Static public interface!
        /// <summary>
        /// Read a message from a stream.
        /// An object of the read message type will be assigned and called OnReceive with the data retrived.
        /// This message object is a volatile pointer and can not be stored without making a deep copy first.
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

        //Callbacks to user created messages.
        /// <summary>
        /// Creates a deep copy of the <paramref name="message"/>.
        /// This copy can be stored knowing it will not change or be alterd by the system.
        /// </summary>
        /// <param name="message">Message to be copied</param>
        /// <returns>A stand alone deep copy of <paramref name="message"/></returns>
        public static object CopyMessage(object message)
        {
            _copyStream.Seek(0, SeekOrigin.Begin); //Seek zero.
            WritableSystem.Write(_copyStream, message);//Write this message to copyStream.
            _copyStream.Seek(0, SeekOrigin.Begin); //Seek zero.
            return WritableSystem.Read(_copyStream, message.GetType());//Return copy read from stream.
        }
    }
}
