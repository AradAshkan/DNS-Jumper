using System;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.Collections.Generic;

namespace DNSJumperApp
{
    public partial class Form1 : Form
    {
        private ComboBox cbAdapters = null!;
        private ComboBox cbDNS = null!;
        private Button btnSetDNS = null!;
        private Button btnResetDNS = null!;
        private Button btnFlushDNS = null!;
        private Button btnThemeToggle = null!;
        
        private bool isDarkMode = true;

        private Color BgColor => isDarkMode ? Color.FromArgb(20, 20, 25) : Color.FromArgb(245, 245, 250);
        private Color CardColor => isDarkMode ? Color.FromArgb(35, 35, 45) : Color.White;
        private Color TextColor => isDarkMode ? Color.White : Color.FromArgb(30, 30, 30);
        private Color AccentColor = Color.FromArgb(0, 174, 219);

        public Form1()
        {
            InitializeComponent();
            LoadThemePreference();
            SetupFormLayout();
            LoadRealNetworkAdapters();
            ApplyTheme();

            try
            {
                this.Icon = new Icon("dns_icon.ico");

            }
            catch
            {
                // 
            }
        }

        private void SetupFormLayout()
        {
            this.Text = "DNS Jumper Pro";
            this.Size = new Size(420, 520);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgColor;

            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.Transparent };
            var lblTitle = new Label {
                Text = "DNS SETTINGS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = AccentColor,
                Location = new Point(20, 20),
                AutoSize = true
            };

            btnThemeToggle = new Button {
                Size = new Size(40, 40),
                Location = new Point(350, 12),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12)
            };
            btnThemeToggle.FlatAppearance.BorderSize = 0;
            btnThemeToggle.Click += (s, e) => { isDarkMode = !isDarkMode; SaveThemePreference(); ApplyTheme(); };

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(btnThemeToggle);

            int contentWidth = 360;
            int startX = 22;

            var lblAdapter = CreateLabel("Network Adapter (Real Only):", 80, startX);
            cbAdapters = CreateComboBox(110, startX, contentWidth);

            var lblDNS = CreateLabel("Select DNS Provider:", 180, startX);
            cbDNS = CreateComboBox(210, startX, contentWidth);
            cbDNS.Items.AddRange(new string[] {
                "Google (8.8.8.8 | 8.8.4.4)", 
                "Cloudflare (1.1.1.1 | 1.0.0.1)", 
                "Shecan (Iran Bypass)", 
                "Electro (Gaming)", 
                "Custom DNS..."
            });
            cbDNS.SelectedIndex = 0;

            btnSetDNS = CreateButton("APPLY DNS", 300, startX, contentWidth, true);
            btnResetDNS = CreateButton("RESTORE TO DHCP (DEFAULT)", 365, startX, contentWidth, false);
            btnFlushDNS = CreateButton("FLUSH DNS CACHE", 415, startX, contentWidth, false);

            btnSetDNS.Click += BtnSetDNS_Click;
            btnResetDNS.Click += BtnResetDNS_Click;
            btnFlushDNS.Click += BtnFlushDNS_Click;

