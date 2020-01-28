using Home.App.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Home.App.Service
{
	public class AuthService
	{

		private readonly IHttpClientFactory _httpClientFactory;

		private readonly IConfiguration _config;

		public AuthService(IHttpClientFactory httpClientFactory, IConfiguration config)
		{
			_httpClientFactory = httpClientFactory;
			_config = config;
		}

		public async Task<string> Auth(HttpContext context, LoginViewModel model)
		{
			var client = _httpClientFactory.CreateClient();

			var discoDoc = await client.GetDiscoveryDocumentAsync(_config.GetConnectionString("AuthConnection"));

			if (discoDoc.IsError)
			{
				return discoDoc.Error;
			}

			var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
			{
				Address = discoDoc.TokenEndpoint,
				ClientId = "thamco_home_app",
				ClientSecret = "secret",

				UserName = model.Email,
				Password = model.Password
			});

			if (tokenResponse.IsError)
			{
				return tokenResponse.ErrorDescription;
			}

			var userInfoResponse = await client.GetUserInfoAsync(new UserInfoRequest
			{
				Address = discoDoc.UserInfoEndpoint,
				Token = tokenResponse.AccessToken
			});

			if (userInfoResponse.IsError)
			{
				return userInfoResponse.Error;
			}

			var claimsIdentity = new ClaimsIdentity(userInfoResponse.Claims, "Cookies");
			var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

			var authToken = new AuthenticationToken { Name = "access_token", Value = tokenResponse.AccessToken };

			var authProperties = new AuthenticationProperties();
			authProperties.StoreTokens(new AuthenticationToken[] { authToken });

			await context.SignInAsync("Cookies", claimsPrincipal, authProperties);

			return string.Empty;
		}

		public async Task Deauth(HttpContext httpContext)
		{
			await httpContext.SignOutAsync("Cookies");
		}

		public async Task<string> Register(string email, string password, string fullName, params string[] roles)
		{
			var client = _httpClientFactory.CreateClient();

			var discoDoc = await client.GetDiscoveryDocumentAsync(_config.GetConnectionString("AuthConnection"));

			var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
			{
				Address = discoDoc.TokenEndpoint,
				ClientId = "thamco_home_app",
				ClientSecret = "secret"
			});

			if (tokenResponse.IsError)
			{
				return tokenResponse.ErrorDescription;
			}

			client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
			client.SetBearerToken(tokenResponse.AccessToken);

			var request = new UserPutDto
			{
				Email = email,
				Password = password,
				FullName = fullName,
				Roles = roles
			};

			var response = await client.PostAsJsonAsync(_config.GetConnectionString("AuthConnection") + "/api/users/", request);

			if (!response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}

			return string.Empty;
		}

	}
}