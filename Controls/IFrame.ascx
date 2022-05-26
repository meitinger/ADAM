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

<%@ Control Language="C#" ClassName="Aufbauwerk.Tools.Emm.Controls.IFrame" %>

<script runat="server">

#nullable enable

    public string Feature { get; set; } = string.Empty;

</script>

<script src="https://apis.google.com/js/api.js"></script>
<div id="iframe_<%= Feature %>"></div>
<script>
    gapi.load("gapi.iframes", function () {
        const iframe = gapi.iframes.getContext().openChild({
            "url": <%= HttpUtility.JavaScriptStringEncode(Enterprise.Current.GetIFrame(Feature), addDoubleQuotes: true) %>,
            "where": document.getElementById("iframe_<%= Feature %>"),
            "attributes": { "uk-height-viewport": "offset-top: true", scrolling: 'yes' }
        });
        iframe.register('onproductselect', function (event) {
            navigator.clipboard.writeText(event.packageName);
            alert(`Package name copied to clipboard:\n${event.packageName}`);
        }, gapi.iframes.CROSS_ORIGIN_IFRAMES_FILTER);
    });
</script>
