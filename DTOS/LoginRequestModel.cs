using System.ComponentModel.DataAnnotations;

namespace WorkSpaceApi.DTOS
{
    public class LoginRequestModel
    {
        [Required,EmailAddress]
        public string email { get; set; }
        [Required]
        public string password { get; set; }

    }
}
