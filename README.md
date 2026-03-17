# DNS Jumper Pro

**Beta Version – Windows DNS Management Tool**

DNS Jumper Pro is a simple and practical tool for managing DNS settings on Windows systems. With this app, you can quickly change your DNS, restore it to DHCP, and flush the DNS cache.

> ⚠️ Note: This is a Beta version. Some features or load operations may not work perfectly. The **Release** contains the built executable for download.

---

## Features

- Shows only **real network adapters** (physical adapters)
- Change DNS to popular providers:
  - Google (8.8.8.8 | 8.8.4.4)
  - Cloudflare (1.1.1.1 | 1.0.0.1)
  - Shecan (Iran Bypass)
  - Electro (Gaming)
  - Or enter a custom DNS
- Restore DNS settings to **DHCP (automatic)**
- Flush DNS cache
- Light and dark themes with saved preferences

---

## Installation and Usage

1. Clone or download the source code.
2. Open the project in **Visual Studio**.
3. Build the project, or download the ready-made executable from **[Release](https://github.com/AradAshkan/DNS-Jumper/releases/)**.
4. Run the program as **Administrator** to apply DNS changes.

---

## Limitations

- Beta version: some elements may not load correctly.
- Certain DNS servers may not work properly without internet connection or limited network access.
- Only real (non-virtual) adapters are detected.
- The build file in Release may have minor issues, but core DNS management works as intended.

---

## Quick Usage Guide

1. Open the program.
2. Select your network adapter.
3. Choose a DNS provider or enter a custom DNS.
4. Click **APPLY DNS** to apply changes.
5. Use **RESTORE TO DHCP** to revert to automatic settings.
6. Click **FLUSH DNS CACHE** to clear the DNS cache.

---

## Technology

- Programming language: C# (.NET Framework / .NET 6)
- User interface: Windows Forms
- DNS changes via `netsh` commands
- Theme settings stored in Windows Registry

---

## Notes

- This program is designed for Windows only and does not run on other operating systems.
- Administrator privileges are required to modify DNS settings.

---

## Contributing

Suggestions, bug reports, and Pull Requests are always welcome! Please ensure your changes do not break core functionality before submitting a PR.

---

**Developer:** Arad  
**Version:** Beta
