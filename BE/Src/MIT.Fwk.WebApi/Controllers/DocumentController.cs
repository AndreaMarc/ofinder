using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    //[EnableCors("AllowSpecificOrigin")]
    public class DocumentController : ControllerBase
    {
        protected readonly IDocumentService _docService;
        protected ILogger _logger;

        public DocumentController(
            IDocumentService DocService,
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            ILoggerFactory loggerFactory)
        {
            _docService = DocService;
            _logger = loggerFactory.CreateLogger<DocumentController>();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("document")]
        public async Task<IActionResult> Upload(IFormFile uploadedFile, [FromQuery] string title, [FromQuery] string description, [FromQuery] string meta)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                using StreamReader reader = new(uploadedFile.OpenReadStream());
                string contentAsString = reader.ReadToEnd();
                ContentDispositionHeaderValue parsedContentDisposition = ContentDispositionHeaderValue.Parse(uploadedFile.ContentDisposition);

                DocumentFile file2Add = new()
                {
                    FileName = uploadedFile.FileName,
                    Extension = uploadedFile.ContentType,

                    Title = title,
                    Description = description,
                    Meta = meta
                };

                byte[] data = new byte[contentAsString.Length * sizeof(char)];
                Buffer.BlockCopy(contentAsString.ToCharArray(), 0, data, 0, data.Length);

                file2Add.BinaryData = data;

                // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                DocumentFile newDocument = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(file2Add);
                return Ok(new { success = true, data = newDocument });

            }

            return BadRequest();
        }

        [HttpPost]
        [Route("documents/save")]
        public async Task<List<DocumentFile>> SaveFiles(IFormFileCollection uploadedFiles, string[] titles, string[] descriptions)
        {
            List<DocumentFile> newDocuments = [];
            if (uploadedFiles != null && uploadedFiles.Count > 0)
            {
                int count = 0;
                foreach (IFormFile uploadedFile in uploadedFiles)
                {
                    if (uploadedFile.Length > 0)
                    {
                        using MemoryStream memoryStream = new();
                        await uploadedFile.CopyToAsync(memoryStream);
                        //var contentAsString = reader.ReadToEnd();
                        ContentDispositionHeaderValue parsedContentDisposition = ContentDispositionHeaderValue.Parse(uploadedFile.ContentDisposition);

                        DocumentFile file2Add = new()
                        {
                            FileName = uploadedFile.FileName,
                            Extension = uploadedFile.ContentType,

                            Title = titles[count],
                            Description = descriptions[count]
                        };
                        //file2Add.Meta = meta;

                        //byte[] data = new byte[memoryStream.Length * sizeof(char)];
                        //Buffer.BlockCopy(contentAsString.ToCharArray(), 0, data, 0, data.Length);
                        byte[] data = memoryStream.ToArray();

                        file2Add.BinaryData = data;

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile newDocument = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(file2Add);
                        newDocuments.Add(newDocument);
                    }
                    count++;
                }
            }
            return newDocuments;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("document/add")]
        public async Task<IActionResult> Add([FromQuery] string jsonBody, [FromQuery] string title, [FromQuery] string description, [FromQuery] string meta)
        {
            if (jsonBody != null && jsonBody.Length > 0)
            {
                DocumentFile file2Add = new()
                {
                    FileName = title,
                    Extension = ".json",

                    Title = title,
                    Description = description,
                    Meta = meta
                };

                byte[] data = new byte[jsonBody.Length * sizeof(char)];
                Buffer.BlockCopy(jsonBody.ToCharArray(), 0, data, 0, data.Length);

                file2Add.BinaryData = data;

                // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                DocumentFile newDocument = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(file2Add);
                return Ok(new { success = true, data = newDocument });
            }

            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("documents")]
        public async Task<IActionResult> Uploads(IFormFileCollection uploadedFiles, [FromQuery] string title, [FromQuery] string description, [FromQuery] string meta)
        {
            if (uploadedFiles != null && uploadedFiles.Count > 0)
            {
                List<DocumentFile> newDocuments = [];
                foreach (IFormFile uploadedFile in uploadedFiles)
                {
                    if (uploadedFile.Length > 0)
                    {
                        using MemoryStream memoryStream = new();
                        await uploadedFile.CopyToAsync(memoryStream);
                        //var contentAsString = reader.ReadToEnd();
                        ContentDispositionHeaderValue parsedContentDisposition = ContentDispositionHeaderValue.Parse(uploadedFile.ContentDisposition);

                        DocumentFile file2Add = new()
                        {
                            FileName = uploadedFile.FileName,
                            Extension = uploadedFile.ContentType,

                            Title = title,
                            Description = description,
                            Meta = meta
                        };

                        //byte[] data = new byte[memoryStream.Length * sizeof(char)];
                        //Buffer.BlockCopy(contentAsString.ToCharArray(), 0, data, 0, data.Length);
                        byte[] data = memoryStream.ToArray();

                        file2Add.BinaryData = data;

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile newDocument = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(file2Add);
                        newDocuments.Add(newDocument);
                    }
                }
                return Ok(new { success = true, data = newDocuments });
            }

            return BadRequest();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("document")]
        public IActionResult Download(long id)
        {
            DocumentFile doc = _docService.Get(id);
            if (doc == null)
            {
                NotFound();
            }

            return Ok(new { success = true, data = doc });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("document/all")]
        public IActionResult ListDocuments(string filterMeta = "", string filterTitle = "", string filterName = "")
        {
            Dictionary<string, object> filters = [];

            if (!string.IsNullOrEmpty(filterMeta))
            {
                filters.Add("meta", filterMeta);
            }

            if (!string.IsNullOrEmpty(filterTitle))
            {
                filters.Add("title", filterTitle);
            }

            if (!string.IsNullOrEmpty(filterName))
            {
                filters.Add("name", filterName);
            }

            return Ok(new { success = true, data = _docService.GetAll(1, filters) });
        }

        [HttpDelete]
        [Route("document")]
        public async Task<IActionResult> Delete(int id)
        {
            DocumentFile doc = _docService.Get(id);
            if (doc == null)
            {
                return NotFound();
            }

            // FASE 8A: Use DocumentManager.DeleteAsync instead of obsolete Remove()
            await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.DeleteAsync(doc);

            return Ok(new { success = true, data = doc });
        }

    }
}
