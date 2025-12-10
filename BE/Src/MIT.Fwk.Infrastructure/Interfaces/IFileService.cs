using System;
using System.IO;

namespace MIT.Fwk.Infrastructure.Interfaces
{
    public interface IFileService : IDisposable
    {

        #region Query
        FileInfo Get(string path);

        FileInfo[] GetAll(string path);

        #endregion

        #region Command

        FileInfo Create(string path, FileInfo file);

        void Update(string path, FileInfo file, bool overwrite = true);

        void Remove(string path);

        #endregion

    }
}
