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

<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.Devices" %>

<script runat="server">

#nullable enable

    private enum Column
    {
        Name,
        UserName,
        DisplayName,
        WorkProfile,
        State,
        AppliedState,
        LastReportTime,
        EnrollmentTime,
        Policy,
        Model,
        Ram,
        InternalStorage,
    }

    private enum DeviceState
    {
        Unknown,
        Active,
        Disabled,
        Deleted,
        Provisioning,
    }

    private class DisplayColumn
    {
        public DisplayColumn(string title, Column column)
        {
            Title = title;
            Column = column;
        }

        public string Title { get; }
        public Column Column { get; }
    }

    private static readonly IEnumerable<DisplayColumn> _allDisplayColumns = Array.AsReadOnly(new DisplayColumn[]
    {
         new ("User", Column.UserName),
         new ("Name", Column.DisplayName),
         new ("Last Report", Column.LastReportTime),
         new ("Policy", Column.Policy),
         new ("Model", Column.Model),
         new ("RAM", Column.Ram),
         new ("Storage", Column.InternalStorage),
         new ("Enrollment", Column.EnrollmentTime),
         new ("Work Profile Only", Column.WorkProfile),
    });

    private IEnumerable<IDictionary<Column, object>> CachedDevices
    {
        get
        {
            var devices = ViewState[nameof(CachedDevices)] as IEnumerable<IDictionary<Column, object>> ?? Enumerable.Empty<IDictionary<Column, object>>();
            return OrderDescending ? devices.OrderByDescending(device => device[OrderBy]) : devices.OrderBy(device => device[OrderBy]);
        }
        set => ViewState[nameof(CachedDevices)] = value;
    }

    private string DeviceName => GetDeviceValue<string>(Column.Name);

    private IEnumerable<DisplayColumn> DisplayColumns => _allDisplayColumns.Skip(ShowAllDevices ? 0 : 1);

    private DisplayColumn Header => (DisplayColumn)Page.GetDataItem();

    private Column OrderBy
    {
        get => ViewState[nameof(OrderBy)] as Column? ?? Column.UserName;
        set => ViewState[nameof(OrderBy)] = value;
    }

    private bool OrderDescending
    {
        get => ViewState[nameof(OrderDescending)] as bool? ?? false;
        set => ViewState[nameof(OrderDescending)] = value;
    }

    public bool ShowAllDevices { get; set; } = false;

    private void DeviceCommand(object sender, CommandEventArgs e)
    {
        var deviceName = (string)e.CommandArgument;
        try
        {
            switch (e.CommandName)
            {
                case "ENABLE":
                case "DISABLE":
                    Enterprise.Current.PatchDeviceState(deviceName, e.CommandName == "DISABLE" ? "DISABLED" : "ACTIVE");
                    RefreshCachedDevices(this, e);
                    break;
                case "DELETE":
                    Enterprise.Current.DeleteDevice(deviceName);
                    RefreshCachedDevices(this, e);
                    break;
                default:
                    var operation = Enterprise.Current.IssueCommand(deviceName, new() { Type = e.CommandName });
                    Page.Alert(operation.Error is not null ? $"Operation failed: {operation.Error.Message}" : (operation.Done ?? false) ? "Operation succeeded." : "Operation is pending.");
                    break;
            }
        }
        catch (Google.GoogleApiException ex) { Page.Alert(ex.GetMessage()); }
    }

    private string FormatBytes(long bytes, bool binary)
    {
        // show the given number of bytes in a user-friedly way
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int step = binary ? 1024 : 1000;
        int index = 0;
        while (value >= step && index < suffixes.Length - 1)
        {
            value /= step;
            index++;
        }
        return $"{value:0.##} {suffixes[index]}";
    }

    private T GetDeviceValue<T>(Column column) => (T)((IDictionary<Column, object>)Page.GetDataItem())[column];

    private bool IsDevice(DeviceState state) => GetDeviceValue<DeviceState>(Column.State) == state;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        HeaderRepeater.DataBind();
        if (IsPostBack) { DeviceRepeater.DataBind(); }
        else { RefreshCachedDevices(this, e); }
    }

    private void RefreshCachedDevices(object sender, EventArgs e)
    {
        // query and sort all devices
        var enterprise = Enterprise.Current;
        var rawDevices = enterprise.ListDevices();
        if (!ShowAllDevices) { rawDevices = rawDevices.Where(device => enterprise.TryGetUserSidFromDevice(device) == Helpers.CurrentUser.Sid); }
        CachedDevices = rawDevices.Select(device => new Dictionary<Column, object>()
        {
            {Column.Name, enterprise.GetDeviceName(device)},
            {Column.UserName, ResolveName(enterprise.TryGetUserSidFromDevice(device)) ?? string.Empty},
            {Column.DisplayName, device.EnrollmentTokenData ?? string.Empty},
            {Column.WorkProfile, device.ManagementMode switch { "DEVICE_OWNER" => "No", "PROFILE_OWNER" => "Yes", _ => string.Empty }},
            {Column.State, Enum.TryParse<DeviceState>(device.State, ignoreCase: true, out var state) ? state : DeviceState.Unknown},
            {Column.AppliedState, Enum.TryParse<DeviceState>(device.AppliedState, ignoreCase: true, out var appliedState) ? appliedState : DeviceState.Unknown},
            {Column.LastReportTime, device.LastStatusReportTimeDateTimeOffset?.ToLocalTime() ?? DateTime.MinValue},
            {Column.EnrollmentTime, device.EnrollmentTimeDateTimeOffset?.ToLocalTime() ?? DateTime.MinValue},
            {Column.Policy, $"{device.AppliedPolicyName ?? Enterprise.DefaultPolicyName} (v{device.AppliedPolicyVersion ?? 0})"},
            {Column.Model, device.HardwareInfo.Model ?? "Unknown"},
            {Column.Ram, device.MemoryInfo.TotalRam ?? 0},
            {Column.InternalStorage, device.MemoryInfo.TotalInternalStorage ?? 0},
        }).ToArray();
        DeviceRepeater.DataBind();

        static string? ResolveName(System.Security.Principal.SecurityIdentifier? sid) => sid is null ? null : (Helpers.FindUserBySid(sid)?.Name ?? sid.ToString());
    }

    private void SortCommand(object sender, CommandEventArgs e)
    {
        // set the new sort
        var column = (Column)Enum.Parse(typeof(Column), (string)e.CommandArgument);
        if (column == OrderBy) { OrderDescending = !OrderDescending; }
        else
        {
            OrderBy = column;
            OrderDescending = false;
        }
        HeaderRepeater.DataBind();
        DeviceRepeater.DataBind();
    }

