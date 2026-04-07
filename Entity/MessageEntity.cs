namespace OntarioGo.Entity
{
    public class MessageEntity
    {
        public int MessageId { get; set; }
        public int PropertyId { get; set; }
        public int? SenderId { get; set; }
        public string SenderEmail { get; set; }
        public string MessageText { get; set; }
        public DateTime SentAt { get; set; }

        public int? ConversationId { get; set; }  

        public PropertyEntity Property { get; set; }
        public UserEntity Sender { get; set; }
    }
}