namespace OntarioGo.Entity
{
    public class AdminActionEntity
    {
        public int ActionId { get; set; }
        public int AdminId { get; set; }
        public int? PropertyId { get; set; }
        public string ActionType { get; set; }
        public string Notes { get; set; }
        public DateTime ActionDate { get; set; }

        public UserEntity Admin { get; set; }
        public PropertyEntity Property { get; set; }
    }
}
