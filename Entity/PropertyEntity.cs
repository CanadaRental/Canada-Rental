namespace OntarioGo.Entity
{
    public class PropertyEntity
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Address { get; set; }
        public string PropertyType { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }

        public int OwnerId { get; set; }

        public UserEntity Owner { get; set; }

        public List<PropertyImageEntity> Images { get; set; }
        public List<FavoriteEntity> Favorites { get; set; }
        public List<MessageEntity> Messages { get; set; }
        public List<PropertyViewEntity> Views { get; set; }
        public List<ReviewEntity> Reviews { get; set; }
    }
}
