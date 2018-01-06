# cpcontrib.aspxhelper
Asp.Net project publishing helper library

- Assists with generating ASP.Net directives, include Page/Master/UserControls
- Helps CrownPeak aspx parser to be able to ignore directives that should only be interpreted after publishing

```
cppm install cpcontrib.aspxhelper
```

## MasterPages and CrownPeak Nav Wrappers

In your Nav Wrap, we use:
- ```AspxHelper.WriteMasterPageDirective``` writes a MasterPage directive, with additional options
- ```AspxHelper.WriteContentPlaceholder``` writes equivalent ```<asp:ContentPlaceholder>``` control declarations.

These methods work with the CrownPeak concept of wrapping via ```Out.Wrap```.

With ```AspxHelper.WriteContentPlaceholder``` you have to specify which  contentPlaceholderId represents the main body of html.  By default, none will be previewable, so you must specify which is being used as the main body.

## Template Pages that now rely on MasterPage/Nav Wrapper

In your Templates' output.aspx, we use:
- ```AspxHelper.WrapMasterPage``` which writes the ```<%@ Page %>``` directive, and writes out the declaration to the MasterPage.
- ```AspxHelper.WrapContent``` writes out corresponding ```<asp:Content>``` tags which assign content into placeholders defined in the MasterPage, via ```AspxHelper.WriteContentPlaceholder```

## User Control declarations

User Controls are an interesting beast when working with a publishing CMS.  They are included/run at runtime but have a bit of work to make them work in a preview setting.

Two methods are included to help with these situations:
- ```AspxHelper.WriteASCXDeclaration``` writes a declaration for a usercontrol directly
- (Coming soon) ```AspxHelper.ASCX_Output(assetPath:"~/Controls/control.ascx")``` will help to bridge the gap between UserControls only running in the published runtime.

## Nav Wrapper Setup

Within the Nav Wrapper template, a developer would use ```WriteMasterDirective``` which properly outputs the Master directive that Asp.Net expects for the implementation:

```
<% AspxHelper.WriteMasterDirective(context, asset); %>

<html>
<body>

<% 
//preview: writes nothing
//publishing: writes "<asp:ContentPlaceholder runat="server" id="contentPlaceholderId" />"
AsoxHekoer.WritePlaceholder(context, asset, AspxHelper.GetMasterPage(asset), "contentPlaceholderId"); 
%>

</body>
</html>

```

## Template Output.aspx Setup

Your other template output.aspx implementations then use the following code to output the correct Asp.Net Content Placeholders

```
<% AspxHelper.WrapAspxMaster(context, asset, AspxHelper.GetMasterPage(asset)); %>

<% using(AspxHelper.WrapContent(context, asset, "contentPlaceholderId") { %>

<!-- create your html for template's output.aspx here -->

<% } %>
```

## Other features

Other features supported include:
- default placeholder content, for within CrownPeak preview mode
- nested content placeholders, for nested masterpages

## Credits

Built by Eric Newton and Diego Nunez
