using System.ComponentModel.DataAnnotations;

namespace GloboClima.API.DTOs
{
    /// <summary>
    /// Input model representing user credentials for authentication or registration.
    /// </summary>
    public class UserInput
    {
        /// <summary>
        /// Username chosen by the user.
        /// </summary>
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "O nome de usuário deve ter entre 5 e 50 caracteres")]
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Password associated with the user. Must meet complexity requirements.
        /// </summary>
        [Required(ErrorMessage = "A senha é obrigatória")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "A senha deve ter no mínimo 5 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
