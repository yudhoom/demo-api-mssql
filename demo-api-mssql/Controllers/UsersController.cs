using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Services;
using WebApi.Entities;
using WebApi.Models.Users;
using AutoMapper.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings
            )
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.email, model.password);

            if (user == null)
                return BadRequest(new
                {
                    status = new
                    {
                        code = 0,
                        message = "Incorrect Username or Password."
                    },
                    body = ""
                });

            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new Claim[]
            //    {
            //        new Claim(ClaimTypes.Name, user.Id.ToString())
            //    }),
            //    Expires = DateTime.UtcNow.AddDays(7),
            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //};
            //var token = tokenHandler.CreateToken(tokenDescriptor);
            //var tokenString = tokenHandler.WriteToken(token);

            // return basic user info and authentication token
            //return Ok(new
            //{
            //    id = user.id,
            //    email = user.email,
            //    fullname = user.fullname,
            //    role = user.role,
            //    organization = user.organization
            //});

            return Ok(new
            {
                status = new
                {
                    code = 1,
                    message = "successfully login"
                },
                body = new {
                    email = user.email,
                    fullname = user.fullname,
                    role = user.role,
                    organization = user.organization
                }
            });
       
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            // map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                // create user
                _userService.Create(user, model.password);
                return Ok(new
                {
                    status = new
                    {
                        code = 1,
                        message = "successfully register user."
                    },
                    body = new
                    {
                        email = user.email,
                        fullname = user.fullname,
                        role = user.role,
                        organization = user.organization
                    }
                });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new
                {
                    status = new
                    {
                        code = 0,
                        message = ex.Message
                    },
                    body = ""
                });
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var model = _mapper.Map<IList<UserModel>>(users);
            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            var model = _mapper.Map<UserModel>(user);
            return Ok(model);
        }

        [AllowAnonymous]
        [HttpPut("update_password")]
        public IActionResult Update([FromBody]UpdateModel model)
        {
            // map model to entity and set id
            var user = _mapper.Map<User>(model);
            //user.id = id;

            try
            {
                // update user 
                _userService.Update(user);
                var usr = _userService.GetByEmail(user.email);
                
                return Ok(new
                {
                    status = new
                    {
                        code = 1,
                        message = "successfully update password user."
                    },
                    body = new
                    {
                        email = usr.email,
                        fullname = usr.fullname,
                        role = usr.role,
                        organization = usr.organization
                    }
                });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new
                {
                    status = new
                    {
                        code = 0,
                        message = ex.Message
                    },
                    body = ""
                });
            }
        }

        [AllowAnonymous]
        [HttpPut("forgot_password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordModel model)
        {
            try
            {
                // update user 
                var usr= await _userService.ForgotPasswordAsync(model.email);

                return Ok(new
                {
                    status = new
                    {
                        code = 1,
                        message = "successfully forgot password user."
                    },
                    body = new
                    {
                        email = usr.email,
                        fullname = usr.fullname,
                        role = usr.role,
                        organization = usr.organization
                    }
                });
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new
                {
                    status = new
                    {
                        code = 0,
                        message = ex.Message
                    },
                    body = ""
                });
            }
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
    }
}
