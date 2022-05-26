# Active Directory-integrated Android Management

This ASP.NET Web Application allows you to manage Android devices using Google's Android Management API. In contrast to other EMM providers, all user and permission management remains within the on-premise Active Directory.

Users can register fully managed devices as well as work profiles using Zero Touch QR codes. Policies can include expansion strings, which can for example be used in managed properties to limit the User Principal Name that is allowed to sign in to an application.


## Installation

1. Create a new project using the [Google Cloud Console](https://console.cloud.google.com/).
2. Enable the [Android Management API](https://console.cloud.google.com/apis/library/androidmanagement.googleapis.com) for the project.
3. Create a [service account](https://console.cloud.google.com/iam-admin/serviceaccounts) and download it's key JSON file.
4. Use the [API explorer](https://developers.google.com/android/management/reference/rest/v1/enterprises/create) to create one (or more) enterprise(s). The program uses and/or displays the following properties: `name`, `enterpriseDisplayName`, `logo` and `primaryColor`. 
5. Download the source code into a directory served by IIS. Ensure the directory is converted to an application.
6. Enable **Windows Authentication** and disable **Anonymous Authentication** for the IIS application.
7. Install the following NuGet packages within that directory:
   - Google.Apis (1.57.0)
   - Google.Apis.AndroidManagement.v1 (1.57.0.2671)
   - Google.Apis.Auth (1.57.0)
   - Google.Apis.Core (1.57.0)
   - Microsoft.CodeDom.Providers.DotNetCompilerPlatform (4.1.0-preview1)
   - Newtonsoft.Json (13.0.1)
8. Create a `Web.config` based on `Web.sample.config` and adjust the settings in the `appSettings` section to your needs:
   - `CREDENTIALS`: Path to the downloaded service account key file.
   - `LANGUAGES`: Space-separated list of locales that user messages should be translated to. Can be empty, in which case only a default message without localization has to be provided.
   - `PROVISIONING_*`: Additional settings to be included in the Zero Touch QR code. For more information see [create a QR code](https://developers.google.com/android/work/play/emm-api/prov-devices#create_a_qr_code).
9. For each created enterprise in step 4, create an `aspx` file in the directory. This file manages permissions and specifies the available policies. Have a look at the example below to get an idea of the basic settings.

## Sample `enterprise.aspx` file:

```aspx
<%@ Page Language="C#" MasterPageFile="~/Site.Master" %>

<script runat="server">
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        Enterprise.Register("AB12c3de4f", e => e
            // The ID above is the name of the enterprise created in step 4, excluding the "enterprises/" prefix.

            .Admins(@"COMPANY\EMM-Admins", @"COMPANY\a.miller")
            // Members of EMM-Admins and user a.miller can manage policies and all devices.

            .Policy("Sales Large Cities", @"COMPANY\Sales-Vienna", @"COMPANY\Sales-Berlin")
            // The policy "Sales Large Cities" will be applied to the Vienna and Berlin sales group.

            .Policy("Sales Vienna", @"COMPANY\Sales-Vienna")
            // The Vienna sales team is assigned an additional policy that overrides some of the large cities settings.

            .Users(@"COMPANY\Sales-Vienna", @"COMPANY\Sales-Berlin", @"COMPANY\Sales-Innsbruck")
            // Members of all sales groups can register their own devices.
        );
    }
</script>
```
