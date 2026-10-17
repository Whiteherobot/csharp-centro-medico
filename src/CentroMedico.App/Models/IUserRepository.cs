using System.Net;

namespace CentroMedico.App.Models
{
    public interface IUserRepository
    {
        bool AuthenticateUser(NetworkCredential credential);
        void Add(UserModel userModel);
        UserModel GetByUsername(string username);
        //...
    }
}
