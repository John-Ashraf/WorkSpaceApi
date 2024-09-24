namespace WorkSpaceApi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string ItemName { get; set; }
        public double Itemprice { get; set; }
        public int checkinId { set; get; }
        public CheckIns CheckIn { get; set; }

    }
}
