using JwtApiSample.Logic;
using JwtApiSample.Models;
using Microsoft.AspNetCore.Mvc;

namespace JwtApiSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountLogic _accountLogic;

        public AccountController(IAccountLogic accountLogic)
        {
            _accountLogic = accountLogic;
        }

        [HttpPost]
        [Route("login-token")]
        public IActionResult GetLoginToken(LoginModel model)
        {
            var token = _accountLogic.GetAuthenticationToken(model);

            if (string.IsNullOrEmpty(token))
            {
                return NotFound();
            }
            return Ok(new {token});
        }
    }
}