using Microsoft.AspNetCore.Authorization;

namespace KXlsxConverterAPI.AuthHandlers;

public class AdminOrBelongsToStoreHandler : AuthorizationHandler<AdminOrBelongsToStoreRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOrBelongsToStoreRequirement requirement)
    {
        // Check if the user has the "Admin" role
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if the user meets the "BelongsToStore" condition
        if (context.User.HasClaim(c => c.Type == "StoreNumber")) // Example claim check
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class AdminOrBelongsToStoreRequirement : IAuthorizationRequirement { }