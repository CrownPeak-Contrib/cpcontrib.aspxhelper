# cpcontrib.aspxhelper
Asp.Net project publishing helper library

Main functions are for helping CrownPeak template developers to blend the Nav Wrapper concepts by using the Asp.Net MasterPages implementation

```
cppm install cpcontrib.aspxhelper
```

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
