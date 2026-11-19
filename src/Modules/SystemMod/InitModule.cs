using EntityFramework.AppDbContext;

namespace SystemMod;

public static class InitModule
{
    public static async Task InitializeAsync(
        DefaultDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (await context.SysUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        const string defaultPassword = "Perigon.2026";
        SysRole superRole = new()
        {
            Name = WebConst.SuperAdmin,
            NameValue = WebConst.SuperAdmin,
            IsSystem = true,
        };
        SysRole adminRole = new()
        {
            Name = WebConst.AdminUser,
            NameValue = WebConst.AdminUser,
            IsSystem = true,
        };
        const string email = "admin@default.com";
        string salt = HashCrypto.BuildSalt();
        SysUser adminUser = new()
        {
            UserName = "admin",
            Email = email,
            PasswordSalt = salt,
            PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
        };

        await context.TransactionAsync(async transactionCancellationToken =>
        {
            await context.SysRoles.BulkInsertAsync(
                [superRole, adminRole],
                cancellationToken: transactionCancellationToken);
            await context.SysUsers.InsertAsync(
                adminUser,
                cancellationToken: transactionCancellationToken);
            await context.SysUserRoles.BulkInsertAsync(
                [
                    new SysUserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = superRole.Id,
                    },
                    new SysUserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                    },
                ],
                cancellationToken: transactionCancellationToken);
        }, cancellationToken);
    }
}
