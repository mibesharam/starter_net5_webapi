using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Web_Api.Dtos;
using Web_Api.Models;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration _configuration;

        public Auth(UserManager<AppUser> userManager, IConfiguration conf)
        {
            this.userManager = userManager;
            _configuration = conf;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUserDto data)
        {
            var response = new ApiResponse();
            try
            {
                var user = await userManager.FindByNameAsync(data.Username) == null ? await userManager.FindByEmailAsync(data.Username) : await userManager.FindByNameAsync(data.Username);
                
                if (user != null && await userManager.CheckPasswordAsync(user,data.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                return Unauthorized();
            }
            catch(Exception ex)
            {
                response.Error = ex.Message;
                return Ok(response);
            }
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDto data)
        {
            var response = new ApiResponse();
            try
            {
                var userExists = await userManager.FindByNameAsync(data.Username);
                if(userExists != null)
                {
                    response.Error = "User Already Exists";
                    return Ok( response);
                }

                var userToCreate = new AppUser
                {
                    Email = data.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = data.Username
                };
                var result = await userManager.CreateAsync(userToCreate, data.Password);
                if (!result.Succeeded)
                {
                    response.Error = "Error Registering User, ";
                    foreach (var error in result.Errors)
                    {
                        response.Error += error.Description;
                    }
                    return Ok(response);
                }

                response.Success = true;
                response.Message = "User Registered Successfully";
                return Ok(response);
                
            }catch(Exception ex)
            {
                response.Error = ex.Message;
                return Ok(response);
            }

        }
    }
}
