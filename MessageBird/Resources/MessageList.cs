
using MessageBird.Json.Converters;
using Newtonsoft.Json;
using System.Text;

namespace MessageBird.Resources
{
    public class MessageLists : BaseLists<Objects.Message>
    {
        private const string Format = "yyyy-MM-dd'T'HH:mm:ssK";
        public MessageLists()
            : base("messages", new Objects.MessageList())
        {
        }

        public MessageLists(Objects.MessageList messageList) : base("messages", messageList) { }
        
        public override string QueryString
        {
            get
            {
                var baseList = (Objects.MessageList)Object;

                var builder = new StringBuilder();

                if (!string.IsNullOrEmpty(base.QueryString))
                {
                    builder.AppendFormat("{0}", base.QueryString);
                }

                if (baseList.Status != "") { 
                    builder.AppendFormat("&status={0}", baseList.Status);
                }

                if (baseList.Recipient>0)
                {
                    builder.AppendFormat("&recipient={0}", baseList.Recipient);
                }

                if (baseList.From.HasValue)
                {
                    builder.AppendFormat("&from={0}", baseList.From.Value.ToString(Format));
                }

                if (baseList.Until.HasValue)
                {
                    builder.AppendFormat("&until={0}", baseList.Until.Value.ToString(Format));
                }

                return builder.ToString();
            }
        }
    }
}
