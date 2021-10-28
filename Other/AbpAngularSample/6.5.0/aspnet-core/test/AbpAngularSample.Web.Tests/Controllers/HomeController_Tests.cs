using System.Threading.Tasks;
using AbpAngularSample.Models.TokenAuth;
using AbpAngularSample.Web.Controllers;
using Shouldly;
using Xunit;

namespace AbpAngularSample.Web.Tests.Controllers
{
    public class HomeController_Tests: AbpAngularSampleWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}