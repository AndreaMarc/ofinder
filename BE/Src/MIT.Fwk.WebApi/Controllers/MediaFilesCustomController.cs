using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Models;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Core.Services;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.Infrastructure.Interfaces;
using MIT.Fwk.WebApi.Models;
using MongoDB.Bson;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MIT.Fwk.WebApi.Controllers
{
    [Authorize]
    [Route("fileUpload")]
    public class MediaFilesCustomController(
    IJsonApiManualService jsonService,
    IDocumentService docService,
    IConnectionStringProvider connStringProvider,
    IOptions<FileUploadOptions> fileUploadOptions) : ControllerBase
    {
        protected readonly IDocumentService _docService = docService;
        private readonly List<string> acceptedImageFormats = [
                "image/png",
                "image/jpg",
                "image/jpeg"
            ];

        private readonly IJsonApiManualService _jsonService = jsonService;
        private readonly IConnectionStringProvider _connStringProvider = connStringProvider;
        private readonly string _uploadsPath = fileUploadOptions.Value.UploadsPath;

        /// <summary>
        /// Resolves a MediaCategory reference: if it's a GUID, use it; otherwise lookup by name
        /// </summary>
        private async Task<string> ResolveMediaCategoryId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // If it's a valid GUID, use it directly
            if (Guid.TryParse(value, out _))
                return value;

            // Otherwise, lookup by name
            var category = await _jsonService.GetAllQueryable<MediaCategory, string>()
                .FirstOrDefaultAsync(mc => mc.Name == value);

            return category?.Id;
        }

        [HttpGet]
        [Route("roots")]
        public Task<IActionResult> RootsInfo()
        {
            try
            {
                using SqlConnection connection = new(_connStringProvider.GetConnectionString("DefaultConnection"));
                List<string> albums = [];
                List<string> categories = [];
                List<string> typologies = [];
                List<string> primaryContentTypes = [];

                string query = "SELECT distinct typologyArea FROM MediaFiles";
                IEnumerable<string> result = connection.Query<string>(query);
                foreach (string item in result)
                {
                    typologies.Add(item);
                }

                query = "SELECT distinct category FROM MediaFiles";
                result = connection.Query<string>(query);
                foreach (string item in result)
                {
                    categories.Add(item);
                }

                query = "SELECT distinct album FROM MediaFiles";
                result = connection.Query<string>(query);
                foreach (string item in result)
                {
                    albums.Add(item);
                }

                query = "SELECT distinct primaryContentType FROM MediaFiles";
                result = connection.Query<string>(query);
                foreach (string item in result)
                {
                    primaryContentTypes.Add(item);
                }

                var res = new
                {
                    typology = typologies,
                    category = categories,
                    album = albums,
                    primaryContentType = primaryContentTypes
                };

                return Task.FromResult<IActionResult>(StatusCode(200, res));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, ex.Message + "\n" + ex.StackTrace + "\n\n" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace));
            }
        }

        [HttpPost]
        [Route("FromBE")]
        public async Task<IActionResult> FileUploadFromBE([FromBody] FileUploadDTO fa)
        {
            try
            {
                FormFileCollection coll = null;

                string contentTypeConvertedFile = "";

                if (coll == null || coll.Count == 0)
                {
                    if (string.IsNullOrEmpty(fa.base64String.ToString()) || string.IsNullOrEmpty(fa.base64Name.ToString()))
                    {
                        return BadRequest("File mancanti");
                    }
                    else
                    {
                        string base64String = fa.base64String.ToString();

                        byte[] bytes = Convert.FromBase64String(base64String[(base64String.IndexOf(',') + 1)..]);
                        MemoryStream stream = new(bytes);

                        FormFile file = new(stream, 0, bytes.Length, fa.base64Name.ToString(), fa.base64Name.ToString());

                        contentTypeConvertedFile = base64String.Substring(base64String.IndexOf(':') + 1, base64String.IndexOf(';') - base64String.IndexOf(':') - 1);

                        coll =
                        [
                            file
                        ];
                    }
                }

                string jsonApiResponse = "{\n\t\"data\":";

                if (coll.Count > 1)
                {
                    jsonApiResponse += "[";
                }

                foreach (FormFile file in coll.Cast<FormFile>())
                {
                    if (acceptedImageFormats.Contains(contentTypeConvertedFile) || acceptedImageFormats.Contains(file.ContentType))
                    {
                        jsonApiResponse += "\n\t\t{\"type\":\"mediaFiles\",\n\t\t\"attributes\":";

                        string[] fileNameArray = file.FileName.Split('.');
                        string fileNameNoExt = "";

                        for (int i = 0; i < fileNameArray.Length - 1; i++)
                        {
                            fileNameNoExt += fileNameArray[i] + ".";
                        }

                        fileNameNoExt = fileNameNoExt[..^1];

                        // Resolve category IDs (GUID or name lookup)
                        string albumId = await ResolveMediaCategoryId(fa.album.ToString());
                        string categoryId = await ResolveMediaCategoryId(fa.category.ToString());
                        string typologyAreaId = await ResolveMediaCategoryId(fa.typologyArea.ToString());

                        if (string.IsNullOrEmpty(typologyAreaId) || string.IsNullOrEmpty(categoryId))
                        {
                            return BadRequest($"TypologyArea '{fa.typologyArea}' or Category '{fa.category}' not found");
                        }

                        MediaFile mediafile = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            album = albumId,
                            alt = fa.alt.ToString(),
                            category = categoryId,
                            extension = fileNameArray.Last().ToLower(),
                            base64 = "",
                            fileUrl = "",
                            mongoGuid = "",
                            originalFileName = fileNameNoExt,
                            tag = fa.tag.ToString(),
                            tenantId = Int32.Parse(fa.tenantId.ToString()),
                            typologyArea = typologyAreaId,
                            uploadDate = DateTime.Parse(fa.uploadDate.ToString()),
                            userGuid = fa.userGuid.ToString(),
                            primaryContentType = contentTypeConvertedFile != "" ? contentTypeConvertedFile.Split('/')[0] : file.ContentType.Split('/')[0],
                            global = bool.Parse(fa.global.ToString())
                        };

                        // CREAZIONE CARTELLA
                        bool exists = System.IO.Directory.Exists(_uploadsPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(_uploadsPath); // lo creo se non esiste
                        }

                        string subDirBig, subDirMedium, subDirSmall;
                        string subDir = Path.Combine(_uploadsPath + "/" + mediafile.typologyArea);

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir = subDir + "/" + mediafile.category;
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        if (mediafile.album != "")
                        {
                            subDir = subDir + "/" + mediafile.album;
                        }

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        mediafile.fileUrl = subDir[1..];

                        subDirBig = subDir + "/big";
                        exists = System.IO.Directory.Exists(subDirBig);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirBig);
                        }

                        subDirMedium = subDir + "/medium";
                        exists = System.IO.Directory.Exists(subDirMedium);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirMedium);
                        }

                        subDirSmall = subDir + "/small";
                        exists = System.IO.Directory.Exists(subDirSmall);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirSmall);
                        }

                        MemoryStream stream = new();
                        file.CopyTo(stream);
                        byte[] imageBytes = stream.ToArray();

                        SKBitmap bigFormat = SKBitmap.Decode(imageBytes);

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

                        using (SKImage image = SKImage.FromBitmap(bigFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] bigImageBytes = data.ToArray();

                        using (SKImage image = SKImage.FromBitmap(mediumFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] mediumImageBytes = data.ToArray();

                        using (SKImage image = SKImage.FromBitmap(smallFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] smallImageBytes = data.ToArray();

                        string bigBase64 = Convert.ToBase64String(bigImageBytes),
                            mediumBase64 = Convert.ToBase64String(mediumImageBytes),
                            smallbase64 = Convert.ToBase64String(smallImageBytes);

                        DocumentFile mongoMediaFile = new()
                        {
                            TenantId = -10, //USED IN GETALL TO FILTER
                            SmallFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + smallbase64,
                            MediumFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + mediumBase64,
                            BigFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + bigBase64,
                            FileGuid = Guid.NewGuid().ToString(),
                        };

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile mongoMediaFileCreated = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(mongoMediaFile);

                        if (mongoMediaFileCreated == null)
                        {
                            Console.WriteLine("[ERROR] MongoDB DocumentManager.CreateAsync returned null");
                            return StatusCode(500, new { error = "Failed to save document to MongoDB" });
                        }

                        mediafile.mongoGuid = mongoMediaFileCreated.FileGuid;

                        Console.WriteLine($"[DEBUG] Saving MediaFile to MySQL - ID: {mediafile.Id}, mongoGuid: {mediafile.mongoGuid}");

                        //scrivo tramite il dbcontext
                        MediaFile response = await _jsonService.CreateAsync<MediaFile, string>(mediafile);

                        if (response == null)
                        {
                            Console.WriteLine("[ERROR] MySQL CreateAsync returned null for MediaFile");
                            Console.WriteLine($"[DEBUG] MediaFile data: Id={mediafile.Id}, extension={mediafile.extension}, tenantId={mediafile.tenantId}");
                            return StatusCode(500, new { error = "Failed to save MediaFile to MySQL. Check database logs and field validations." });
                        }

                        Console.WriteLine($"[SUCCESS] MediaFile saved to MySQL - ID: {response.Id}");

                        SKEncodedImageFormat imageFormat = response.extension switch
                        {
                            "png" => SKEncodedImageFormat.Png,
                            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
                            _ => throw new ArgumentException("Estensione del file non supportata", contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType),
                        };
                        using (SKImage image = SKImage.FromBitmap(bigFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirBig}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        using (SKImage image = SKImage.FromBitmap(mediumFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirMedium}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        using (SKImage image = SKImage.FromBitmap(smallFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirSmall}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        jsonApiResponse += response.ToJson().Replace("CSUUID(", "").Replace("ISODate(", "").Replace("\")", "\"") + "\n\t},";

                    }
                    else
                    {
                        jsonApiResponse += "\n\t\t{\"type\":\"mediaFiles\",\n\t\t\"attributes\":";

                        string[] fileNameArray = file.FileName.Split('.');
                        string fileNameNoExt = "";

                        for (int i = 0; i < fileNameArray.Length - 1; i++)
                        {
                            fileNameNoExt += fileNameArray[i] + ".";
                        }

                        fileNameNoExt = fileNameNoExt[..^1];

                        // Resolve category IDs (GUID or name lookup)
                        string albumId = await ResolveMediaCategoryId(fa.album.ToString());
                        string categoryId = await ResolveMediaCategoryId(fa.category.ToString());
                        string typologyAreaId = await ResolveMediaCategoryId(fa.typologyArea.ToString());

                        if (string.IsNullOrEmpty(typologyAreaId) || string.IsNullOrEmpty(categoryId))
                        {
                            return BadRequest($"TypologyArea '{fa.typologyArea}' or Category '{fa.category}' not found");
                        }

                        MediaFile mediafile = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            album = albumId,
                            alt = fa.alt.ToString(),
                            category = categoryId,
                            extension = fileNameArray.Last().ToLower(),
                            base64 = "",
                            fileUrl = "",
                            mongoGuid = "",
                            originalFileName = fileNameNoExt,
                            tag = fa.tag.ToString(),
                            tenantId = Int32.Parse(fa.tenantId.ToString()),
                            typologyArea = typologyAreaId,
                            uploadDate = DateTime.Parse(fa.uploadDate.ToString()),
                            userGuid = fa.userGuid.ToString(),
                            primaryContentType = contentTypeConvertedFile != "" ? contentTypeConvertedFile.Split('/')[0] : file.ContentType.Split('/')[0]
                        };

                        // CREAZIONE CARTELLA
                        bool exists = System.IO.Directory.Exists(_uploadsPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(_uploadsPath); // lo creo se non esiste
                        }

                        string subDir = Path.Combine(_uploadsPath + "/" + mediafile.typologyArea);

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir = subDir + "/" + mediafile.category;
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        if (mediafile.album != "")
                        {
                            subDir = subDir + "/" + mediafile.album;
                        }

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir += "/files";
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        mediafile.fileUrl = subDir[1..];

                        MemoryStream stream = new();
                        file.CopyTo(stream);
                        byte[] fileBytes = stream.ToArray();
                        string base64 = Convert.ToBase64String(fileBytes);

                        DocumentFile mongoMediaFile = new()
                        {
                            TenantId = -10, //USED IN GETALL TO FILTER
                            FileBase64 = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + base64,
                            FileGuid = Guid.NewGuid().ToString(),
                        };

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile mongoMediaFileCreated = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(mongoMediaFile);

                        mediafile.mongoGuid = mongoMediaFileCreated.FileGuid;

                        MediaFile response = await _jsonService.CreateAsync<MediaFile, string>(mediafile);

                        string path = Path.Combine(subDir, mediafile.mongoGuid + "." + mediafile.extension);

                        using (FileStream localFile = System.IO.File.OpenWrite(path))
                        using (Stream uploadedFile = file.OpenReadStream())
                        {
                            uploadedFile.CopyTo(localFile);
                        }

                        jsonApiResponse += response.ToJson().Replace("CSUUID(", "").Replace("ISODate(", "").Replace("\")", "\"") + "\n\t},";
                    }
                }

                jsonApiResponse = jsonApiResponse[..^1];

                if (coll.Count > 1)
                {
                    jsonApiResponse += "\n]";
                }

                jsonApiResponse += "\n}";

                return StatusCode(201, JsonConvert.DeserializeObject(jsonApiResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace + "\n\n" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace);
            }
        }


        [HttpPost]
        public async Task<IActionResult> FileUpload([FromForm] IFormCollection fa)
        {
            try
            {
                IFormFileCollection coll = fa.Files;

                string contentTypeConvertedFile = "";

                if (coll == null || coll.Count == 0)
                {
                    if (string.IsNullOrEmpty(fa["base64String"].ToString()) || string.IsNullOrEmpty(fa["base64Name"].ToString()))
                    {
                        return BadRequest("File mancanti");
                    }
                    else
                    {
                        string base64String = fa["base64String"].ToString();

                        byte[] bytes = Convert.FromBase64String(base64String[(base64String.IndexOf(',') + 1)..]);
                        MemoryStream stream = new(bytes);

                        FormFile file = new(stream, 0, bytes.Length, fa["base64Name"].ToString(), fa["base64Name"].ToString());

                        contentTypeConvertedFile = base64String.Substring(base64String.IndexOf(':') + 1, base64String.IndexOf(';') - base64String.IndexOf(':') - 1);

                        coll = new FormFileCollection
                        {
                            file
                        };
                    }
                }

                string jsonApiResponse = "{\n\t\"data\":";

                if (coll.Count > 1)
                {
                    jsonApiResponse += "[";
                }

                foreach (FormFile file in coll.Cast<FormFile>())
                {
                    if (acceptedImageFormats.Contains(contentTypeConvertedFile) || acceptedImageFormats.Contains(file.ContentType))
                    {
                        jsonApiResponse += "\n\t\t{\"type\":\"mediaFiles\",\n\t\t\"attributes\":";

                        string[] fileNameArray = file.FileName.Split('.');
                        string fileNameNoExt = "";

                        for (int i = 0; i < fileNameArray.Length - 1; i++)
                        {
                            fileNameNoExt += fileNameArray[i] + ".";
                        }

                        fileNameNoExt = fileNameNoExt[..^1];

                        // Resolve category IDs (GUID or name lookup)
                        string albumId = await ResolveMediaCategoryId(fa["album"].ToString());
                        string categoryId = await ResolveMediaCategoryId(fa["category"].ToString());
                        string typologyAreaId = await ResolveMediaCategoryId(fa["typologyArea"].ToString());

                        if (string.IsNullOrEmpty(typologyAreaId) || string.IsNullOrEmpty(categoryId))
                        {
                            return BadRequest($"TypologyArea '{fa["typologyArea"]}' or Category '{fa["category"]}' not found");
                        }

                        MediaFile mediafile = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            album = albumId,
                            alt = fa["alt"].ToString(),
                            category = categoryId,
                            extension = fileNameArray.Last().ToLower(),
                            base64 = "",
                            fileUrl = "",
                            mongoGuid = "",
                            originalFileName = fileNameNoExt,
                            tag = fa["tag"].ToString(),
                            tenantId = Int32.Parse(fa["tenantId"].ToString()),
                            typologyArea = typologyAreaId,
                            uploadDate = DateTime.Parse(fa["uploadDate"].ToString()),
                            userGuid = fa["userGuid"].ToString(),
                            primaryContentType = contentTypeConvertedFile != "" ? contentTypeConvertedFile.Split('/')[0] : file.ContentType.Split('/')[0],
                            global = bool.Parse(fa["global"].ToString())
                        };

                        // CREAZIONE CARTELLA
                        bool exists = System.IO.Directory.Exists(_uploadsPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(_uploadsPath); // lo creo se non esiste
                        }

                        string subDirBig, subDirMedium, subDirSmall;
                        string subDir = Path.Combine(_uploadsPath + "/" + mediafile.typologyArea);

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir = subDir + "/" + mediafile.category;
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        if (mediafile.album != "")
                        {
                            subDir = subDir + "/" + mediafile.album;
                        }

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        mediafile.fileUrl = subDir[1..];

                        subDirBig = subDir + "/big";
                        exists = System.IO.Directory.Exists(subDirBig);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirBig);
                        }

                        subDirMedium = subDir + "/medium";
                        exists = System.IO.Directory.Exists(subDirMedium);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirMedium);
                        }

                        subDirSmall = subDir + "/small";
                        exists = System.IO.Directory.Exists(subDirSmall);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDirSmall);
                        }

                        MemoryStream stream = new();
                        file.CopyTo(stream);
                        byte[] imageBytes = stream.ToArray();

                        SKBitmap bigFormat = SKBitmap.Decode(imageBytes);

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

                        using (SKImage image = SKImage.FromBitmap(bigFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] bigImageBytes = data.ToArray();

                        using (SKImage image = SKImage.FromBitmap(mediumFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] mediumImageBytes = data.ToArray();

                        using (SKImage image = SKImage.FromBitmap(smallFormat))
                        {
                            data = image.Encode();
                        }

                        byte[] smallImageBytes = data.ToArray();

                        string bigBase64 = Convert.ToBase64String(bigImageBytes),
                            mediumBase64 = Convert.ToBase64String(mediumImageBytes),
                            smallbase64 = Convert.ToBase64String(smallImageBytes);

                        DocumentFile mongoMediaFile = new()
                        {
                            TenantId = -10, //USED IN GETALL TO FILTER
                            SmallFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + smallbase64,
                            MediumFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + mediumBase64,
                            BigFormat = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + bigBase64,
                            FileGuid = Guid.NewGuid().ToString(),
                        };

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile mongoMediaFileCreated = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(mongoMediaFile);

                        if (mongoMediaFileCreated == null)
                        {
                            Console.WriteLine("[ERROR] MongoDB DocumentManager.CreateAsync returned null");
                            return StatusCode(500, new { error = "Failed to save document to MongoDB" });
                        }

                        mediafile.mongoGuid = mongoMediaFileCreated.FileGuid;

                        Console.WriteLine($"[DEBUG] Saving MediaFile to MySQL - ID: {mediafile.Id}, mongoGuid: {mediafile.mongoGuid}");

                        //scrivo tramite il dbcontext
                        MediaFile response = await _jsonService.CreateAsync<MediaFile, string>(mediafile);

                        if (response == null)
                        {
                            Console.WriteLine("[ERROR] MySQL CreateAsync returned null for MediaFile");
                            Console.WriteLine($"[DEBUG] MediaFile data: Id={mediafile.Id}, extension={mediafile.extension}, tenantId={mediafile.tenantId}");
                            return StatusCode(500, new { error = "Failed to save MediaFile to MySQL. Check database logs and field validations." });
                        }

                        Console.WriteLine($"[SUCCESS] MediaFile saved to MySQL - ID: {response.Id}");

                        SKEncodedImageFormat imageFormat = response.extension switch
                        {
                            "png" => SKEncodedImageFormat.Png,
                            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
                            _ => throw new ArgumentException("Estensione del file non supportata", contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType),
                        };
                        using (SKImage image = SKImage.FromBitmap(bigFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirBig}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        using (SKImage image = SKImage.FromBitmap(mediumFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirMedium}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        using (SKImage image = SKImage.FromBitmap(smallFormat))
                        using (SKData encoding = image.Encode(imageFormat, 100))
                        using (FileStream pathStream = System.IO.File.OpenWrite($"{subDirSmall}/{response.Id}.{response.extension}"))
                        {
                            encoding.SaveTo(pathStream);
                        }

                        jsonApiResponse += response.ToJson().Replace("CSUUID(", "").Replace("ISODate(", "").Replace("\")", "\"") + "\n\t},";

                    }
                    else
                    {
                        jsonApiResponse += "\n\t\t{\"type\":\"mediaFiles\",\n\t\t\"attributes\":";

                        string[] fileNameArray = file.FileName.Split('.');
                        string fileNameNoExt = "";

                        for (int i = 0; i < fileNameArray.Length - 1; i++)
                        {
                            fileNameNoExt += fileNameArray[i] + ".";
                        }

                        fileNameNoExt = fileNameNoExt[..^1];

                        // Resolve category IDs (GUID or name lookup)
                        string albumId = await ResolveMediaCategoryId(fa["album"].ToString());
                        string categoryId = await ResolveMediaCategoryId(fa["category"].ToString());
                        string typologyAreaId = await ResolveMediaCategoryId(fa["typologyArea"].ToString());

                        if (string.IsNullOrEmpty(typologyAreaId) || string.IsNullOrEmpty(categoryId))
                        {
                            return BadRequest($"TypologyArea '{fa["typologyArea"]}' or Category '{fa["category"]}' not found");
                        }

                        MediaFile mediafile = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            album = albumId,
                            alt = fa["alt"].ToString(),
                            category = categoryId,
                            extension = fileNameArray.Last().ToLower(),
                            base64 = "",
                            fileUrl = "",
                            mongoGuid = "",
                            originalFileName = fileNameNoExt,
                            tag = fa["tag"].ToString(),
                            tenantId = Int32.Parse(fa["tenantId"].ToString()),
                            typologyArea = typologyAreaId,
                            uploadDate = DateTime.Parse(fa["uploadDate"].ToString()),
                            userGuid = fa["userGuid"].ToString(),
                            primaryContentType = contentTypeConvertedFile != "" ? contentTypeConvertedFile.Split('/')[0] : file.ContentType.Split('/')[0]
                        };

                        // CREAZIONE CARTELLA
                        bool exists = System.IO.Directory.Exists(_uploadsPath);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(_uploadsPath); // lo creo se non esiste
                        }

                        string subDir = Path.Combine(_uploadsPath + "/" + mediafile.typologyArea);

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir = subDir + "/" + mediafile.category;
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        if (mediafile.album != "")
                        {
                            subDir = subDir + "/" + mediafile.album;
                        }

                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        subDir += "/files";
                        exists = System.IO.Directory.Exists(subDir);
                        if (!exists)
                        {
                            System.IO.Directory.CreateDirectory(subDir);
                        }

                        mediafile.fileUrl = subDir[1..];

                        MemoryStream stream = new();
                        file.CopyTo(stream);
                        byte[] fileBytes = stream.ToArray();
                        string base64 = Convert.ToBase64String(fileBytes);

                        DocumentFile mongoMediaFile = new()
                        {
                            TenantId = -10, //USED IN GETALL TO FILTER
                            FileBase64 = $"data:{(contentTypeConvertedFile != "" ? contentTypeConvertedFile : file.ContentType)};base64," + base64,
                            FileGuid = Guid.NewGuid().ToString(),
                        };

                        // FASE 8A: Use DocumentManager.CreateAsync instead of obsolete Create()
                        DocumentFile mongoMediaFileCreated = await MIT.Fwk.Infrastructure.Data.NoSql.Document.DocumentManager.CreateAsync(mongoMediaFile);

                        mediafile.mongoGuid = mongoMediaFileCreated.FileGuid;

                        MediaFile response = await _jsonService.CreateAsync<MediaFile, string>(mediafile);

                        string path = Path.Combine(subDir, mediafile.mongoGuid + "." + mediafile.extension);

                        using (FileStream localFile = System.IO.File.OpenWrite(path))
                        using (Stream uploadedFile = file.OpenReadStream())
                        {
                            uploadedFile.CopyTo(localFile);
                        }

                        jsonApiResponse += response.ToJson().Replace("CSUUID(", "").Replace("ISODate(", "").Replace("\")", "\"") + "\n\t},";
                    }
                }

                jsonApiResponse = jsonApiResponse[..^1];

                if (coll.Count > 1)
                {
                    jsonApiResponse += "\n]";
                }

                jsonApiResponse += "\n}";

                return StatusCode(201, JsonConvert.DeserializeObject(jsonApiResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message + "\n" + ex.StackTrace + "\n\n" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace);
            }
        }
    }
}
