namespace BIMS.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {

            List<UsersList> users = new()
            {
                new UsersList
                {
                    Password = "P@ssword123",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "superadmin",
                        Email = "superadmin@BIMS.com",
                        FullName = "Super Admin User",
                        EmailConfirmed = true,
                    },
                    Roles =
                    {
                        AppRoles.Admin,
                        AppRoles.Reception,
                        AppRoles.Archive,
                    }
                },
                new UsersList
                {
                    Password = "P@ssword123",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@BIMS.com",
                        FullName = "Admin User",
                        EmailConfirmed = true,
                    },
                    Roles =
                    {
                        AppRoles.Admin,
                    }
                },
                new UsersList
                {
                    Password = "P@ssword123",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "reception",
                        Email = "reception@BIMS.com",
                        FullName = "Reception User",
                        EmailConfirmed = true,
                    },
                    Roles =
                    {
                        AppRoles.Reception,
                    }
                },
                new UsersList
                {
                    Password = "P@ssword123",
                    ApplicationUser = new ApplicationUser
                    {
                        UserName = "archive",
                        Email = "archive@BIMS.com",
                        FullName = "Archive User",
                        EmailConfirmed = true,
                    },
                    Roles =
                    {
                        AppRoles.Archive,
                    }
                },
            };

            foreach (var user in users)
            {
                var applicationUser = await userManager.FindByNameAsync(user.ApplicationUser.UserName!);

                if (applicationUser is null)
                {
                    var result = await userManager.CreateAsync(user.ApplicationUser, user.Password);
                }

                foreach (var role in user.Roles)
                {
                    if (!await userManager.IsInRoleAsync(user.ApplicationUser!, role))
                        await userManager.AddToRoleAsync(user.ApplicationUser!, role);
                }
            }

            //ApplicationUser admin = new()
            //{
            //    UserName = "admin",
            //    Email = "admin@BIMS.com",
            //    FullName = "Admin",
            //    EmailConfirmed = true,
            //};

            //var user = await userManager.FindByNameAsync(admin.UserName);

            //if (user is null)
            //{
            //    await userManager.CreateAsync(admin, "P@ssword123");
            //    await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            //}

        }
    }
}
