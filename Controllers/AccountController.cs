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
            var tokenModel = _accountLogic.GetAuthenticationToken(model);

            if (tokenModel == null)
            {
                return NotFound();
            }
            return Ok(tokenModel);
        }

        [HttpPost]
        [Route("activate-token-by-refreshtoken")]
        public IActionResult ActivateAccessTokenByRefresh(TokenModel refreshToken)
        {
            var resultTokenModel = _accountLogic.ActivateTokenUsingRefreshToke(refreshToken);
            if (refreshToken == null)
            {
                return NotFound();
            }
            return Ok(resultTokenModel);
        }
    }
}