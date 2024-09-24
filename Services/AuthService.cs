using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WorkSpaceApi.DTOS;
using WorkSpaceApi.Helpers;
using WorkSpaceApi.Models;

namespace WorkSpaceApi.Services
{
    public class AuthService : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;
        public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> LoginAsync(LoginRequestModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);
            if(user==null||!await _userManager.CheckPasswordAsync(user, model.password))
            {
                return new AuthModel { Message = "Email or Password Not valid!" };
            }
            var jwt=await CreateJwtToken(user);
            var rolelist=await _userManager.GetRolesAsync(user);
            
            AuthModel authModel = new AuthModel
            {
                IsAuthenticated = true,
                Message = "login successfully",
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                Email = model.email,
                Username = model.email,
                FullName = user.FirstName + " " + user.LastName,
                Roles = rolelist.ToList(),
               // ExpiresOn = jwt.ValidTo,
            };
            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activerefreshtoken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authModel.RefreshToken = activerefreshtoken.Token;
                authModel.ExpiresOn=activerefreshtoken.ExpiresOn;

            }
            else
            {
                var newrefreshtoken = GenerateRefreshToken();
                authModel.RefreshToken = newrefreshtoken.Token;
                authModel.ExpiresOn = newrefreshtoken.ExpiresOn;
                user.RefreshTokens.Add(newrefreshtoken);
                await _userManager.UpdateAsync(user);
            }

            return authModel;  
        
        }

        public async Task<AuthModel> RegisterAsync(RegisterationModel dto)
        {

            if (await _userManager.FindByEmailAsync(dto.email) != null)
                return new AuthModel { Message = "Email Is not Valid!" };


            if (await _userManager.FindByNameAsync(dto.userName) is not null)
                return new AuthModel { Message = "Username is already registered!" };

            var user = new AppUser
            {
                UserName = dto.userName,
                Email = dto.email,
                FirstName = dto.firstName,
                LastName = dto.lastName,
                PhoneNumber = dto.phoneNumber,

            };
            var result = await _userManager.CreateAsync(user, dto.password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;

                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Message = errors };
            }
            await _userManager.AddToRoleAsync(user, "User");
            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName,

            };

        }

        public async Task<string> AddRole(AddRoleRequestModel model)
        {
           
            var user =await _userManager.FindByEmailAsync(model.email);
            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "already Assgined";

            var result=await _userManager.AddToRoleAsync(user,model.Role);
            if (!result.Succeeded)
            {
                return "Somthing went Wrong!";
            }
            else
            {
               return string.Empty;
            }
            

        }


        private async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using var generator = new RNGCryptoServiceProvider();

            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };
        }

        public async Task<AuthModel> RefreshToken(string token)
        {
            AuthModel authModel=new AuthModel();
            var user=await _userManager.Users.SingleOrDefaultAsync(u=>u.RefreshTokens.Any(t => t.Token == token));
            if(user == null)
            {
                authModel.Message = "Invalid Token";
                return authModel;
            }
            var refreshToken=user.RefreshTokens.FirstOrDefault(t=>t.Token==token);
            if (!refreshToken.IsActive)
            {
                authModel.Message = "Inactive Token";
                return authModel;
            }
            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await CreateJwtToken(user);
            authModel.IsAuthenticated=true;
            authModel.Token=new JwtSecurityTokenHandler().WriteToken(jwtToken);
            authModel.RefreshToken=newRefreshToken.Token;
            authModel.RefreshTokenExpiration=refreshToken.ExpiresOn;
            authModel.Email=user.Email;
            var roles = await _userManager.GetRolesAsync(user);
            authModel.Roles = roles.ToList();

            return authModel;


        }
        public async Task<bool> RevokeTokenAsync(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null)
                return false;

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return false;

            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return true;
        }
    }
}
