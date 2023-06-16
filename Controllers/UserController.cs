using IS_ZJZ_B.Context;
using IS_ZJZ_B.Helpers;
using IS_ZJZ_B.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace IS_ZJZ_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        
        private readonly AppDbContext _authContext;
        public UserController(AppDbContext appDbContext)
        {
            _authContext = appDbContext;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if(userObj == null)
                return BadRequest();
                
            var user= await _authContext.Users.
                FirstOrDefaultAsync(x=>x.email==userObj.email);
            if(user == null)
                return NotFound(new {Message= "Korisnik nepostoji."});
            if(user.status=="načekanju")
                return NotFound(new { Message = "Vaš zahtev za registraciju još nije odobren." });
            
            if (user.status == "odbijen")
                return NotFound(new { Message = "Vaš zahtev za registraciju je odbijen." });
            if (!PasswordHasher.VerifyPassword(userObj.pwd, user.pwd)) 
            {
                return BadRequest(new { Message = "Lozinka nije ispravna." });    
            }

            user.Token = CreateJwtToken(user);

            return Ok(new
            {
                Token=user.Token,
                Message = "Uspešno ste se prijavili!"
            }); 
        }

        [HttpPost("authenticatea")]
        public async Task<IActionResult> Authenticatea([FromBody] AdministrativeWorker userObj)
        {
            if (userObj == null)
                return BadRequest();
            var adminr = await _authContext.Administrativeworkers.
                FirstOrDefaultAsync(x => x.email == userObj.email);
            if (adminr == null)
                return NotFound(new { Message = "Korisnik nepostoji." });

            if (!PasswordHasher.VerifyPassword(userObj.pwd, adminr.pwd))
            {
                return BadRequest(new { Message = "Lozinka nije ispravna." });
            }
            adminr.Token = CreateJwtTokenAd(adminr);

            return Ok(new
            {
                Token = adminr.Token,
                Message = "Uspešno ste se prijavili!"
            });
            
        }

        [HttpPost("authenticateadmin")]
        public async Task<IActionResult> Authenticateadmins([FromBody] Admin userObj)
        {
            if (userObj == null)
                return BadRequest();
            var admin = await _authContext.Admin.
                FirstOrDefaultAsync(x => x.email == userObj.email);
            if (admin == null)
                return NotFound(new { Message = "Korisnik nepostoji." });

            if (!PasswordHasher.VerifyPassword(userObj.pwd, admin.pwd))
            {
                return BadRequest(new { Message = "Lozinka nije ispravna." });
            }
            admin.Token = CreateJwtTokenAdmin(admin);

            return Ok(new
            {
                Token = admin.Token,
                Message = "Uspešno ste se prijavili!"
            });

        }

        [HttpPost ("register")]

        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if( userObj == null)
                return BadRequest();
            //check email
            if (await CheckEmailExistAsync(userObj.email))
                return BadRequest( new 
                { 
                    Message = " Email već postoji!" 
                });
            if (await CheckJmbgExistAsync(userObj.jmbg))
                return BadRequest(new
                { 
                    Message = "Jmbg već postoji!" 
                });

            userObj.pwd= PasswordHasher.HashPassword(userObj.pwd);
            userObj.status = "načekanju";
            userObj.Token = "";
           // if(string.IsNullOrEmpty(userObj.email))
           await _authContext.Users.AddAsync(userObj);
           await _authContext.SaveChangesAsync();
           return Ok(new
            {
                Message = "Uspešno ste se registrovali!"
            });
        }

        private Task<bool> CheckEmailExistAsync( string email) 
            =>_authContext.Users.AnyAsync(x => x.email == email);
        private Task<bool> CheckJmbgExistAsync(string jmbg)
           => _authContext.Users.AnyAsync(x => x.jmbg == jmbg);

        
        private string CreateJwtToken(User user)
        {
            var jwtTokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {   
                new Claim(ClaimTypes.Name, $"{user.firstName}{user.lastName}")
            });

            var credentials= new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                 Subject = identity,
                 Expires = DateTime.Now.AddDays(1),
                 SigningCredentials = credentials
            };
            var token = jwtTokenhandler.CreateToken(tokenDescriptor);
            return jwtTokenhandler.WriteToken(token);
        }
        private string CreateJwtTokenAd(AdministrativeWorker user)
        {
            var jwtTokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{user.firstName}{user.lastName}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenhandler.CreateToken(tokenDescriptor);
            return jwtTokenhandler.WriteToken(token);
        }
        private string CreateJwtTokenAdmin(Admin user)
        {
            var jwtTokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, $"{user.email}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenhandler.CreateToken(tokenDescriptor);
            return jwtTokenhandler.WriteToken(token);
        }
        [Authorize] ///da zastiti  api
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(
                await _authContext.Users.ToListAsync()
                );
        }
    }
}
