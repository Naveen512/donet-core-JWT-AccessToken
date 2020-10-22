using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JwtApiSample.Data.Context;
using JwtApiSample.Data.Entities;
using JwtApiSample.Models;
using JwtApiSample.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JwtApiSample.Logic
{
    public class AccountLogic : IAccountLogic
    {
        private readonly TokenSettings _tokenSettings;
        private readonly MyWorldDbContext _myWorldDbContext;
        public AccountLogic(
            IOptions<TokenSettings> tokenSettings,
            MyWorldDbContext myWorldDbContext
            )
        {
            _tokenSettings = tokenSettings.Value;
            _myWorldDbContext = myWorldDbContext;
        }

        public TokenModel GetAuthenticationToken(LoginModel loginModel)
        {
            User currentUser = _myWorldDbContext.User.Where(_ => _.Email.ToLower() == loginModel.Email.ToLower() &&
            _.Password == loginModel.Password).FirstOrDefault();

            if (currentUser != null)
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
                var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var userCliams = new List<Claim>{
                    new Claim("email", currentUser.Email),
                    new Claim("phone", currentUser.PhoneNumber),
                };

                return GetTokens(currentUser, userCliams);
            }

            return null;

        }

        private string GetRefreshToken()
        {
            var key = new Byte[32];
            using (var refreshTokenGenerator = RandomNumberGenerator.Create())
            {
                refreshTokenGenerator.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        private TokenModel GetTokens(
            User currentUser,
            List<Claim> userClaims
        )
        {
             var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var newJwtToken = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Audience,
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: credentials,
                    claims: userClaims
            );

            string token = new JwtSecurityTokenHandler().WriteToken(newJwtToken);
            string refreshToken = GetRefreshToken();

            currentUser.RefreshToken = refreshToken;
            _myWorldDbContext.SaveChanges();


            return new TokenModel
            {
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public TokenModel ActivateTokenUsingRefreshToke(TokenModel tokenModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(tokenModel.Token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _tokenSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _tokenSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key)),
                ValidateLifetime = true
            }, out SecurityToken validatedToken);


            var jwtToken = validatedToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            {
                return null;
            }

            var email = claimsPrincipal.Claims.Where(_ => _.Type == ClaimTypes.Email).Select(_ => _.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var currentUser = _myWorldDbContext.User.Where(_ => _.Email == email && _.RefreshToken == tokenModel.RefreshToken).FirstOrDefault();
            if (currentUser == null)
            {
                return null;
            }

            return GetTokens(currentUser, jwtToken.Claims.ToList());
        }
    }
}