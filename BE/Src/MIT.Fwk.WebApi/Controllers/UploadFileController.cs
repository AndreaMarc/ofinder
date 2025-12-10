using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MIT.Fwk.Core.Attributes;
using MIT.Fwk.Core.Helpers;
using MIT.Fwk.Core.Options;
using MIT.Fwk.Infrastructure.Data;
using MIT.Fwk.Infrastructure.Entities;
using MIT.Fwk.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace MIT.Fwk.WebApi.Controllers

{
    [Authorize]
    [SkipClaimsValidation(JwtHttpMethod.GET)]
    public class UploadFileController : ControllerBase
    {
        private readonly JsonApiDbContext _context;
        private readonly IMapper _mapper;
        protected ILogger _logger;
        private readonly string _uploadsPath;

        public UploadFileController(
            JsonApiDbContext context,
            IMapper mapper,
            UserManager<MITApplicationUser> userManager,
            RoleManager<MITApplicationRole> roleManager,
            IOptions<FileUploadOptions> fileUploadOptions,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<UploadFileController>();
            _uploadsPath = fileUploadOptions.Value.UploadsPath;
        }

        #region UploadFile

        [HttpGet]
        [AllowAnonymous]
        [Route("UploadFile")]
        public async Task<IActionResult> Get()
        {
            List<UploadFile> entities = await _context.UploadFiles
                .AsNoTracking()
                .ToListAsync();
            List<UploadFileDTO> dtos = _mapper.Map<List<UploadFileDTO>>(entities);
            return Ok(new { success = true, data = dtos });
        }



        [HttpGet]
        [AllowAnonymous]
        [Route("UploadFile/{id}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            UploadFile entity = await _context.UploadFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UploadId == id);

            if (entity == null)
            {
                return NotFound();
            }

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);
            return Ok(new { success = true, data = dto });
        }


        [HttpPost]
        [Route("UploadFile")]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] UploadFileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UploadFile entity = _mapper.Map<UploadFile>(dto);
            _context.UploadFiles.Add(entity);
            await _context.SaveChangesAsync();

            UploadFileDTO retDto = _mapper.Map<UploadFileDTO>(entity);
            return Ok(new { success = true, data = retDto });
        }




        [HttpDelete]
        [Route("UploadFile/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(Guid id)
        {
            UploadFile entity = await _context.UploadFiles
                .FirstOrDefaultAsync(f => f.UploadId == id);

            if (entity == null)
            {
                return NotFound();
            }

            _context.UploadFiles.Remove(entity);
            await _context.SaveChangesAsync();

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);
            return Ok(new { success = true, data = dto });
        }


        [HttpPut]
        [AllowAnonymous]
        [Route("UploadFile")]
        public async Task<IActionResult> Put([FromBody] UploadFileDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UploadFile entity = _mapper.Map<UploadFile>(dto);
            _context.UploadFiles.Update(entity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, data = dto });
        }
        #endregion

        #region Custom
        // CARICA FILE SUL SERVER
        [HttpPost]
        [Route("Process")]
        [AllowAnonymous]
        public async Task<IActionResult> Process()
        {
            #region getParams
            // estraggo i dati aggiuntivi
            StringValues value1;
            Request.Headers.TryGetValue("typology", out value1);
            string typology = value1.FirstOrDefault();

            StringValues value2;
            Request.Headers.TryGetValue("album", out value2);
            string album = value2.FirstOrDefault();

            StringValues value3;
            Request.Headers.TryGetValue("userGuid", out value3);
            string userGuid = value3.FirstOrDefault();

            StringValues value4;
            Request.Headers.TryGetValue("oneOnlyImage", out value4);
            string oneOnlyImage = value4.FirstOrDefault();
            #endregion



            if (typology == "" || userGuid == "")
            {
                return BadRequest("Errore di sistema");
            }


            Microsoft.AspNetCore.Http.IFormFileCollection files = Request.Form.Files; // estraggo i file caricati
            long now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            string ext = "";
            string generalPath = "";
            string fName = "";
            string fileSize = "";

            foreach (Microsoft.AspNetCore.Http.IFormFile f in files)
            {
                // imposto il percorso di salvataggio
                bool exists = System.IO.Directory.Exists(_uploadsPath);
                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(_uploadsPath); // lo creo se non esiste
                }

                string subDir = Path.Combine(_uploadsPath + "/" + userGuid);

                exists = System.IO.Directory.Exists(subDir);
                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(subDir); // lo creo se non esiste
                }

                subDir = subDir + "/" + typology;
                exists = System.IO.Directory.Exists(subDir);
                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(subDir); // lo creo se non esiste
                }

                if (album != "")
                {
                    subDir = subDir + "/" + album;
                }

                exists = System.IO.Directory.Exists(subDir);
                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(subDir); // lo creo se non esiste
                }

                ext = Path.GetExtension(f.FileName);
                string nomeFile = now.ToString(); // rinomino il file

                bool savePath = false;
                if (f.FileName.Contains("thumb_medium_"))
                {
                    nomeFile = "thumb_medium_" + nomeFile;
                }
                else if (f.FileName.Contains("thumb_small_"))
                {
                    nomeFile = "thumb_small_" + nomeFile;
                }
                else
                {
                    fName = nomeFile;
                    savePath = true;
                    fileSize = f.Length.ToString();
                }



                string path = Path.Combine(_uploadsPath, subDir, nomeFile + ext); // usato per salvare il file
                string path_to_db = Path.Combine(_uploadsPath, subDir); // usato per la memorizzazione sul db
                if (savePath == true)
                {
                    generalPath = path_to_db;
                }

                using FileStream stream = new(path, FileMode.Create);
                f.CopyTo(stream);
            }

            // Creo il DTO 
            UploadFileDTO dto = new()
            {
                UploadId = Guid.NewGuid(),
                UploadUid = userGuid,
                UploadType = typology,
                UploadSrc = generalPath,
                UploadFileName = fName,
                UploadActive = 1,
                //dto.UploadCreationDate = DateTime.Now;
                UploadAlbum = (album != null) ? album : "",
                UploadExtension = ext,
                UploadFileSize = fileSize
            };

            if (!ModelState.IsValid)
            {
                return BadRequest(dto.ToString());
            }

            if (oneOnlyImage == "true")
            {
                // disattivo eventuali record precedenti
                List<UploadFile> previousFiles = await _context.UploadFiles
                    .Where(f => f.UploadUid == dto.UploadUid
                             && f.UploadType == dto.UploadType
                             && f.UploadAlbum == dto.UploadAlbum)
                    .ToListAsync();

                foreach (UploadFile file in previousFiles)
                {
                    file.UploadActive = 0;
                }
                await _context.SaveChangesAsync();
            }

            // Ora salvo le info nel DB
            UploadFile entity = _mapper.Map<UploadFile>(dto);
            _context.UploadFiles.Add(entity);
            await _context.SaveChangesAsync();

            UploadFileDTO retDto = _mapper.Map<UploadFileDTO>(entity);

            return Ok(new { retDto });

        }




        // CANCELLA (DISATTIVA) UN FILE
        [HttpDelete]
        [Route("Revert")]
        [AllowAnonymous]
        public async Task<IActionResult> Revert()
        {
            string content = await new StreamReader(Request.Body).ReadToEndAsync();
            dynamic stuff1 = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            Guid id = stuff1["uploadId"];

            UploadFile entity = await _context.UploadFiles
                .FirstOrDefaultAsync(f => f.UploadId == id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.UploadActive = 0;
            await _context.SaveChangesAsync();

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);
            return Ok(new { success = true, data = dto });
        }

        [HttpDelete]
        [Route("DelPic/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DelPic(Guid id)
        {
            UploadFile entity = await _context.UploadFiles
                .FirstOrDefaultAsync(f => f.UploadId == id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.UploadActive = 0;
            await _context.SaveChangesAsync();

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);
            return Ok(new { success = true, data = dto });
        }




        // CERCA FOTO UTENTE
        // Recupera le foto associate ad un utente, della stessa tipologia ed album
        [HttpGet]
        [AllowAnonymous]
        [Route("UploadFileGetByDetails")]
        public async Task<IActionResult> UploadFileGetByDetails([FromQuery] Guid userId, [FromQuery] string typology, [FromQuery] string album)
        {
            List<UploadFile> results = await _context.UploadFiles
                .AsNoTracking()
                .Where(f => f.UploadActive >= 1
                         && f.UploadUid == userId.ToString()
                         && f.UploadType == typology
                         && f.UploadAlbum == album)
                .OrderByDescending(f => f.UploadCreationDate)
                .ToListAsync();

            List<UploadFileDTO> dtos = _mapper.Map<List<UploadFileDTO>>(results);
            return Ok(new { success = true, data = dtos });
        }



        // LOAD
        [HttpGet]
        [AllowAnonymous]
        [Route("Load/{id}")]
        public async Task<HttpResponseMessage> Load(string id)
        {
            UploadFile entity = await _context.UploadFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UploadId == Guid.Parse(id));

            if (entity == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);

            // ricreo il file da restituire al browser
            MemoryStream stream = new();
            HttpResponseMessage result = new(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };

            result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = dto.UploadSrc + dto.UploadFileName + dto.UploadExtension
            };


            //var contentType = MimeTypeMap.GetMimeType("jpg");
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // qui bisogna inserire il vero content-type

            return result;
        }

        // GET IMAGE
        [HttpGet]
        [AllowAnonymous]
        [Route("Get/{id}")]
        public async Task<IActionResult> Get(string id, [FromQuery] string size = "B") // B = big, M = medium, S = small
        {
            UploadFile entity = await _context.UploadFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UploadId == Guid.Parse(id));

            if (entity == null)
            {
                return NotFound();
            }

            UploadFileDTO dto = _mapper.Map<UploadFileDTO>(entity);

            string contentType = MimeTypes.GetMimeType(dto.UploadExtension);

            // seleziono il file in base alla grandezza desiderata
            string src = dto.UploadSrc + dto.UploadFileName + dto.UploadExtension;
            if (size == "M")
            {
                src = dto.UploadSrc + "thumb_medium_" + dto.UploadFileName + dto.UploadExtension;
            }
            else if (size == "S")
            {
                src = dto.UploadSrc + "thumb_small_" + dto.UploadFileName + dto.UploadExtension;
            }

            MemoryStream memory = new();
            using (FileStream stream = new(src, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, contentType); //, dto.UploadFileNameWithExtension

        }


        #endregion

    }
}
