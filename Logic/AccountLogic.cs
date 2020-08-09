using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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

        public string GetAuthenticationToken(LoginModel loginModel)
        {
            User currentUser = _myWorldDbContext.User.Where(_ => _.Email.ToLower() == loginModel.Email.ToLower() &&
            _.Password == loginModel.Password).FirstOrDefault();

            if (currentUser != null)
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
                var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var userCliams = new Claim[]{
                    new Claim("email", currentUser.Email),
                    new Claim("phone", currentUser.PhoneNumber),
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Audience,
                    expires: DateTime.Now.AddMinutes(20),
                    signingCredentials: credentials,
                    claims:userCliams
                );

                string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                return token;
            }

            return string.Empty;

        }
    }
}