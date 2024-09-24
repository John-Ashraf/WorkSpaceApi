namespace WorkSpaceApi.DTOS
{
    public class CheckOutResultDto
    {
        public string Message { get; set; }
        public bool status { get; set; }
        public double Amount { get; set; }
        public int CheckinID { get; set; }
        public string Username { get; set; }
    }
}
