using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApi.Models
{
    public class RegisterationModel
    {
        [Required,MaxLength(50)]
        public string firstName { get; set; }
        [Required, MaxLength(50)]

        public string lastName { get; set; }
        [Required, MaxLength(50)]
        public string userName { get; set; }
        [EmailAddress]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string phoneNumber { get; set; }

  
       

    }
}
