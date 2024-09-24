using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApi.DTOS
{
    public class EditOrderDto
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int CheckInId { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double ItemPrice { get; set; }

    }
}
