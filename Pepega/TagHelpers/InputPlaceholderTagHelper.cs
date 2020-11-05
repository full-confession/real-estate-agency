using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pepega.TagHelpers
{
	[HtmlTargetElement("input", Attributes = PlaceholderAttributeName, TagStructure = TagStructure.WithoutEndTag)]
	public class InputPlaceholderTagHelper : InputTagHelper
	{
		private const string PlaceholderAttributeName = "asp-placeholder-for";

		public InputPlaceholderTagHelper(IHtmlGenerator generator) 
			: base(generator)
		{ }

		[HtmlAttributeName(PlaceholderAttributeName)]
		public ModelExpression Placeholder { get; set; }


		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(context, output);

			var placeholder = GetPlaceholder(Placeholder.ModelExplorer);
			TagHelperAttribute placeholderAttribute;

			if (!output.Attributes.TryGetAttribute("placeholder", out placeholderAttribute))
			{
				output.Attributes.Add(new TagHelperAttribute("placeholder", placeholder));
			}
		}

		private static string GetPlaceholder(ModelExplorer modelExplorer)
		{
			var placeholder = modelExplorer.Metadata.Placeholder;

			if (string.IsNullOrWhiteSpace(placeholder))
			{
				placeholder = modelExplorer.Metadata.GetDisplayName();
			}

			return placeholder;
		}
	}
}
