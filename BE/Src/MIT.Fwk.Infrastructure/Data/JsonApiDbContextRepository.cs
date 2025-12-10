using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MIT.Fwk.Infrastructure.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class JsonApiDbContextRepository<TResource, TId>(
    ITargetedFields targetedFields, DbContextResolver<JsonApiDbContext> dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<TResource, TId>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<TId>;