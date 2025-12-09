using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace MIT.Fwk.Examples.Data;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class OtherDbContextRepository<TResource, TId>(
    ITargetedFields targetedFields, DbContextResolver<OtherDbContext> dbContextResolver, IResourceGraph resourceGraph, IResourceFactory resourceFactory,
    IEnumerable<IQueryConstraintProvider> constraintProviders, ILoggerFactory loggerFactory, IResourceDefinitionAccessor resourceDefinitionAccessor)
    : EntityFrameworkCoreRepository<TResource, TId>(targetedFields, dbContextResolver, resourceGraph, resourceFactory, constraintProviders, loggerFactory,
        resourceDefinitionAccessor)
    where TResource : class, IIdentifiable<TId>;