using System.Threading.Tasks;

namespace UserManagement
{
    public interface IDbAdapter
    {
        UserProfile GetUser(int user_id);
        UserProfile GetUser(string username);
        int DeleteUser(int user_id);
        UserPrefs GetUserPrefs(int user_id);
        Task<int> EditUserPrefsAsync(int user_id, dynamic userPrefs);
        Task<int> EditUserAsync(int user_id, dynamic userPrefs);
        Task<int> CreateUserAsync(dynamic data);
    }
}
