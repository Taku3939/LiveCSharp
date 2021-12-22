using System;
using System.Net.Sockets;
using MessagePack;

namespace LiveCoreLibrary.Commands
{
    public record Data(ICommand Command, TcpClient Client, int Length);
    
    [Union(0, typeof(Join))]
    [Union(1, typeof(Remove))]
    [Union(2, typeof(ChatMessage))]
    public interface ICommand {}
    
    [MessagePackObject]
    public class SendMessageCommandTest : ICommand
    {

        [Key(0)] private Guid _to;
        [Key(1)] private string _content;

        public SendMessageCommandTest(Guid to, string content)
        {
            _to = to;
            _content = content;
        }
    }

    [MessagePackObject]
    public class Join : ICommand
    {
        [Key(0)] public Guid _to;
            
        [Key(1)] public string _content;

        public Join(Guid to, string content)
        {
            _to = to;
            _content = content;
        }
    }
        
    [MessagePackObject]
    public record Remove([property: Key(0)]Guid To, [property: Key(1)]string Content) : ICommand;
    // public record TriggerEvent([property: Key(0)] String EventName) : ICommand;
    
    [MessagePackObject]
    public class ChatMessage : ICommand
    {
        [Key(0)] public ulong Id { get; set; }
        [Key(1)] public string Message { get; set; }

        public ChatMessage(ulong id, string message)
        {
            Id = id;
            Message = message;
        }
    }
    // public record TriggerEvent([property: Key(0)]String eventName) : ICommand;

    [MessagePackObject]
    public class TriggerEvent : ICommand {
        public TriggerEvent(string eventName) {
            this.EventName = eventName;
        }
        string EventName { get; }
    }
}