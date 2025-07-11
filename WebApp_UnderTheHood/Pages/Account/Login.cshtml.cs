using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp_UnderTheHood.Pages.Account
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Credential Credential {  get; set; } = new Credential();     //Initialize, to avoid null reference problems
        public void OnGet()
        {
        }

        public void OnPost()
        {

        }
    }

    public class Credential
    {
        [Required]
        [Display(Description ="User Name")]

        public string UserName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
