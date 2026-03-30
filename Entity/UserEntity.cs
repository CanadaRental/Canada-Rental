namespace OntarioGo.Entity
{
    public class UserEntity
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<PropertyEntity> Properties { get; set; }
        public List<FavoriteEntity> Favorites { get; set; }
        public List<MessageEntity> Messages { get; set; }
        public List<PropertyViewEntity> PropertyViews { get; set; }
        public List<ReviewEntity> Reviews { get; set; }
        public List<NotificationEntity> Notifications { get; set; }
    }
}
