using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebApp_UnderTheHood.Authorization;
using WebApp_UnderTheHood.DTO;
using WebApp_UnderTheHood.Pages.Account;

namespace WebApp_UnderTheHood.Pages
{
    [Authorize(Policy = "HRManagerOnly")]
    public class HRManagerModel : PageModel
    {
        private readonly IHttpClientFactory httpClientFactory;

        [BindProperty]
        public List<WeatherForecastDTO> weatherForecastItems { get; set; } = new List<WeatherForecastDTO>();    //new List<WeatherForecastDTO>() is to avoid null reference warning

        public HRManagerModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        // Http client to trigger WebAPI
        public async Task OnGetAsync()
        {
            //If we already have token stored in the session, get token from the session first
            JwtToken token = new JwtToken();        // If empty, initialize

            var strTokenObj = HttpContext.Session.GetString("acces_token");
            if (string.IsNullOrEmpty(strTokenObj))      // If no token in session yet
            {
                token = await Authenticate();
            }
            else   // If there is token in session
            {
                token = JsonConvert.DeserializeObject<JwtToken>(strTokenObj)?? new JwtToken();      // If empty, initialize - might produce null token
            }

            // If token is null or expired
            if (token == null ||
                string.IsNullOrWhiteSpace(token.AccessToken) ||
                token.ExpiresAt <= DateTime.UtcNow)
            {
                token = await Authenticate();
            }

            var httpClient = httpClientFactory.CreateClient("OurWebAPI");
            // Sending token in Header (Bearer is scheme name)
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token?.AccessToken ?? string.Empty);    // Bearer is the scheme name
            weatherForecastItems = await httpClient.GetFromJsonAsync<List<WeatherForecastDTO>>("WeatherForecast")??new List<WeatherForecastDTO>();     //new List<WeatherForecastDTO>() is to avoid null reference warning
        }

        private async Task<JwtToken> Authenticate ()
        {
            //authentication and getting the token
            var httpClient = httpClientFactory.CreateClient("OurWebAPI");

            //Post request with credentials to API endpoint/ Auth 
            var res = await httpClient.PostAsJsonAsync("auth", new Credential { UserName = "admin", Password = "123" });
            res.EnsureSuccessStatusCode();
            string strJwt = await res.Content.ReadAsStringAsync();      // This is JWT token

            //	If we get the token for first time or after token is expired, we store it in session
            HttpContext.Session.SetString("access_token", strJwt);

            //return token
            return JsonConvert.DeserializeObject<JwtToken>(strJwt)??new JwtToken();       //JwtToken from Authorization/JwtToken   // deserialize strJwt into an object
        }
    }
}