            this.Controls.AddRange(new Control[] { 
                headerPanel, lblAdapter, cbAdapters, lblDNS, cbDNS, 
                btnSetDNS, btnResetDNS, btnFlushDNS 
            });
        }

        private void LoadRealNetworkAdapters()
        {
            cbAdapters.Items.Clear();
            
            string[] virtualKeywords = { "virtual", "vmware", "hyper-v", "pseudo", "loopback", "vpn", "docker", "npcap" };

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && 
                           (n.NetworkInterfaceType == NetworkInterfaceType.Ethernet || n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                .Where(n => !virtualKeywords.Any(v => n.Description.ToLower().Contains(v) || n.Name.ToLower().Contains(v)))
                .Select(n => n.Name).ToArray();

            if (adapters.Length == 0) cbAdapters.Items.Add("No Physical Adapter Found");
            else { cbAdapters.Items.AddRange(adapters); cbAdapters.SelectedIndex = 0; }
        }

        private void BtnSetDNS_Click(object? sender, EventArgs e)
        {
            string adapter = cbAdapters.Text;
            if (adapter.Contains("No")) return;

            (string dns1, string dns2) = cbDNS.Text switch {
                "Google (8.8.8.8 | 8.8.4.4)" => ("8.8.8.8", "8.8.4.4"),
                "Cloudflare (1.1.1.1 | 1.0.0.1)" => ("1.1.1.1", "1.0.0.1"),
                "Shecan (Iran Bypass)" => ("178.22.122.100", "185.51.200.2"),
                "Electro (Gaming)" => ("78.157.42.100", "78.157.42.101"),
                _ => (Prompt.ShowDialog("Enter Primary DNS:", "DNS 1"), Prompt.ShowDialog("Enter Secondary DNS:", "DNS 2"))
            };

            if (!string.IsNullOrEmpty(dns1)) {
                ExecuteNetsh($"interface ipv4 set dns name=\"{adapter}\" static {dns1} primary");
                if (!string.IsNullOrEmpty(dns2)) {
                    ExecuteNetsh($"interface ipv4 add dns name=\"{adapter}\" addr={dns2} index=2");
                }
                MessageBox.Show($"DNS successfully set to:\n1: {dns1}\n2: {dns2}", "Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnResetDNS_Click(object? sender, EventArgs e) {
            ExecuteNetsh($"interface ipv4 set dns name=\"{cbAdapters.Text}\" dhcp");
            MessageBox.Show("DNS settings restored to Automatic (DHCP).", "Restored");
        }

        private void BtnFlushDNS_Click(object? sender, EventArgs e) {
            Process.Start(new ProcessStartInfo("ipconfig", "/flushdns") { CreateNoWindow = true, UseShellExecute = true, Verb = "runas" });
            MessageBox.Show("DNS Cache Flushed Successfully!");
        }

        private void ExecuteNetsh(string args) {
            try {
                var psi = new ProcessStartInfo("netsh", args) {
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private Label CreateLabel(string text, int top, int left) => new Label { Text = text, Top = top, Left = left, AutoSize = true, Font = new Font("Segoe UI Semibold", 9), ForeColor = TextColor };
        private ComboBox CreateComboBox(int top, int left, int width) => new ComboBox { Top = top, Left = left, Width = width, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10), BackColor = CardColor, ForeColor = TextColor };
        private Button CreateButton(string text, int top, int left, int width, bool isPrimary) => new Button { Text = text, Top = top, Left = left, Width = width, Height = isPrimary ? 50 : 40, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9, FontStyle.Bold), BackColor = isPrimary ? AccentColor : Color.Transparent, ForeColor = isPrimary ? Color.White : TextColor, TextAlign = ContentAlignment.MiddleCenter };

        private void ApplyTheme() {
            this.BackColor = BgColor;
            btnThemeToggle.Text = isDarkMode ? "☀️" : "🌙";
            btnThemeToggle.ForeColor = isDarkMode ? Color.Gold : Color.DimGray;
            foreach (Control c in this.Controls) {
                if (c is Label lbl) lbl.ForeColor = TextColor;
                if (c is ComboBox cb) { cb.BackColor = CardColor; cb.ForeColor = TextColor; }
                if (c is Button btn && btn != btnSetDNS && btn != btnThemeToggle) {
                    btn.BackColor = isDarkMode ? Color.FromArgb(45, 45, 55) : Color.FromArgb(230, 230, 235);
                    btn.ForeColor = TextColor;
                    btn.FlatAppearance.BorderSize = 0;
                }
            }
            btnSetDNS.FlatAppearance.BorderSize = 0;
        }

        private void LoadThemePreference() { try { using var key = Registry.CurrentUser.OpenSubKey(@"Software\DNSJumper"); if (key != null) isDarkMode = key.GetValue("Theme", "Dark").ToString() == "Dark"; } catch { isDarkMode = true; } }
        private void SaveThemePreference() { try { using var key = Registry.CurrentUser.CreateSubKey(@"Software\DNSJumper"); key.SetValue("Theme", isDarkMode ? "Dark" : "Light"); } catch { } }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form() { Width = 320, Height = 160, Text = caption, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            Label lbl = new Label() { Left = 20, Top = 20, Text = text, AutoSize = true };
            TextBox txt = new TextBox() { Left = 20, Top = 45, Width = 260 };
            Button ok = new Button() { Text = "Set", Left = 205, Top = 80, Width = 75, DialogResult = DialogResult.OK };
            prompt.Controls.AddRange(new Control[] { lbl, txt, ok });
            return prompt.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
    }
}