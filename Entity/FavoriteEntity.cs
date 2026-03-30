namespace OntarioGo.Entity
{
    public class FavoriteEntity
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserEntity User { get; set; }
        public PropertyEntity Property { get; set; }
    }
}
