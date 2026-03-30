namespace OntarioGo.Entity
{
    public class PropertyImageEntity
    {
        public int ImageId { get; set; }
        public int PropertyId { get; set; }
        public byte[] ImageData { get; set; }

        public string Base64Image { get; set; }
    }
}
