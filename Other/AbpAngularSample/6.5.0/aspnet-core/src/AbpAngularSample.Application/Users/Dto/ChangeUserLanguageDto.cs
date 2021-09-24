using System.ComponentModel.DataAnnotations;

namespace AbpAngularSample.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}