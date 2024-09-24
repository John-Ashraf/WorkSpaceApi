namespace WorkSpaceApi.Models
{
    public class CheckIns
    {
        public int Id { get; set; }
        public double HourPrice { get; set; }
        public string Name { get; set; }
        public DateTime checkin { get; set; }
        public DateTime? checkout { get; set; }
        public string AppuserId { get; set; }
        public AppUser Appuser { get; set; }
        public List<Order> ?Orders { get; set; }

    }
}
