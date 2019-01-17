using System;
using Abp;
using Abp.Authorization;
using Abp.Dependency;
using Abp.UI;

namespace AngularBPWorkflow.Authorization
{
    public class AbpLoginResultTypeHelper : AbpServiceBase, ITransientDependency
    {
        public AbpLoginResultTypeHelper()
        {
            LocalizationSourceName = AngularBPWorkflowConsts.LocalizationSourceName;
        }

        public Exception CreateExceptionForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    return new Exception("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return new UserFriendlyException(L("LoginFailed"), L("InvalidUserNameOrPassword"));
                case AbpLoginResultType.InvalidTenancyName:
                    return new UserFriendlyException(L("LoginFailed"), L("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                case AbpLoginResultType.TenantIsNotActive:
                    return new UserFriendlyException(L("LoginFailed"), L("TenantIsNotActive", tenancyName));
                case AbpLoginResultType.UserIsNotActive:
                    return new UserFriendlyException(L("LoginFailed"), L("UserIsNotActiveAndCanNotLogin", usernameOrEmailAddress));
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return new UserFriendlyException(L("LoginFailed"), L("UserEmailIsNotConfirmedAndCanNotLogin"));
                case AbpLoginResultType.LockedOut:
                    return new UserFriendlyException(L("LoginFailed"), L("UserLockedOutMessage"));
                default: // Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return new UserFriendlyException(L("LoginFailed"));
            }
        }

        public string CreateLocalizedMessageForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    throw new Exception("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return L("InvalidUserNameOrPassword");
                case AbpLoginResultType.InvalidTenancyName:
                    return L("ThereIsNoTenantDefinedWithName{0}", tenancyName);
                case AbpLoginResultType.TenantIsNotActive:
                    return L("TenantIsNotActive", tenancyName);
                case AbpLoginResultType.UserIsNotActive:
                    return L("UserIsNotActiveAndCanNotLogin", usernameOrEmailAddress);
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return L("UserEmailIsNotConfirmedAndCanNotLogin");
                default: // Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return L("LoginFailed");
            }
        }
    }
}
