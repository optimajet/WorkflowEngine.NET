using System.ComponentModel.DataAnnotations;

namespace Workflow.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}