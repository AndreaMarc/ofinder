using MIT.Fwk.Core.CQRS;
using MIT.Fwk.Core.IoC;
using MIT.Fwk.Core.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace MIT.Fwk.Core.Domain.Interfaces
{
    /// <summary>
    /// Marker interface for JsonAPI DbContext auto-discovery.
    /// Implement this interface on custom DbContext classes to enable auto-registration.
    /// </summary>
    public interface IJsonApiDbContext
    {
    }

    // FASE 8E: IUnitOfWork removed - never registered in DI, not used
    // Modern approach: use DbContext.SaveChangesAsync() directly

    /// <summary>
    /// Contract for document entities (files, images, etc.).
    /// </summary>
    public interface IDocument
    {
        long Id { get; set; }
        int TenantId { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string FileName { get; set; }
        string Extension { get; set; }
        string FileGuid { get; set; }
        string SmallFormat { get; set; }
        string MediumFormat { get; set; }
        string BigFormat { get; set; }
        string Meta { get; set; }
        byte[] BinaryData { get; set; }
    }

    /// <summary>
    /// Contract for framework log entries.
    /// </summary>
    public interface IFwkLog
    {
        long Id { get; set; }
        string LogType { get; set; }
        MinimalUserInfo User { get; set; }
        DateTime Date { get; set; }
        string Data { get; set; }
    }

    // FASE 8E: IDTO removed - was only used by BaseTemplateMapper (also removed)
    // Modern approach: use AutoMapper IMapper with explicit DTO classes

    /// <summary>
    /// Contract for user identity.
    /// </summary>
    public interface IUser
    {
        string Name { get; }
        bool IsAuthenticated();
        IEnumerable<Claim> GetClaimsIdentity();
    }
}
