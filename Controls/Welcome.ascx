<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.Welcome" %>

<script runat="server">

#nullable enable

    public string? EnrollHref { get; set; } = null;

</script>

<div class="uk-position-center uk-text-center">
    <%
        if (Enterprise.Current.Logo is not null)
        {
    %>
    <div class="uk-margin">
        <img class="uk-width-1-1" src="<%= HttpUtility.HtmlAttributeEncode(Enterprise.Current.Logo.AbsoluteUri) %>" alt="<%= HttpUtility.HtmlAttributeEncode(Enterprise.Current.DisplayName) %>" />
    </div>
    <%
        }
        if (EnrollHref is not null)
        {
    %>
    <div class="uk-margin">
        <a href="<%= HttpUtility.HtmlAttributeEncode(EnrollHref) %>" class="uk-button uk-button-primary">Enroll New Device</a>
    </div>
    <%
        }
    %>
</div>
