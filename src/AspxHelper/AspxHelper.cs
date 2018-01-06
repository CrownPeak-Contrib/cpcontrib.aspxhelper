using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrownPeak.CMSAPI;
using CrownPeak.CMSAPI.Services;
/* Some Namespaces are not allowed. */
namespace CrownPeak.CMSAPI.CustomLibrary
{

	public class AspxHelper
	{
		//#require("cpcontrib.cplog@^0.1.0")

#if cpcontrib_cplog
		//private static readonly ILog Log = Logger.GetLogger.GetCurrentClass();
#else
		private static LogStub Log;
		private class LogStub
		{
			[System.Diagnostics.Conditional("cpcontrib_cplog")]
			public void Debug(params object[] args) { }

			[System.Diagnostics.Conditional("cpcontrib_cplog")]
			public void Info(params object[] args) { }

			[System.Diagnostics.Conditional("cpcontrib_cplog")]
			public void Error(params object[] args) { }

			[System.Diagnostics.Conditional("cpcontrib_cplog")]
			public void Trace(params object[] args) { }

			public bool IsDebugEnabled { get { return false; } }
			public bool IsTraceEnabled { get { return false; } }
		}
#endif

		public const string DEFAULT_PAGE_INHERITS = "System.Web.UI.Page";
		public const string DEFAULT_PAGE_LANGUAGE = "C#";

		public static string REPLACE_GETWRAPCONTENTPLACEHOLDER = "@ReplaceGetWrapContentPlaceholder";

		/// <summary>
		/// Looks up the path of folders for what can be identified as a Site root folder, which typically contains a _Web folder asset for deployment of supporting items.
		/// </summary>
		/// <param name="asset"></param>
		/// <param name="rootIdentifier"> 
		/// Special folder contained within the site root folder that
		/// can be used to identify a folder as the root of the site.
		/// </param>
		/// <returns></returns>
		public static Asset GetSiteRoot(Asset asset, string rootIdentifier = "_Global")
		{
			Asset result = null;

			if(rootIdentifier.StartsWith("/"))
			{
				Log.Debug("rootIdentifier startswith /");
				string siteRootPath = rootIdentifier.Substring(0, rootIdentifier.IndexOf("/", 1));
				Log.Debug("siteRootPath={0}", siteRootPath);
				result = Asset.Load(siteRootPath);
				return result;
			}

			if(null != asset)
			{
				Asset current = asset;

				while(null != current && current.AssetPath.Count > 0)
				{
					if(current.GetFolderList().Count(x => x.Label == rootIdentifier) == 1)
					{
						result = current;
						break;
					}

					current = current.Parent;
				}
			}

			return result;
		}

		public static Asset GetMasterPage(Asset asset, int masterTemplateId, string masterIdentifier = "_Master")
		{
			try
			{
				Asset result = null;

				Asset masterFolder = GetSiteRoot(asset, masterIdentifier);
				if(Log.IsDebugEnabled) Log.Debug("GetSiteRoot returns {0}", masterFolder);

				if(null != masterFolder)
				{
					FilterParams filter = new FilterParams();
					filter.Limit = 1;
					filter.Add(AssetPropertyNames.TemplateId, Comparison.Equals, masterTemplateId);

					result = masterFolder.GetFilterList(filter).FirstOrDefault(x => x.WorkflowStatus.Name == asset.WorkflowStatus.Name);

					if(null == result)
					{
						filter.SetFilterStatus(new string[] { "", "Draft", "Stage", "Live" });
						result = masterFolder.GetFilterList(filter).FirstOrDefault();
					}
				}

				if(Log.IsTraceEnabled) Log.Trace("getmasterpage result={0}", result);
				return result;
			}
			catch(Exception ex)
			{
				Log.Error("GetMasterPage", ex);
				throw;
			}
		}

		/// <summary>
		/// Wraps an ASPX page with the master page.  When previewing, will call Out.Wrap.  When publishing, will output the Page Directive to use a specified Masterpage.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="asset"></param>
		/// <param name="masterTemplateId"></param>
		/// <param name="pageInherits"></param>
		/// <param name="pageLanguage"></param>
		/// <param name="masterPageType">This is the Nav Wrap templateid</param>
		/// <param name="extraParams"></param>
		public static void WrapAspxMaster(OutputContext context, Asset asset, int masterTemplateId,
			string pageInherits = DEFAULT_PAGE_INHERITS, string pageLanguage = DEFAULT_PAGE_LANGUAGE,
			string masterPageType = "", string extraParams = "")
		{
			try
			{
				Asset masterPageAsset = GetMasterPage(asset, masterTemplateId);
				WrapAspxMaster(context, asset, masterPageAsset, pageInherits, pageLanguage, masterPageType, extraParams);
			}
			catch(Exception ex)
			{
				Log.Error("WrapAspxMaster: error occurred.", ex);
			}
		}

