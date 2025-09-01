/*
 * Active Directory-integrated Android Management
 * Copyright (C) 2022  Manuel Meitinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

#nullable enable

namespace Aufbauwerk.Tools.Emm
{
    public partial class Schema
    {
        public static Schema ForEnterprise(Enterprise enterprise) => new SchemaObject("Policy", "A policy resource represents a group of settings that govern the behavior of a managed device and the apps installed on it.")
        {
            { "applications", new SchemaApplications("Applications", "Policy applied to apps.", enterprise: enterprise, key: ("packageName", new SchemaString("Package Name", "The package name of the app. For example, com.google.android.youtube for the YouTube app."))) },
            { "accountTypesWithManagementDisabled", new SchemaStringArray("Account Types With Management Disabled", "Account types that can't be managed by the user.") },
            { "addUserDisabled", new SchemaBoolean("Add User Disabled", "Whether adding new users and profiles is disabled.") },
            { "adjustVolumeDisabled", new SchemaBoolean("Adjust Volume Disabled", "Whether adjusting the master volume is disabled. Also mutes the device.") },
            { "advancedSecurityOverrides", new SchemaObject("Advanced Security Overrides", "Security policies set to the most secure values by default. To maintain the security posture of a device, we don't recommend overriding any of the default values.") {
                { "commonCriteriaMode", new SchemaEnum("Common Criteria Mode", "Controls Common Criteria Mode—security standards defined in the Common Criteria for Information Technology Security Evaluation (https://www.commoncriteriaportal.org/) (CC). Enabling Common Criteria Mode increases certain security components on a device, including AES-GCM encryption of Bluetooth Long Term Keys, and Wi-Fi configuration stores. Warning: Common Criteria Mode enforces a strict security model typically only required for IT products used in national security systems and other highly sensitive organizations. Standard device use may be affected. Only enabled if required.") {
                    { "COMMON_CRITERIA_MODE_UNSPECIFIED", "Unspecified. Defaults to COMMON_CRITERIA_MODE_DISABLED." },
                    { "COMMON_CRITERIA_MODE_DISABLED", "Default. Disables Common Criteria Mode." },
                    { "COMMON_CRITERIA_MODE_ENABLED", "Enables Common Criteria Mode." },
                } },
                { "developerSettings", new SchemaEnum("Developer Settings", "Controls access to developer settings: developer options and safe boot.") {
                    { "DEVELOPER_SETTINGS_UNSPECIFIED", "Unspecified. Defaults to DEVELOPER_SETTINGS_DISABLED." },
                    { "DEVELOPER_SETTINGS_DISABLED", "Default. Disables all developer settings and prevents the user from accessing them." },
                    { "DEVELOPER_SETTINGS_ALLOWED", "Allows all developer settings. The user can access and optionally configure the settings." },
                } },
                { "googlePlayProtectVerifyApps", new SchemaEnum("Google Play Protect Verify Apps", "Whether Google Play Protect verification (https://support.google.com/accounts/answer/2812853) is enforced.") {
                    { "GOOGLE_PLAY_PROTECT_VERIFY_APPS_UNSPECIFIED", "Unspecified. Defaults to VERIFY_APPS_ENFORCED." },
                    { "VERIFY_APPS_ENFORCED", "Default. Force-enables app verification." },
                    { "VERIFY_APPS_USER_CHOICE", "Allows the user to choose whether to enable app verification." },
                } },
                { "personalAppsThatCanReadWorkNotifications", new SchemaStringArray("Personal Apps That Can Read Work Notifications", "Personal apps that can read work profile notifications using a NotificationListenerService (https://developer.android.com/reference/android/service/notification/NotificationListenerService). By default, no personal apps (aside from system apps) can read work notifications. Each value in the list must be a package name.") },
                { "untrustedAppsPolicy", new SchemaEnum("Untrusted Apps Policy", "The policy for untrusted apps (apps from unknown sources) enforced on the device.") {
                    { "UNTRUSTED_APPS_POLICY_UNSPECIFIED", "Unspecified. Defaults to DISALLOW_INSTALL." },
                    { "DISALLOW_INSTALL", "Default. Disallow untrusted app installs on entire device." },
                    { "ALLOW_INSTALL_IN_PERSONAL_PROFILE_ONLY", "For devices with work profiles, allow untrusted app installs in the device's personal profile only." },
                    { "ALLOW_INSTALL_DEVICE_WIDE", "Allow untrusted app installs on entire device." },
                } },
            } },
            { "alwaysOnVpnPackage", new SchemaObject("Always On VPN Package", "Configuration for an always-on VPN connection. Use with vpnConfigDisabled to prevent modification of this setting.") {
                { "packageName", new SchemaString("Package Name", "The package name of the VPN app.") },
                { "lockdownEnabled", new SchemaBoolean("Lockdown Enabled", "Disallows networking when the VPN is not connected.") },
            } },
            { "appAutoUpdatePolicy", new SchemaEnum("App Auto-Update Policy", "The app auto update policy, which controls when automatic app updates can be applied. Recommended alternative: autoUpdateMode which is set per app, provides greater flexibility around update frequency. When autoUpdateMode is set to AUTO_UPDATE_POSTPONED or AUTO_UPDATE_HIGH_PRIORITY, this field has no effect.") {
                { "APP_AUTO_UPDATE_POLICY_UNSPECIFIED", "The auto-update policy is not set. Equivalent to CHOICE_TO_THE_USER." },
                { "CHOICE_TO_THE_USER", "The user can control auto-updates." },
                { "NEVER", "Apps are never auto-updated." },
                { "WIFI_ONLY", "Apps are auto-updated over Wi-Fi only." },
                { "ALWAYS", "Apps are auto-updated at any time. Data charges may apply." },
            } },
            { "appFunctions", new SchemaEnum("App Functions", "Controls whether apps on the device for fully managed devices or in the work profile for devices with work profiles are allowed to expose app functions.") {
                { "APP_FUNCTIONS_UNSPECIFIED", "Unspecified. Defaults to APP_FUNCTIONS_ALLOWED." },
                { "APP_FUNCTIONS_DISALLOWED", "Apps on the device for fully managed devices or in the work profile for devices with work profiles are not allowed to expose app functions. If this is set, crossProfileAppFunctions must not be set to CROSS_PROFILE_APP_FUNCTIONS_ALLOWED, otherwise the policy will be rejected." },
                { "APP_FUNCTIONS_ALLOWED", "Apps on the device for fully managed devices or in the work profile for devices with work profiles are allowed to expose app functions." },
            } },
            { "assistContentPolicy", new SchemaEnum("Assist Content Policy", "Controls whether AssistContent is allowed to be sent to a privileged app such as an assistant app. AssistContent includes screenshots and information about an app, such as package name. This is supported on Android 15 and above.") {
                { "ASSIST_CONTENT_POLICY_UNSPECIFIED", "Unspecified. Defaults to ASSIST_CONTENT_ALLOWED." },
                { "ASSIST_CONTENT_DISALLOWED", "Assist content is blocked from being sent to a privileged app." },
                { "ASSIST_CONTENT_ALLOWED", "Assist content is allowed to be sent to a privileged app." },
            } },
            { "autoDateAndTimeZone", new SchemaEnum("Auto Date and Time Zone", "Whether auto date, time, and time zone are enabled on a company-owned device.") {
                { "AUTO_DATE_AND_TIME_ZONE_UNSPECIFIED", "Unspecified. Defaults to AUTO_DATE_AND_TIME_ZONE_USER_CHOICE." },
                { "AUTO_DATE_AND_TIME_ZONE_USER_CHOICE", "Auto date, time, and time zone are left to user's choice." },
                { "AUTO_DATE_AND_TIME_ZONE_ENFORCED", "Enforce auto date, time, and time zone on the device." },
            } },
            { "bluetoothConfigDisabled", new SchemaBoolean("Bluetooth Config Disabled", "Whether configuring bluetooth is disabled.") },
            { "bluetoothContactSharingDisabled", new SchemaBoolean("Bluetooth Contact Sharing Disabled", "Whether bluetooth contact sharing is disabled.") },
            { "bluetoothDisabled", new SchemaBoolean("Bluetooth Disabled", "Whether bluetooth is disabled. Prefer this setting over bluetoothConfigDisabled because bluetoothConfigDisabled can be bypassed by the user.") },
            { "cameraAccess", new SchemaEnum("Camera Access", "Controls the use of the camera and whether the user has access to the camera access toggle.") {
                { "CAMERA_ACCESS_UNSPECIFIED", "Unspecified. Defaults to CAMERA_ACCESS_USER_CHOICE." },
                { "CAMERA_ACCESS_USER_CHOICE", "This is the default device behavior: all cameras on the device are available. On Android 12 and above, the user can use the camera access toggle." },
                { "CAMERA_ACCESS_DISABLED", "All cameras on the device are disabled (for fully managed devices, this applies device-wide and for work profiles this applies only to the work profile). There are no explicit restrictions placed on the camera access toggle on Android 12 and above: on fully managed devices, the camera access toggle has no effect as all cameras are disabled. On devices with a work profile, this toggle has no effect on apps in the work profile, but it affects apps outside the work profile." },
                { "CAMERA_ACCESS_ENFORCED", "All cameras on the device are available. On fully managed devices running Android 12 and above, the user is unable to use the camera access toggle. On devices which are not fully managed or which run Android 11 or below, this is equivalent to CAMERA_ACCESS_USER_CHOICE." },
            } },
            { "cellBroadcastsConfigDisabled", new SchemaBoolean("Cell Broadcasts Config Disabled", "Whether configuring cell broadcast is disabled.") },
            { "choosePrivateKeyRules", new SchemaObjectArray("Choose Private Key Rules", "Rules for determining apps' access to private keys.") {
                { "packageNames", new SchemaStringArray("Package Names", "The package names to which this rule applies. The hash of the signing certificate for each app is verified against the hash provided by Play. If no package names are specified, then the alias is provided to all apps that call KeyChain.choosePrivateKeyAlias (https://developer.android.com/reference/android/security/KeyChain#choosePrivateKeyAlias%28android.app.Activity,%20android.security.KeyChainAliasCallback,%20java.lang.String[],%20java.security.Principal[],%20java.lang.String,%20int,%20java.lang.String%29) or any overloads (but not without calling KeyChain.choosePrivateKeyAlias, even on Android 11 and above). Any app with the same Android UID as a package specified here will have access when they call KeyChain.choosePrivateKeyAlias.") },
                { "urlPattern", new SchemaString("URL Pattern", "The URL pattern to match against the URL of the request. If not set or empty, it matches all URLs. This uses the regular expression syntax of java.util.regex.Pattern.") },
                { "privateKeyAlias", new SchemaString("Private Key Alias", "The alias of the private key to be used.") },
            } },
            { "createWindowsDisabled", new SchemaBoolean("Create Windows Disabled", "Whether creating windows besides app windows is disabled.") },
            { "credentialProviderPolicyDefault", new SchemaEnum("Credential Provider Policy Default", "Controls which apps are allowed to act as credential providers on Android 14 and above. These apps store credentials, see this and this for details.") {
                { "CREDENTIAL_PROVIDER_POLICY_DEFAULT_UNSPECIFIED", "Unspecified. Defaults to CREDENTIAL_PROVIDER_DEFAULT_DISALLOWED." },
                { "CREDENTIAL_PROVIDER_DEFAULT_DISALLOWED", "Apps with credentialProviderPolicy unspecified are not allowed to act as a credential provider." },
                { "CREDENTIAL_PROVIDER_DEFAULT_DISALLOWED_EXCEPT_SYSTEM", "Apps with credentialProviderPolicy unspecified are not allowed to act as a credential provider except for the OEM default credential providers. OEM default credential providers are always allowed to act as credential providers." },
            } },
            { "credentialsConfigDisabled", new SchemaBoolean("Credentials Config Disabled", "Whether configuring user credentials is disabled.") },
            { "crossProfilePolicies", new SchemaObject("Cross Profile Policies", "Cross-profile policies applied on the device.") {
                { "crossProfileCopyPaste", new SchemaEnum("Cross Profile Copy Paste", "Whether text copied from one profile (personal or work) can be pasted in the other profile.") {
                    { "CROSS_PROFILE_COPY_PASTE_UNSPECIFIED", "Unspecified. Defaults to COPY_FROM_WORK_TO_PERSONAL_DISALLOWED." },
                    { "COPY_FROM_WORK_TO_PERSONAL_DISALLOWED", "Default. Prevents users from pasting into the personal profile text copied from the work profile. Text copied from the personal profile can be pasted into the work profile, and text copied from the work profile can be pasted into the work profile." },
                    { "CROSS_PROFILE_COPY_PASTE_ALLOWED", "Text copied in either profile can be pasted in the other profile." },
                } },
                { "crossProfileDataSharing", new SchemaEnum("Cross Profile Data Sharing", "Whether data from one profile (personal or work) can be shared with apps in the other profile. Specifically controls simple data sharing via intents. Management of other cross-profile communication channels, such as contact search, copy/paste, or connected work & personal apps, are configured separately.") {
                    { "CROSS_PROFILE_DATA_SHARING_UNSPECIFIED", "Unspecified. Defaults to DATA_SHARING_FROM_WORK_TO_PERSONAL_DISALLOWED." },
                    { "CROSS_PROFILE_DATA_SHARING_DISALLOWED", "Prevents data from being shared from both the personal profile to the work profile and the work profile to the personal profile." },
                    { "DATA_SHARING_FROM_WORK_TO_PERSONAL_DISALLOWED", "Default. Prevents users from sharing data from the work profile to apps in the personal profile. Personal data can be shared with work apps." },
                    { "CROSS_PROFILE_DATA_SHARING_ALLOWED", "Data from either profile can be shared with the other profile." },
                } },
                { "exemptionsToShowWorkContactsInPersonalProfile", new SchemaObject("Exemptions to Show Work Contacts in Personal Profile", "List of apps which are excluded from the ShowWorkContactsInPersonalProfile setting. Supported on Android 14 and above.") {
                    { "packageNames", new SchemaStringArray("Package Names", "A list of package names.") },
                } },
                { "showWorkContactsInPersonalProfile", new SchemaEnum("Show Work Contacts in Personal Profile", "Whether contacts stored in the work profile can be shown in personal profile contact searches and incoming calls.") {
                    { "SHOW_WORK_CONTACTS_IN_PERSONAL_PROFILE_UNSPECIFIED", "Unspecified. Defaults to SHOW_WORK_CONTACTS_IN_PERSONAL_PROFILE_ALLOWED." },
                    { "SHOW_WORK_CONTACTS_IN_PERSONAL_PROFILE_DISALLOWED", "Prevents personal apps from accessing work profile contacts and looking up work contacts." },
                    { "SHOW_WORK_CONTACTS_IN_PERSONAL_PROFILE_ALLOWED", "Default. Allows apps in the personal profile to access work profile contacts including contact searches and incoming calls." },
                    { "SHOW_WORK_CONTACTS_IN_PERSONAL_PROFILE_DISALLOWED_EXCEPT_SYSTEM", "Prevents most personal apps from accessing work profile contacts including contact searches and incoming calls, except for the OEM default Dialer, Messages, and Contacts apps. Neither user-configured Dialer, Messages, and Contacts apps, nor any other system or play installed apps, will be able to query work contacts directly." },
                } },
                { "workProfileWidgetsDefault", new SchemaEnum("Work Profile Widgets Default", "Specifies the default behaviour for work profile widgets. If the policy does not specify workProfileWidgets for a specific application, it will behave according to the value specified here.") {
                    { "WORK_PROFILE_WIDGETS_DEFAULT_UNSPECIFIED", "Unspecified. Defaults to WORK_PROFILE_WIDGETS_DEFAULT_DISALLOWED." },
                    { "WORK_PROFILE_WIDGETS_DEFAULT_ALLOWED", "Work profile widgets are allowed by default. This means that if the policy does not specify workProfileWidgets as WORK_PROFILE_WIDGETS_DISALLOWED for the application, it will be able to add widgets to the home screen." },
                    { "WORK_PROFILE_WIDGETS_DEFAULT_DISALLOWED", "Work profile widgets are disallowed by default. This means that if the policy does not specify workProfileWidgets as WORK_PROFILE_WIDGETS_ALLOWED for the application, it will be unable to add widgets to the home screen." },
                } },
            } },
            { "dataRoamingDisabled", new SchemaBoolean("Data Roaming Disabled", "Whether roaming data services are disabled.") },
            { "defaultPermissionPolicy", new SchemaEnum("Default Permission Policy", "The default permission policy for runtime permission requests.") {
                { "PERMISSION_POLICY_UNSPECIFIED", "Policy not specified. If no policy is specified for a permission at any level, then the PROMPT behavior is used by default." },
                { "PROMPT", "Prompt the user to grant a permission." },
                { "GRANT", "Automatically grant a permission." },
                { "DENY", "Automatically deny a permission." },
            } },
            { "deviceConnectivityManagement", new SchemaObject("Device Connectivity Management", "Covers controls for device connectivity such as Wi-Fi, USB data access, keyboard/mouse connections, and more.") {
                { "configureWifi", new SchemaEnum("Configure Wi-Fi", "Controls Wi-Fi configuring privileges. Based on the option set, user will have either full or limited or no control in configuring Wi-Fi networks.") {
                    { "CONFIGURE_WIFI_UNSPECIFIED", "Unspecified. Defaults to ALLOW_CONFIGURING_WIFI." },
                    { "ALLOW_CONFIGURING_WIFI", "The user is allowed to configure Wi-Fi." },
                    { "DISALLOW_ADD_WIFI_CONFIG", "Adding new Wi-Fi configurations is disallowed. The user is only able to switch between already configured networks. Supported on Android 13 and above, on fully managed devices and work profiles on company-owned devices. If the setting is not supported, ALLOW_CONFIGURING_WIFI is set." },
                    { "DISALLOW_CONFIGURING_WIFI", "Disallows configuring Wi-Fi networks. Supported on fully managed devices and work profile on company-owned devices, on all supported API levels. For fully managed devices, setting this removes all configured networks and retains only the networks configured using openNetworkConfiguration policy. For work profiles on company-owned devices, existing configured networks are not affected and the user is not allowed to add, remove, or modify Wi-Fi networks. Note: If a network connection can't be made at boot time and configuring Wi-Fi is disabled then network escape hatch will be shown in order to refresh the device policy (see networkEscapeHatchEnabled)." },
                } },
                { "tetheringSettings", new SchemaEnum("Tethering Settings", "Controls tethering settings. Based on the value set, the user is partially or fully disallowed from using different forms of tethering.") {
                    { "TETHERING_SETTINGS_UNSPECIFIED", "Unspecified. Defaults to ALLOW_ALL_TETHERING." },
                    { "ALLOW_ALL_TETHERING", "Allows configuration and use of all forms of tethering." },
                    { "DISALLOW_WIFI_TETHERING", "Disallows the user from using Wi-Fi tethering. Supported on company owned devices running Android 13 and above. If the setting is not supported, ALLOW_ALL_TETHERING will be set." },
                    { "DISALLOW_ALL_TETHERING", "Disallows all forms of tethering. Supported on fully managed devices and work profile on company-owned devices, on all supported Android versions." },
                } },
                { "usbDataAccess", new SchemaEnum("USB Data Access", "Controls what files and/or data can be transferred via USB. Supported only on company-owned devices.") {
                    { "USB_DATA_ACCESS_UNSPECIFIED", "Unspecified. Defaults to ALLOW_USB_DATA_TRANSFER." },
                    { "ALLOW_USB_DATA_TRANSFER", "All types of USB data transfers are allowed." },
                    { "DISALLOW_USB_FILE_TRANSFER", "Transferring files over USB is disallowed. Other types of USB data connections, such as mouse and keyboard connection, are allowed." },
                    { "DISALLOW_USB_DATA_TRANSFER", "When set, all types of USB data transfers are prohibited. Supported for devices running Android 12 or above with USB HAL 1.3 or above. If the setting is not supported, DISALLOW_USB_FILE_TRANSFER will be set." },
                } },
                { "wifiDirectSettings", new SchemaEnum("Wi-Fi Direct Settings", "Controls configuring and using Wi-Fi direct settings. Supported on company-owned devices running Android 13 and above.") {
                    { "WIFI_DIRECT_SETTINGS_UNSPECIFIED", "Unspecified. Defaults to ALLOW_WIFI_DIRECT." },
                    { "ALLOW_WIFI_DIRECT", "The user is allowed to use Wi-Fi direct." },
                    { "DISALLOW_WIFI_DIRECT", "The user is not allowed to use Wi-Fi direct." },
                } },
            } },
            { "deviceOwnerLockScreenInfo", new SchemaLocalizedString("Device Owner Lock Screen Info", "The device owner information to be shown on the lock screen.") },
            { "deviceRadioState", new SchemaObject("Device Radio State", "Covers controls for radio state such as Wi-Fi, bluetooth, and more.") {
                { "cellularTwoGState", new SchemaEnum("Cellular 2G State", "Controls whether cellular 2G setting can be toggled by the user or not.") {
                    { "CELLULAR_TWO_G_STATE_UNSPECIFIED", "Unspecified. Defaults to CELLULAR_TWO_G_USER_CHOICE." },
                    { "CELLULAR_TWO_G_USER_CHOICE", "The user is allowed to toggle cellular 2G on or off." },
                    { "CELLULAR_TWO_G_DISABLED", "Cellular 2G is disabled. The user is not allowed to toggle cellular 2G on via settings." },
                } },
                { "airplaneModeState", new SchemaEnum("Airplane Mode State", "Controls whether airplane mode can be toggled by the user or not.") {
                    { "AIRPLANE_MODE_STATE_UNSPECIFIED", "Unspecified. Defaults to AIRPLANE_MODE_USER_CHOICE." },
                    { "AIRPLANE_MODE_USER_CHOICE", "The user is allowed to toggle airplane mode on or off." },
                    { "AIRPLANE_MODE_DISABLED", "Airplane mode is disabled. The user is not allowed to toggle airplane mode on." },
                } },
                { "ultraWidebandState", new SchemaEnum("Ultra Wideband State", "Controls the state of the ultra wideband setting and whether the user can toggle it on or off.") {
                    { "ULTRA_WIDEBAND_STATE_UNSPECIFIED", "Unspecified. Defaults to ULTRA_WIDEBAND_USER_CHOICE." },
                    { "ULTRA_WIDEBAND_USER_CHOICE", "The user is allowed to toggle ultra wideband on or off." },
                    { "ULTRA_WIDEBAND_DISABLED", "Ultra wideband is disabled. The user is not allowed to toggle ultra wideband on via settings." },
                } },
                { "wifiState", new SchemaEnum("Wi-Fi State", "Controls current state of Wi-Fi and if user can change its state.") {
                    { "WIFI_STATE_UNSPECIFIED", "Unspecified. Defaults to WIFI_STATE_USER_CHOICE." },
                    { "WIFI_STATE_USER_CHOICE", "User is allowed to enable/disable Wi-Fi." },
                    { "WIFI_ENABLED", "Wi-Fi is on and the user is not allowed to turn it off." },
                    { "WIFI_DISABLED", "Wi-Fi is off and the user is not allowed to turn it on." },
                } },
            } },
            { "displaySettings", new SchemaObject("Display Settings", "Controls for the display settings.") {
                { "screenBrightnessSettings", new SchemaObject("Screen Brightness Settings", "Controls the screen brightness settings.") {
                    { "screenBrightnessMode", new SchemaEnum("Screen Brightness Mode", "Controls the screen brightness mode.") {
                        { "SCREEN_BRIGHTNESS_MODE_UNSPECIFIED", "Unspecified. Defaults to BRIGHTNESS_USER_CHOICE." },
                        { "BRIGHTNESS_USER_CHOICE", "The user is allowed to configure the screen brightness. screenBrightness must not be set." },
                        { "BRIGHTNESS_AUTOMATIC", "The screen brightness mode is automatic in which the brightness is automatically adjusted and the user is not allowed to configure the screen brightness. screenBrightness can still be set and it is taken into account while the brightness is automatically adjusted. Supported on Android 9 and above on fully managed devices." },
                        { "BRIGHTNESS_FIXED", "The screen brightness mode is fixed in which the brightness is set to screenBrightness and the user is not allowed to configure the screen brightness. screenBrightness must be set. Supported on Android 9 and above on fully managed devices." },
                    } },
                    { "screenBrightness", new SchemaInteger("Screen Brightness", "The screen brightness between 1 and 255 where 1 is the lowest and 255 is the highest brightness. A value of 0 (default) means no screen brightness set. Any other value is rejected. screenBrightnessMode must be either BRIGHTNESS_AUTOMATIC or BRIGHTNESS_FIXED to set this."){ Maximum = 255 } },
                } },
                { "screenTimeoutSettings", new SchemaObject("Screen Timeout Settings", "Controls the screen timeout settings.") {
                    { "screenTimeoutMode", new SchemaEnum("Screen Timeout Mode", "Controls whether the user is allowed to configure the screen timeout.") {
                        { "SCREEN_TIMEOUT_MODE_UNSPECIFIED", "Defaults to SCREEN_TIMEOUT_USER_CHOICE." },
                        { "SCREEN_TIMEOUT_USER_CHOICE", "The user is allowed to configure the screen timeout. screenTimeout must not be set." },
                        { "SCREEN_TIMEOUT_ENFORCED", "The screen timeout is set to screenTimeout and the user is not allowed to configure the timeout. screenTimeout must be set. Supported on Android 9 and above on fully managed devices." },
                    } },
                    { "screenTimeout", new SchemaDuration("Screen Timeout", "Controls the screen timeout duration. The screen timeout duration must be greater than 0, otherwise it is rejected. Additionally, it should not be greater than maximumTimeToLock, otherwise the screen timeout is set to maximumTimeToLock and a NonComplianceDetail with INVALID_VALUE reason and SCREEN_TIMEOUT_GREATER_THAN_MAXIMUM_TIME_TO_LOCK specific reason is reported. If the screen timeout is less than a certain lower bound, it is set to the lower bound. The lower bound may vary across devices. If this is set, screenTimeoutMode must be SCREEN_TIMEOUT_ENFORCED. Supported on Android 9 and above on fully managed devices.") },
                } },
            } },
            { "encryptionPolicy", new SchemaEnum("Encryption Policy", "Whether encryption is enabled") {
                { "ENCRYPTION_POLICY_UNSPECIFIED", "This value is ignored, i.e. no encryption required" },
                { "ENABLED_WITHOUT_PASSWORD", "Encryption required but no password required to boot" },
                { "ENABLED_WITH_PASSWORD", "Encryption required with password required to boot" },
            } },
            { "enterpriseDisplayNameVisibility", new SchemaEnum("Enterprise DisplayName Visibility", "Controls whether the enterpriseDisplayName is visible on the device (e.g. lock screen message on company-owned devices).") {
                { "ENTERPRISE_DISPLAY_NAME_VISIBILITY_UNSPECIFIED", "Unspecified. Defaults to displaying the enterprise name that's set at the time of device setup. In future, this will default to ENTERPRISE_DISPLAY_NAME_VISIBLE." },
                { "ENTERPRISE_DISPLAY_NAME_VISIBLE", "The enterprise display name is visible on the device. Supported on work profiles on Android 7 and above. Supported on fully managed devices on Android 8 and above." },
                { "ENTERPRISE_DISPLAY_NAME_HIDDEN", "The enterprise display name is hidden on the device." },
            } },
            { "factoryResetDisabled", new SchemaBoolean("Factory Reset Disabled", "Whether factory resetting from settings is disabled.") },
            { "frpAdminEmails", new SchemaStringArray("FRP Admin Emails", "Email addresses of device administrators for factory reset protection. When the device is factory reset, it will require one of these admins to log in with the Google account email and password to unlock the device. If no admins are specified, the device won't provide factory reset protection.") },
            { "funDisabled", new SchemaBoolean("Fun Disabled", "Whether the user is allowed to have fun. Controls whether the Easter egg game in Settings is disabled.") },
            { "installAppsDisabled", new SchemaBoolean("Install Apps Disabled", "Whether user installation of apps is disabled.") },
            { "keyguardDisabled", new SchemaBoolean("Keyguard Disabled", "Whether the keyguard is disabled.") },
            { "keyguardDisabledFeatures", new SchemaFlags("Keyguard Disabled Features", "Disabled keyguard customizations, such as widgets.") {
                { "KEYGUARD_DISABLED_FEATURE_UNSPECIFIED", "This value is ignored." },
                { "CAMERA", "Disable the camera on secure keyguard screens (e.g. PIN)." },
                { "NOTIFICATIONS", "Disable showing all notifications on secure keyguard screens." },
                { "UNREDACTED_NOTIFICATIONS", "Disable unredacted notifications on secure keyguard screens." },
                { "TRUST_AGENTS", "Ignore trust agent state on secure keyguard screens." },
                { "DISABLE_FINGERPRINT", "Disable fingerprint sensor on secure keyguard screens." },
                { "DISABLE_REMOTE_INPUT", "Disable text entry into notifications on secure keyguard screens." },
                { "FACE", "Disable face authentication on secure keyguard screens." },
                { "IRIS", "Disable iris authentication on secure keyguard screens." },
                { "BIOMETRICS", "Disable all biometric authentication on secure keyguard screens." },
                { "SHORTCUTS", "Disable all shortcuts on secure keyguard screen on Android 14 and above." },
                { "ALL_FEATURES", "Disable all current and future keyguard customizations." },
            } },
            { "kioskCustomization", new SchemaObject("Kiosk Customization", "Settings controlling the behavior of a device in kiosk mode. To enable kiosk mode, set kioskCustomLauncherEnabled to true or specify an app in the policy with installType KIOSK.") {
                { "deviceSettings", new SchemaEnum("Device Settings", "Specifies whether the Settings app is allowed in kiosk mode.") {
                    { "DEVICE_SETTINGS_UNSPECIFIED", "Unspecified, defaults to SETTINGS_ACCESS_ALLOWED." },
                    { "SETTINGS_ACCESS_ALLOWED", "Access to the Settings app is allowed in kiosk mode." },
                    { "SETTINGS_ACCESS_BLOCKED", "Access to the Settings app is not allowed in kiosk mode." },
                } },
                { "statusBar", new SchemaEnum("Status Bar", "Specifies whether system info and notifications are disabled in kiosk mode.") {
                    { "STATUS_BAR_UNSPECIFIED", "Unspecified, defaults to INFO_AND_NOTIFICATIONS_DISABLED." },
                    { "NOTIFICATIONS_AND_SYSTEM_INFO_ENABLED", "System info and notifications are shown on the status bar in kiosk mode. Note: For this policy to take effect, the device's home button must be enabled using kioskCustomization.systemNavigation." },
                    { "NOTIFICATIONS_AND_SYSTEM_INFO_DISABLED", "System info and notifications are disabled in kiosk mode." },
                    { "SYSTEM_INFO_ONLY", "Only system info is shown on the status bar." },
                } },
                { "systemErrorWarnings", new SchemaEnum("System Error Warnings", "Specifies whether system error dialogs for crashed or unresponsive apps are blocked in kiosk mode. When blocked, the system will force-stop the app as if the user chooses the \"close app\" option on the UI.") {
                    { "SYSTEM_ERROR_WARNINGS_UNSPECIFIED", "Unspecified, defaults to ERROR_AND_WARNINGS_MUTED." },
                    { "ERROR_AND_WARNINGS_ENABLED", "All system error dialogs such as crash and app not responding (ANR) are displayed." },
                    { "ERROR_AND_WARNINGS_MUTED", "All system error dialogs, such as crash and app not responding (ANR) are blocked. When blocked, the system force-stops the app as if the user closes the app from the UI." },
                } },
                { "systemNavigation", new SchemaEnum("System Navigation", "Specifies which navigation features are enabled (e.g. Home, Overview buttons) in kiosk mode.") {
                    { "SYSTEM_NAVIGATION_UNSPECIFIED", "Unspecified, defaults to NAVIGATION_DISABLED." },
                    { "NAVIGATION_ENABLED", "Home and overview buttons are enabled." },
                    { "NAVIGATION_DISABLED", "The home and Overview buttons are not accessible." },
                    { "HOME_BUTTON_ONLY", "Only the home button is enabled." },
                } },
                { "powerButtonActions", new SchemaEnum("Power Button Actions", "Sets the behavior of a device in kiosk mode when a user presses and holds (long-presses) the Power button.") {
                    { "POWER_BUTTON_ACTIONS_UNSPECIFIED", "Unspecified, defaults to POWER_BUTTON_AVAILABLE." },
                    { "POWER_BUTTON_AVAILABLE", "The power menu (e.g. Power off, Restart) is shown when a user long-presses the Power button of a device in kiosk mode." },
                    { "POWER_BUTTON_BLOCKED", "The power menu (e.g. Power off, Restart) is not shown when a user long-presses the Power button of a device in kiosk mode. Note: this may prevent users from turning off the device." },
                } },
            } },
            { "kioskCustomLauncherEnabled", new SchemaBoolean("Kiosk Custom Launcher Enabled", "Whether the kiosk custom launcher is enabled. This replaces the home screen with a launcher that locks down the device to the apps installed via the applications setting. Apps appear on a single page in alphabetical order. Use kioskCustomization to further configure the kiosk device behavior.") },
            { "locationMode", new SchemaEnum("Location Mode", "The degree of location detection enabled.") {
                { "LOCATION_MODE_UNSPECIFIED", "Defaults to LOCATION_USER_CHOICE." },
                { "LOCATION_USER_CHOICE", "Location setting is not restricted on the device. No specific behavior is set or enforced." },
                { "LOCATION_ENFORCED", "Enable location setting on the device." },
                { "LOCATION_DISABLED", "Disable location setting on the device." },
            } },
            { "longSupportMessage", new SchemaLocalizedString("Long Support Message", "A message displayed to the user in the device administrators settings screen.") },
            { "maximumTimeToLock", new SchemaInteger("Maximum Time To Lock", "Maximum time in milliseconds for user activity until the device locks. A value of 0 means there is no restriction.") },
            { "microphoneAccess", new SchemaEnum("Microphone Access", "Controls the use of the microphone and whether the user has access to the microphone access toggle. This applies only on fully managed devices.") {
                { "MICROPHONE_ACCESS_UNSPECIFIED", "This is equivalent to MICROPHONE_ACCESS_USER_CHOICE." },
                { "MICROPHONE_ACCESS_USER_CHOICE", "This is the default device behavior: the microphone on the device is available. On Android 12 and above, the user can use the microphone access toggle." },
                { "MICROPHONE_ACCESS_DISABLED", "The microphone on the device is disabled (for fully managed devices, this applies device-wide). The microphone access toggle has no effect as the microphone is disabled." },
                { "MICROPHONE_ACCESS_ENFORCED", "The microphone on the device is available. On devices running Android 12 and above, the user is unable to use the microphone access toggle. On devices which run Android 11 or below, this is equivalent to MICROPHONE_ACCESS_USER_CHOICE." },
            } },
            { "minimumApiLevel", new SchemaInteger("Minimum API Level", "The minimum allowed Android API level.") },
            { "mobileNetworksConfigDisabled", new SchemaBoolean("Mobile Networks Config Disabled", "Whether configuring mobile networks is disabled.") },
            { "modifyAccountsDisabled", new SchemaBoolean("Modify Accounts Disabled", "Whether adding or removing accounts is disabled.") },
            { "mountPhysicalMediaDisabled", new SchemaBoolean("Mount Physical Media Disabled", "Whether the user mounting physical external media is disabled.") },
            { "networkEscapeHatchEnabled", new SchemaBoolean("Network Escape Hatch Enabled", "Whether the network escape hatch is enabled. If a network connection can't be made at boot time, the escape hatch prompts the user to temporarily connect to a network in order to refresh the device policy. After applying policy, the temporary network will be forgotten and the device will continue booting. This prevents being unable to connect to a network if there is no suitable network in the last policy and the device boots into an app in lock task mode, or the user is otherwise unable to reach device settings Note: Setting wifiConfigDisabled to true will override this setting under specific circumstances. Please see wifiConfigDisabled for further details.") },
            { "networkResetDisabled", new SchemaBoolean("Network Reset Disabled", "Whether resetting network settings is disabled.") },
            { "openNetworkConfiguration", new SchemaObject("Open Network Configuration", "Network configuration for the device.") {
                { "NetworkConfigurations", new SchemaObjectMap("Network Configurations", "Describes WiFi connections.", key: ("GUID",new SchemaString("GUID", "A unique identifier for this network connection, which exists to make it possible to update previously imported configurations. Must be a non-empty string.")), editAllProperties: true) {
                    { "Name", new SchemaString("Name", "A user-friendly description of this connection. This name will not be used for referencing and may not be unique. Instead it may be used for describing the network to the user.") },
                    { "Type", new SchemaEnum("Type", "Indicates which kind of connection this is.") {
                        { "", "Unspecified." },
                        { "WiFi", "Specify a WiFi connection." },
                    } },
                    { "WiFi", new SchemaObject("WiFi", "WiFi settings.") {
                        { "SSID", new SchemaString("SSID", "The network's SSID.") },
                        { "HiddenSSID", new SchemaBoolean("Hidden SSID", "Indicated that the SSID is not being broadcasted.") },
                        { "AutoConnect", new SchemaBoolean("Auto Connect", "Indicates that the network should be connected to automatically when in range.") },
                        { "Security", new SchemaEnum("Security", "The network's security.") {
                            { "None", "No network security." },
                            { "WEP-PSK", "Use Wired Equivalent Privacy with a pre-shared key to connect to the network." },
                            { "WEP-8021X", "Use Wired Equivalent Privacy with the Extensible Authentication Protocol to connect to the network." },
                            { "WPA-PSK", "Use Wi-Fi Protected Access with a pre-shared key to connect to the network." },
                            { "WPA-EAP", "Use Wi-Fi Protected Access with the Extensible Authentication Protocol to connect to the network." },
                        } },
                        { "Passphrase", new SchemaString("Passphrase", "Describes the passphrase for WEP/WPA/WPA2 connections. If WEP-PSK is used, the passphrase must be of the format 0x<hex-number>, where <hex-number> is 40, 104, 128, or 232 bits. Required if Security is WEP-PSK or WPA-PSK, otherwise ignored.") },
                        { "EAP", new SchemaObject("EAP", "EAP settings, required if Security is WEP-8021X or WPA-EAP, otherwise ignored.") {
                            { "Inner", new SchemaEnum("Inner", "For tunneling outer protocols. Optional if Outer is EAP-TTLS or PEAP, otherwise ignored.") {
                                { "Automatic", "Automatically detect the inner protocol." },
                                { "MSCHAPv2", "Use Microsoft's version of the Challenge-Handshake Authentication Protocol as the inner protocol." },
                                { "PAP", "Use Password Authentication Protocol as the inner protocol." },
                            } },
                            { "Outer", new SchemaEnum("Outer", "Specifies the outer protocol.") {
                                { "", "Unspecified." },
                                { "EAP-AKA", "Use the Extensible Authentication Protocol with Authentication and Key Agreement as outer protocol." },
                                { "EAP-TLS", "Use the Extensible Authentication Protocol with Transport Layer Security as outer protocol." },
                                { "EAP-TTLS", "Use the Extensible Authentication Protocol with Tunneled Transport Layer Security as outer protocol." },
                                { "EAP-SIM", "Use the Extensible Authentication Protocol with Subscriber Identity Module as outer protocol." },
                                { "PEAP", "Use the Protected Extensible Authentication Protocol as outer protocol." },
                            } },
                            { "Identity", new SchemaString("Identity", "Identity of user. For tunneling outer protocols (PEAP and EAP-TTLS), this is used to authenticate inside the tunnel, and AnonymousIdentity is used for the EAP identity outside the tunnel. For non-tunneling outer protocols, this is used for the EAP identity. This value is subject to string expansions.") },
                            { "AnonymousIdentity", new SchemaBoolean("Anonymous Identity", "For tunnelling protocols only, this indicates the identity of the user presented to the outer protocol. This value is subject to string expansions. Optional if Outer is PEAP or EAP-TTLS, otherwise ignored.") },
                            { "Password", new SchemaString("Password", "Password of user. If not specified, defaults to prompting the user.") },
                            { "ClientCertType", new SchemaEnum("Client Certificate Type", "Indicates the type of the certificate to use.") {
                                { "None", "Do not use a certificate." },
                                { "Ref", "Use the ClientCertRef property to identify the client certificate." },
                            } },
                            { "ClientCertRef", new SchemaString("Client Certificate Reference", "Reference to client certificate stored in certificate section.") },
                            { "ServerCARefs", new SchemaStringArray("Server CA References", "List of references to CA certificates in the certificates section to be used for verifying the host's certificate chain. At least one of the CA certificates must match. If not set, the client does not check that the server certificate is signed by a specific CA. A verification using the system's CA certificates may still apply.") },
                            { "SubjectMatch", new SchemaString("Subject Match", "A substring which a remote RADIUS service certificate subject name must contain in order to connect.") },
                            { "SubjectAlternativeNameMatch", new SchemaObjectArray("Subject Alternative Name Match", "A list of alternative subject names to be matched against the alternative subject name of an authentication server certificate.", distinct: true, editAllProperties: true) {
                                { "Type", new SchemaEnum("Type", "Type of the alternative subject name.") {
                                    { "", "Unspecified." },
                                    { "EMAIL", "The Value is an email address." },
                                    { "DNS", "The Value is a FQDN." },
                                    { "URI", "THe Value is an URI." },
                                } },
                                { "Value", new SchemaString("Value", "Value of the alternative subject name.") },
                            } },
                            { "UseProactiveKeyCaching", new SchemaBoolean("Use Proactive Key Caching", "Indicates whether Proactive Key Caching (also known as Opportunistic Key Caching) should be used on a per-service basis.") },
                        } },
                    } },
                } },
                { "Certificates", new SchemaObjectMap("Certificates", "Contains certificates stored in X.509 or PKCS#12 format.", key: ("GUID", new SchemaString("GUID", "A unique identifier for this certificate. Must be a non-empty string.")), editAllProperties: true) {
                    { "Type", new SchemaEnum("Type", "Specified the certificate's type. Note: If Type disagrees with the x509 v3 basic constraints or key usage attributes, the Type field should be honored.") {
                        { "", "Unspecified." },
                        { "Client", "Indicates the certificate is for identifying the user or device over HTTPS or for VPN/802.1X." },
                        { "Server", "Indicates the certificate identifies an HTTPS or VPN/802.1X peer." },
                        { "Authority", "Indicates the certificate is a certificate authority and any certificates it issues should be trusted." },
                    } },
                    { "X509", new SchemaString("X509", "For certificate without private keys, this is the X509 certificate in PEM format.") },
                    { "PKCS12", new SchemaString("PKCS12", "For certificates with private keys, this is the base64 encoding of a PKCS#12 file. Required if Type is Client, otherwise ignored.") },
                    { "Scope", new SchemaObject("Scope", "Specifies the scope in which the certificate should be applied.") {
                        { "Type", new SchemaEnum("Type", "Specifies the scope's type.") {
                            { "Default", "Indicates that the scope the certificate applies in should not be restricted." },
                            { "Extension", "Indicates that the certificate should only be applied in the scope of a chrome extension." },
                        } },
                        { "ID", new SchemaString("ID", "If Type is Extension, this is the ID of the chrome extension for which the certificate should be applied.") },
                    } },
                } },
            } },
            { "outgoingBeamDisabled", new SchemaBoolean("Outgoing Beam Disabled", "Whether using NFC to beam data from apps is disabled.") },
            { "outgoingCallsDisabled", new SchemaBoolean("Outgoing Calls Disabled", "Whether outgoing calls are disabled.") },
            { "passwordPolicies", new SchemaObjectMap("Password Policies", "Password requirement policies. Different policies can be set for work profile or fully managed devices by setting the passwordScope field in the policy.", key: ("passwordScope", new SchemaEnum("Password Scope", "The scope that the password requirement applies to.") {
                    { "SCOPE_UNSPECIFIED", "The scope is unspecified. The password requirements are applied to the work profile for work profile devices and the whole device for fully managed or dedicated devices." },
                    { "SCOPE_DEVICE", "The password requirements are only applied to the device." },
                    { "SCOPE_PROFILE", "The password requirements are only applied to the work profile." },
                })) {
                { "requirePasswordUnlock", new SchemaEnum("Require Password Unlock", "The length of time after a device or work profile is unlocked using a strong form of authentication (password, PIN, pattern) that it can be unlocked using any other authentication method (e.g. fingerprint, trust agents, face). After the specified time period elapses, only strong forms of authentication can be used to unlock the device or work profile.") {
                    { "REQUIRE_PASSWORD_UNLOCK_UNSPECIFIED", "Unspecified. Defaults to USE_DEFAULT_DEVICE_TIMEOUT." },
                    { "USE_DEFAULT_DEVICE_TIMEOUT", "The timeout period is set to the device's default." },
                    { "REQUIRE_EVERY_DAY", "The timeout period is set to 24 hours." },
                } },
                { "passwordExpirationTimeout", new SchemaDuration("Password Expiration Timeout", "Password expiration timeout.") },
                { "passwordHistoryLength", new SchemaInteger("Password History Length", "The length of the password history. After setting this field, the user won't be able to enter a new password that is the same as any password in the history. A value of 0 means there is no restriction.") },
                { "maximumFailedPasswordsForWipe", new SchemaInteger("Maximum Failed Passwords For Wipe", "Number of incorrect device-unlock passwords that can be entered before a device is wiped. A value of 0 means there is no restriction.") },
                { "passwordQuality", new SchemaEnum("Password Quality", "The required password quality.") {
                    { "PASSWORD_QUALITY_UNSPECIFIED", "There are no password requirements." },
                    { "BIOMETRIC_WEAK", "The device must be secured with a low-security biometric recognition technology, at minimum. This includes technologies that can recognize the identity of an individual that are roughly equivalent to a 3-digit PIN (false detection is less than 1 in 1,000). This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_LOW for application." },
                    { "SOMETHING", "A password is required, but there are no restrictions on what the password must contain. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_LOW for application." },
                    { "NUMERIC", "The password must contain numeric characters. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_MEDIUM for application." },
                    { "NUMERIC_COMPLEX", "The password must contain numeric characters with no repeating (4444) or ordered (1234, 4321, 2468) sequences. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_MEDIUM for application." },
                    { "ALPHABETIC", "The password must contain alphabetic (or symbol) characters. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_HIGH for application." },
                    { "ALPHANUMERIC", "The password must contain both numeric and alphabetic (or symbol) characters. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_HIGH for application." },
                    { "COMPLEX", "The password must meet the minimum requirements specified in passwordMinimumLength, passwordMinimumLetters, passwordMinimumSymbols, etc. For example, if passwordMinimumSymbols is 2, the password must contain at least two symbols. This, when applied on personally owned work profile devices on Android 12 device-scoped, will be treated as COMPLEXITY_HIGH for application. In this case, the requirements in passwordMinimumLength, passwordMinimumLetters, passwordMinimumSymbols, etc are not applied." },
                    { "COMPLEXITY_LOW", "Define the low password complexity band as: pattern PIN with repeating (4444) or ordered (1234, 4321, 2468) sequences. This sets the minimum complexity band which the password must meet. Enforcement varies among different Android versions, management modes and password scopes." },
                    { "COMPLEXITY_MEDIUM", "Define the medium password complexity band as: PIN with no repeating (4444) or ordered (1234, 4321, 2468) sequences, length at least 4 alphabetic, length at least 4 alphanumeric, length at least 4. This sets the minimum complexity band which the password must meet. Enforcement varies among different Android versions, management modes and password scopes." },
                    { "COMPLEXITY_HIGH", "Define the high password complexity band as:On Android 12 and above: PIN with no repeating (4444) or ordered (1234, 4321, 2468) sequences, length at least 8 alphabetic, length at least 6 alphanumeric, length at least 6. This sets the minimum complexity band which the password must meet. Enforcement varies among different Android versions, management modes and password scopes." },
                } },
                { "passwordMinimumLength", new SchemaInteger("Password Minimum Length", "The minimum allowed password length. A value of 0 means there is no restriction. Only enforced when passwordQuality is NUMERIC, NUMERIC_COMPLEX, ALPHABETIC, ALPHANUMERIC, or COMPLEX.") },
                { "passwordMinimumLetters", new SchemaInteger("Password Minimum Letters", "Minimum number of letters required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "passwordMinimumLowerCase", new SchemaInteger("Password Minimum Lower Case", "Minimum number of lower case letters required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "passwordMinimumUpperCase", new SchemaInteger("Password Minimum Upper Case", "Minimum number of upper case letters required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "passwordMinimumNonLetter", new SchemaInteger("Password Minimum Non Letter", "Minimum number of non-letter characters (numerical digits or symbols) required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "passwordMinimumNumeric", new SchemaInteger("Password Minimum Numeric", "Minimum number of numerical digits required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "passwordMinimumSymbols", new SchemaInteger("Password Minimum Symbols", "Minimum number of symbols required in the password. Only enforced when passwordQuality is COMPLEX.") },
                { "unifiedLockSettings", new SchemaEnum("Unified Lock Settings", "Controls whether a unified lock is allowed for the device and the work profile, on devices running Android 9 and above with a work profile. This can be set only if passwordScope is set to SCOPE_PROFILE, the policy will be rejected otherwise. If user has not set a separate work lock and this field is set to REQUIRE_SEPARATE_WORK_LOCK, a NonComplianceDetail is reported with nonComplianceReason set to USER_ACTION.") {
                    { "UNIFIED_LOCK_SETTINGS_UNSPECIFIED", "Unspecified. Defaults to ALLOW_UNIFIED_WORK_AND_PERSONAL_LOCK." },
                    { "ALLOW_UNIFIED_WORK_AND_PERSONAL_LOCK", "A common lock for the device and the work profile is allowed." },
                    { "REQUIRE_SEPARATE_WORK_LOCK", "A separate lock for the work profile is required." },
                } },
            } },
            { "permissionGrants", new SchemaObjectMap("Permission Grants", "Explicit permission or group grants or denials for all apps. These values override the defaultPermissionPolicy.", key: ("permission",new SchemaString("Permission", "The Android permission or group, e.g. android.permission.READ_CALENDAR or android.permission_group.CALENDAR.")), editAllProperties: true) {
                { "policy", new SchemaEnum("Policy", "The policy for granting the permission.") {
                    { "PERMISSION_POLICY_UNSPECIFIED", "Policy not specified. If no policy is specified for a permission at any level, then the PROMPT behavior is used by default." },
                    { "PROMPT", "Prompt the user to grant a permission." },
                    { "GRANT", "Automatically grant a permission." },
                    { "DENY", "Automatically deny a permission." },
                } },
            } },
            { "permittedAccessibilityServices", new SchemaObject("Permitted Accessibility Services", "Specifies permitted accessibility services. If the field is not set, any accessibility service can be used. If the field is set, only the accessibility services in this list and the system's built-in accessibility service can be used. In particular, if the field is set to empty, only the system's built-in accessibility services can be used.", editAllProperties: true) {
                { "packageNames", new SchemaStringArray("Package Names", "A list of package names.") },
            } },
            { "permittedInputMethods", new SchemaObject("Permitted Input Methods", "If present, only the input methods provided by packages in this list are permitted. If this field is present, but the list is empty, then only system input methods are permitted.", editAllProperties: true) {
                { "packageNames", new SchemaStringArray("Package Names", "A list of package names.") },
            } },
            { "persistentPreferredActivities", new SchemaObjectMap("Persistent Preferred Activities", "Default intent handler activities.", key: ("receiverActivity",  new SchemaString("Receiver Activity", "The activity that should be the default intent handler. This should be an Android component name, e.g. com.android.enterprise.app/.MainActivity. Alternatively, the value may be the package name of an app, which causes Android Device Policy to choose an appropriate activity from the app to handle the intent."))) {
                { "actions", new SchemaStringArray("Actions", "The intent actions to match in the filter. If any actions are included in the filter, then an intent's action must be one of those values for it to match. If no actions are included, the intent action is ignored.") },
                { "categories", new SchemaStringArray("Categories", "The intent categories to match in the filter. An intent includes the categories that it requires, all of which must be included in the filter in order to match. In other words, adding a category to the filter has no impact on matching unless that category is specified in the intent.") },
            } },
            { "personalUsagePolicies", new SchemaObject("Personal Usage Policies", "Policies managing personal usage on a company-owned device.") {
                { "accountTypesWithManagementDisabled", new SchemaStringArray("Account Types With Management Disabled", "Account types that can't be managed by the user.") },
                { "cameraDisabled", new SchemaBoolean("Camera Disabled", "Whether camera is disabled.") },
                { "maxDaysWithWorkOff", new SchemaInteger("Max Days With Work Off", "Controls how long the work profile can stay off. The duration must be at least 3 days.") },
                { "personalPlayStoreMode", new SchemaEnum("Personal Play Store Mode", "Used together with personalApplications to control how apps in the personal profile are allowed or blocked.") {
                    { "PLAY_STORE_MODE_UNSPECIFIED", "Unspecified. Defaults to BLOCKLIST." },
                    { "BLOCKLIST", "All Play Store apps are available for installation in the personal profile, except those whose installType is BLOCKED in personalApplications." },
                    { "ALLOWLIST", "Only apps explicitly specified in personalApplications with installType set to AVAILABLE are allowed to be installed in the personal profile." },
                } },
                { "screenCaptureDisabled", new SchemaBoolean("Screen Capture Disabled", "Whether screen capture is disabled.") },
                { "personalApplications", new SchemaObjectMap("Personal Applications", "Policy applied to applications in the personal profile.", key:( "packageName", new SchemaString("Package Name", "The package name of the application.")), editAllProperties: true) {
                    { "installType", new SchemaEnum("Install Type", "The type of installation to perform.") {
                        { "INSTALL_TYPE_UNSPECIFIED", "Unspecified. Defaults to AVAILABLE." },
                        { "BLOCKED", "The app is blocked and can't be installed in the personal profile." },
                        { "AVAILABLE", "The app is available to install in the personal profile." },
                    } },
                } },
            } },
            { "playStoreMode", new SchemaEnum("Play Store Mode", "This mode controls which apps are available to the user in the Play Store and the behavior on the device when apps are removed from the policy.") {
                { "PLAY_STORE_MODE_UNSPECIFIED", "Unspecified. Defaults to WHITELIST." },
                { "WHITELIST", "Only apps that are in the policy are available and any app not in the policy will be automatically uninstalled from the device." },
                { "BLACKLIST", "All apps are available and any app that should not be on the device should be explicitly marked as 'BLOCKED' in the applications policy." },
            } },
            { "policyEnforcementRules", new SchemaObjectMap("Policy Enforcement Rules", "Rules that define the behavior when a particular policy can not be applied on device", key: ("settingName", new SchemaString("Setting Name", "The top-level policy to enforce. For example, applications or passwordPolicies.")), editAllProperties: true) {
                { "blockAction", new SchemaObject("Block Action", "An action to block access to apps and data on a fully managed device or in a work profile. This action also triggers a user-facing notification with information (where possible) on how to correct the compliance issue. Note: wipeAction must also be specified.") {
                    { "blockScope", new SchemaEnum("Block Scope", "Specifies the scope of this BlockAction. Only applicable to devices that are company-owned.") {
                        { "BLOCK_SCOPE_UNSPECIFIED", "Unspecified. Defaults to BLOCK_SCOPE_WORK_PROFILE." },
                        { "BLOCK_SCOPE_WORK_PROFILE", "Block action is only applied to apps in the work profile. Apps in the personal profile are unaffected." },
                        { "BLOCK_SCOPE_DEVICE", "Block action is applied to the entire device, including apps in the personal profile." },
                    } },
                    { "blockAfterDays", new SchemaInteger("Block After Days", "Number of days the policy is non-compliant before the device or work profile is blocked. To block access immediately, set to 0. blockAfterDays must be less than wipeAfterDays.") },
                } },
                { "wipeAction", new SchemaObject("Wipe Action", "An action to reset a fully managed device or delete a work profile. Note: blockAction must also be specified.") {
                    { "wipeAfterDays", new SchemaInteger("Wipe After Days", "Number of days the policy is non-compliant before the device or work profile is wiped. wipeAfterDays must be greater than blockAfterDays.") },
                    { "preserveFrp", new SchemaBoolean("Preserve FRP", "Whether the factory-reset protection data is preserved on the device. This setting doesn't apply to work profiles.") },
                } },
            } },
            { "preferentialNetworkService", new SchemaEnum("Preferential Network Service", "Controls whether preferential network service is enabled on the work profile. For example, an organization may have an agreement with a carrier that all of the work data from its employees' devices will be sent via a network service dedicated for enterprise use. An example of a supported preferential network service is the enterprise slice on 5G networks. This has no effect on fully managed devices.") {
                { "PREFERENTIAL_NETWORK_SERVICE_UNSPECIFIED", "Unspecified. Defaults to PREFERENTIAL_NETWORK_SERVICES_DISABLED." },
                { "PREFERENTIAL_NETWORK_SERVICE_DISABLED", "Preferential network service is disabled on the work profile." },
                { "PREFERENTIAL_NETWORK_SERVICE_ENABLED", "Preferential network service is enabled on the work profile." },
            } },
            { "printingPolicy", new SchemaEnum("Printing Policy", "Controls whether printing is allowed. This is supported on devices running Android 9 and above.") {
                { "PRINTING_POLICY_UNSPECIFIED", "Unspecified. Defaults to PRINTING_ALLOWED." },
                { "PRINTING_DISALLOWED", "Printing is disallowed." },
                { "PRINTING_ALLOWED", "Printing is allowed." },
            } },
            { "privateKeySelectionEnabled", new SchemaBoolean("Private Key Selection Enabled", "Allows showing UI on a device for a user to choose a private key alias if there are no matching rules in ChoosePrivateKeyRules. For devices below Android P, setting this may leave enterprise keys vulnerable.") },
            { "recommendedGlobalProxy", new SchemaObject("Recommended Global Proxy", "The network-independent global HTTP proxy. Typically proxies should be configured per-network in openNetworkConfiguration. However for unusual configurations like general internal filtering a global HTTP proxy may be useful. If the proxy is not accessible, network access may break. The global proxy is only a recommendation and some apps may ignore it.") {
                { "pacUri", new SchemaString("PAC URI", "The URI of the PAC script used to configure the proxy.") },
                { "host", new SchemaString("Host", "The host of the direct proxy.") },
                { "port", new SchemaInteger("Port", "The port of the direct proxy.") },
                { "excludedHosts", new SchemaStringArray("Excluded Hosts", "For a direct proxy, the hosts for which the proxy is bypassed. The host names may contain wildcards such as *.example.com.") },
            } },
            { "removeUserDisabled", new SchemaBoolean("Remove User Disabled", "Whether removing other users is disabled.") },
            { "screenCaptureDisabled", new SchemaBoolean("Screen Capture Disabled", "Whether screen capture is disabled.") },
            { "setupActions", new SchemaObjectArray("Setup Actions", "Actions to take during the setup process.", editAllProperties: true) {
                { "title", new SchemaLocalizedString("Title", "Title of this action.") },
                { "description", new SchemaLocalizedString("Description", "Description of this action.") },
                { "launchApp", new SchemaObject("Launch App", "An action to launch an app.", editAllProperties: true) {
                    { "packageName", new SchemaString("Package Name", "Package name of app to be launched") },
                } },
            } },
            { "setUserIconDisabled", new SchemaBoolean("Set User Icon Disabled", "Whether changing the user icon is disabled.") },
            { "setWallpaperDisabled", new SchemaBoolean("Set Wallpaper Disabled", "Whether changing the wallpaper is disabled.") },
            { "shareLocationDisabled", new SchemaBoolean("Share Location Disabled", "Whether location sharing is disabled.") },
            { "shortSupportMessage", new SchemaLocalizedString("Short Support Message", "A message displayed to the user in the settings screen wherever functionality has been disabled by the admin. If the message is longer than 200 characters it may be truncated.") },
            { "skipFirstUseHintsEnabled", new SchemaBoolean("Skip First Use Hints Enabled", "Flag to skip hints on the first use. Enterprise admin can enable the system recommendation for apps to skip their user tutorial and other introductory hints on first start-up.") },
            { "smsDisabled", new SchemaBoolean("SMS Disabled", "Whether sending and receiving SMS messages is disabled.") },
            { "statusReportingSettings", new SchemaObject("Status Reporting Settings", "Status reporting settings") {
                { "applicationReportsEnabled", new SchemaBoolean("Application Reports Enabled", "Whether app reports are enabled.") },
                { "applicationReportingSettings", new SchemaObject("Application Reporting Settings", "Application reporting settings. Only applicable if applicationReportsEnabled is true.", editAllProperties: true) {
                    { "includeRemovedApps", new SchemaBoolean("Include Removed Apps", "Whether removed apps are included in application reports.") },
                } },
                { "commonCriteriaModeEnabled", new SchemaBoolean("Common Criteria Mode Enabled", "Whether Common Criteria Mode reporting is enabled.") },
                { "deviceSettingsEnabled", new SchemaBoolean("Device Settings Enabled", "Whether device settings reporting is enabled.") },
                { "displayInfoEnabled", new SchemaBoolean("Display Info Enabled", "Whether displays reporting is enabled. Report data is not available for personally owned devices with work profiles.") },
                { "hardwareStatusEnabled", new SchemaBoolean("Hardware Status Enabled", "Whether hardware status reporting is enabled. Report data is not available for personally owned devices with work profiles.") },
                { "memoryInfoEnabled", new SchemaBoolean("Memory Info Enabled", "Whether memory event reporting is enabled.") },
                { "networkInfoEnabled", new SchemaBoolean("Network Info Enabled", "Whether network info reporting is enabled.") },
                { "powerManagementEventsEnabled", new SchemaBoolean("Power Management Events Enabled", "Whether power management event reporting is enabled. Report data is not available for personally owned devices with work profiles.") },
                { "softwareInfoEnabled", new SchemaBoolean("Software Info Enabled", "Whether software info reporting is enabled.") },
                { "systemPropertiesEnabled", new SchemaBoolean("System Properties Enabled", "Whether system properties reporting is enabled.") },
            } },
            { "stayOnPluggedModes", new SchemaFlags("Stay On Plugged Modes", "The battery plugged in modes for which the device stays on. When using this setting, it is recommended to clear maximumTimeToLock so that the device doesn't lock itself while it stays on.") {
                { "BATTERY_PLUGGED_MODE_UNSPECIFIED", "This value is ignored." },
                { "AC", "Power source is an AC charger." },
                { "USB", "Power source is a USB port." },
                { "WIRELESS", "Power source is wireless." },
            } },
            { "systemUpdate", new SchemaObject("System Update", "The system update policy, which controls how OS updates are applied. If the update type is WINDOWED, the update window will automatically apply to Play app updates as well.") {
                { "type", new SchemaEnum("Type", "The type of system update to configure.") {
                    { "SYSTEM_UPDATE_TYPE_UNSPECIFIED", "Follow the default update behavior for the device, which typically requires the user to accept system updates." },
                    { "AUTOMATIC", "Install automatically as soon as an update is available." },
                    { "WINDOWED", "Install automatically within a daily maintenance window. This also configures Play apps to be updated within the window. This is strongly recommended for kiosk devices because this is the only way apps persistently pinned to the foreground can be updated by Play. If autoUpdateMode is set to AUTO_UPDATE_HIGH_PRIORITY for an app, then the maintenance window is ignored for that app and it is updated as soon as possible even outside of the maintenance window." },
                    { "POSTPONE", "Postpone automatic install up to a maximum of 30 days." },
                } },
                { "freezePeriods", new SchemaObjectArray("Freeze Periods", "An annually repeating time period in which over-the-air (OTA) system updates are postponed to freeze the OS version running on a device. To prevent freezing the device indefinitely, each freeze period must be separated by at least 60 days.", distinct: true, editAllProperties: true) {
                    { "startDate", new SchemaObject("Start Date", "The start date (inclusive) of the freeze period.", editAllProperties: true) {
                        { "month", new SchemaInteger("Month", "Month of start date."){ Maximum=12 } },
                        { "date", new SchemaInteger("Date", "Day of month of start date."){ Maximum=31 } },
                    } },
                    { "endDate", new SchemaObject("End Date", "The end date (inclusive) of the freeze period. Must be no later than 90 days from the start date. If the end date is earlier than the start date, the freeze period is considered wrapping year-end.", editAllProperties: true) {
                        { "month", new SchemaInteger("Month", "Month of end date."){ Maximum=12 } },
                        { "date", new SchemaInteger("Date", "Day of month of end date."){ Maximum=31 } },
                    } },
                } },
                { "startMinutes", new SchemaInteger("Start Minutes", "If the type is WINDOWED, the start of the maintenance window, measured as the number of minutes after midnight in the device's local time. This value must be between 0 and 1439, inclusive."){ Maximum=1439 } },
                { "endMinutes", new SchemaInteger("End Minutes", "If the type is WINDOWED, the end of the maintenance window, measured as the number of minutes after midnight in device's local time. This value must be between 0 and 1439, inclusive. If this value is less than startMinutes, then the maintenance window spans midnight. If the maintenance window specified is smaller than 30 minutes, the actual window is extended to 30 minutes beyond the start time."){ Maximum=1439 } },
            } },
            { "uninstallAppsDisabled", new SchemaBoolean("Uninstall Apps Disabled", "Whether user uninstallation of applications is disabled.") },
            { "usageLog", new SchemaObject("Usage Log", "Configuration of device activity logging.") {
                { "enabledLogTypes", new SchemaFlags("Enabled Log Types", "Specifies which log types are enabled. Note that users will receive on-device messaging when usage logging is enabled.") {
                    { "LOG_TYPE_UNSPECIFIED", "This value is not used." },
                    { "SECURITY_LOGS", "Enable logging of on-device security events, like when the device password is incorrectly entered or removable storage is mounted. See UsageLogEvent for a complete description of the logged security events. Supported for fully managed devices on Android 7 and above. Supported for company-owned devices with a work profile on Android 12 and above, on which only security events from the work profile are logged. Can be overridden by the application delegated scope SECURITY_LOGS" },
                    { "NETWORK_ACTIVITY_LOGS", "Enable logging of on-device network events, like DNS lookups and TCP connections. See UsageLogEvent for a complete description of the logged network events. Supported for fully managed devices on Android 8 and above. Supported for company-owned devices with a work profile on Android 12 and above, on which only network events from the work profile are logged. Can be overridden by the application delegated scope NETWORK_ACTIVITY_LOGS" },
                } },
                { "uploadOnCellularAllowed", new SchemaFlags("Upload On Cellular Allowed", "Specifies which of the enabled log types can be uploaded over mobile data. By default logs are queued for upload when the device connects to WiFi.") {
                    { "LOG_TYPE_UNSPECIFIED", "This value is not used." },
                    { "SECURITY_LOGS", "Enable logging of on-device security events, like when the device password is incorrectly entered or removable storage is mounted. See UsageLogEvent for a complete description of the logged security events. Supported for fully managed devices on Android 7 and above. Supported for company-owned devices with a work profile on Android 12 and above, on which only security events from the work profile are logged. Can be overridden by the application delegated scope SECURITY_LOGS" },
                    { "NETWORK_ACTIVITY_LOGS", "Enable logging of on-device network events, like DNS lookups and TCP connections. See UsageLogEvent for a complete description of the logged network events. Supported for fully managed devices on Android 8 and above. Supported for company-owned devices with a work profile on Android 12 and above, on which only network events from the work profile are logged. Can be overridden by the application delegated scope NETWORK_ACTIVITY_LOGS" },
                } },
            } },
            { "vpnConfigDisabled", new SchemaBoolean("VPN Config Disabled", "Whether configuring VPN is disabled.") },
            { "wipeDataFlags", new SchemaFlags("Wipe Data Flags", "Wipe flags to indicate what data is wiped when a device or profile wipe is triggered due to any reason (for example, non-compliance). This does not apply to the enterprises.devices.delete method. This list must not have duplicates.") {
                { "WIPE_DATA_FLAG_UNSPECIFIED", "This value must not be used." },
                { "WIPE_ESIMS", "For company-owned devices, setting this in wipeDataFlags will remove all eSIMs on the device when wipe is triggered due to any reason. On personally-owned devices, this will remove only managed eSIMs on the device. (eSIMs which are added via the ADD_ESIM command). This is supported on devices running Android 15 and above." },
            } },
            { "workAccountSetupConfig", new SchemaObject("Work Account Setup Config", "Controls the work account setup configuration, such as details of whether a Google authenticated account is required.") {
                { "authenticationType", new SchemaEnum("Authentication Type", "The authentication type of the user on the device.") {
                    { "AUTHENTICATION_TYPE_UNSPECIFIED", "Unspecified. Defaults to AUTHENTICATION_TYPE_NOT_ENFORCED." },
                    { "AUTHENTICATION_TYPE_NOT_ENFORCED", "Authentication status of user on device is not enforced." },
                    { "GOOGLE_AUTHENTICATED", "Requires device to be managed with a Google authenticated account." },
                } },
                { "requiredAccountEmail", new SchemaString("Required Account Email", "The specific google work account email address to be added. This field is only relevant if authenticationType is GOOGLE_AUTHENTICATED. This must be an enterprise account and not a consumer account. Once set and a Google authenticated account is added to the device, changing this field will have no effect, and thus recommended to be set only once.") },
            } },
        };
    }
}