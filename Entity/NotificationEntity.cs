namespace OntarioGo.Entity
{
    public class NotificationEntity
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserEntity User { get; set; }
    }
}
