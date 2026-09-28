using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Idsrv.Models;
using uBeac.Idsrv.Repositories;

namespace uBeac.Idsrv.Services
{
    public class UserStore :
        IUserStore<User>,
        IUserLoginStore<User>,
        IUserClaimStore<User>,
        IUserPasswordStore<User>,
        IUserSecurityStampStore<User>,
        IUserTwoFactorStore<User>,
        IUserEmailStore<User>,
        IUserLockoutStore<User>,
        IUserPhoneNumberStore<User>,
        IUserAuthenticatorKeyStore<User>,
        IUserAuthenticationTokenStore<User>,
        IUserTwoFactorRecoveryCodeStore<User>,
        IUserRoleStore<User>
    {

        private readonly IUserRepository _repository;

        public UserStore(IUserRepository repository)
        {
            _repository = repository;
        }
        
        public void Dispose()
        {
        }

        #region IUserStore Implementation

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            await _repository.InsertAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            user.IsDeleted = true;
            await _repository.UpdateAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(Guid.Parse(userId), cancellationToken);
            if (user != null && user.IsDeleted) return null;
            return user;
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var user = await _repository.FindByNameAsync(normalizedUserName, cancellationToken);
            if (user != null && user.IsDeleted) return null;
            return user;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("Changing the username is not supported.");
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            await _repository.UpdateAsync(user, cancellationToken);
            return IdentityResult.Success;
        }

        #endregion IUserStore Implementation

        #region IUserLoginStore Implementation
        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            // NOTE: Not the best way to ensure uniquness.
            if (user.Logins.Any(x => x.Equals(login)))
            {
                throw new InvalidOperationException("Login already exists.");
            }

            var newUserLogin = new UserLogin()
            {
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName,
                ProviderKey = login.ProviderKey,
                UserId = user.Id.ToString()
            };

            user.Logins.Add(newUserLogin);

            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var login = new UserLoginInfo(loginProvider, providerKey, string.Empty);
            var loginToRemove = user.Logins.FirstOrDefault(x => x.Equals(login));

            if (loginToRemove != null) user.Logins.Remove(loginToRemove);

