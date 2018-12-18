using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Workflow.Roles.Dto;
using Workflow.Users.Dto;

namespace Workflow.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);
    }
}
