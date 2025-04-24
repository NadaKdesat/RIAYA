using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RIAYA.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        [NotMapped]
        [Compare("PasswordHash", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
