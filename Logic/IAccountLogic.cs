using JwtApiSample.Models;

namespace JwtApiSample.Logic
{
    public interface IAccountLogic
    {
         string GetAuthenticationToken(LoginModel loginModel);
    }
}