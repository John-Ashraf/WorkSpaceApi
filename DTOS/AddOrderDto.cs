using System.ComponentModel.DataAnnotations;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.DTOS
{
    public class AddOrderDto
    {
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double Itemprice { get; set; }
        [Required]
        public int checkinId { set; get; }
    }
}
