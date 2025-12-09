using AutoMapper;
using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.Domain.Interfaces;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace MIT.Fwk.Infrastructure.Services
{
    /// <summary>
    /// FileService refactored to use System.IO directly.
    /// FASE 4: Removed unused IFileRepository dependency - uses System.IO methods.
    /// </summary>
    public class FileService : IFileService
    {
        protected readonly IMapper _mapper;
        protected readonly IMediatorHandler _bus;

        public FileService(IMapper mapper,
                                  IMediatorHandler bus)
        {
            _mapper = mapper;
            _bus = bus;
        }

        #region Query

        public FileInfo Get(string path)
        {
            return new FileInfo(path);
        }

        public FileInfo[] GetAll(string path)
        {
            List<FileInfo> list = [];
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                list.Add(new FileInfo(file));
            }
            return list.ToArray();
        }

        #endregion

        #region Command

        public FileInfo Create(string path, FileInfo file)
        {
            throw new NotImplementedException();
        }

        public void Update(string path, FileInfo file, bool overwrite = true)
        {
            throw new NotImplementedException();
        }

        public void Remove(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GC
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
