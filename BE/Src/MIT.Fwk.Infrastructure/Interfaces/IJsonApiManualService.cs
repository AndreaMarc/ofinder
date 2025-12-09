using JsonApiDotNetCore.Resources;
using MIT.Fwk.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IJsonApiManualService
    {
        #region Generic CRUD Methods

        /// <summary>
        /// Metodo per ottenere tutti gli elementi di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        IEnumerable<T> GetAll<T, TId>()
            where T : Identifiable<TId>
            where TId : IConvertible;

        /// <summary>
        /// Metodo per ottenere un elemento di un determinato tipo dato il suo id
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        public T GetById<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible;

        /// <summary>
        /// Metodo per creare un elemento di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        public T Create<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible;

        /// <summary>
        /// Metodo per eliminare un elemento di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        public T Delete<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible;

        /// <summary>
        /// Metodo per aggiornare un elemento di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        public T Update<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible;



        /// <summary>
        /// Metodo per ottenere tutti gli elementi di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        IQueryable<T> GetAllQueryable<T, TId>()
            where T : Identifiable<TId>
            where TId : IConvertible;

        /// <summary>
        /// Metodo per eliminare tutti gli elementi di un determinato tipo
        /// </summary>
        /// <typeparam name="T">Tipo dell'entità jsonApi del dbContext da recuperare</typeparam>
        /// <typeparam name="TId">Tipo dell'id dell'entità jsonApi del dbContext da recuperare</typeparam>
        bool DeleteRange<T, TId>(IQueryable<T> entities)
            where T : Identifiable<TId>
            where TId : IConvertible;

        // New Generic Async Methods
        Task<List<T>> GetAllAsync<T, TId>() where T : Identifiable<TId> where TId : IConvertible;
        Task<T> GetByIdAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;
        Task<T> CreateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;
        Task<T> UpdateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;
        Task<T> DeleteAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;
        Task<bool> DeleteRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        // Predicate Methods
        T FirstOrDefault<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;
        Task<T> FirstOrDefaultAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;
        IQueryable<T> Where<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;
        Task<List<T>> WhereAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;

        // Query Methods
        bool Any<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;
        Task<bool> AnyAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;
        int Count<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;
        Task<int> CountAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;

        // Batch Methods
        Task<List<T>> AddRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;
        Task<List<T>> UpdateRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        #endregion

        Task<string> NewSalt();

        Tuple<Type, Type> GetEntityTypeAndIdType(string entityName);


        Task<BannedUser> IsBanned(string userId, int? tenantId = null);
        Task<List<MediaFile>> FindAllMediaInMediaCategories(List<MediaCategory> mediaCategories);
        Task<string> CalculateMD5Hash(string input);
        List<Role> GetAllRolesByRoleIdList(string roles);
        List<Role> GetAllRolesByTenant(int tenantId);
        List<RoleClaim> GetAllRoleClaimsInRoles(List<Role> roles);
        Task<List<Category>> GetChildrenCategories(Category category);
        Task<List<MediaCategory>> GetChildrenMediaCategories(MediaCategory category);
        bool ExecuteQueryToOdbc(string connectionString, string query, List<object> parameters);
        IEnumerable<DateTime> GetGiorniFestivitaAnnue(int anno);

        Task DisconnectOneDevice(string userId, string deviceHash);











        Task<List<Tenant>> GetAllChildrenTenants(Tenant tenant);
        Task<List<Tenant>> GetAllChildrenTenants(int tenantID);
        Task<bool> CheckIfTenantIsChild(int parentTenant, int childTenant, bool recursive);



        Task<List<Role>> GetAllRolesByNames(List<string> rolesName);

        Task<List<RoleClaim>> SetRoleClaims(List<RoleClaim> entity, string roleId, bool isCrudType);

        Task<List<RoleClaim>> UpdateRoutesRole(List<RoleClaim> entity);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByGoogleEmail(string email);
        Task<Otp> GenerateNewOtp(string userId, string otpValue, int tenantId);
        Task<string> Encrypt(string plainText, string encryptionKey);
        Task<string> Decrypt(string encryptedString, string encryptionKey);
        Task<ThirdPartsToken> GetExistingThirdPartAssociation(string email);
        Task<User> GetUserById(string id);
        Task<bool> CheckIsSuperadmin(string id);
        Task<bool> CheckIsOwner(string id);
        Task<List<UserRole>> GetAllUserRolesByEmail(string email);
        Task<List<Category>> GetAllCategoriesByTenantId(int tenantId);
        Task<List<MediaCategory>> GetAllMediaCategoriesByTenantId(int tenantId);
        Task<Template> getTemplateByCodeAndLanguage(string code, string language, int tenantId);
        Task<LegalTerm> GetLegalTermByKeyLCV(string language, string code, string version);
        Task<List<Template>> GetAllTemplatesByCategories(string categoriesId);

        Task<List<Template>> GetAllSystemTemplates();

        Task<List<Template>> GetAllTemplatesByCategories(List<Category> categories);
        Task<bool> DeleteAllTemplatesByCategoryId(int categoryId);
        Task<bool> MoveAllToOtherMediaCategory(string oldCategoryId, string newCategoryId);
        Task<bool> DeleteMediaCategoryRecursively(string categoryId, IDocumentService _docService);
        Task<bool> SetLegalTerms(string id);
        void MoveFile(string oldPath, string newPath, MediaFile file);

        Task<bool> DeleteTenantReferiments(int id, int alternativeTenant = 0);
        Task<bool> setPasswordTryTo0(string userId);
        Task<bool> MoveAllTemplatesByCategoryId(int categoryId, int alternativeCategoryId);

        Task<List<UserDevice>> GetAllUserDevices(string userId);
        Task<List<User>> GetAllUsers(List<string> userIds = null);

        Task<UserDevice> GetUserDevice(string userId, string deviceHash);

        Task<UserDevice> CreateUserDevice(UserDevice userDevice);

        Task<UserDevice> UpdateUserDevice(UserDevice ud);

        Task<bool> ExistClaim(string userEmail, string claim, string tenantId);
        Task<bool> SetUserCurrentTenant(string tenantId, string userId);

        Task<List<Tenant>> GetTenantsByUsername(string username);
        Task<List<Tenant>> GetNonBlockedTenantsByUserId(string userId);
        Task<UserTenant> GetUserTenantById(string userTenantId);
        Task<UserProfile> GetUserProfileByUsername(string username);

        Task<UserProfile> UpdateUserProfile(UserProfile up);

        Task<UserProfile> GetUserProfileById(string id);
        Task<string> GetClaimsByUsername(string username, string tenantId);
        List<string> GetClaimsPoolByUsername(string username, string tenantId);
        Task<bool> CheckClaimsById(string id, string tenantId, List<string> claims);
        Task<UserTenant> RegisterUserTenant(string userId, int tenantId, string ip, string state);
        Task<UserTenant> UpdateUserTenant(UserTenant ut);
        Task<UserTenant> RegisterUserTenantPending(string userId, int tenantId);
        Task<UserRole> RegisterUserRole(string userId, int tenantId, string roleName);
        Task<List<RoleClaim>> GetAllClaimsByRole(int tenantId, string roleName);
        Task<List<RoleClaim>> GetAllClaimsByRoleId(string roleId);
        List<UserTenant> GetAllUserTenantsByUserId(string userId);
        UserTenant GetUserTenant(string userId, int tenantId);

        Task<UserRole> DeleteUserRole(string userId, int tenantId, string roleName);
        Task<List<Role>> RolesFromParent(Tenant tenant);

        List<Role> RolesFromTenant(int tenant);
        Task<Tenant> CreateTenant(Tenant tenant, bool copyFromParent);
        Task CopyCategoryTreeRecursive(IEnumerable<Category> categoriesToCopy, int parentId, int newTenantId,
            int oldTenantId);
        Task CopyMediaCategoryTreeRecursive(IEnumerable<MediaCategory> categoriesToCopy, string parentId,
            int newTenantId, int oldTenantId);
        Task<bool> SendOtpEmail(string userTenantId, string SetupType, string baseEndpoint, string templateCode = "em.10", Dictionary<string, string> customContent = null);
        Task<bool> SendOtpEmail(string userName, string otp, string platform, int tenantId, string baseEndpoint, string templateId, Dictionary<string, string> customContent = null);
        Task<bool> SendGenericEmail(string userName, string templateCode, int tenantId, string baseEndpoint, Dictionary<string, string> customContent = null);
        Task<bool> SendGenericEmailToNonRegisteredEmail(string email, string templateCode, int tenantId, string baseEndpoint, Dictionary<string, string> customContent = null);

        Task<User> DeleteUser(string id);
        Task<User> DeleteUserLite(string id);




        Task<PasswordHistory> CreatePasswordHistory(string userId, string userEmail, string oldPasswordHash, DateTime validFrom);

        Task<List<PasswordHistory>> GetUserPasswordHistory(string userId, int number = 1000);
        Task<LegalTerm> Activation(string id);
        Task<List<string>> GetAllUsersGuidInTenant(int tenantId);
        List<string> GetAllUsersGuidInRole(string roleGuid);
        List<string> GetAllUsersGuidFromClaimName(string claimName);
        Task<bool> CheckEmailBlock(string id, int tenantId);
        Task<bool> CheckPushBlock(string id, int tenantId);
    }
}
