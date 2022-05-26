<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.Sync" %>

<script runat="server">
#nullable enable
</script>

<div class="uk-margin uk-container">
    <div class="uk-card uk-card-default">
        <div class="uk-card-header">
            <h3 class="uk-card-title">Sync User Policies</h3>
        </div>
        <div class="uk-card-body">
            <ul class="uk-list uk-list-divider">
                <%
                    foreach (var (name, result) in Enterprise.Current.Sync())
                    {
                        Response.Flush();
                %>
                <li>
                    <span class="uk-label <%= result switch { SyncResult.Updated => "uk-label-success", SyncResult.Ignored => "uk-label-warning", SyncResult.Deleted => "uk-label-danger", _ => "" } %>"><%= result %></span>
                    <%: name %>
                </li>
                <%
                    }
                %>
            </ul>
        </div>
    </div>
</div>
