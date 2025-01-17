﻿using Microsoft.AspNetCore.Identity;

namespace WorkSpaceApi.Models
{
    public class AppUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<RefreshToken>? RefreshTokens { get; set; }

    }
}