</script>
<div class="uk-margin uk-margin-left uk-margin-right">
    <asp:UpdatePanel runat="server">
        <ContentTemplate>
            <div class="uk-card uk-card-default">
                <div class="uk-card-header">
                    <h3 class="uk-card-title">Devices</h3>
                </div>
                <div class="uk-card-body">
                    <table class="uk-table">
                        <thead>
                            <tr>
                                <asp:Repeater runat="server" ID="HeaderRepeater" DataSource='<%# DisplayColumns %>'>
                                    <ItemTemplate>
                                        <th>
                                            <asp:LinkButton runat="server" Text='<%# $"{Header.Title}{(OrderBy == Header.Column ? (OrderDescending ? " ▼" : " ▲" ) : "")}" %>' OnCommand="SortCommand" CommandArgument='<%# Header.Column %>' />
                                        </th>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater runat="server" ID="DeviceRepeater" DataSource='<%# CachedDevices %>'>
                                <ItemTemplate>
                                    <tr class='<%# GetDeviceValue<DeviceState>(Column.AppliedState) switch { DeviceState.Disabled => "uk-text-muted", DeviceState.Deleted => "uk-text-danger", DeviceState.Provisioning => "uk-text-primary", _ => "" } %>'>
                                        <asp:Repeater runat="server" DataSource='<%# DisplayColumns.Select(entry => entry.Column switch { Column.Ram or Column.InternalStorage => FormatBytes(GetDeviceValue<long>(entry.Column), binary: entry.Column == Column.Ram), _ => GetDeviceValue<object>(entry.Column) }) %>'>
                                            <ItemTemplate>
                                                <td><%#: Page.GetDataItem() %></td>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                        <td>
                                            <div class="uk-button-group">
                                                <asp:Button runat="server" CssClass='<%# IsDevice(DeviceState.Disabled) ? "uk-button uk-button-small uk-button-primary" : "uk-button uk-button-small uk-button-secondary" %>' UseSubmitBehavior="false" OnCommand="DeviceCommand" CommandName='<%# IsDevice(DeviceState.Disabled) ? "ENABLE" : "DISABLE" %>' CommandArgument='<%# DeviceName %>' Enabled='<%# IsDevice(DeviceState.Disabled) || IsDevice(DeviceState.Active) %>' Text='<%# IsDevice(DeviceState.Disabled) ? "Enable" : "Disable" %>' />
                                                <asp:Button runat="server" CssClass="uk-button uk-button-small uk-button-default" UseSubmitBehavior="false" OnCommand="DeviceCommand" CommandName="LOCK" CommandArgument='<%# DeviceName %>' Text="Lock" />
                                                <asp:Button runat="server" CssClass="uk-button uk-button-small uk-button-default" UseSubmitBehavior="false" OnCommand="DeviceCommand" CommandName="RESET_PASSWORD" CommandArgument='<%# DeviceName %>' Text="Unlock" />
                                                <asp:Button runat="server" CssClass="uk-button uk-button-small uk-button-default" UseSubmitBehavior="false" OnCommand="DeviceCommand" CommandName="REBOOT" CommandArgument='<%# DeviceName %>' Text="Reboot" />
                                                <asp:Button runat="server" CssClass="uk-button uk-button-small uk-button-danger" UseSubmitBehavior="false" OnCommand="DeviceCommand" CommandName="DELETE" CommandArgument='<%# DeviceName %>' Enabled='<%# !IsDevice(DeviceState.Deleted) %>' Text="Delete" />
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>
            <asp:Timer runat="server" ID="RefreshTimer" Interval="60000" OnTick="RefreshCachedDevices" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="RefreshTimer" EventName="Tick" />
        </Triggers>
    </asp:UpdatePanel>
</div>
