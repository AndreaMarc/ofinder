using AutoMapper;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Data;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIT.Fwk.Infrastructure.Services
{
    /// <summary>
    /// DocumentService refactored to use IDocFactory directly (MongoDB).
    /// FASE 4: Removed IDocumentRepository dependency - uses factory pattern.
    /// </summary>
    public class DocumentService : IDocumentService
    {
        protected readonly IMapper _mapper;
        protected readonly IDocFactory _docFactory;
        protected readonly IMediatorHandler _bus;

        public DocumentService(IMapper mapper,
                                  IDocFactory docFactory,
                                  IMediatorHandler bus)
        {
            _mapper = mapper;
            _docFactory = docFactory;
            _bus = bus;
        }

        #region Query

        public DocumentFile Get(long id)
        {
            try
            {
                DocumentFile entity = _docFactory.ReadDocument<DocumentFile>(id);
                return _mapper.Map<DocumentFile>(entity);
            }
            catch (Exception ex)
            {
                _bus.RaiseEvent(new DomainNotification(DomainNotification.INTERNAL_SERVER_ERROR, ex.Message));
            }

            return null;
        }

        public IEnumerable<DocumentFile> GetAll(int tenantId, Dictionary<string, object> filters = null)
        {
            List<DocumentFile> list = [];
            try
            {
                IEnumerable<DocumentFile> objs = _docFactory.ListDocuments<DocumentFile>(tenantId, filters);

                foreach (DocumentFile obj in objs)
                {
                    list.Add(_mapper.Map<DocumentFile>(obj));
                }
            }
            catch (Exception ex)
            {
                _bus.RaiseEvent(new DomainNotification(DomainNotification.INTERNAL_SERVER_ERROR, ex.Message));
            }

            return list;
        }

        #endregion

        #region Command

        // FASE 8A: Create(), Update(), and Remove() methods removed - use DocumentManager directly

        #endregion

        #region GC
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }



        #endregion
    }
}
