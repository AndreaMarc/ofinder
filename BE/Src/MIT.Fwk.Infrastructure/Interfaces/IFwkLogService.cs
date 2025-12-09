using MIT.Fwk.Core.Models;
using System;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IFwkLogService : IDisposable
    {

        #region Query

        FwkLog Get(long id);

        IEnumerable<FwkLog> GetAll(Dictionary<string, object> filters = null);

        #endregion

        // FASE 8A: Command methods removed - use IFwkLogFactory directly
        // FASE 8A: LogProcessInfo removed - use LogService.ForMongo() instead

    }
}
