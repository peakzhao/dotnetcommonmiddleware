
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DotnetCore2_Jwt.Controllers
{
    [Route("api/jwt/[controller]")]
    public class TestController : Controller
    {
        // GET api/values
        [HttpGet]
        [Authorize]//添加Authorize标签，可以加在方法上，也可以加在类上
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
         // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
