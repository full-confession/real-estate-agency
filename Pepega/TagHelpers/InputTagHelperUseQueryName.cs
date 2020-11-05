using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pepega.TagHelpers
{

	[HtmlTargetElement("input", Attributes = "asp-name-for", TagStructure = TagStructure.WithoutEndTag)]
	public class InputTagHelperUseQueryName : TagHelper
	{
		[HtmlAttributeName("asp-name-for")]
		public ModelExpression For { get; set; }


		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			base.Process(context, output);

			var prop = For.Metadata.ContainerType.GetProperty(For.Metadata.PropertyName);
			var name = prop.GetCustomAttribute(typeof(FromQueryAttribute)) as FromQueryAttribute;
			output.Attributes.SetAttribute("name", name.Name);
		}
	}
}
