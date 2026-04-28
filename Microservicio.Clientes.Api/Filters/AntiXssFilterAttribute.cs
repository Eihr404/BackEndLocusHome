using Ganss.Xss;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace Microservicio.Clientes.Api.Filters;

public class AntiXssFilterAttribute : ActionFilterAttribute
{
    private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var actionArgument in context.ActionArguments.Values)
        {
            if (actionArgument != null)
            {
                SanitizeProperties(actionArgument);
            }
        }
        base.OnActionExecuting(context);
    }

    private void SanitizeProperties(object obj)
    {
        if (obj == null) return;

        var type = obj.GetType();
        
        // Evitar sanitizar tipos primitivos directamente en la iteraciA3n raíz
        if (type.IsPrimitive || type == typeof(string) || type.IsValueType)
            return;

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                             .Where(p => p.CanRead && p.CanWrite);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var value = property.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(value))
                {
                    // Sanitiza el string removiendo tags HTML peligrosos
                    var sanitizedValue = _sanitizer.Sanitize(value);
                    property.SetValue(obj, sanitizedValue);
                }
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedValue = property.GetValue(obj);
                if (nestedValue != null)
                {
                    SanitizeProperties(nestedValue);
                }
            }
        }
    }
}
