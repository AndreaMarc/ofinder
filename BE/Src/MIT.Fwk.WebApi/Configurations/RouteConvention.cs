using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Linq;

namespace MIT.Fwk.WebApi.Configurations
{
    public class RouteConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public RouteConvention(IRouteTemplateProvider routeTemplateProvider)
        {
            _centralPrefix = new AttributeRouteModel(routeTemplateProvider);
        }

        public void Apply(ApplicationModel application)
        {
            foreach (ControllerModel controller in application.Controllers)
            {
                System.Collections.Generic.List<SelectorModel> matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                if (matchedSelectors.Any())
                {
                    foreach (SelectorModel selectorModel in matchedSelectors)
                    {
                        selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix,
                            selectorModel.AttributeRouteModel);
                    }
                }

                System.Collections.Generic.List<SelectorModel> unmatchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel == null).ToList();

                if (unmatchedSelectors.Any())
                {
                    foreach (SelectorModel selectorModel in unmatchedSelectors)
                    {
                        selectorModel.AttributeRouteModel = _centralPrefix;
                    }
                }
            }
        }
    }

    public static class MvcOptionsExtensions
    {
        public static void UseCentralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Insert(0, new RouteConvention(routeAttribute));
        }
    }
}