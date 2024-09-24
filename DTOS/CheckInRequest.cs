using System.ComponentModel.DataAnnotations;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.DTOS
{
    public class CheckInRequest
    {
        [Required]
        public double HourPrice { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string userId { get; set; }
        


    }
}
