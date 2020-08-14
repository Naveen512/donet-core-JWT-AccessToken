using JwtApiSample.Models;

namespace JwtApiSample.Logic
{
    public interface IAccountLogic
    {
         TokenModel GetAuthenticationToken(LoginModel loginModel);
         TokenModel ActivateTokenUsingRefreshToke(TokenModel tokenModel);
    }
}