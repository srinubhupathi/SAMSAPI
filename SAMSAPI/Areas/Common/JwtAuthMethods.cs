using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SAMSAPI.Areas.Common
{

    public class JwtAuthMethods
    {
        //public string createToken(string username)
        //{
        //    //Set issued at date
        //    DateTime issuedAt = DateTime.UtcNow;
        //    //set the time when it expires
        //    //DateTime expires = DateTime.UtcNow.AddDays(7);
        //    //int expireTime = Convert.ToInt32(ConfigurationManager.AppSettings["jwtExpireInMinutes"].ToString());
        //    //DateTime expires = DateTime.UtcNow.AddMinutes(expireTime);

        //    int expireTime = Convert.ToInt32(ConfigurationManager.AppSettings["jwtExpireInDays"].ToString());
        //    DateTime expires = DateTime.UtcNow.AddDays(expireTime);

        //    //http://stackoverflow.com/questions/18223868/how-to-encrypt-jwt-security-token
        //    var tokenHandler = new JwtSecurityTokenHandler();

        //    //create a identity and add claims to the user which we want to log in
        //    ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
        //    {
        //        new Claim(ClaimTypes.Name, username)
        //    });

        //    const string sec = "401b09eab3c013d4ca54922bb802bec8fd5318192b0a75f201d8b3727429090fb337591abd3e44453b954555b7a0812e1081c39b740293f765eae731f5a65ed1";
        //    // string sec = ConfigurationManager.AppSettings["/http://stackoverflow.com/questions/18223868/how-to-encrypt-jwt-security-token"].ToString();
        //    var now = DateTime.UtcNow;
        //    var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(sec));
        //    var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature);

        //    var hostName = ConfigurationManager.AppSettings["baseUrl"].ToString();
        //    //create the jwt
        //    var token =
        //        (JwtSecurityToken)
        //            tokenHandler.CreateJwtSecurityToken(issuer: hostName, audience: hostName,
        //                subject: claimsIdentity, notBefore: issuedAt, expires: expires, signingCredentials: signingCredentials);
        //    var tokenString = tokenHandler.WriteToken(token);

        //    return tokenString;
        //}
    }

}