using Amazon.S3.Model;
using aws_bucket.Data;
using aws_bucket.Model;
using aws_bucket.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace aws_bucket.Controllers
{
    [ApiController]
    [Route("/api/[controller]/")]
    public class AuthenticateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DbContextClass _context;
        public AuthenticateController(DbContextClass context,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login( Login model)
        {
            
            var user = await _userManager.FindByNameAsync(model.UserName);

           
                
            
            
                var d = _userManager.CheckPasswordAsync(user, model.Password);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRole = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                    foreach (var role in userRole)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var token = GetToken(authClaims);

                    if (userRole[0] == "User")
                    {
                    if (DateTime.Now < _context.UserInfos.ToList().Where(x => x.Login.Equals(user.UserName)).FirstOrDefault().blockedUntil)
                    {
                        return Unauthorized();
                    }
                    else 
                    return Ok(new
                        {

                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            userId = user.UserName,

                            package = _context.UserInfos.ToList().Where(x => x.Login.Equals(user.UserName)).FirstOrDefault().Package,
                            userRole

                        });
                    }
                    else
                    {
                        return Ok(new
                        {

                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            userId = user.UserName,
                            userRole

                        });
                    }
                }
                return Unauthorized();
            

        }

        [HttpPost]
        [Route("regUser")]
        public async Task<IActionResult> RegUser( Register model)
        {
            var userEx = await _userManager.FindByNameAsync(model.UserName);
            if (userEx != null) return StatusCode(StatusCodes.Status500InternalServerError, "User in db already");

            IdentityUser user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var res = await _userManager.CreateAsync(user, model.Password);
            if (!res.Succeeded) { return StatusCode(StatusCodes.Status500InternalServerError, "Creation failed!"); }
            if (await _roleManager.RoleExistsAsync(UserRoles.User))
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            _context.Add(new UserInfo() { Login = user.UserName, Package = "BASE" });
            _context.SaveChanges();
            return Ok("User added!");
        }


        [HttpPost]
        [Route("regAdmin")]
        public async Task<IActionResult> RegAdmin([FromBody] Register model)
        {
            var userEx = await _userManager.FindByNameAsync(model.UserName);
            if (userEx != null) return StatusCode(StatusCodes.Status500InternalServerError, "Admin in db already");

            IdentityUser user = new()
            {
                UserName = model.UserName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var res = await _userManager.CreateAsync(user, model.Password);
            if (!res.Succeeded) { return StatusCode(StatusCodes.Status500InternalServerError, "Creation failed!"); }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));


            //Доступно только то, что авторизированно админу !
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            // доступны методы и пользователей
            //if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            //    await _userManager.AddToRoleAsync(user, UserRoles.User);

            return Ok("Admin added!");
        }

        private JwtSecurityToken GetToken(List<Claim> claimsList)
        {
            var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(6),
                    claims: claimsList,
                    signingCredentials: new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpPost]
        [Route("Block")]

        public async Task<IActionResult> Block([FromForm] string Login, [FromForm]  int typeblock)
        {
            if(typeblock==1)
            {
                _context.UserInfos.ToList().Where(x => x.Login.Equals(Login)).FirstOrDefault().blockedUntil= DateTime.Now.AddDays(1);
            }
            else if(typeblock==2) 
            {
                _context.UserInfos.ToList().Where(x => x.Login.Equals(Login)).FirstOrDefault().blockedUntil = DateTime.Now.AddDays(7);

            }
            else if (typeblock == 3)
            {
                _context.UserInfos.ToList().Where(x => x.Login.Equals(Login)).FirstOrDefault().blockedUntil = DateTime.Now.AddMonths(1);

            }
            else if (typeblock == 4)
            {
                _context.UserInfos.ToList().Where(x => x.Login.Equals(Login)).FirstOrDefault().blockedUntil = DateTime.Now.AddYears(1);

            }
            else if (typeblock == 5)
            {
                _context.UserInfos.ToList().Where(x => x.Login.Equals(Login)).FirstOrDefault().blockedUntil = DateTime.MaxValue;

            }
            _context.SaveChanges();
            return Ok("User blocked!");

        }


        }
}
