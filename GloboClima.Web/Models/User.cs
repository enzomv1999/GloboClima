using System.ComponentModel.DataAnnotations;

namespace GloboClima.Web.Models
{
    public class User
    {
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "O nome de usuário deve ter no mínimo 5 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "A senha deve ter no mínimo 5 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
