using System.ComponentModel.DataAnnotations;

namespace AngularBPWorkflow.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}