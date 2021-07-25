using System;
using System.Threading.Tasks;

namespace Authentication
{
    public interface IDbAdapter
    {
        public int GetIDByUsername(String username);
        public string GetStatusByID(int user_id);
        public string LookupPasswordByUserId(int user_id);
        public Task<int> CreateCredentialsEntry(int userId, string password);
        public Task<int> UpdateCredentialsEntry(int userId, string password);
        public bool DoesIdExist(int userId);
    }
}
