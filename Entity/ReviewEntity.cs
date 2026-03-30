namespace OntarioGo.Entity
{
    public class ReviewEntity
    {
        public int ReviewId { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public PropertyEntity Property { get; set; }
        public UserEntity User { get; set; }
    }
}
