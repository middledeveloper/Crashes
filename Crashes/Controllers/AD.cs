using System.DirectoryServices.AccountManagement;

namespace Crashes.Controllers
{
    public class AD
    {
        public static string GetName(string login)
        {
            var ctx = new PrincipalContext(ContextType.Domain);
            var user = UserPrincipal.FindByIdentity(ctx, login);

            if (user != null) return user.DisplayName;
            else return login;
        }
    }
}