            return Task.FromResult(0);

        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            var logins = user.Logins.Select(login =>
                new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName));

            return Task.FromResult<IList<UserLoginInfo>>(logins.ToList());
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var user = await _repository.FindByLoginAsync(loginProvider, providerKey);
            return user != null && user.IsDeleted ? null : user;

        }

        #endregion IUserLoginStore Implementation

        #region IUserClaimStore Implementation

        public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var claims = user.Claims.Select(clm => new Claim(clm.ClaimType, clm.ClaimValue)).ToList();

            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                var newUserClaim = new UserClaim() { UserId = user.Id.ToString() };
                newUserClaim.InitializeFromClaim(claim);
                user.Claims.Add(newUserClaim);
            }

            return Task.FromResult(0);
        }

        public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            var oldUserClaim = (from p in user.Claims where p.UserId == user.Id.ToString() && p.ClaimType == claim.Type && p.ClaimValue == claim.Value select p).SingleOrDefault();

            if (oldUserClaim != null) user.Claims.Remove(oldUserClaim);

            var newUserClaim = new UserClaim() { UserId = user.Id.ToString() };
            newUserClaim.InitializeFromClaim(claim);
            user.Claims.Add(newUserClaim);

            return Task.FromResult(0);
        }

        public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            foreach (var claim in claims)
            {
                var oldUserClaim = (from p in user.Claims where p.UserId == user.Id.ToString() && p.ClaimType == claim.Type && p.ClaimValue == claim.Value select p).SingleOrDefault();
                if (oldUserClaim != null) user.Claims.Remove(oldUserClaim);
            }

            return Task.FromResult(0);
        }

        public async Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            IList<User> users = await _repository.GetUsersForClaimAsync(claim.Type, claim.Value, cancellationToken);
            return users;
        }

        #endregion IUserClaimStore Implementation

        #region IUserPasswordStore Implementation

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        #endregion IUserPasswordStore Implementation

        #region IUserSecurityStampStore Implementation

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        #endregion IUserSecurityStampStore Implementation

        #region IUserTwoFactorStore Implementation

        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        #endregion IUserTwoFactorStore Implementation

        #region IUserEmailStore Implementation

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.Email))
                throw new InvalidOperationException("Cannot get the confirmation status of the e-mail since the user doesn't have an e-mail.");

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.Email))
                throw new InvalidOperationException("Cannot get the confirmation status of the e-mail since the user doesn't have an e-mail.");


            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = await _repository.FindByEmailAsync(normalizedEmail, cancellationToken);

            return user;
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        #endregion IUserEmailStore Implementation

        #region IUserLockoutStore Implementation

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.FromResult(0);
        }

        public async Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            var oldUser = await _repository.GetByIdAsync(user.Id);
            oldUser.AccessFailedCount += 1;
            await _repository.UpdateAsync(oldUser);

            return oldUser.AccessFailedCount;
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        #endregion IUserLockoutStore Implementation

        #region IUserPhoneNumberStore Implementation

        public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.PhoneNumber))
                throw new InvalidOperationException("Cannot get the confirmation status of the phone number since the user doesn't have a phone number.");

            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.PhoneNumber))
                throw new InvalidOperationException("Cannot set the confirmation status of the phone number since the user doesn't have a phone number.");

            user.PhoneNumberConfirmed = confirmed;

            return Task.FromResult(0);
        }


        #endregion IUserPhoneNumberStore Implementation

        #region IUserAuthenticatorKeyStore Implementation

        public Task SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken)
        {
            return SetTokenAsync(user, Constants.INTERNAL_LOGIN_PROVIDER_NAME, Constants.AUTHENTICATOR_KEY_TOKEN_NAME, key, cancellationToken);
        }

        public Task<string> GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken)
        {
            return GetTokenAsync(user, Constants.INTERNAL_LOGIN_PROVIDER_NAME, Constants.AUTHENTICATOR_KEY_TOKEN_NAME, cancellationToken);
        }

        #endregion IUserAuthenticatorKeyStore Implementation

        #region IUserAuthenticationTokenStore Implementation
        public Task SetTokenAsync(User user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            var token = user.Tokens.SingleOrDefault(l => l.Name == name && l.LoginProvider == loginProvider && l.UserId == user.Id.ToString());
            if (token != null)
            {
                token.Value = value;
            }
            else
            {
                user.Tokens.Add(new UserToken
                {
                    UserId = user.Id.ToString(),
                    LoginProvider = loginProvider,
                    Name = name,
                    Value = value
                });
            }
            return Task.FromResult(0);
        }

        public Task RemoveTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var token = user.Tokens.SingleOrDefault(l => l.Name == name && l.LoginProvider == loginProvider && l.UserId == user.Id.ToString());

            if (token != null) user.Tokens.Remove(token);

            return Task.FromResult(0);
        }

        public Task<string> GetTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var token = user.Tokens.SingleOrDefault(l => l.Name == name && l.LoginProvider == loginProvider && l.UserId == user.Id.ToString());
            return Task.FromResult(token?.Value);
        }

        #endregion IUserAuthenticationTokenStore Implementation

        #region IUserTwoFactorRecoveryCodeStore Implementation

        public Task ReplaceCodesAsync(User user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            var mergedCodes = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, Constants.INTERNAL_LOGIN_PROVIDER_NAME, Constants.RECOVERY_CODE_TOKEN_NAME, mergedCodes, cancellationToken);
        }

        public async Task<bool> RedeemCodeAsync(User user, string code, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, Constants.INTERNAL_LOGIN_PROVIDER_NAME, Constants.RECOVERY_CODE_TOKEN_NAME, cancellationToken) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<int> CountCodesAsync(User user, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, Constants.INTERNAL_LOGIN_PROVIDER_NAME, Constants.RECOVERY_CODE_TOKEN_NAME, cancellationToken) ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }
            return 0;
        }

        #endregion IUserTwoFactorRecoveryCodeStore Implementation

        #region IUserRoleStore Implementation

        public Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            if (!user.Roles.Contains(roleName)) user.Roles.Add(roleName);
            return Task.FromResult(0);
        }

        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            user.Roles.Remove(roleName);
            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            IList<string> roles = user.Roles;
            return Task.FromResult(roles);
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Roles.Contains(roleName));
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            return await _repository.GetUsersInRoleAsync(roleName, cancellationToken);
        }

        #endregion IUserRoleStore Implementation
    }
}
