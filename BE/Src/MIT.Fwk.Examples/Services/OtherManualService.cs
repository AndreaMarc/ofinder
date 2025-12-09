using JsonApiDotNetCore.Resources;
using Microsoft.EntityFrameworkCore;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Examples.Data;
using MIT.Fwk.Examples.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Services
{
    /// <summary>
    /// Generic manual service implementation for OtherDbContext
    /// Provides generic CRUD operations for any entity type in OtherDbContext
    /// </summary>
    public class OtherManualService : IOtherManualService
    {
        private readonly OtherDbContext _context;
        private readonly ILogService _logService;

        public OtherManualService(OtherDbContext context, ILogService logService = null)
        {
            _context = context;
            _context.Database.SetCommandTimeout(3600);
            _logService = logService;
        }

        #region Generic Synchronous Methods

        public IEnumerable<T> GetAll<T, TId>() where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetAll<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return new List<T>();
            }
        }

        public IQueryable<T> GetAllQueryable<T, TId>() where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsQueryable();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetAllQueryable<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return Enumerable.Empty<T>().AsQueryable();
            }
        }

        public T GetById<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().Find(id);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetById<{typeof(T).Name}, {typeof(TId).Name}>({id}): {ex.Message}");
                return null;
            }
        }

        public T Create<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().Add(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Create<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public T Update<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().Update(entity);
                _context.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Update<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public T Delete<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                var entity = _context.Set<T>().Find(id);
                if (entity != null)
                {
                    _context.Set<T>().Remove(entity);
                    _context.SaveChanges();
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Delete<{typeof(T).Name}, {typeof(TId).Name}>({id}): {ex.Message}");
                return null;
            }
        }

        public bool DeleteRange<T, TId>(IQueryable<T> entities) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().RemoveRange(entities);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in DeleteRange<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return false;
            }
        }

        public T FirstOrDefault<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().AsNoTracking().FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in FirstOrDefault<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public IQueryable<T> Where<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return _context.Set<T>().Where(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Where<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return Enumerable.Empty<T>().AsQueryable();
            }
        }

        public bool Any<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return predicate == null ? _context.Set<T>().Any() : _context.Set<T>().Any(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Any<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return false;
            }
        }

        public int Count<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return predicate == null ? _context.Set<T>().Count() : _context.Set<T>().Count(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in Count<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region Generic Async Methods

        public async Task<List<T>> GetAllAsync<T, TId>() where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetAllAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task<T> GetByIdAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().FindAsync(id);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in GetByIdAsync<{typeof(T).Name}, {typeof(TId).Name}>({id}): {ex.Message}");
                return null;
            }
        }

        public async Task<T> CreateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                await _context.Set<T>().AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in CreateAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public async Task<T> UpdateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in UpdateAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public async Task<T> DeleteAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                var entity = await _context.Set<T>().FindAsync(id);
                if (entity != null)
                {
                    _context.Set<T>().Remove(entity);
                    await _context.SaveChangesAsync();
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in DeleteAsync<{typeof(T).Name}, {typeof(TId).Name}>({id}): {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().RemoveRange(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in DeleteRangeAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return false;
            }
        }

        public async Task<T> FirstOrDefaultAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in FirstOrDefaultAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return null;
            }
        }

        public async Task<List<T>> WhereAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return await _context.Set<T>().AsNoTracking().Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in WhereAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task<bool> AnyAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return predicate == null ? await _context.Set<T>().AnyAsync() : await _context.Set<T>().AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in AnyAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return false;
            }
        }

        public async Task<int> CountAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                return predicate == null ? await _context.Set<T>().CountAsync() : await _context.Set<T>().CountAsync(predicate);
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in CountAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return 0;
            }
        }

        public async Task<List<T>> AddRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                await _context.Set<T>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in AddRangeAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task<List<T>> UpdateRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible
        {
            try
            {
                _context.Set<T>().UpdateRange(entities);
                await _context.SaveChangesAsync();
                return entities.ToList();
            }
            catch (Exception ex)
            {
                _logService?.Error($"Error in UpdateRangeAsync<{typeof(T).Name}, {typeof(TId).Name}>: {ex.Message}");
                return new List<T>();
            }
        }

        #endregion
    }
}
