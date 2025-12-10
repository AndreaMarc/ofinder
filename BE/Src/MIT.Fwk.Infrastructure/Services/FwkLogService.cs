using AutoMapper;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Services
{
    /// <summary>
    /// FwkLogService refactored to use IFwkLogFactory directly (MongoDB).
    /// FASE 4: Removed IFwkLogRepository dependency - uses factory pattern.
    /// </summary>
    public class FwkLogService : IFwkLogService
    {
        protected readonly IMapper _mapper;
        protected readonly IFwkLogFactory _logFactory;
        protected readonly IMediatorHandler _bus;

        public FwkLogService(IMapper mapper,
                                  IFwkLogFactory logFactory,
                                  IMediatorHandler bus)
        {
            _mapper = mapper;
            _logFactory = logFactory;
            _bus = bus;
        }

        #region Query

        public FwkLog Get(long id)
        {
            FwkLog entity = _logFactory.ReadFwkLog<FwkLog>(id);
            return _mapper.Map<FwkLog>(entity);
        }

        public IEnumerable<FwkLog> GetAll(Dictionary<string, object> filters = null)
        {
            List<FwkLog> list = [];
            IEnumerable<FwkLog> objs = _logFactory.ListFwkLogs<FwkLog>(filters);

            foreach (FwkLog obj in objs)
            {
                list.Add(_mapper.Map<FwkLog>(obj));
            }

            return list;
        }

        #endregion

        #region Command

        /// <summary>
        /// Update not implemented for FwkLog (logs are immutable).
        /// </summary>
        public void Update(FwkLog file)
        {
            throw new NotImplementedException("FwkLog entities are immutable - update not supported.");
        }

        // FASE 8A: Create() and Remove() methods removed - use IFwkLogFactory directly

        #endregion

        #region GC
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
