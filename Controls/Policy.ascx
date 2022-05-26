<%--
Active Directory-integrated Android Management
Copyright (C) 2022  Manuel Meitinger

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
--%>

<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.Policy" %>

<script runat="server">

#nullable enable

    public string PolicyName { get; set; } = string.Empty;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var enterprise = Enterprise.Current;
        var isKnown = enterprise.Policies.Contains(PolicyName);
        var policy = enterprise.FindPolicy(PolicyName) ?? (isKnown ? new() { Version = 0 } : throw new ArgumentException($"Policy '{PolicyName}' does not exist."));
        if (IsPostBack) { ScriptManager.RegisterStartupScript(Page, Page.GetType(), "dirty", "window.onbeforeunload = e => {e.preventDefault();return e.returnValue = 'You have made changes to the policy. Are you sure you want to leave without saving?';}", addScriptTags: true); }
        Build();

        void Build()
        {
            Button saveButton = new()
            {
                ID = "save",
                Text = "Save",
                CssClass = "uk-button uk-button-primary",
                Enabled = isKnown,
            };
            var control = enterprise.Schema.BuildControl(policy.ToToken(), out var result, allowView: false, customEditButtons: saveButton).ID(System.FormattableString.Invariant($"policy_v{policy.Version}"));
            saveButton.Click += (s, e) =>
            {
                if (isKnown && result(out var token))
                {
                    try { policy = enterprise.PatchPolicy(PolicyName, token.ToPolicy()); }
                    catch (Google.GoogleApiException ex)
                    {
                        Page.Alert(ex.GetMessage());
                        return;
                    }
                    PolicyContainer.Controls.Remove(control);
                    Build();
                    Page.Alert("The policy has been successfully saved.");
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "clean", "window.onbeforeunload = undefined", addScriptTags: true);
                }
            };
            PolicyContainer.Controls.Add(control);
        }
    }

</script>

<div class="uk-margin uk-margin-left uk-margin-right">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <asp:PlaceHolder runat="server" ID="PolicyContainer" />
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
