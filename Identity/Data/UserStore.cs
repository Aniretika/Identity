using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ObjectRelationMapping.Repository;
using ObjectRelationMapping.UnitOfWork;

namespace Identity.Models.Identity
{
    public class UserStore : IUserStore<ApplicationUser>,
        IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
    {
        private readonly string connectionString;
        private readonly UnitOfWork dataProvider;
        public UserStore(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IdentityResult identityResult = new();
            using (var connection = new SqlConnection(connectionString))
            {
                dataProvider.Context = connection;
                var result = dataProvider.GetRepository<ApplicationUser>().Add(user);
                if (result >= 0)
                {
                    identityResult = IdentityResult.Success;
                }
                else
                {
                    identityResult = IdentityResult.Failed();
                }
            }

            return Task.FromResult(identityResult);
        }

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IdentityResult identityResult = new();
            using (var connection = new SqlConnection(connectionString))
            {
                dataProvider.Context = connection;
                var result = dataProvider.GetRepository<ApplicationUser>().Delete(user.ID);
                if (result >= 0)
                {
                    identityResult = IdentityResult.Success;
                }
                else
                {
                    identityResult = IdentityResult.Failed();
                }
            }

            return Task.FromResult(identityResult);
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            int idUser = 0;
            ApplicationUser applicationUser = new();
            try
            {
                idUser = Int32.Parse(userId);
            }
            catch (FormatException)
            {
                throw new Exception($"Unable to parse '{idUser}'");
            }

            using (var connection = new SqlConnection(connectionString))
            {
                dataProvider.Context = connection;

                return await Task.Run(() => dataProvider.GetRepository<ApplicationUser>().GetById(idUser));

            }
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.ID.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IdentityResult identityResult = new();
            using (var connection = new SqlConnection(connectionString))
            {
                dataProvider.Context = connection;
                var result = dataProvider.GetRepository<ApplicationUser>().Update(user);
                if (result >= 0)
                {
                    identityResult = IdentityResult.Success;
                }
                else
                {
                    identityResult = IdentityResult.Failed();
                }
            }

            return Task.FromResult(identityResult);
        }

        public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }


        public void Dispose()
        {
            // Nothing to dispose.
        }

        public Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
