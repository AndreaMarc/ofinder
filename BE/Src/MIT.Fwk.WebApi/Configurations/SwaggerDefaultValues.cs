namespace MIT.Fwk.WebApi.Configurations
{
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Linq;

    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to document the implicit API version parameter.
    /// </summary>
    /// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
    /// Once they are fixed and published, this class can be removed.</remarks>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Defensive null checks for Swashbuckle 10.x + OpenAPI 2.x compatibility
            if (operation == null || context?.ApiDescription == null)
                return;

            Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription apiDescription = context.ApiDescription;

            //operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
            {
                return;
            }

            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
            foreach (OpenApiParameter parameter in operation.Parameters)
            {
                Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription description = apiDescription.ParameterDescriptions.FirstOrDefault(p => p.Name == parameter.Name);

                // Skip if no matching parameter description found (e.g., JsonAPI auto-generated parameters)
                if (description == null)
                    continue;

                if (parameter.Description == null)
                {
                    parameter.Description = description.ModelMetadata?.Description;
                }

                // Note: In OpenAPI 2.x (Swashbuckle 10+), Schema.Default is readonly
                // The framework now handles default values automatically
                // if (parameter.Schema.Default == null && description.DefaultValue != null)
                // {
                //     parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                // }

                parameter.Required |= description.IsRequired;
            }

            string customMediaType = "application/vnd.api+json";

            // Skip if operation has no responses defined
            if (operation.Responses == null)
                return;

            foreach (System.Collections.Generic.KeyValuePair<string, IOpenApiResponse> response in operation.Responses)
            {
                // Skip if response has no content (e.g., 204 No Content, 304 Not Modified)
                if (response.Value.Content == null)
                    continue;

                foreach (System.Collections.Generic.KeyValuePair<string, OpenApiMediaType> schema in response.Value.Content.Where(x => x.Key == "application/json").ToList())
                {
                    response.Value.Content[customMediaType] = schema.Value;
                }
            }

            // Aggiungi il MediaType personalizzato ai parametri del corpo della richiesta
            IOpenApiRequestBody requestBody = operation.RequestBody;
            if (requestBody != null && requestBody.Content != null)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, OpenApiMediaType> schema in requestBody.Content.Where(x => x.Key == "application/json").ToList())
                {
                    requestBody.Content.Clear();
                    requestBody.Content[customMediaType] = schema.Value;
                }
            }
        }
    }
}
