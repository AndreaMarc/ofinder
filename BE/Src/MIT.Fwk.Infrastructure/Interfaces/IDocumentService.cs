using MIT.Fwk.Core.Models;
using System;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IDocumentService : IDisposable
    {

        #region Query

        DocumentFile Get(long id);

        IEnumerable<DocumentFile> GetAll(int tenantId, Dictionary<string, object> filters = null);

        #endregion

        // FASE 8A: Command methods removed - use DocumentManager directly

    }
}
