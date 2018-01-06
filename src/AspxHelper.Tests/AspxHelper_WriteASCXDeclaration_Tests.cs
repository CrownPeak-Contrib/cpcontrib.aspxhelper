using CrownPeak.CMSAPI.CustomLibrary;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cpcontrib.aspxhelper_Tests
{
	[TestFixture]
	public class AspxHelper_WriteASCXDeclaration_Tests
	{
		[Test]
		public void WriteASCXDeclaration_gets_name()
		{
			//arrange
			var ascxSrc = "~/Controls/SampleControl.ascx";
			var name = "SampleControl";

			//act
			var output = AspxHelper.WriteASCXDeclaration(ascxSrc);
			
			var registername = Regex.Match(output, @"Name=""([A-Z0-9]*)""", RegexOptions.IgnoreCase);
			var controlname = Regex.Match(output, @"\<ctl\:([A-Z0-9]*)", RegexOptions.IgnoreCase);

			//assert
			Assert.That(name, Is.EqualTo(registername.Groups[1].Value));
			Assert.That(name, Is.EqualTo(controlname.Groups[1].Value));
		}

		public void WriteASCXDeclaration_null_attributes_works()
		{
			//arrange
			var ascxSrc = "~/Controls/SampleControl.ascx";
			var name = "SampleControl";

			string[] attributes = null;

			//act
			var output = AspxHelper.WriteASCXDeclaration(ascxSrc, attributes);

			//assert
			Assert.Pass();
		}

		public void WriteASCXDeclaration_emptyarray_attributes_works()
		{
			//arrange
			var ascxSrc = "~/Controls/SampleControl.ascx";
			var name = "SampleControl";

			string[] attributes = new string[0];

			//act
			var output = AspxHelper.WriteASCXDeclaration(ascxSrc, attributes);

			//assert
			Assert.Pass();
		}

		public void WriteASCXDeclaration_writesattributes()
		{
			//arrange
			var ascxSrc = "~/Controls/SampleControl.ascx";
			var name = "SampleControl";

			var attributes = new string[] { "Property1=Value", "Property2=Value2" };

			//act
			var output = AspxHelper.WriteASCXDeclaration(ascxSrc, attributes);

			//assert
			Assert.Pass();
		}
	}
}
