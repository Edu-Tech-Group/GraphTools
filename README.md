# GraphTools

GraphTools is a Windows command-line tool for updating the group tag and, optionally, the device name of Windows Autopilot devices registered in Microsoft Intune.

The application supports Microsoft 365 work accounts from multiple organizations. You must sign in with an account that is allowed to manage Windows Autopilot devices in the tenant you want to update.

## Update Autopilot group tags

1. Download the latest `GroupTags.exe` release.
2. Export the Windows Autopilot device list from Intune, or create a CSV file using the format below.
3. Start `GroupTags.exe`.
4. Enter the full path to the CSV file and press Enter.
5. Follow the displayed Microsoft device sign-in instructions and sign in to the tenant you want to update.
6. Review the result in the terminal. Press Enter to close the application when it reports `Done`.

The application only updates devices whose serial number occurs in both the CSV file and the tenant's Autopilot device list. Other registered devices are not changed.

## CSV format

The CSV file must contain a serial-number column and a group-tag column. English and Dutch Intune column names are supported:

```csv
Serial number,Group tag,Devicename
PW0P25VL,2608_TRC,TRC-LAPTOP-001
PW0P25VM,2608_TRC,
```

| Value | Accepted column name | Required |
| --- | --- | --- |
| Device serial number | `Serial number` or `Serienummer` | Yes |
| New Autopilot group tag | `Group tag` or `Groepstag` | Yes |
| New device name | `Devicename` | No |

CSV exports downloaded directly from the current Intune portal are supported, including exports that wrap an entire row in quotes.

## Test without changing devices

Run the application from PowerShell with `--dry-run` to verify Microsoft Graph access and the CSV contents without updating Intune:

```powershell
.\GroupTags.exe "C:\Exports\Windows Autopilot devices.csv" --dry-run
```

The dry run reads the tenant's Autopilot inventory and reports each update it would perform. It does not submit any device changes.

## Normal command-line use

You can also provide the CSV path directly:

```powershell
.\GroupTags.exe "C:\Exports\Windows Autopilot devices.csv"
```

Device names are changed only when the optional `Devicename` column contains a value.

## Troubleshooting

- **CSV file not found**: Check that the path is correct. Put paths containing spaces between quotation marks.
- **Microsoft sign-in or permission error**: Sign in with a work account that has access to Intune and Windows Autopilot in the intended tenant. If the wrong cached account is used, ask IT support to clear the GraphTools sign-in cache.
- **A serial number is not mentioned in the result**: Confirm that it exactly matches a registered Autopilot device in the tenant.
- **A row failed**: Keep the error shown in the terminal and provide it to IT support. Other rows will continue to be processed.
- **Group tags do not appear immediately**: Refresh the Autopilot device list in Intune after processing finishes; Microsoft may take a short time to display the update.

## Security

GraphTools uses Microsoft's device sign-in flow. It does not ask for or store your password. Microsoft authentication tokens may be cached securely on the computer to reduce repeated sign-ins.

GraphTools requests the delegated Microsoft Graph permission `DeviceManagementServiceConfig.ReadWrite.All`. The signed-in user's existing Intune permissions still apply.

### First use in another organization

GraphTools is a multitenant application registered by The Rent Company. Before it can be used in another Microsoft 365 tenant, an administrator for that tenant must approve the requested Microsoft Graph permission. Microsoft may show an admin-approval or consent prompt during first sign-in. Consent granted by one organization does not grant access to any other organization.

The external tenant must have an active Microsoft Intune subscription, and the signed-in user must have a suitable Intune role for managing Windows Autopilot devices. If user consent is disabled or the permission requires administrator approval, contact that tenant's Microsoft 365 administrator.

Always use `--dry-run` first when working with a new or edited CSV file.
