using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Threading;
using System.Threading.Tasks;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MIT.Fwk.WebApi.Controllers
{
    [method: ActivatorUtilitiesConstructor]
    public class MediaFilesController(IJsonApiManualService jsonApiManualService, IDocumentService docService, IOptions<FileUploadOptions> fileUploadOptions, ILogService logService, IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<MediaFile, String> resourceService) : JsonApiController<MediaFile, String>(options, resourceGraph, loggerFactory, resourceService)
    {
        private readonly IResourceService<MediaFile, String> _resourceService = resourceService;
        protected readonly IDocumentService _docService = docService;
        protected readonly IJsonApiManualService _manualService = jsonApiManualService;
        private readonly ILogService _logService = logService;
        private readonly string _uploadsPath = fileUploadOptions.Value.UploadsPath;

        [HttpGet]
        public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<MediaFile> getAll = await _resourceService.GetAsync(cancellationToken);

            if (getAll.Count == 0)
            {
                return Ok(getAll);
            }
            List<string> guids = [];

            foreach (MediaFile item in getAll)
            {
                guids.Add(item.mongoGuid);
            }

            IQueryCollection queryString = HttpContext.Request.Query;
            string size = "big", querySize = "lg";

            if (queryString.ContainsKey("size"))
            {
                querySize = queryString["size"].ToString();
                switch (querySize)
                {
                    case "sm":
                        size = "small";
                        break;
                    case "md":
                        size = "medium";
                        break;
                    case "lg":
                        break;
                    default:
                        return StatusCode(500, "Size not existing.");
                }
            }

            List<DocumentFile> mongoRecords;

            Dictionary<string, object> filter = new()
            {
                {"size", querySize },
                {"guids", guids }
            };

            mongoRecords = [.. _docService.GetAll(-10, filter)];

            if (mongoRecords == null)
            {
                return StatusCode(500, "Record not existing in mongoDb.");
            }

            foreach (MediaFile m in getAll)
            {
                try
                {
                    if (m.primaryContentType == "image")
                    {
                        m.fileUrl += $"/{size}";

                        m.base64 = size switch
                        {
                            "small" => mongoRecords.Find(x => x.FileGuid == m.mongoGuid).SmallFormat,
                            "medium" => mongoRecords.Find(x => x.FileGuid == m.mongoGuid).MediumFormat,
                            _ => mongoRecords.Find(x => x.FileGuid == m.mongoGuid).BigFormat,
                        };
                    }
                    else
                    {
                        m.base64 = mongoRecords.Find(x => x.FileGuid == m.mongoGuid).FileBase64;
                    }
                }
                catch
                {
                    try
                    {
                        string filePath = $"{_uploadsPath}/{m.typologyArea}/{m.category}/{m.album}/{size}/{m.Id}.{m.extension}";

                        FileInfo file = new(filePath);

                        FileExtensionContentTypeProvider provider = new();
                        provider.TryGetContentType(filePath, out string contentType);

                        FormFile formFile = new(new MemoryStream(System.IO.File.ReadAllBytes(file.FullName)), 0, file.Length, file.Name, file.Name);

                        MemoryStream stream = new();
                        formFile.CopyTo(stream);

                        stream.Position = 0;
                        SKBitmap bigFormat = SKBitmap.Decode(stream);

                        SKBitmap mediumFormat = bigFormat, smallFormat = bigFormat;

                        int altezza, larghezza;

                        if (bigFormat.Height > 100 || bigFormat.Width > 100)
                        {
                            if (bigFormat.Height >= bigFormat.Width)
                            {
                                altezza = 100;
                                larghezza = (100 * bigFormat.Width) / bigFormat.Height;
                            }
                            else
                            {
                                larghezza = 100;
                                altezza = (100 * bigFormat.Height) / bigFormat.Width;
                            }

                            smallFormat = bigFormat.Resize(new SKImageInfo(larghezza, altezza), SKSamplingOptions.Default);

                            if (bigFormat.Height > 854 || bigFormat.Width > 854)
                            {
                                if (bigFormat.Height >= bigFormat.Width)
                                {
                                    altezza = 854;
                                    larghezza = (854 * bigFormat.Width) / bigFormat.Height;
                                }
                                else
                                {
                                    larghezza = 854;
                                    altezza = (854 * bigFormat.Height) / bigFormat.Width;
                                }

                                mediumFormat = bigFormat.Resize(new SKImageInfo(larghezza, altezza), SKSamplingOptions.Default);

                                if (bigFormat.Height > 2560 || bigFormat.Width > 2560)
                                {
                                    if (bigFormat.Height >= bigFormat.Width)
                                    {
                                        altezza = 2560;
                                        larghezza = (2560 * bigFormat.Width) / bigFormat.Height;
                                    }
                                    else
                                    {
                                        larghezza = 2560;
                                        altezza = (2560 * bigFormat.Height) / bigFormat.Width;
                                    }

                                    bigFormat = bigFormat.Resize(new SKImageInfo(larghezza, altezza), SKSamplingOptions.Default);

                                }
                            }
                        }

                        SKData data;

                        switch (size)
                        {
                            case "small":
                                using (SKImage image = SKImage.FromBitmap(smallFormat))
                                {
                                    data = image.Encode();
                                }

                                byte[] smallImageBytes = data.ToArray();
                                m.base64 = $"data:{contentType};base64," + Convert.ToBase64String(smallImageBytes);
                                break;
                            case "medium":
                                using (SKImage image = SKImage.FromBitmap(mediumFormat))
                                {
                                    data = image.Encode();
                                }

                                byte[] mediumImageBytes = data.ToArray();

                                m.base64 = $"data:{contentType};base64," + Convert.ToBase64String(mediumImageBytes);
                                break;
                            default:
                                using (SKImage image = SKImage.FromBitmap(bigFormat))
                                {
                                    data = image.Encode();
                                }

                                byte[] bigImageBytes = data.ToArray();

                                m.base64 = $"data:{contentType};base64," + Convert.ToBase64String(bigImageBytes);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        _logService.Error(e.Message, "Images");
                        m.base64 = "NoData";
                    }
                }
            }

            return Ok(getAll);
        }

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(String id, CancellationToken cancellationToken)
        {
            MediaFile getOne = await _resourceService.GetAsync(id, cancellationToken);

            if (getOne == null)
            {
                return NotFound();
            }

            IQueryCollection queryString = HttpContext.Request.Query;
            string size = "big", querySize = "lg";

            if (queryString.ContainsKey("size"))
            {
                querySize = queryString["size"].ToString();
                switch (querySize)
                {
                    case "sm":
                        size = "small";
                        break;
                    case "md":
                        size = "medium";
                        break;
                    case "lg":
                        break;
                    default:
                        return StatusCode(500, "Size not existing.");
                }
            }

            DocumentFile mongoRecord;

            Dictionary<string, object> filter = new()
            {
                    {"size", querySize },
                    {"guids", new List<string> { getOne.mongoGuid } }
                };

            mongoRecord = _docService.GetAll(-10, filter).FirstOrDefault();

            if (mongoRecord == null)
            {
                return StatusCode(500, "Record not existing in mongoDb.");
            }

            if (getOne.primaryContentType == "image")
            {
                getOne.fileUrl += $"/{size}";

                getOne.base64 = size switch
                {
                    "small" => mongoRecord.SmallFormat,
                    "medium" => mongoRecord.MediumFormat,
                    _ => mongoRecord.BigFormat,
                };
            }
            else
            {
                getOne.base64 = mongoRecord.FileBase64;
            }

            return Ok(getOne);
        }

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(String id, [FromBody] MediaFile resource, CancellationToken cancellationToken)
        {
            MediaFile oldMediaFile = (await _resourceService.GetAsync(cancellationToken)).ToList().FirstOrDefault(x => x.Id == id);

            if (oldMediaFile != null && (oldMediaFile.album != resource.album || oldMediaFile.category != resource.category || oldMediaFile.typologyArea != resource.typologyArea))
            {
                string oldPath = $"{_uploadsPath}/{oldMediaFile.typologyArea}/{oldMediaFile.category}/{oldMediaFile.album}";
                string newPath = $"{_uploadsPath}/{resource.typologyArea}/{resource.category}/{resource.album}";

                _manualService.MoveFile(oldPath, newPath, oldMediaFile);

                resource.fileUrl = oldMediaFile.fileUrl.Contains("files") ? $"{newPath}/files"[1..] : newPath[1..];
            }

            return await base.PatchAsync(id, resource, cancellationToken);
        }
    }
}

