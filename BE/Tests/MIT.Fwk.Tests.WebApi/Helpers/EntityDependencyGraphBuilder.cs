using JsonApiDotNetCore.Resources.Annotations;
using System.Reflection;

namespace MIT.Fwk.Tests.WebApi.Helpers
{
    /// <summary>
    /// Builds a dependency graph for entity types based on [HasOne] relationships.
    /// Uses topological sort to determine the correct order for CRUD operations.
    /// </summary>
    public class EntityDependencyGraphBuilder
    {
        private readonly List<Type> _entityTypes;
        private readonly Dictionary<Type, List<Type>> _adjacencyList;
        private readonly Dictionary<Type, int> _inDegree;

        public EntityDependencyGraphBuilder(List<Type> entityTypes)
        {
            _entityTypes = entityTypes;
            _adjacencyList = new Dictionary<Type, List<Type>>();
            _inDegree = new Dictionary<Type, int>();

            BuildGraph();
        }

        /// <summary>
        /// Builds the dependency graph by analyzing [HasOne] relationships.
        /// Edge: A → B means "A depends on B" (A has a FK to B).
        /// </summary>
        private void BuildGraph()
        {
            // Initialize adjacency list and in-degree for all entities
            foreach (var entityType in _entityTypes)
            {
                _adjacencyList[entityType] = new List<Type>();
                _inDegree[entityType] = 0;
            }

            // Build edges based on [HasOne] relationships
            foreach (var entityType in _entityTypes)
            {
                var relationships = EntityReflectionHelper.GetHasOneRelationships(entityType);

                foreach (var (propertyName, relatedType) in relationships)
                {
                    // Check if the related type is in our entity list
                    if (_entityTypes.Contains(relatedType))
                    {
                        // Edge: entityType depends on relatedType
                        // For creation order: relatedType must be created before entityType
                        _adjacencyList[relatedType].Add(entityType);
                        _inDegree[entityType]++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns entities in topological order (for creation).
        /// Entities without dependencies come first.
        /// </summary>
        public List<Type> GetTopologicalOrder()
        {
            var result = new List<Type>();
            var queue = new Queue<Type>();
            var localInDegree = new Dictionary<Type, int>(_inDegree);

            // Start with entities that have no dependencies
            foreach (var entityType in _entityTypes)
            {
                if (localInDegree[entityType] == 0)
                {
                    queue.Enqueue(entityType);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                // Process all entities that depend on the current entity
                foreach (var dependent in _adjacencyList[current])
                {
                    localInDegree[dependent]--;
                    if (localInDegree[dependent] == 0)
                    {
                        queue.Enqueue(dependent);
                    }
                }
            }

            // Check for circular dependencies
            if (result.Count != _entityTypes.Count)
            {
                var missing = _entityTypes.Except(result).Select(t => t.Name).ToList();
                throw new InvalidOperationException(
                    $"Circular dependency detected. Cannot process entities: {string.Join(", ", missing)}");
            }

            return result;
        }

        /// <summary>
        /// Returns entities in reverse topological order (for deletion).
        /// Entities with dependencies are deleted first.
        /// </summary>
        public List<Type> GetReverseDeletionOrder()
        {
            var creationOrder = GetTopologicalOrder();
            creationOrder.Reverse();
            return creationOrder;
        }

        /// <summary>
        /// Gets a human-readable dependency tree for debugging.
        /// </summary>
        public string GetDependencyTreeString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Entity Dependency Graph:");
            sb.AppendLine("------------------------");

            foreach (var entityType in _entityTypes.OrderBy(t => t.Name))
            {
                sb.AppendLine($"{entityType.Name}:");

                var relationships = EntityReflectionHelper.GetHasOneRelationships(entityType);
                if (relationships.Count == 0)
                {
                    sb.AppendLine("  (no dependencies)");
                }
                else
                {
                    foreach (var (propertyName, relatedType) in relationships)
                    {
                        sb.AppendLine($"  → depends on {relatedType.Name} (via {propertyName})");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets statistics about the dependency graph.
        /// </summary>
        public (int TotalEntities, int EntitiesWithDependencies, int EntitiesWithoutDependencies) GetStatistics()
        {
            int withDeps = _inDegree.Count(kv => kv.Value > 0);
            int withoutDeps = _inDegree.Count(kv => kv.Value == 0);

            return (_entityTypes.Count, withDeps, withoutDeps);
        }
    }
}
