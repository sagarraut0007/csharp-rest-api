using System;

namespace MessageBird.Objects
{
    public class MessageList : BaseList<Message>
    {
        public string Status { get; set; }
        public long Recipient { get; set; }
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
    }
}
