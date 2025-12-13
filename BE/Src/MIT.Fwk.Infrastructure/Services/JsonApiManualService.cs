using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;



namespace MIT.Fwk.Infrastructure.Services
{
    //questo serve per fare operazioni di crud su entita che hanno controller jsonapi, utilizzando il dbcontext
    //ad esempio se sto su un controller dell'entita A e faccio un override, con questo posso fare crud anche sull'entita B
    public class JsonApiManualService : IJsonApiManualService
    {
        private readonly JsonApiDbContext _context;
        private readonly IEmailSender _mail;
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;
        public readonly string _allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public JsonApiManualService(
            JsonApiDbContext context,
            IEmailSender mail,
            IConfiguration configuration,
            ILogService logService = null)
        {
            _context = context;
            _context.Database.SetCommandTimeout(3600);
            _mail = mail;
            _configuration = configuration;
            _logService = logService;
        }

        public IEnumerable<T> GetAll<T, TId>()
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsNoTracking();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting all {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public IQueryable<T> GetAllQueryable<T, TId>()
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return _context.Set<T>();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting queryable {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public T GetById<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsNoTracking().FirstOrDefault(x => x.Id.Equals(id));
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting {typeof(T).Name} by id {id}: {ex.Message}");
                return null;
            }
        }

