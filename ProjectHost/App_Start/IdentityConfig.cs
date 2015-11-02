using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using ProjectHost.Models;

namespace ProjectHost
{
    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    // This is useful if you do not want to tear down the database each time you run the application.
    // You want to create a new database if the Model changes
    //public class ApplicationDbContextInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    public class ApplicationDbContextInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            //InitializeIdentityForEF(context);
            base.Seed(context);
        }

        private void InitializeIdentityForEF(ApplicationDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var myinfo = new MyUserInfo() { FirstName = "*", LastName = "*" };
            string name = "*";
            string password = "*";

            //Create Role Test and User Test
            roleManager.Create(new IdentityRole("*"));
            userManager.Create(new ApplicationUser() { UserName = "*", });

            //Create Role Admin if it does not exist
            if (!roleManager.RoleExists(name))
            {
                var roleresult = roleManager.Create(new IdentityRole(name));
            }

            //Create User=Admin with password=123456
            var user = new ApplicationUser
            {
                UserName = name,
                HomeTown = "*",
                MyUserInfo = myinfo
            };

            var adminresult = userManager.Create(user, password);

            //Add User Admin to Role Admin
            if (adminresult.Succeeded)
            {
                var result = userManager.AddToRole(user.Id, name);
            }
        }
    }
}
