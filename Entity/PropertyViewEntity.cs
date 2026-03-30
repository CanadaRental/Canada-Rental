namespace OntarioGo.Entity
{
    public class PropertyViewEntity
    {
        public int ViewId { get; set; }
        public int PropertyId { get; set; }
        public int? UserId { get; set; }
        public DateTime ViewedAt { get; set; }

        public PropertyEntity Property { get; set; }
        public UserEntity User { get; set; }
    }
}