        public T Create<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Set<T>().Add(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error creating {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public T Update<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Set<T>().Update(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error updating {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public T Delete<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                T entity = _context.Set<T>().FirstOrDefault(x => x.Id.Equals(id));
                if (entity != null)
                {
                    _context.Set<T>().Remove(entity);
                    _context.SaveChanges();
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error deleting {typeof(T).Name} with id {id}: {ex.Message}");
                return null;
            }
        }

        public bool DeleteRange<T, TId>(IQueryable<T> entities)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                if (entities == null || !entities.Any())
                {
                    return true;
                }

                _context.ChangeTracker.Clear();
                _context.Set<T>().RemoveRange(entities);
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error deleting range of {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        #region New Generic Async Methods

        public async Task<List<T>> GetAllAsync<T, TId>()
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting all {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public async Task<T> GetByIdAsync<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting {typeof(T).Name} by id {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<T> CreateAsync<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                await _context.Set<T>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error creating {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public async Task<T> UpdateAsync<T, TId>(T entity)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error updating {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public async Task<T> DeleteAsync<T, TId>(TId id)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                _context.ChangeTracker.Clear();
                T entity = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                if (entity != null)
                {
                    _context.Set<T>().Remove(entity);
                    await _context.SaveChangesAsync();
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error deleting {typeof(T).Name} with id {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteRangeAsync<T, TId>(IEnumerable<T> entities)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                if (entities == null || !entities.Any())
                {
                    return true;
                }

                _context.ChangeTracker.Clear();
                _context.Set<T>().RemoveRange(entities);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error deleting range of {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        public T FirstOrDefault<T, TId>(Expression<Func<T, bool>> predicate)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsNoTracking().FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting first {typeof(T).Name} with predicate: {ex.Message}");
                return null;
            }
        }

        public async Task<T> FirstOrDefaultAsync<T, TId>(Expression<Func<T, bool>> predicate)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting first {typeof(T).Name} with predicate (async): {ex.Message}");
                return null;
            }
        }

        public IQueryable<T> Where<T, TId>(Expression<Func<T, bool>> predicate)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().Where(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error querying {typeof(T).Name} with predicate: {ex.Message}");
                return null;
            }
        }

        public async Task<List<T>> WhereAsync<T, TId>(Expression<Func<T, bool>> predicate)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().Where(predicate).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error querying {typeof(T).Name} with predicate (async): {ex.Message}");
                return null;
            }
        }

        public bool Any<T, TId>(Expression<Func<T, bool>> predicate = null)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return predicate == null ? _context.Set<T>().Any() : _context.Set<T>().Any(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error checking existence of {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AnyAsync<T, TId>(Expression<Func<T, bool>> predicate = null)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return predicate == null ? await _context.Set<T>().AnyAsync() : await _context.Set<T>().AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error checking existence of {typeof(T).Name} (async): {ex.Message}");
                return false;
            }
        }

        public int Count<T, TId>(Expression<Func<T, bool>> predicate = null)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return predicate == null ? _context.Set<T>().Count() : _context.Set<T>().Count(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error counting {typeof(T).Name}: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> CountAsync<T, TId>(Expression<Func<T, bool>> predicate = null)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                return predicate == null ? await _context.Set<T>().CountAsync() : await _context.Set<T>().CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error counting {typeof(T).Name} (async): {ex.Message}");
                return 0;
            }
        }

        public async Task<List<T>> AddRangeAsync<T, TId>(IEnumerable<T> entities)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                if (entities == null || !entities.Any())
                {
                    return new List<T>();
                }

                _context.ChangeTracker.Clear();
                await _context.Set<T>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error adding range of {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<T>> UpdateRangeAsync<T, TId>(IEnumerable<T> entities)
            where T : Identifiable<TId>
            where TId : IConvertible
        {
            try
            {
                if (entities == null || !entities.Any())
                {
                    return new List<T>();
                }

                _context.ChangeTracker.Clear();
                _context.Set<T>().UpdateRange(entities);
                await _context.SaveChangesAsync();

                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error updating range of {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        #endregion

        public Task<string> NewSalt()
        {
            Random random = new();
            string randomString = new([.. Enumerable.Repeat(_allowedChars, 32).Select(s => s[random.Next(s.Length)])]);
            return Task.FromResult(randomString);
        }

        public Tuple<Type, Type> GetEntityTypeAndIdType(string entityName)
        {
            try
            {
                Type entityType = _context.Model.GetEntityTypes().First(x => x.ClrType.Name == entityName).ClrType;
                Type idType = entityType.GetProperty("Id").PropertyType;
                return new Tuple<Type, Type>(entityType, idType);
            }
            catch
            {
                return null;
            }
        }

        public async Task DisconnectOneDevice(string userId, string deviceHash)
        {
            UserDevice device = (await GetAllUserDevices(userId)).FirstOrDefault(x => x.deviceHash == deviceHash);

            if (device == null)
            {
                return;
            }

            device.salt = await NewSalt();
            device.PushToken = null;
            device.AppleToken = null;
            device.GoogleToken = null;
            device.FacebookToken = null;
            device.TwitterToken = null;
            await UpdateUserDevice(device);
        }

        public async Task<List<MediaFile>> FindAllMediaInMediaCategories(List<MediaCategory> mediaCategories)
        {
            try
            {
                var albumsToTake = mediaCategories
                    .Where(x => x.Type == "album")
                    .Select(y => y.Id)
                    .ToList();

                if (!albumsToTake.Any())
                    return new List<MediaFile>();

                return await _context.MediaFiles
                    .Where(x => albumsToTake.Contains(x.album))
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error finding media files in categories: {ex.Message}");
                return new List<MediaFile>();
            }
        }

        public Task<string> CalculateMD5Hash(string input)
        {
            // Crea un'istanza dell'algoritmo di hash MD5
            // Converte la stringa di input in un array di byte
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Calcola l'hash MD5 dell'array di byte
            byte[] hashBytes = MD5.HashData(inputBytes);

            // Converte l'hash in una stringa esadecimale
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return Task.FromResult(sb.ToString());
        }



        public async Task<BannedUser> IsBanned(string userId, int? tenantId = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var bannedUserRows = await _context.BannedUsers
                    .Where(x => x.LockActive
                        && x.UserId == userId
                        && x.LockEnd > now
                        && x.LockStart < now)
                    .AsNoTracking()
                    .ToListAsync();

                if (!bannedUserRows.Any())
                    return null;

                return bannedUserRows.FirstOrDefault(x =>
                    tenantId != null ? x.TenantId == tenantId : x.CrossTenantBanned);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error checking ban status for user {userId}, tenant {tenantId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Category>> GetChildrenCategories(Category category)
        {
            try
            {
                // Load all categories once to avoid N+1 queries
                var allCategories = await _context.Categories
                    .AsNoTracking()
                    .ToListAsync();

                var children = new List<Category>();
                CollectChildrenCategoriesRecursive(category.Id, allCategories, children);
                return children;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting children categories for category {category?.Id}: {ex.Message}");
                return new List<Category>();
            }
        }

        private void CollectChildrenCategoriesRecursive(int parentId, List<Category> allCategories, List<Category> result)
        {
            var directChildren = allCategories.Where(x => x.ParentCategory == parentId).ToList();
            result.AddRange(directChildren);

            foreach (var child in directChildren)
            {
                CollectChildrenCategoriesRecursive(child.Id, allCategories, result);
            }
        }

        public async Task<bool> CheckEmailBlock(string userId, int tenantId)
        {
            try
            {
                return await _context.BlockNotifications
                    .Where(b => b.UserId == userId && b.TenantId == tenantId && b.EmailBlock != null)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error checking email block for user {userId}, tenant {tenantId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CheckPushBlock(string userId, int tenantId)
        {
            try
            {
                return await _context.BlockNotifications
                    .Where(b => b.UserId == userId && b.TenantId == tenantId && b.PushBlock != null)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error checking push block for user {userId}, tenant {tenantId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MediaCategory>> GetChildrenMediaCategories(MediaCategory category)
        {
            try
            {
                // Load all media categories once to avoid N+1 queries
                var allMediaCategories = await _context.MediaCategories
                    .AsNoTracking()
                    .ToListAsync();

                var children = new List<MediaCategory>();
                CollectChildrenMediaCategoriesRecursive(category.Id, allMediaCategories, children);
                return children;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error getting children media categories for category {category?.Id}: {ex.Message}");
                return new List<MediaCategory>();
            }
        }

        private void CollectChildrenMediaCategoriesRecursive(string parentId, List<MediaCategory> allMediaCategories, List<MediaCategory> result)
        {
            var directChildren = allMediaCategories.Where(x => x.ParentMediaCategory == parentId).ToList();
            result.AddRange(directChildren);

            foreach (var child in directChildren)
            {
                CollectChildrenMediaCategoriesRecursive(child.Id, allMediaCategories, result);
            }
        }

        public Task<List<Tenant>> GetAllChildrenTenants(Tenant tenant)
        {
            return Task.FromResult(_context.Tenants.Where(x => x.ParentTenant == tenant.Id).ToList());
        }

        public Task<List<Tenant>> GetAllChildrenTenants(int tenantID)
        {
            return Task.FromResult(_context.Tenants.Where(x => x.ParentTenant == tenantID).ToList());
        }

        public Task<Boolean> CheckIfTenantIsChild(int parentTenant, int childTenant, bool recursive)
        {
            return recursive
                ? Task.FromResult(CheckIfIsChildRecursive(parentTenant, childTenant))
                : Task.FromResult(_context.Tenants.Any(x => (x.ParentTenant == parentTenant) && (x.Id == childTenant)));
        }

        public bool CheckIfIsChildRecursive(int parentToFind, int childTenant)
        {
            Tenant tenantPadre = _context.Tenants.First(x => (x.Id == childTenant));
            return tenantPadre != null && (tenantPadre.Id == parentToFind || CheckIfIsChildRecursive(parentToFind, tenantPadre.ParentTenant));
        }

        public List<Role> GetAllRolesByTenant(int tenantId)
        {
            return [.. _context.Roles.Where(x => x.TenantId == tenantId || x.TenantId == 1)];
        }

        public List<Role> GetAllRolesByRoleIdList(string roles)
        {
            List<string> roleIds = [.. roles.Split(',')];
            return [.. _context.Roles.Where(x => roleIds.Contains(x.Id))];
        }

        public Task<List<Role>> GetAllRolesByNames(List<String> rolesName)
        {
            return Task.FromResult(_context.Roles.Where(x => rolesName.Contains(x.Name)).ToList());
        }

        public List<RoleClaim> GetAllRoleClaimsInRoles(List<Role> roles)
        {
            // Fix MySQL: Materializza prima gli ID per evitare type mapping error
            List<string> roleIds = roles.Select(x => x.Id).ToList();

            // Query separata per ogni role - MySQL gestisce facilmente query semplici con indici
            List<RoleClaim> allClaims = new List<RoleClaim>();
            foreach (string roleId in roleIds)
            {
                List<RoleClaim> claims = _context.RoleClaims
                    .Where(x => x.RoleId == roleId)
                    .ToList();
                allClaims.AddRange(claims);
            }
            return allClaims;
        }

        public Task<List<Role>> RolesFromParent(Tenant tenant)
        {
            return tenant.Id == 1
                ? Task.FromResult(new List<Role>())
                : Task.FromResult(_context.Roles.Where(x => x.TenantId == tenant.Id).ToList());
        }

        public List<Role> RolesFromTenant(int tenant)
        {
            return [.. _context.Roles.Where(x => x.TenantId == tenant)];
        }

        public async Task<Tenant> CreateTenant(Tenant entity, bool copyFromParent = true)
        {
            _context.ChangeTracker.Clear();
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Tenant> newTenant = _context.Tenants.Add(entity);
            if (copyFromParent)
            {
                Tenant parentTenant = _context.Tenants.FirstOrDefault(x => x.Id == (entity.ParentTenant < 1 ? 1 : entity.ParentTenant));

                if (parentTenant != null && parentTenant.Id != 1)
                {
                    List<Role> rolesFromParent = await RolesFromParent(parentTenant);

                    //copio tutti i ruoli di sistema e relativi claims
                    foreach (Role role in rolesFromParent)
                    {
                        if (role.CopyInNewTenants == false)
                        {
                            continue;
                        }

                        Role newRole = new() { Level = role.Level, Name = role.Name, CopyInNewTenants = role.CopyInNewTenants, Needful = role.Needful, TenantId = newTenant.Entity.Id, NormalizedName = role.NormalizedName, Id = Guid.NewGuid().ToString(), ConcurrencyStamp = Guid.NewGuid().ToString() };

                        await CreateAsync<Role, string>(newRole);

                        // Query separata per ogni role - MySQL gestisce facilmente query semplici
                        List<RoleClaim> roleClaims = await _context.RoleClaims
                            .Where(x => x.RoleId == role.Id)
                            .ToListAsync();

                        foreach (RoleClaim roleClaim in roleClaims)
                        {
                            RoleClaim newRoleClaim = new() { ClaimType = roleClaim.ClaimType, ClaimValue = roleClaim.ClaimValue, RoleId = newRole.Id };
                            await CreateAsync<RoleClaim, int>(newRoleClaim);
                        }
                    }
                }

                //copio tutte le categorie del padre
                IEnumerable<Category> categoriesToCopy = (await GetAllCategoriesByTenantId(parentTenant.Id)).Where(x => x.CopyInNewTenants && x.ParentCategory == 0);

                await CopyCategoryTreeRecursive([.. categoriesToCopy], 0, newTenant.Entity.Id, parentTenant.Id);

                IEnumerable<MediaCategory> mediaCategoriesToCopy = (await GetAllMediaCategoriesByTenantId(parentTenant.Id)).ToList().Where(x => x.copyInNewTenant && x.ParentMediaCategory == "");

                await CopyMediaCategoryTreeRecursive(mediaCategoriesToCopy, "", newTenant.Entity.Id, parentTenant.Id);

            }

            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task CopyCategoryTreeRecursive(IEnumerable<Category> categoriesToCopy, int parentId,
            int newTenantId, int oldTenantId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                foreach (Category category in categoriesToCopy)
                {
                    Category toWrite = new()
                    {
                        CopyInNewTenants = category.CopyInNewTenants,
                        TenantId = newTenantId,
                        Description = category.Description,
                        Erasable = category.Erasable,
                        Name = category.Name,
                        ParentCategory = parentId,
                        Type = category.Type,
                        Code = category.Code
                    };

                    Category newCategory = await CreateAsync<Category, int>(toWrite);

                    IEnumerable<Template> templatesToCopy = (await GetAllTemplatesByCategories(category.Id.ToString())).Where(x => x.CopyInNewTenants);

                    foreach (Template temp in from template in templatesToCopy
                                              where !template.Erased
                                              select new Template
                                              {
                                                  Id = Guid.NewGuid().ToString(),
                                                  Active = template.Active,
                                                  CategoryId = newCategory.Id,
                                                  Code = template.Code,
                                                  Content = template.Content,
                                                  ContentNoHtml = template.ContentNoHtml,
                                                  Description = template.Description,
                                                  Erasable = template.Erasable,
                                                  Name = template.Name,
                                                  Language = template.Language,
                                                  ObjectText = template.ObjectText,
                                                  Tags = template.Tags,
                                                  Order = template.Order,
                                                  FeaturedImage = template.FeaturedImage,
                                                  Erased = false,
                                                  CopyInNewTenants = template.CopyInNewTenants,
                                                  FreeField = template.FreeField,
                                              })
                    {
                        await CreateAsync<Template, string>(temp);
                    }

                    IEnumerable<Category> figlie = (await GetAllCategoriesByTenantId(oldTenantId)).Where(x => x.ParentCategory == category.Id && x.CopyInNewTenants);

                    if (figlie.Any())
                    {
                        await CopyCategoryTreeRecursive([.. figlie], newCategory.Id, newTenantId, oldTenantId);
                    }
                }

                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task CopyMediaCategoryTreeRecursive(IEnumerable<MediaCategory> categoriesToCopy, string parentId,
            int newTenantId, int oldTenantId)
        {
            try
            {
                _context.ChangeTracker.Clear();
                foreach (MediaCategory category in categoriesToCopy)
                {
                    string newId = Guid.NewGuid().ToString();

                    MediaCategory toWrite = new()
                    {
                        Id = newId,
                        copyInNewTenant = category.copyInNewTenant,
                        TenantId = newTenantId,
                        Description = category.Description,
                        Erasable = category.Erasable,
                        Name = category.Name,
                        ParentMediaCategory = parentId,
                        Type = category.Type,
                        Code = category.Code
                    };

                    MediaCategory newCategory = await CreateAsync<MediaCategory, string>(toWrite);

                    IEnumerable<MediaCategory> figlie = (await GetAllMediaCategoriesByTenantId(oldTenantId)).Where(x => x.ParentMediaCategory == category.Id && x.copyInNewTenant);

                    if (figlie.Any())
                    {
                        await CopyMediaCategoryTreeRecursive([.. figlie], newCategory.Id, newTenantId, oldTenantId);
                    }
                }

                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        public Task<User> GetUserByEmail(string email)
        {
            User res = _context.Users.FirstOrDefault(x => x.Email == email);
            if (res != null)
            {
                _context.Entry(res).Reload();
            }
            return Task.FromResult(res);
        }

        public Task<User> GetUserByGoogleEmail(string email)
        {
            ThirdPartsToken existingRow = _context.ThirdPartsTokens.FirstOrDefault(x => x.Email == email);

            return existingRow == null
                ? Task.FromResult<User>(null)
                : Task.FromResult(_context.Users.FirstOrDefault(x => x.Id == existingRow.UserId));
        }

        public async Task<Otp> GenerateNewOtp(string userId, string otpValue, int tenantId)
        {
            Otp newOtp = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                OtpValue = otpValue,
                TenantId = tenantId,
                CreationDate = DateTime.UtcNow,
                OtpSended = Guid.NewGuid().ToString(),
                IsValid = true
            };

            return await CreateAsync<Otp, string>(newOtp);
        }

        public Task<string> Encrypt(string plainText, string encryptionKey)
        {
            // Conversione della stringa di testo in chiaro in un array di byte
            byte[] cipherBytes = Encoding.Unicode.GetBytes(plainText);

            // Creazione di un oggetto AES per eseguire la cifratura
            using (Aes encryptor = Aes.Create())
            {
                // Derivazione della chiave di cifratura utilizzando Rfc2898DeriveBytes (metodo statico Pbkdf2)
                byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

                // Configurazione dell'encryptor con la chiave e il vettore di inizializzazione derivati
                encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, salt, 10000, HashAlgorithmName.SHA256, 32);
                encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, salt, 10000, HashAlgorithmName.SHA256, 16);

                // Creazione di un MemoryStream per contenere i byte cifrati
                using MemoryStream ms = new();
                // Creazione di un oggetto CryptoStream per eseguire la cifratura
                using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // Scrittura dei byte in chiaro nel CryptoStream per cifrarli
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                // Assegnazione dell'array di byte cifrati dalla MemoryStream a cipherBytes
                cipherBytes = ms.ToArray();
            }
            // Conversione dell'array di byte cifrati in una stringa Base64 e restituzione del risultato
            return Task.FromResult(WebEncoders.Base64UrlEncode(cipherBytes));
        }

        public Task<string> Decrypt(string encryptedString, string encryptionKey)
        {
            // Sostituzione degli spazi bianchi con "+" nella password cifrata
            string decryptedPassword;

            // Conversione della stringa cifrata in un array di byte
            byte[] cipherBytes = WebEncoders.Base64UrlDecode(encryptedString);

            // Creazione di un oggetto AES per eseguire la decifratura
            using (Aes encryptor = Aes.Create())
            {
                // Derivazione della chiave di cifratura utilizzando Rfc2898DeriveBytes (metodo statico Pbkdf2)
                byte[] salt = new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 };

                // Configurazione dell'encryptor con la chiave e il vettore di inizializzazione derivati
                encryptor.Key = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, salt, 10000, HashAlgorithmName.SHA256, 32);
                encryptor.IV = Rfc2898DeriveBytes.Pbkdf2(encryptionKey, salt, 10000, HashAlgorithmName.SHA256, 16);

                // Creazione di un MemoryStream per contenere i byte decifrati
                using MemoryStream ms = new();
                // Creazione di un oggetto CryptoStream per eseguire la decifratura
                using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    // Scrittura dei byte cifrati nel CryptoStream per decifrarli
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                // Conversione dei byte decifrati in una stringa e assegnazione alla variabile decryptedPassword
                decryptedPassword = Encoding.Unicode.GetString(ms.ToArray());
            }
            // Restituzione della password decifrata
            return Task.FromResult(decryptedPassword);
        }

        public Task<ThirdPartsToken> GetExistingThirdPartAssociation(string email)
        {
            return Task.FromResult(_context.ThirdPartsTokens.FirstOrDefault(x => x.Email == email));
        }

        public Task<User> GetUserById(string id)
        {
            User res = _context.Users.FirstOrDefault(x => x.Id == id);
            if (res != null)
            {
                _context.Entry(res).Reload();
            }
            return Task.FromResult(res);
        }

        public async Task<bool> CheckIsSuperadmin(string id)
        {
            User user = await GetUserById(id);

            List<UserRole> userRoles = (await GetAllUserRolesByEmail(user.Email))
                .Where(x => x.TenantId == 1)
                .ToList();

            if (!userRoles.Any()) return false;

            // Query separata per ogni role - MySQL gestisce facilmente query semplici
            foreach (var userRole in userRoles)
            {
                bool hasClaim = await _context.RoleClaims
                    .AnyAsync(x => x.RoleId == userRole.RoleId && x.ClaimValue == "isSuperAdmin");

                if (hasClaim) return true;
            }

            return false;
        }

        public async Task<bool> CheckIsOwner(string id)
        {
            User user = await GetUserById(id);

            List<UserRole> userRoles = (await GetAllUserRolesByEmail(user.Email))
                .Where(x => x.TenantId == 1)
                .ToList();

            if (!userRoles.Any()) return false;

            // Query separata per ogni role - MySQL gestisce facilmente query semplici
            foreach (var userRole in userRoles)
            {
                bool hasClaim = await _context.RoleClaims
                    .AnyAsync(x => x.RoleId == userRole.RoleId && x.ClaimValue == "isOwner");

                if (hasClaim) return true;
            }

            return false;
        }

        public Task<List<Category>> GetAllCategoriesByTenantId(int tenantId)
        {
            return Task.FromResult(_context.Categories.Where(x => x.TenantId == tenantId).ToList());
        }

        public Task<List<MediaCategory>> GetAllMediaCategoriesByTenantId(int tenantId)
        {
            return Task.FromResult(_context.MediaCategories.Where(x => x.TenantId == tenantId).ToList());
        }

        public Task<Template> getTemplateByCodeAndLanguage(string code, string language, int tenantId)
        {
            Template res = null;

            List<Template> templates = _context.Templates.Where(x => x.Code == code && x.Language == language).ToList();

            foreach (Template template in templates)
            {
                Category category = _context.Categories.FirstOrDefault(x => x.Id == template.CategoryId);

                if (category != null && category.TenantId == tenantId)
                {
                    res = template; break;
                }
            }

            return Task.FromResult(res);
        }

        public Task<LegalTerm> GetLegalTermByKeyLCV(string language, string code, string version)
        {
            return Task.FromResult(_context.LegalTerms.FirstOrDefault(x => x.Language == language && x.Code == code && x.Version == version));
        }

        public Task<List<Template>> GetAllTemplatesByCategories(string categoriesId)
        {
            List<string> categoriesIds = [.. categoriesId.Split(',')];
            return Task.FromResult(_context.Templates.Where(x => categoriesIds.Contains(x.CategoryId.ToString())).ToList());
        }

        public Task<List<Template>> GetAllSystemTemplates()
        {
            return Task.FromResult(_context.Templates.Where(x => !x.Erasable).ToList());
        }

        public async Task<List<Template>> GetAllTemplatesByCategories(List<Category> categories)
        {
            string categoriesId = string.Join(',', categories.Select(x => x.Id));
            return await GetAllTemplatesByCategories(categoriesId);
        }

        public async Task<bool> DeleteAllTemplatesByCategoryId(int categoryId)
        {
            IQueryable<Template> templates = _context.Templates.Where(x => x.CategoryId == categoryId);

            _context.Templates.RemoveRange(templates);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MoveAllToOtherMediaCategory(string oldCategoryId, string newCategoryId)
        {
            MediaCategory oldCategory = _context.MediaCategories.FirstOrDefault(x => x.Id == oldCategoryId);

            if (oldCategory is { Type: "album" })
            {
                List<MediaFile> filesToMove = _context.MediaFiles.Where(x => x.album == oldCategory.Id).ToList();

                foreach (MediaFile file in filesToMove)
                {
                    string oldPath = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{file.category}/{file.album}";
                    string newPath = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{file.category}/{newCategoryId}";

                    MoveFile(oldPath, newPath, file);

                    file.album = newCategoryId;
                    file.fileUrl = file.fileUrl.Contains("files") ? $"{newPath}/files"[1..] : newPath[1..];

                    _context.Entry(file).State = EntityState.Modified;
                }

                _context.MediaCategories.Remove(oldCategory);

                await _context.SaveChangesAsync();

                return true;
            }

            List<MediaCategory> directChildren = _context.MediaCategories.Where(x => x.ParentMediaCategory == oldCategory.Id).ToList();

            foreach (MediaCategory child in directChildren)
            {
                if (child.Type == "category")
                {
                    List<MediaCategory> albumDirectChildren = _context.MediaCategories.Where(x => x.ParentMediaCategory == child.Id).ToList();

                    foreach (MediaCategory albumDirectChild in albumDirectChildren)
                    {
                        List<MediaFile> filesToMove = _context.MediaFiles.Where(x => x.album == albumDirectChild.Id).ToList();

                        foreach (MediaFile file in filesToMove)
                        {
                            string oldPath = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{file.category}/{file.album}";
                            string newPath = $"{_configuration["UploadsPath"]}/{newCategoryId}/{file.category}/{file.album}";

                            MoveFile(oldPath, newPath, file);

                            file.typologyArea = newCategoryId;
                            file.fileUrl = file.fileUrl.Contains("files") ? $"{newPath}/files"[1..] : newPath[1..];

                            _context.Entry(file).State = EntityState.Modified;
                        }
                    }

                }
                else
                {
                    List<MediaFile> filesToMove = _context.MediaFiles.Where(x => x.album == child.Id).ToList();

                    foreach (MediaFile file in filesToMove)
                    {
                        string oldPath = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{file.category}/{file.album}";
                        string newPath = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{newCategoryId}/{file.album}";

                        MoveFile(oldPath, newPath, file);

                        file.category = newCategoryId;
                        file.fileUrl = file.fileUrl.Contains("files") ? $"{newPath}/files"[1..] : newPath[1..];

                        _context.Entry(file).State = EntityState.Modified;
                    }
                }

                child.ParentMediaCategory = newCategoryId;

                _context.Entry(child).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public void MoveFile(string oldPath, string newPath, MediaFile file)
        {
            if (File.Exists($"{oldPath}/big/{file.Id}.{file.extension}"))
            {
                if (!Directory.Exists($"{newPath}/big"))
                {
                    Directory.CreateDirectory($"{newPath}/big");
                }

                File.Move($"{oldPath}/big/{file.Id}.{file.extension}", $"{newPath}/big/{file.Id}.{file.extension}");
            }

            if (File.Exists($"{oldPath}/medium/{file.Id}.{file.extension}"))
            {
                if (!Directory.Exists($"{newPath}/medium"))
                {
                    Directory.CreateDirectory($"{newPath}/medium");
                }

                File.Move($"{oldPath}/medium/{file.Id}.{file.extension}", $"{newPath}/medium/{file.Id}.{file.extension}");
            }

            if (File.Exists($"{oldPath}/small/{file.Id}.{file.extension}"))
            {
                if (!Directory.Exists($"{newPath}/small"))
                {
                    Directory.CreateDirectory($"{newPath}/small");
                }

                File.Move($"{oldPath}/small/{file.Id}.{file.extension}", $"{newPath}/small/{file.Id}.{file.extension}");
            }

            if (File.Exists($"{oldPath}/files/{file.Id}.{file.extension}"))
            {
                if (!Directory.Exists($"{newPath}/files"))
                {
                    Directory.CreateDirectory($"{newPath}/files");
                }

                File.Move($"{oldPath}/files/{file.Id}.{file.extension}", $"{newPath}/files/{file.Id}.{file.extension}");
            }
        }

        public async Task<bool> DeleteMediaCategoryRecursively(string categoryId, IDocumentService _docService)
        {
            MediaCategory father = _context.MediaCategories.FirstOrDefault(x => x.Id == categoryId);

            List<MediaCategory> toDelete = await GetChildrenMediaCategories(father);

            toDelete.Add(father);

            foreach (MediaCategory item in toDelete)
            {
                if (item.Type == "album")
                {
                    List<MediaFile> filesToDelete = [.. _context.MediaFiles.Where(x => x.album == item.Id)];

                    foreach (MediaFile file in filesToDelete)
                    {
                        string path = $"{_configuration["UploadsPath"]}/{file.typologyArea}/{file.category}/{file.album}/";

                        if (File.Exists($"{path}/big/{file.Id}.{file.extension}"))
                        {
                            File.Delete($"{path}/big/{file.Id}.{file.extension}");
                        }

                        if (File.Exists($"{path}/medium/{file.Id}.{file.extension}"))
                        {
                            File.Delete($"{path}/medium/{file.Id}.{file.extension}");
                        }

                        if (File.Exists($"{path}/small/{file.Id}.{file.extension}"))
                        {
                            File.Delete($"{path}/small/{file.Id}.{file.extension}");
                        }

                        // FASE 8A: Use DocumentManager.DeleteAsync instead of obsolete Remove()
                        Dictionary<string, object> filter = new()
                        {
                            {"guids", new List<string> { file.mongoGuid }}
                        };

                        Core.Models.DocumentFile mongoRecord = _docService.GetAll(-10, filter).FirstOrDefault();

                        if (mongoRecord != null)
                        {
                            await Data.NoSql.Document.DocumentManager.DeleteAsync(mongoRecord);
                        }

                        _context.MediaFiles.Remove(file);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTenantReferiments(int id, int alternativeTenant = 0)
        {
            // Materializza query per evitare "open DataReader" error con MySQL
            List<User> users = _context.Users.Where(x => x.TenantId == id).ToList();

            Tenant recoveryTenant = _context.Tenants.FirstOrDefault(x => x.isRecovery);

            foreach (User user in users)
            {
                List<UserDevice> devices = await GetAllUserDevices(user.Id);

                foreach (UserDevice device in devices)
                {
                    device.salt = await NewSalt();
                    await UpdateUserDevice(device);
                }

                UserTenant userTenant = _context.UserTenants.FirstOrDefault(x => x.UserId == user.Id && x.TenantId != id);

                if (userTenant != null)
                {
                    user.TenantId = userTenant.TenantId;
                }
                else
                {
                    await RegisterUserTenant(user.Id, recoveryTenant.Id, "127.0.0.1", "ownerCreated");
                    await RegisterUserRole(user.Id, recoveryTenant.Id, "User");

                    user.TenantId = recoveryTenant.Id;
                }

                _context.Entry(user).State = EntityState.Modified;
            }

            List<Category> categories = _context.Categories.Where(x => x.TenantId == id).ToList();

            foreach (Category category in categories)
            {
                List<Template> templates = _context.Templates.Where(x => x.CategoryId == category.Id).ToList();

                _context.Templates.RemoveRange(templates);
            }

            _context.Categories.RemoveRange(categories);

            List<MediaCategory> mediaCategories = _context.MediaCategories.Where(x => x.TenantId == id).ToList();

            foreach (MediaCategory mediaCategory in mediaCategories)
            {
                List<MediaFile> files = _context.MediaFiles.Where(x => x.album == mediaCategory.Id || x.category == mediaCategory.Id || x.typologyArea == mediaCategory.Id).ToList();

                _context.MediaFiles.RemoveRange(files);
            }

            _context.MediaCategories.RemoveRange(mediaCategories);

            List<Role> roles = _context.Roles.Where(x => x.TenantId == id).ToList();

            // Fix MySQL: Materializza prima gli ID per evitare type mapping error
            List<string> roleIds = roles.Select(x => x.Id).ToList();
            List<RoleClaim> allClaims = new List<RoleClaim>();
            foreach (string roleId in roleIds)
            {
                List<RoleClaim> claims = _context.RoleClaims.Where(x => x.RoleId == roleId).ToList();
                allClaims.AddRange(claims);
            }

            foreach (Role role in roles)
            {
                List<UserRole> userRoles = _context.UserRoles.Where(x => x.RoleId == role.Id).ToList();

                _context.UserRoles.RemoveRange(userRoles);

                List<RoleClaim> roleClaims = allClaims.Where(x => x.RoleId == role.Id).ToList();

                _context.RoleClaims.RemoveRange(roleClaims);
            }

            _context.Roles.RemoveRange(roles);

            List<UserRole> userRolesToDelete = _context.UserRoles.Where(x => x.TenantId == id).ToList();
            _context.UserRoles.RemoveRange(userRolesToDelete);

            List<UserTenant> userTenants = _context.UserTenants.Where(x => x.TenantId == id).ToList();

            _context.UserTenants.RemoveRange(userTenants);

            List<Tenant> figli = _context.Tenants.Where(x => x.ParentTenant == id).ToList();

            foreach (Tenant tenant in figli)
            {
                if (alternativeTenant != 0)
                {
                    tenant.ParentTenant = alternativeTenant;
                    _context.Entry(tenant).State = EntityState.Modified;
                }
                else
                {
                    await DeleteTenantReferiments(tenant.Id);
                }
            }

            if (alternativeTenant == 0)
            {
                DeleteTenantChildsRecursive(id);
            }

            _context.SaveChanges();

            return true;
        }

        private bool DeleteTenantChildsRecursive(int tenantId)
        {
            IQueryable<Tenant> figli = _context.Tenants.Where(x => x.ParentTenant == tenantId);

            foreach (Tenant tenant in figli)
            {
                DeleteTenantChildsRecursive(tenant.Id);
                _context.Tenants.Remove(tenant);
                _context.SaveChanges();
            }

            return true;
        }

        public async Task<bool> setPasswordTryTo0(string userId)
        {
            User user = _context.Users.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                return false;
            }

            user.AccessFailedCount = 0;
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SetLegalTerms(string id)
        {
            LegalTerm legalTermToActive = _context.LegalTerms.FirstOrDefault(x => x.Id == id);

            if (legalTermToActive == null)
            {
                return false;
            }

            IQueryable<LegalTerm> legalTermsToDisactive = _context.LegalTerms.Where(x => x.Language == legalTermToActive.Language && x.Code == legalTermToActive.Code);

            foreach (LegalTerm lt in legalTermsToDisactive)
            {
                if (lt.Active)
                {
                    lt.Active = false;
                    _context.Entry(lt).State = EntityState.Modified;
                }
            }

            legalTermToActive.Active = true;
            legalTermToActive.DataActivation = DateTime.UtcNow;

            _context.Entry(legalTermToActive).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return true;
        }

        public Task<List<UserRole>> GetAllUserRolesByEmail(string email)
        {
            User user = _context.Users.FirstOrDefault(x => x.Email == email);

            return user == null
                ? Task.FromResult(new List<UserRole>())
                : Task.FromResult(_context.UserRoles.Where(x => x.UserId == user.Id).ToList());
        }

        public async Task<bool> MoveAllTemplatesByCategoryId(int categoryId, int alternativeCategoryId)
        {
            IQueryable<Template> templates = _context.Templates.Where(x => x.CategoryId == categoryId);

            foreach (Template template in templates)
            {
                template.CategoryId = alternativeCategoryId;
                _context.Entry(template).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public Task<List<UserDevice>> GetAllUserDevices(string userId)
        {
            return Task.FromResult(_context.UserDevices.Where(x => x.userId == userId).ToList());
        }

        public Task<List<User>> GetAllUsers(List<string> userIds = null)
        {
            return userIds != null
                ? Task.FromResult(_context.Users.Where(x => userIds.Contains(x.Id)).ToList())
                : Task.FromResult(_context.Users.ToList());
        }

        public List<UserTenant> GetAllUserTenantsByUserId(string userId)
        {
            return [.. _context.UserTenants.Where(x => x.UserId == userId)];
        }

        public UserTenant GetUserTenant(string userId, int tenantId)
        {
            return _context.UserTenants.FirstOrDefault(x => x.UserId == userId && x.TenantId == tenantId);
        }

        public Task<UserDevice> GetUserDevice(string userId, string deviceHash)
        {
            return Task.FromResult(_context.UserDevices.FirstOrDefault(x => x.userId == userId && x.deviceHash == deviceHash));
        }

        public async Task<List<RoleClaim>> SetRoleClaims(List<RoleClaim> rcl, string roleId, bool isCrudType)
        {
            IQueryable<RoleClaim> allClaimsInRole = _context.RoleClaims.Where(x => x.RoleId == roleId);

            foreach (RoleClaim roleToAdd in from rc in rcl
                                            where !allClaimsInRole.Any(x => x.ClaimValue == rc.ClaimValue)
                                            select new RoleClaim()
                                            {
                                                RoleId = roleId,
                                                ClaimValue = rc.ClaimValue,
                                                ClaimType = rc.ClaimType
                                            })
            {
                //creo il claim perche non c'era
                _context.RoleClaims.Add(roleToAdd);
            }

            foreach (RoleClaim rc in allClaimsInRole)
            {
                if (isCrudType)
                {
                    if (rc.ClaimType == "crud" && !rcl.Exists(x => x.ClaimValue == rc.ClaimValue))
                    {
                        //cancello il claim perche ora non c'è piu
                        _context.RoleClaims.Remove(rc);
                    }
                }
                else
                {
                    if (rc.ClaimType == "custom" && !rcl.Exists(x => x.ClaimValue == rc.ClaimValue))
                    {
                        //cancello il claim perche ora non c'è piu
                        _context.RoleClaims.Remove(rc);
                    }
                }

            }
            await _context.SaveChangesAsync();
            return await _context.RoleClaims.ToListAsync();
        }

        public async Task<List<RoleClaim>> UpdateRoutesRole(List<RoleClaim> rcl)
        {
            string roleId = rcl.First().RoleId;
            string claimType = rcl.First().ClaimType;
            IQueryable<RoleClaim> allClaimsInRole = _context.RoleClaims.Where(x => x.RoleId == roleId && x.ClaimType == claimType);

            foreach (RoleClaim rc in rcl)
            {
                if (!allClaimsInRole.Any(x => x.ClaimValue == rc.ClaimValue))
                {
                    //creo il claim perche non c'era
                    _context.RoleClaims.Add(rc);

                }

            }

            foreach (RoleClaim rc in allClaimsInRole)
            {
                if (!rcl.Exists(x => x.ClaimValue == rc.ClaimValue))
                {
                    //cancello il claim perche ora non c'è piu
                    _context.RoleClaims.Remove(rc);

                }

            }
            await _context.SaveChangesAsync();
            return await _context.RoleClaims.ToListAsync();

        }

        public async Task<UserDevice> CreateUserDevice(UserDevice userDevice)
        {
            await _context.UserDevices.AddAsync(userDevice);
            await _context.SaveChangesAsync();
            return userDevice;
        }

        public async Task<UserDevice> UpdateUserDevice(UserDevice ud)
        {
            _context.Entry(ud).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ud;
        }

        public async Task<bool> ExistClaim(string userEmail, string claim, string tenantId)
        {
            string claimPool = await GetClaimsByUsername(userEmail, tenantId);

            return claimPool.Contains(claim);
        }

        public async Task<bool> SetUserCurrentTenant(string tenantId, string userId)
        {
            User u = _context.Users.FirstOrDefault(x => x.Id == userId);
            u.TenantId = int.Parse(tenantId);

            _context.Entry(u).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<Tenant>> GetTenantsByUsername(string username)
        {
            User user = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null) { return Task.FromResult(new List<Tenant>()); }

            // Ottimizzazione: usa JOIN invece di loop + N query separate (fix open DataReader + N+1 problem)
            List<Tenant> tenants = _context.UserTenants
                .Where(x => x.UserId == user.Id && (x.State == "accepted" || x.State == "selfCreated" || x.State == "ownerCreated"))
                .Select(x => x.TenantId)
                .Distinct()
                .Join(_context.Tenants,
                      tenantId => tenantId,
                      tenant => tenant.Id,
                      (tenantId, tenant) => tenant)
                .ToList();

            return Task.FromResult(tenants);
        }

        public Task<List<Tenant>> GetNonBlockedTenantsByUserId(string userId)
        {
            // Materializza le query prima di usarle per evitare "open DataReader" error con MySQL
            List<int> bannedTenantsId = _context.BannedUsers
                .Where(x => x.UserId == userId && x.LockActive && x.LockEnd > DateTime.UtcNow && x.LockStart < DateTime.UtcNow)
                .Select(x => x.TenantId)
                .ToList();

            // Ottimizzazione: carica tenants con JOIN invece di N query separate (fix N+1 problem)
            List<Tenant> tenants = _context.UserTenants
                .Where(x => x.UserId == userId && (x.State == "accepted" || x.State == "selfCreated" || x.State == "ownerCreated"))
                .Where(x => !bannedTenantsId.Contains(x.TenantId))
                .Select(x => x.TenantId)
                .Distinct()
                .Join(_context.Tenants,
                      tenantId => tenantId,
                      tenant => tenant.Id,
                      (tenantId, tenant) => tenant)
                .ToList();

            return Task.FromResult(tenants);
        }

        public Task<string> GetClaimsByUsername(string username, string tenantId)
        {
            User user = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null) { return Task.FromResult(""); }

            IQueryable<string> roleIds = _context.UserRoles.Where(x => x.UserId == user.Id && (x.TenantId.ToString() == tenantId || x.TenantId == 1)).Select(x => x.RoleId).Distinct();
            IQueryable<string> claimNames = _context.RoleClaims.Where(x => roleIds.Contains(x.RoleId)).Select(x => x.ClaimValue).Distinct();

            return Task.FromResult(String.Join(',', claimNames));
        }


        public List<string> GetClaimsPoolByUsername(string username, string tenantId)
        {
            User user = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                return [];
            }

            IQueryable<string> roleIds = _context.UserRoles.Where(x => x.UserId == user.Id && (x.TenantId.ToString() == tenantId || x.TenantId == 1)).Select(x => x.RoleId).Distinct();
            IQueryable<string> claimNames = _context.RoleClaims.Where(x => roleIds.Contains(x.RoleId)).Select(x => x.ClaimValue).Distinct();

            return [.. claimNames];
        }
        public Task<bool> CheckClaimsById(string id, string tenantId, List<string> claims)
        {
            User user = _context.Users.FirstOrDefault(x => x.Id == id);
            IQueryable<User> query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserRoles.Any(ut => ut.Tenant.Id == 1));
            if (user == null) { return Task.FromResult(false); }

            // Fix MySQL: Materializza userRoles per evitare DataReader error
            List<UserRole> userRoles = (tenantId == ""
                ? _context.UserRoles.Where(x => x.UserId == user.Id)
                : _context.UserRoles.Where(x => x.UserId == user.Id && (x.TenantId.ToString() == tenantId || x.TenantId == 1)))
                .ToList();

            List<string> claimNames = [];

            // Fix MySQL: Query separata per ogni role invece di usare Select().Contains()
            foreach (UserRole userRole in userRoles)
            {
                Role role = _context.Roles.FirstOrDefault(x => x.Id == userRole.RoleId);
                if (role == null) { continue; }

                // Query separata materializzata - MySQL gestisce facilmente query semplici con indici
                List<RoleClaim> roleClaims = _context.RoleClaims
                    .Where(x => x.RoleId == role.Id)
                    .ToList();

                foreach (RoleClaim claim in roleClaims)
                {
                    if (!claimNames.Contains(claim.ClaimValue))
                    {
                        claimNames.Add(claim.ClaimValue);
                    }
                }
            }
            foreach (string claim in claims)
            {
                if (!claimNames.Contains(claim))
                {
                    return Task.FromResult(false);
                }

            }

            return Task.FromResult(true);
        }

        public async Task<UserTenant> RegisterUserTenant(string userId, int tenantId, string ip, string state)
        {
            UserTenant ut = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                Ip = ip,
                AcceptedAt = DateTime.UtcNow,
                State = state,
            };
            await _context.UserTenants.AddAsync(ut);
            await _context.SaveChangesAsync();
            return ut;
        }

        public async Task<UserTenant> RegisterUserTenantPending(string userId, int tenantId)
        {
            UserTenant ut = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                TenantId = tenantId,
                CreatedAt = DateTime.Now,
                Ip = "",
                State = "pending",
            };
            await _context.UserTenants.AddAsync(ut);
            await _context.SaveChangesAsync();
            return ut;
        }

        public Task<List<RoleClaim>> GetAllClaimsByRoleId(string roleId)
        {
            List<RoleClaim> claims = [.. _context.RoleClaims.Where(x => x.RoleId == roleId)];
            return Task.FromResult(claims);
        }

        public Task<List<RoleClaim>> GetAllClaimsByRole(int tenantId, string roleName)
        {
            Role role = _context.Roles.FirstOrDefault(x => x.TenantId == tenantId && x.Name == roleName);
            List<RoleClaim> claims = [.. _context.RoleClaims.Where(x => x.RoleId == role.Id)];
            return Task.FromResult(claims);
        }

        public async Task<UserRole> RegisterUserRole(string userId, int tenantId, string roleName)
        {
            Role role = _context.Roles.FirstOrDefault(x => x.TenantId == 1 && x.Name == roleName);

            role ??= _context.Roles.FirstOrDefault(x => x.TenantId == tenantId && x.Name == roleName);

            UserRole userRole = new()
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                UserId = userId,
                RoleId = role.Id
            };
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task<UserRole> DeleteUserRole(string userId, int tenantId, string roleName)
        {
            UserRole userRole = _context.UserRoles.FirstOrDefault(x => x.TenantId == tenantId && x.UserId == userId && x.Role.Name == roleName);

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task<bool> SendOtpEmail(string otpSended, string setupType, string baseEndpoint, string templateCode = "em.10", Dictionary<string, string> customContent = null)
        {
            Otp otp = _context.Otps.FirstOrDefault(x => x.IsValid && x.OtpSended == otpSended);

            UserTenant ut;
            ut = _context.UserTenants.FirstOrDefault(x => x.Id == otp.OtpValue && x.State == "pending");

            User user = _context.Users.FirstOrDefault(x => x.Id == ut.UserId);
            if (user.FakeEmail != null && (bool)user.FakeEmail)
            {
                return true;
            }

            UserProfile up = _context.UserProfiles.FirstOrDefault(x => x.UserId == user.Id);

            Setup setup = _context.Setups.FirstOrDefault(obj => obj.environment == setupType.ToString());

            Template template = null;

            List<Template> verifyTenant1Template = [.. _context.Templates.Where(x => x.Active && x.Code == templateCode && !x.CopyInNewTenants)];

            if (verifyTenant1Template.Count == 0)
            {
                string tenantCategories = String.Join(',', _context.Categories.Where(x => x.TenantId == ut.TenantId).Select(x => x.Id));

                List<Template> possibleLangsTemp = [.. _context.Templates.Where(x => x.Active && x.Code == templateCode && tenantCategories.Contains(x.CategoryId.ToString()))];

                template = possibleLangsTemp.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");
            }
            else
            {
                template = verifyTenant1Template.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= verifyTenant1Template.FirstOrDefault(x => x.Language == "it");
            }

            if (template == null)
            {
                return false;
            }

            Tenant tenant = _context.Tenants.FirstOrDefault(x => x.Id == ut.TenantId);

            DateTime now = DateTime.UtcNow;

            CultureInfo cultureInfo = new(up.UserLang);

            string mailObject = String.IsNullOrEmpty(template.ObjectText) ? "Mail informativa" : template.ObjectText;

            mailObject = mailObject.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    mailObject = mailObject.Replace(item.Key, item.Value);
                }
            }

            string content = String.IsNullOrEmpty(template.Content) ? template.ContentNoHtml : template.Content;
            content = content.Replace("{BaseEndpoint}", baseEndpoint).Replace("{OTP}", otp.OtpSended)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{Data}", now.ToString(cultureInfo.DateTimeFormat.ShortDatePattern))
                .Replace("{DataEOra}", now.ToString(cultureInfo.DateTimeFormat.LongDatePattern))
                .Replace("{Ora}", now.ToString(cultureInfo.DateTimeFormat.ShortTimePattern))
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }

            content = await AddHeaderFooter(content, up.UserLang, ut.TenantId);

            if (content == null)
            {
                return false;
            }

            await _mail.SendEmailAsync(user.Email, mailObject, content, sender: _configuration["SMTP:Sender"]);

            ut.CreatedAt = DateTime.Now;
            _context.Entry(ut).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendOtpEmail(string userName, string otp, string platform, int tenantId, string baseEndpoint, string templateId, Dictionary<string, string> customContent = null)
        {
            Setup setup = _context.Setups.FirstOrDefault(obj => obj.environment == "web");

            User user = _context.Users.FirstOrDefault(x => x.UserName == userName);
            if (user.FakeEmail != null && (bool)user.FakeEmail)
            {
                return true;
            }

            UserProfile up = _context.UserProfiles.FirstOrDefault(x => x.UserId == user.Id);

            Template template = null;

            IQueryable<Template> verifyTenant1Template = _context.Templates.Where(x => x.Active && x.Code == templateId && !x.CopyInNewTenants);

            if (!verifyTenant1Template.Any())
            {
                string tenantCategories = String.Join(',', _context.Categories.Where(x => x.TenantId == tenantId).Select(x => x.Id));

                IQueryable<Template> possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == templateId && tenantCategories.Contains(x.CategoryId.ToString()));

                template = possibleLangsTemp.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");
            }
            else
            {
                template = verifyTenant1Template.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= verifyTenant1Template.FirstOrDefault(x => x.Language == "it");
            }

            if (template == null)
            {
                return false;
            }

            Tenant tenant = _context.Tenants.FirstOrDefault(x => x.Id == tenantId);

            string mailObject = String.IsNullOrEmpty(template.ObjectText) ? "Mail informativa" : template.ObjectText;

            mailObject = mailObject.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    mailObject = mailObject.Replace(item.Key, item.Value);
                }
            }

            string content = String.IsNullOrEmpty(template.Content) ? template.ContentNoHtml : template.Content;

            content = content.Replace("{BaseEndpoint}", baseEndpoint).Replace("{OTP}", otp)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }

            content = await AddHeaderFooter(content, up.UserLang, tenantId);

            if (content == null)
            {
                return false;
            }

            await _mail.SendEmailAsync(user.Email, mailObject, content, sender: _configuration["SMTP:Sender"]);

            return true;
        }

        public async Task<bool> SendGenericEmail(string userName, string templateCode, int tenantId, string baseEndpoint, Dictionary<string, string> customContent = null)
        {
            Setup setup = _context.Setups.FirstOrDefault(obj => obj.environment == "web");
            User user = _context.Users.FirstOrDefault(x => x.UserName == userName);
            if (user.FakeEmail != null && (bool)user.FakeEmail)
            {
                return true;
            }

            UserProfile up = _context.UserProfiles.FirstOrDefault(x => x.UserId == user.Id);

            Template template = null;

            IQueryable<Template> verifyTenant1Template = _context.Templates.Where(x => x.Active && x.Code == templateCode && !x.CopyInNewTenants);

            if (!verifyTenant1Template.Any())
            {
                string tenantCategories = String.Join(',', _context.Categories.Where(x => x.TenantId == tenantId).Select(x => x.Id));

                IQueryable<Template> possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == templateCode && tenantCategories.Contains(x.CategoryId.ToString()));

                template = possibleLangsTemp.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");
            }
            else
            {
                template = verifyTenant1Template.FirstOrDefault(x => x.Language == up.UserLang);

                template ??= verifyTenant1Template.FirstOrDefault(x => x.Language == "it");
            }

            if (template == null)
            {
                return false;
            }

            Tenant tenant = _context.Tenants.FirstOrDefault(x => x.Id == tenantId);

            string content = String.IsNullOrEmpty(template.Content) ? template.ContentNoHtml : template.Content;

            content = content.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }

            string mailObject = String.IsNullOrEmpty(template.ObjectText) ? "Mail informativa" : template.ObjectText;

            mailObject = mailObject.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{UserName}", up.FirstName)
                .Replace("{UserSurname}", up.LastName)
                .Replace("{UserEmail}", user.Email)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    mailObject = mailObject.Replace(item.Key, item.Value);
                }
            }

            content = await AddHeaderFooter(content, up.UserLang, tenantId);

            if (content == null)
            {
                return false;
            }

            await _mail.SendEmailAsync(user.Email, mailObject, content, sender: _configuration["SMTP:Sender"]);

            return true;
        }

        public async Task<bool> SendGenericEmailToNonRegisteredEmail(string email, string templateCode, int tenantId, string baseEndpoint, Dictionary<string, string> customContent = null)
        {


            Setup setup = _context.Setups.FirstOrDefault(obj => obj.environment == "web");

            Template template = null;

            IQueryable<Template> verifyTenant1Template = _context.Templates.Where(x => x.Active && x.Code == templateCode && !x.CopyInNewTenants);

            if (!verifyTenant1Template.Any())
            {
                string tenantCategories = String.Join(',', _context.Categories.Where(x => x.TenantId == tenantId).Select(x => x.Id));

                IQueryable<Template> possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == templateCode && tenantCategories.Contains(x.CategoryId.ToString()));

                template = possibleLangsTemp.FirstOrDefault(x => x.Language == "it");
            }
            else
            {
                template = verifyTenant1Template.FirstOrDefault(x => x.Language == "it");
            }

            if (template == null)
            {
                return false;
            }

            Tenant tenant = _context.Tenants.FirstOrDefault(x => x.Id == tenantId);

            string content = String.IsNullOrEmpty(template.Content) ? template.ContentNoHtml : template.Content;

            content = content.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }

            string mailObject = String.IsNullOrEmpty(template.ObjectText) ? "Mail informativa" : template.ObjectText;

            mailObject = mailObject.Replace("{BaseEndpoint}", baseEndpoint)
                .Replace("{TenantVAT}", tenant.TenantVAT)
                .Replace("{TenantEmail}", tenant.Email)
                .Replace("{TenantPhone}", tenant.PhoneNumber)
                .Replace("{TenantAddress}", tenant.RegisteredOfficeAddress)
                .Replace("{TenantCity}", tenant.RegisteredOfficeCity)
                .Replace("{TenantPEC}", tenant.TenantPEC)
                .Replace("{TenantName}", tenant.Name);

            if (customContent != null)
            {
                foreach (KeyValuePair<string, string> item in customContent)
                {
                    mailObject = mailObject.Replace(item.Key, item.Value);
                }
            }

            content = await AddHeaderFooter(content, "it", tenantId);

            if (content == null)
            {
                return false;
            }

            await _mail.SendEmailAsync(email, mailObject, content, sender: _configuration["SMTP:Sender"]);

            return true;
        }

        private Task<string> AddHeaderFooter(string content, string dfLang, int tenantId)
        {
            Template headerTemplate = null;

            string tenantCategories = String.Join(',', _context.Categories.Where(x => x.TenantId == tenantId).Select(x => x.Id));

            string tenant1Categories = String.Join(',', _context.Categories.Where(x => x.TenantId == 1).Select(x => x.Id));

            IQueryable<Template> possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == "5697fb91-35c1-42fa-8be3-b6099f8b2fe2" && tenantCategories.Contains(x.CategoryId.ToString()));

            headerTemplate = possibleLangsTemp.FirstOrDefault(x => x.Language == dfLang);

            headerTemplate ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");

            if (headerTemplate == null)
            {
                possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == "5697fb91-35c1-42fa-8be3-b6099f8b2fe2" && tenant1Categories.Contains(x.CategoryId.ToString()));

                headerTemplate = possibleLangsTemp.FirstOrDefault(x => x.Language == dfLang);

                headerTemplate ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");

                if (headerTemplate == null)
                {
                    return Task.FromResult<string>(null);
                }
            }

            Template footerTemplate = null;

            possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == "cc77d9e7-a6f0-4eae-b843-b98d95b9d7b9" && tenantCategories.Contains(x.CategoryId.ToString()));

            footerTemplate = possibleLangsTemp.FirstOrDefault(x => x.Language == dfLang);

            footerTemplate ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");

            if (footerTemplate == null)
            {
                possibleLangsTemp = _context.Templates.Where(x => x.Active && x.Code == "cc77d9e7-a6f0-4eae-b843-b98d95b9d7b9" && tenant1Categories.Contains(x.CategoryId.ToString()));

                footerTemplate = possibleLangsTemp.FirstOrDefault(x => x.Language == dfLang);

                footerTemplate ??= possibleLangsTemp.FirstOrDefault(x => x.Language == "it");

                if (footerTemplate == null)
                {
                    return Task.FromResult<string>(null);
                }
            }

            string footerContent = String.IsNullOrEmpty(footerTemplate.Content) ? footerTemplate.ContentNoHtml : footerTemplate.Content;
            string headerContent = String.IsNullOrEmpty(headerTemplate.Content) ? headerTemplate.ContentNoHtml : headerTemplate.Content;

            return Task.FromResult(headerContent + "<br>" + content + "<br>" + footerContent);
        }

        public Task<UserTenant> GetUserTenantById(string userTenantId)
        {
            UserTenant ut;
            ut = _context.UserTenants.FirstOrDefault(x => x.Id == userTenantId);
            return Task.FromResult(ut);
        }

        public Task<UserProfile> GetUserProfileByUsername(string username)
        {
            User user = _context.Users.FirstOrDefault(x => x.UserName == username);

            UserProfile up = _context.UserProfiles.FirstOrDefault(x => x.UserId == user.Id);
            return Task.FromResult(up);
        }

        public Task<UserProfile> GetUserProfileById(string Id)
        {
            UserProfile up = _context.UserProfiles.FirstOrDefault(x => x.UserId == Id);
            return Task.FromResult(up);
        }

        public async Task<UserProfile> UpdateUserProfile(UserProfile up)
        {
            _context.Entry(up).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return up;
        }


        public async Task<UserTenant> UpdateUserTenant(UserTenant ut)
        {
            _context.Entry(ut).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return ut;
        }

        public async Task<User> DeleteUser(String id)
        {
            User ut = _context.Users.FirstOrDefault(x => x.Id == id);

            if (ut == null)
            {
                return null;
            }

            IQueryable<Otp> otps = _context.Otps.Where(x => x.UserId == id);
            _context.Otps.RemoveRange(otps);

            IQueryable<PasswordHistory> oldpasswords = _context.PasswordHistories.Where(x => x.UserId == id);
            _context.PasswordHistories.RemoveRange(oldpasswords);

            IQueryable<UserAudit> audits = _context.UserAudits.Where(x => x.UserId == id);
            _context.UserAudits.RemoveRange(audits);

            IQueryable<UserProfile> profiles = _context.UserProfiles.Where(x => x.UserId == id);
            _context.UserProfiles.RemoveRange(profiles);

            IQueryable<UserTenant> tenants = _context.UserTenants.Where(x => x.UserId == id);
            _context.UserTenants.RemoveRange(tenants);

            IQueryable<UserDevice> devices = _context.UserDevices.Where(x => x.userId == id);
            _context.UserDevices.RemoveRange(devices);

            IQueryable<UserRole> roles = _context.UserRoles.Where(x => x.UserId == id);
            _context.UserRoles.RemoveRange(roles);

            List<PasswordHistory> passwordHistories = [.. _context.PasswordHistories.Where(x => x.UserId == id)];
            _context.PasswordHistories.RemoveRange(passwordHistories);

            _context.Users.Remove(ut);

            await _context.SaveChangesAsync();
            return ut;
        }

        public async Task<User> DeleteUserLite(String id)
        {
            User ut = _context.Users.FirstOrDefault(x => x.Id == id);

            if (ut == null)
            {
                return null;
            }

            IQueryable<Otp> otps = _context.Otps.Where(x => x.UserId == id);
            _context.Otps.RemoveRange(otps);

            IQueryable<UserAudit> audits = _context.UserAudits.Where(x => x.UserId == id);
            _context.UserAudits.RemoveRange(audits);

            IQueryable<PasswordHistory> oldpasswords = _context.PasswordHistories.Where(x => x.UserId == id);
            _context.PasswordHistories.RemoveRange(oldpasswords);

            IQueryable<UserProfile> profiles = _context.UserProfiles.Where(x => x.UserId == id);
            UserProfile oldUserProfile = profiles.FirstOrDefault();
            UserProfile newUserProfile = new() { Id = Guid.NewGuid().ToString(), LastName = "Erased" + oldUserProfile.LastName, FirstName = "Erased" + oldUserProfile.FirstName, UserId = oldUserProfile.UserId, registrationDate = oldUserProfile.registrationDate, termsAcceptanceDate = oldUserProfile.termsAcceptanceDate, termsAccepted = oldUserProfile.termsAccepted };
            _context.UserProfiles.Add(newUserProfile);
            _context.UserProfiles.RemoveRange(profiles);

            //List<UserTenant> tenants = (await _context.UserTenants.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserTenants.RemoveRange(tenants);

            IQueryable<UserDevice> devices = _context.UserDevices.Where(x => x.userId == id);
            _context.UserDevices.RemoveRange(devices);

            //List<UserRole> roles = (await _context.UserRoles.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserRoles.RemoveRange(roles);
            ut.Email = "erased" + ut.Email + DateTime.Now.Ticks;
            ut.NormalizedEmail = "erased" + ut.NormalizedEmail + DateTime.Now.Ticks;
            ut.NormalizedUserName = "erased" + ut.NormalizedUserName + DateTime.Now.Ticks;
            ut.PasswordHash = "AQAAAAEAACcQAAAAEACiYwfIB0y34TnS5OLO5uVuj8+Cx7Dz11V55xy+h7HalsnucicTMaU0dyOPYeNeuw==";
            ut.Deleted = true;
            _context.Users.Update(ut);

            await _context.SaveChangesAsync();
            return ut;
        }

        public bool ExecuteQueryToOdbc(string connectionString, string query, List<object> parameters)
        {
            using OdbcConnection connection = new(connectionString);
            connection.ConnectionTimeout = 10;
            connection.Open();
            using OdbcCommand command = new(query, connection);
            foreach (object parameter in parameters)
            {
                command.Parameters.AddWithValue("", parameter ?? DBNull.Value);
            }
            command.CommandTimeout = 10;
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public IEnumerable<DateTime> GetGiorniFestivitaAnnue(int anno)
        {
            List<DateTime> listaFestivita =
            [
                new DateTime(anno, 1, 1, 0, 0, 0), //CAPODANNO
                new DateTime(anno, 1, 6, 0, 0, 0), //EPIFANIA
                new DateTime(anno, 4, 25, 0, 0, 0), //LIBERAZIONE
                new DateTime(anno, 5, 1, 0, 0, 0), //PRIMO MAGGIO
                new DateTime(anno, 6, 2, 0, 0, 0), //2 Giugno
                new DateTime(anno, 8, 15, 0, 0, 0), //FERRAGOSTO
                new DateTime(anno, 11, 1, 0, 0, 0), //OGNISANTI
                new DateTime(anno, 12, 8, 0, 0, 0), //IMMACOLATA
                new DateTime(anno, 12, 25, 0, 0, 0), //Natale
                new DateTime(anno, 12, 26, 0, 0, 0), //S.Stefano
                new DateTime(anno, 2, 14, 0, 0, 0) //S.Valentino
            ];

            DateTime pasqua = CalcolaPasqua(anno);
            DateTime pasquetta = pasqua.AddDays(1);
            listaFestivita.Add(pasquetta); //Pasquetta
            return listaFestivita;
        }

        public static DateTime CalcolaPasqua(int anno)
        {
            int a = anno % 19;
            int b = anno / 100;
            int c = anno % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 16;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 16;
            int j = c % 16;
            int k = (32 + 2 * e + 2 * i - h - j) % 7;
            int l = (a + 11 * h + 22 * k) / 451;
            int mesePasqua = (h + k - l + 114) / 31; // Marzo=3, Aprile=4
            int giornoPasqua = ((h + k - l + 114) % 31) + 1;
            return new DateTime(anno, mesePasqua, giornoPasqua);
        }

        public async Task<PasswordHistory> CreatePasswordHistory(string userId, string userEmail, string oldPasswordHash, DateTime validFrom)
        {
            _context.ChangeTracker.Clear();

            PasswordHistory ph = new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Email = userEmail,
                PasswordHash = oldPasswordHash,
                ValidFrom = validFrom
            };
            await _context.PasswordHistories.AddAsync(ph);
            await _context.SaveChangesAsync();
            return ph;
        }

        public Task<List<PasswordHistory>> GetUserPasswordHistory(string userId, int number = 1000)
        {
            List<PasswordHistory> result = [.. _context.PasswordHistories.Where(x => x.UserId == userId).OrderByDescending(x => x.ValidFrom).Take(number)];
            return Task.FromResult(result);
        }

        public Task<LegalTerm> Activation(String id)
        {
            //User ut = _context.Users.FirstOrDefault(x => x.Id == id);

            //if (ut == null)
            //{
            //	return null;
            //}

            //List<Otp> otps = (await _context.Otps.ToListAsync()).Where(x => x.UserId == id);
            //_context.Otps.RemoveRange(otps);

            //List<UserAudit> audits = (await _context.UserAudits.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserAudits.RemoveRange(audits);

            //List<UserProfile> profiles = (await _context.UserProfiles.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserProfiles.RemoveRange(profiles);

            //List<UserTenant> tenants = (await _context.UserTenants.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserTenants.RemoveRange(tenants);

            //List<UserDevice> devices = (await _context.UserDevices.ToListAsync()).Where(x => x.userId == id);
            //_context.UserDevices.RemoveRange(devices);

            //List<UserRole> roles = (await _context.UserRoles.ToListAsync()).Where(x => x.UserId == id);
            //_context.UserRoles.RemoveRange(roles);

            //_context.Users.Remove(ut);

            //await _context.SaveChangesAsync();
            return Task.FromResult(new LegalTerm() { });
        }

        public List<string> GetAllUsersGuidInRole(string roleGuid)
        {
            return [.. _context.UserRoles.Where(x => x.RoleId == roleGuid).Select(o => o.UserId)];
        }

        public Task<List<string>> GetAllUsersGuidInTenant(int tenantId)
        {
            return Task.FromResult((List<string>)[.. _context.UserTenants.Where(x => x.TenantId == tenantId).Select(o => o.UserId)]);
        }

        public List<string> GetAllUsersGuidFromClaimName(string claimName)
        {
            IQueryable<string> roles = _context.RoleClaims.Where(x => x.ClaimValue == claimName).Select(x => x.RoleId);

            List<string> result = [];

            foreach (string role in roles)
            {
                result.AddRange(GetAllUsersGuidInRole(role));
            }

            return result;
        }

        // REMOVED: Use FirstOrDefault<Ticket, string>(x => x.Id == ticketId) instead
    }

}
