using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using AngularBPWorkflow.Roles.Dto;
using AngularBPWorkflow.Users.Dto;

namespace AngularBPWorkflow.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);
    }
}
