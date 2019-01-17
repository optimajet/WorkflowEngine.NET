using Abp.Application.Services.Dto;
using System;

namespace AngularBPWorkflow.Users.Dto
{
    //custom PagedResultRequestDto
    public class PagedUserResultRequestDto : PagedResultRequestDto
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public DateTimeOffset? From { get; set; }//javascript date within timezone
        public DateTimeOffset? To { get; set; }//javascript date within timezone
    }
}
