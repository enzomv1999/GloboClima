using System.ComponentModel.DataAnnotations;

namespace GloboClima.API.DTOs
{
    /// <summary>
    /// Input model used to create a new favorite item.
    /// </summary>
    public class FavoriteInput
    {
        /// <summary>
        /// Type of the favorite (e.g., "city", "country").
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Display name or identifier of the favorite entity.
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
    }

}
