using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bookify.Web.Helpers
{
    [HtmlTargetElement("a", Attributes = "active-when")]
    public class ActiveTag : TagHelper
    {
        public string? ActiveWhen { get; set; }
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContextDate { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var v = output.PreElement;
            if (string.IsNullOrEmpty(ActiveWhen))
            {
                return;
            }
            var currentController = ViewContextDate?.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            if (currentController!.Equals(ActiveWhen))
            {
                if (output.Attributes.ContainsName("class"))
                    output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} active");
                else
                    output.Attributes.SetAttribute("class", "active");

            }
        }
    }
}
