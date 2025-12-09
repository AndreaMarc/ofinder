using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;

namespace MIT.Fwk.CodeGenerator.Services
{
    /// <summary>
    /// Simple template engine for code generation.
    /// Supports:
    /// - {{PropertyName}} - Simple placeholder replacement
    /// - {{#Collection}} ... {{/Collection}} - Loop over collection
    /// - {{#If Property}} ... {{/If}} - Conditional rendering
    /// </summary>
    public class TemplateEngine
    {
        /// <summary>
        /// Renders a template with the provided data.
        /// </summary>
        /// <param name="template">Template string</param>
        /// <param name="data">Data object or dictionary</param>
        /// <returns>Rendered string</returns>
        public string Render(string template, object data)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            string result = template;

            // Step 1: Process loops {{#Collection}} ... {{/Collection}}
            result = ProcessLoops(result, data);

            // Step 2: Process conditionals {{#If Property}} ... {{/If}}
            result = ProcessConditionals(result, data);

            // Step 3: Replace simple placeholders {{PropertyName}}
            result = ReplacePlaceholders(result, data);

            return result;
        }

        /// <summary>
        /// Processes loop blocks in the template.
        /// </summary>
        private string ProcessLoops(string template, object data)
        {
            // Regex to match {{#Collection}} ... {{/Collection}}
            var loopRegex = new Regex(@"\{\{#(\w+)\}\}(.*?)\{\{/\1\}\}", RegexOptions.Singleline);

            return loopRegex.Replace(template, match =>
            {
                string collectionName = match.Groups[1].Value;
                string loopTemplate = match.Groups[2].Value;

                var collection = GetPropertyValue(data, collectionName) as IEnumerable;

                if (collection == null)
                    return string.Empty;

                var sb = new StringBuilder();

                foreach (var item in collection)
                {
                    // Recursively render the loop template for each item
                    string rendered = Render(loopTemplate, item);
                    sb.Append(rendered);
                }

                return sb.ToString();
            });
        }

        /// <summary>
        /// Processes conditional blocks in the template.
        /// </summary>
        private string ProcessConditionals(string template, object data)
        {
            // Regex to match {{#If Property}} ... {{/If}}
            var conditionalRegex = new Regex(@"\{\{#If\s+(\w+)\}\}(.*?)\{\{/If\}\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return conditionalRegex.Replace(template, match =>
            {
                string propertyName = match.Groups[1].Value;
                string conditionalTemplate = match.Groups[2].Value;

                var value = GetPropertyValue(data, propertyName);

                // Check if value is truthy
                bool isTrue = value switch
                {
                    null => false,
                    bool b => b,
                    string s => !string.IsNullOrEmpty(s),
                    int i => i != 0,
                    IEnumerable enumerable => enumerable.Cast<object>().Any(),
                    _ => true
                };

                return isTrue ? conditionalTemplate : string.Empty;
            });
        }

        /// <summary>
        /// Replaces simple placeholders in the template.
        /// </summary>
        private string ReplacePlaceholders(string template, object data)
        {
            // Regex to match {{PropertyName}}
            var placeholderRegex = new Regex(@"\{\{(\w+)\}\}");

            return placeholderRegex.Replace(template, match =>
            {
                string propertyName = match.Groups[1].Value;
                var value = GetPropertyValue(data, propertyName);
                return value?.ToString() ?? string.Empty;
            });
        }

        /// <summary>
        /// Gets property value from an object or dictionary.
        /// </summary>
        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
                return null;

            // Check if it's a dictionary
            if (obj is IDictionary<string, object> dict)
            {
                return dict.TryGetValue(propertyName, out var value) ? value : null;
            }

            // Use reflection to get property value
            var property = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return property?.GetValue(obj);
        }

        /// <summary>
        /// Helper: Converts string to PascalCase.
        /// </summary>
        public static string ToPascalCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Pascalize();
        }

        /// <summary>
        /// Helper: Converts string to camelCase.
        /// </summary>
        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Camelize();
        }

        /// <summary>
        /// Helper: Pluralizes a word.
        /// </summary>
        public static string Pluralize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Pluralize();
        }

        /// <summary>
        /// Helper: Singularizes a word.
        /// </summary>
        public static string Singularize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return input.Singularize();
        }

        /// <summary>
        /// Creates a data dictionary for template rendering.
        /// </summary>
        public static Dictionary<string, object> CreateData(params (string key, object value)[] pairs)
        {
            var dict = new Dictionary<string, object>();
            foreach (var (key, value) in pairs)
            {
                dict[key] = value;
            }
            return dict;
        }
    }
}
