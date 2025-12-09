using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MIT.Fwk.Examples.Interfaces
{
    /// <summary>
    /// Generic manual service interface for OtherDbContext
    /// Contains only generic CRUD methods for any entity type
    /// </summary>
    public interface IOtherManualService
    {
        #region Generic Synchronous Methods

        /// <summary>
        /// Gets all entities of type T
        /// </summary>
        IEnumerable<T> GetAll<T, TId>() where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Gets all entities of type T as IQueryable for further filtering
        /// </summary>
        IQueryable<T> GetAllQueryable<T, TId>() where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Gets entity by ID
        /// </summary>
        T GetById<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Creates a new entity
        /// </summary>
        T Create<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        T Update<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Deletes entity by ID
        /// </summary>
        T Delete<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Deletes multiple entities
        /// </summary>
        bool DeleteRange<T, TId>(IQueryable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Gets first entity matching predicate or default
        /// </summary>
        T FirstOrDefault<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Filters entities by predicate
        /// </summary>
        IQueryable<T> Where<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Checks if any entity matches predicate
        /// </summary>
        bool Any<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Counts entities matching predicate
        /// </summary>
        int Count<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;

        #endregion

        #region Generic Async Methods

        /// <summary>
        /// Gets all entities of type T asynchronously
        /// </summary>
        Task<List<T>> GetAllAsync<T, TId>() where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Gets entity by ID asynchronously
        /// </summary>
        Task<T> GetByIdAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Creates a new entity asynchronously
        /// </summary>
        Task<T> CreateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Updates an existing entity asynchronously
        /// </summary>
        Task<T> UpdateAsync<T, TId>(T entity) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Deletes entity by ID asynchronously
        /// </summary>
        Task<T> DeleteAsync<T, TId>(TId id) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Deletes multiple entities asynchronously
        /// </summary>
        Task<bool> DeleteRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Gets first entity matching predicate or default asynchronously
        /// </summary>
        Task<T> FirstOrDefaultAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Filters entities by predicate asynchronously
        /// </summary>
        Task<List<T>> WhereAsync<T, TId>(Expression<Func<T, bool>> predicate) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Checks if any entity matches predicate asynchronously
        /// </summary>
        Task<bool> AnyAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Counts entities matching predicate asynchronously
        /// </summary>
        Task<int> CountAsync<T, TId>(Expression<Func<T, bool>> predicate = null) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Adds multiple entities asynchronously
        /// </summary>
        Task<List<T>> AddRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        /// <summary>
        /// Updates multiple entities asynchronously
        /// </summary>
        Task<List<T>> UpdateRangeAsync<T, TId>(IEnumerable<T> entities) where T : Identifiable<TId> where TId : IConvertible;

        #endregion
    }
}
