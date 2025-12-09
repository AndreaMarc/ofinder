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
                Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

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

            foreach (System.Collections.Generic.KeyValuePair<string, IOpenApiResponse> response in operation.Responses)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, OpenApiMediaType> schema in response.Value.Content.Where(x => x.Key == "application/json").ToList())
                {
                    response.Value.Content[customMediaType] = schema.Value;
                }
            }

            // Aggiungi il MediaType personalizzato ai parametri del corpo della richiesta
            IOpenApiRequestBody requestBody = operation.RequestBody;
            if (requestBody != null)
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