		/// <summary>
		/// Wraps an ASPX page with the master page.  When previewing, will call Out.Wrap.  When publishing, will output the Page Directive to use a specified Masterpage.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="asset"></param>
		/// <param name="masterPageAsset">the master page asset.</param>
		public static void WrapAspxMaster(OutputContext context, Asset asset, Asset masterPageAsset,
										  string pageInherits = DEFAULT_PAGE_INHERITS, string pageLanguage = DEFAULT_PAGE_LANGUAGE,
										  string masterPageType = "", string extraParams = "", bool isOutWrapped = true)
		{
			if(context.IsPublishing)
			{
				//write out page directive that ties to the masterpage on the published aspx page
				string pageDirective = string.Format("<$@ Page MasterPageFile=\"~{0}\" Inherits=\"{1}\" Language=\"{2}\" {3}$>\n",
													 masterPageAsset.GetLink(LinkType.Include), pageInherits, pageLanguage,
													 extraParams);

				//add master type if specified or different from default
				if(!String.IsNullOrWhiteSpace(masterPageType))
				{
					pageDirective += string.Format("\n<$@ MasterType TypeName=\"{0}\" $>", masterPageType);
				}

				Out.Write(pageDirective);
			}
			else if(isOutWrapped)
			{
				Asset templateFolder = Asset.Load(masterPageAsset.TemplateId);
				Out.DebugWriteLine("WrapAspxMaster: {0}", masterPageAsset.AssetPath.ToString());
				Out.Wrap(templateFolder.AssetPath.ToString());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="asset"></param>
		/// <param name="masterPage"></param>
		/// <param name="contentPlaceholderId"></param>
		/// <param name="setWrapper">Enables writing the Out.GetWrapContentPlaceholder() function. Normally you'd call this for any content placeholder for body content.  Leave false for other content placeholders like head or nav.</param>
		/// <param name="isReplaced">Uses special placeholder that can be replaced instead of the Out.GetWrapContentPlaceholder() funtion.</param>
		public static void WritePlaceHolder(OutputContext context, Asset asset, Asset masterPage, string contentPlaceholderId,
											bool setWrapper = false, bool isReplaced = false)
		{
			if(context.IsPublishing)
			{
				Out.WriteLine("<asp:ContentPlaceHolder runat=\"server\" ID=\"{0}\"/>", contentPlaceholderId);
			}
			else if(isReplaced)
			{
				Out.WriteLine(AspxHelper.REPLACE_GETWRAPCONTENTPLACEHOLDER);
			}
			else if(null != masterPage)
			{
				Out.WriteLine(setWrapper && null != masterPage && asset.TemplateId != masterPage.TemplateId ? Out.GetWrapContentPlaceholder() : String.Empty);
			}
			else if(setWrapper)
			{
				Out.WriteLine(Out.GetWrapContentPlaceholder());
			}
		}

		public static IDisposable WritePlaceHolderWithContent(OutputContext context, Asset asset, string contentPlaceholderId)
		{
			if(context.IsPublishing)
			{
				Out.WriteLine(String.Empty);
				Out.WriteLine("<asp:ContentPlaceHolder runat=\"server\" ID=\"{0}\">", contentPlaceholderId);
				Out.WriteLine(String.Empty);
				return new Wrapper(() => { Out.Write("\n</asp:ContentPlaceHolder>\n"); });
			}
			else
			{
				return new Wrapper();
			}
		}

		/// <summary>
		/// When publishing, this method will write out the &lt;asp:Content&gt; server tag, with a specified contentPlaceholderId
		/// </summary>
		/// <example>
		///		&lt;% using(AspxHelper.WrapContent(context,asset,"cphMain") { %&gt;
		///			wrapped content.  when published, this area will be wrapped with 
		///			&amp;lt;asp:Content runat="server" ContentPlaceholderId="cphMain"&amp;gt; ... &amp;lt;/asp:Content&amp;&gt; tags.
		///		&lt% } %&gt;
		/// </example>
		/// <param name="context"></param>
		/// <param name="asset"></param>
		/// <param name="contentPlaceholderId">a ContentPlaceholderId, present in the Masterpage.</param>
		public static IDisposable WrapContent(OutputContext context, Asset asset, string contentPlaceholderId)
		{
			if(context.IsPublishing)
			{
				Out.Write("\n<asp:Content runat=\"server\" ContentPlaceholderId=\"{0}\">\n", contentPlaceholderId);
				return new Wrapper(() => { Out.Write("\n</asp:Content>\n"); });
			}
			else
			{
				return new Wrapper();
			}
		}

		public static IDisposable WrapNestedContent(OutputContext context, Asset asset, Asset nestedMasterPage, string contentPlaceholderId)
		{
			if(context.IsPublishing)
			{
				Out.Write("\n<asp:Content runat=\"server\" ContentPlaceholderId=\"{0}\">\n", contentPlaceholderId);
				return new NestedWrapper(() => { Out.Write("\n</asp:Content>\n"); });
			}
			else
			{
				return new NestedWrapper(nestedMasterPage);
			}
		}

		public static IDisposable WrapPreviewContent(OutputContext context, Asset asset, Asset nestedMasterPage)
		{
			if(context.IsPublishing)
			{
				return new Wrapper();
			}
			else
			{
				return new NestedWrapper(nestedMasterPage);
			}
		}

		/// <summary>
		/// When publishing, writes out the &lt;%@ MasterPage %&gt; standard directive 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="asset"></param>
		public static void WriteMasterDirective(OutputContext context, Asset asset,
												string language = DEFAULT_PAGE_LANGUAGE,
												string masterPageType = "")
		{
			if(context.IsPublishing)
			{
				string masterDirective = string.Format("<$@ Master Language=\"{0}\" $>", language);

				if(!String.IsNullOrWhiteSpace(masterPageType))
				{
					masterDirective = masterDirective.Replace(" $>", string.Format(" Inherits=\"{0}\" $>", masterPageType));
				}

				Out.Write(masterDirective);
			}
		}

		public static void WriteNestedMasterDirective(OutputContext context, Asset asset, Asset masterTemplate,
																	 string language = DEFAULT_PAGE_LANGUAGE, string masterPageType = "")
		{
			if(context.IsPublishing)
			{
				string masterDirective = String.Format("<$@ Master MasterPageFile=\"~{0}\" Language=\"{1}\" $>",
																	masterTemplate.GetLink(), language);

				if(!String.IsNullOrWhiteSpace(masterPageType))
				{
					masterDirective = masterDirective.Replace(" $>", string.Format(" Inherits=\"{0}\" $>", masterPageType));
				}

				Out.Write(masterDirective);
			}
		}


		/// <summary>
		/// Writes out a Register directive and the control instatiation declarations.
		/// </summary>
		/// <param name="ascxSrc">Path to the ASCX control in the runtime web application</param>
		/// <returns></returns>
		public static string WriteASCXDeclaration(string ascxSrc, params string[] attributes)
		{
			string[] segments = ascxSrc.Split('/');

			string name = segments.Last();
			name = name.Substring(0, name.IndexOf('.')); //lop off the .ascx part

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("<$@ Register TagPrefix=\"ctl\" Name=\"{0}\" Src=\"{1}\" $>\n", name, ascxSrc);
			sb.AppendFormat("<ctl:{0} runat=\"server\"", name);

			if(attributes != null && attributes.Length > 0)
			{
				foreach(var attr in attributes)
				{
					sb.Append(" ").Append(attr);
				}
			}

			sb.Append(" />\n");

			return sb.ToString();
		}

		#region Wrap Helper

		/// <summary>
		/// A helper class for writing content when disposed.  Designed to be used in a c# using statement.
		/// </summary>
		internal class Wrapper : IDisposable
		{
			public Wrapper()
			{
				this._runOnDispose = null;
			}
			public Wrapper(global::System.Action runOnDispose)
			{
				this._runOnDispose = runOnDispose;
			}

			global::System.Action _runOnDispose;

			public void Dispose()
			{
				try { if(_runOnDispose != null) _runOnDispose(); }
				catch { }
			}
		}

		internal class NestedWrapper : IDisposable
		{

			private Asset _nestedMaster;

			public NestedWrapper()
			{
				this._runOnDispose = null;
			}

			public NestedWrapper(Asset nestedMasterPage)
			{
				Out.StartCapture();
				_nestedMaster = nestedMasterPage;
			}

			public NestedWrapper(global::System.Action runOnDispose)
			{
				this._runOnDispose = runOnDispose;
			}

			global::System.Action _runOnDispose;

			public void Dispose()
			{
				try { if(_runOnDispose != null) _runOnDispose(); }
				catch { }
				finally
				{
					if(null != _nestedMaster)
					{
						Out.WriteLine(_nestedMaster.Show().Replace(AspxHelper.REPLACE_GETWRAPCONTENTPLACEHOLDER, Out.StopCapture()));
					}
				}
			}
		}

#endregion

	}

}

