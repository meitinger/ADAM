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

<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.Enrollment" %>

<script runat="server">

#nullable enable

    public bool AllowPolicySelection { get; set; }

    public System.DirectoryServices.AccountManagement.UserPrincipal? User { get; set; }

    private void Enroll(object sender, EventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name) && User is null) { Page.Alert("Please enter the device's name."); }
        else
        {
            Google.Apis.AndroidManagement.v1.Data.EnrollmentToken token;
            var enterprise = Enterprise.Current;
            try
            {
                token = enterprise.CreateEnrollmentToken
                (
                    deviceDisplayName: name,
                    workProfile: WorkProfileCheckBox.Checked,
                    policyName: User is not null && string.IsNullOrEmpty(PolicyDropDownList.SelectedValue)
                        ? enterprise.GetPolicyName(enterprise.PatchUserPolicy(User))
                        : PolicyDropDownList.SelectedValue
                );
            }
            catch (Google.GoogleApiException ex)
            {
                Page.Alert(ex.GetMessage());
                return;
            }
            var expiration = ((DateTime)token.ExpirationTimestamp).ToLocalTime();
            ExpirationTimer.Interval = (int)(expiration - DateTime.Now).TotalMilliseconds;
            QrImage.ImageUrl = token.GetQrCodeImageUrl();
            InputPanel.Enabled = false;
            OutputPanel.Visible = true;
            ExpirationTimer.Enabled = true;
            ExpirationLabel.Text = $"Expires {expiration}";
            ExpirationLabel.CssClass = "uk-text-meta";
        }
    }

    private void Expire(object sender, EventArgs e)
    {
        ExpirationTimer.Enabled = false;
        ExpirationLabel.Text = "Expired";
        ExpirationLabel.CssClass = "uk-label uk-label-danger";
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        WorkProfileCheckBox.InputAttributes.Add("class", "uk-checkbox");
        if (AllowPolicySelection)
        {
            PolicyDropDownList.DataSource = Enumerable.Repeat(new { Name = "(generated)", Value = "" }, User is null ? 0 : 1).Concat(Enterprise.Current.Policies.Select(policy => new { Name = policy, Value = policy }));
            PolicyDropDownList.DataBind();
        }
    }

</script>

<div class="uk-margin uk-container">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <div class="uk-card uk-card-default">
                <div class="uk-card-header">
                    <h3 class="uk-card-title">Enroll Device <%: User is null ? "" : $"for {User.GetDisplayName()}" %></h3>
                </div>
                <asp:Panel runat="server" ID="InputPanel" CssClass="uk-card-body" DefaultButton="EnrollButton">
                    <%
                        if (AllowPolicySelection)
                        {
                    %>
                    <div class="uk-margin">
                        <label for="PolicyDropDownList" class="uk-form-label">Policy:</label>
                        <div class="uk-form-controls">
                            <asp:DropDownList runat="server" ID="PolicyDropDownList" CssClass="uk-select" DataTextField="Name" DataValueField="Value" />
                        </div>
                    </div>
                    <%
                        }
                    %>
                    <div class="uk-margin">
                        <label for="NameTextBox" class="uk-form-label">Device Name<%= User is null ? "" : " (Optional)"%>:</label>
                        <div class="uk-form-controls">
                            <asp:TextBox runat="server" ID="NameTextBox" CssClass="uk-input" />
                        </div>
                    </div>
                    <%
                        if (User is not null)
                        {
                    %>
                    <div class="uk-margin">
                        <div class="uk-form-label">Work Profile Only:</div>
                        <div class="uk-form-controls uk-form-controls-text">
                            <asp:CheckBox runat="server" ID="WorkProfileCheckBox" Text=" This device was purchased by the user personally. Instead of a fully managed device, only a work profile will be created." Checked="false" />
                        </div>
                    </div>
                    <%
                        }
                    %>
                    <div class="uk-margin">
                        <asp:Button runat="server" ID="EnrollButton" CssClass="uk-button uk-button-primary" Text="Enroll" OnClick="Enroll" />
                    </div>
                </asp:Panel>
                <asp:Panel runat="server" ID="OutputPanel" CssClass="uk-card-footer uk-text-center" Visible="false">
                    <p>
                        <asp:Image runat="server" ID="QrImage" />
                        <br />
                        <asp:Label runat="server" ID="ExpirationLabel" />
                    </p>
                </asp:Panel>
            </div>
            <asp:Timer runat="server" ID="ExpirationTimer" Enabled="false" OnTick="Expire" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="ExpirationTimer" EventName="Tick" />
        </Triggers>
    </asp:UpdatePanel>
</div>
