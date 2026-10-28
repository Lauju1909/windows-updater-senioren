using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ComputerAktualisierer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class PackageItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Available { get; set; }
    }

    public class InstallableApp
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string WingetId { get; set; }
        public string Icon { get; set; }
        public bool IsCheckedDefault { get; set; }
    }

    public class MainForm : Form
    {
        public const string APP_VERSION = "1.2.0";
        private const string UPDATE_METADATA_URL = "https://raw.githubusercontent.com/Lauju1909/windows-updater-senioren/main/version.json";

        // Standby-Verhinderung
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetThreadExecutionState(uint esFlags);
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;

        // UI-Komponenten
        private Panel headerPanel;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Label lblAppVersion;
        private Panel statusBanner;
        private Label lblStatusIcon;
        private Label lblStatusMsg;

        private Panel cardPanel;
        private Label lblStep1Title, lblStep1Desc, lblStep1Badge;
        private Label lblStep2Title, lblStep2Desc, lblStep2Badge;
        private Label lblStep3Title, lblStep3Desc, lblStep3Badge;

        private ProgressBar progressBar;
        private Label lblProgressInfo;

        private CheckBox chkSpeech;
        private CheckBox chkAutoRestart;
        private Button btnStart;
        private Button btnInstallApps;
        private Button btnDetails;
        private Button btnCheckAppUpdate;
        private TextBox txtLog;

        // Sprachausgabe
        private SpeechSynthesizer synth;
        private bool isUpdating = false;
        private bool isFinished = false;
        private bool rebootRequired = false;

        public MainForm()
        {
            InitializeComponent();
            InitSpeech();
        }

        private void InitializeComponent()
        {
            this.Text = "Einfacher Computer-Aktualisierer & App-Installer (Barrierefrei) v" + APP_VERSION;
            this.Size = new Size(880, 750);
            this.MinimumSize = new Size(840, 710);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            this.KeyPreview = true;
            this.AccessibleRole = AccessibleRole.Application;
            this.AccessibleName = "Einfacher Computer-Aktualisierer und App-Installer";
            this.AccessibleDescription = "Programm zur vollständigen Aktualisierung und Installation empfohlener Programme.";

            // 1. KOPFZEILE
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 94;
            headerPanel.BackColor = Color.FromArgb(30, 58, 138);
            headerPanel.Padding = new Padding(20, 14, 20, 14);

            lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "🛡️  Einfacher Computer-Aktualisierer";
            lblHeaderTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Location = new Point(20, 12);
            headerPanel.Controls.Add(lblHeaderTitle);

            lblAppVersion = new Label();
            lblAppVersion.Text = "Version " + APP_VERSION;
            lblAppVersion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblAppVersion.ForeColor = Color.FromArgb(147, 197, 253);
            lblAppVersion.Location = new Point(680, 18);
            lblAppVersion.Size = new Size(150, 24);
            lblAppVersion.TextAlign = ContentAlignment.TopRight;
            headerPanel.Controls.Add(lblAppVersion);

            lblHeaderSubtitle = new Label();
            lblHeaderSubtitle.Text = "Ihr PC wird vollautomatisch, sicher und barrierefrei auf den neuesten Stand gebracht.";
            lblHeaderSubtitle.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            lblHeaderSubtitle.ForeColor = Color.FromArgb(224, 231, 255);
            lblHeaderSubtitle.AutoSize = true;
            lblHeaderSubtitle.Location = new Point(24, 52);
            headerPanel.Controls.Add(lblHeaderSubtitle);

            // 2. STATUS-BANNER
            statusBanner = new Panel();
            statusBanner.Location = new Point(24, 110);
            statusBanner.Size = new Size(815, 60);
            statusBanner.BackColor = Color.FromArgb(239, 246, 255);
            statusBanner.BorderStyle = BorderStyle.FixedSingle;
            statusBanner.AccessibleRole = AccessibleRole.Alert;

            lblStatusIcon = new Label();
            lblStatusIcon.Text = "👉";
            lblStatusIcon.Font = new Font("Segoe UI", 20F);
            lblStatusIcon.Location = new Point(12, 10);
            lblStatusIcon.Size = new Size(45, 40);
            statusBanner.Controls.Add(lblStatusIcon);

            lblStatusMsg = new Label();
            lblStatusMsg.Text = "Drücken Sie die Eingabetaste (Enter) oder wählen Sie eine Aktion unten.";
            lblStatusMsg.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblStatusMsg.ForeColor = Color.FromArgb(30, 64, 175);
            lblStatusMsg.Location = new Point(58, 14);
            lblStatusMsg.Size = new Size(740, 35);
            lblStatusMsg.AccessibleName = "Status: Drücken Sie Enter oder wählen Sie eine Aktion unten.";
            statusBanner.Controls.Add(lblStatusMsg);

            // 3. HAUPT-BEREICH (SCHRITTE)
            cardPanel = new Panel();
            cardPanel.Location = new Point(24, 180);
            cardPanel.Size = new Size(815, 220);
            cardPanel.BackColor = Color.White;
            cardPanel.BorderStyle = BorderStyle.FixedSingle;

            // Schritt 1
            Panel pStep1 = CreateStepRow(
                "📦", "1. Alle Programme aktualisieren",
                "Webbrowser (Chrome, Firefox, Edge), Media-Player, Tools usw.",
                out lblStep1Title, out lblStep1Desc, out lblStep1Badge, 12
            );
            cardPanel.Controls.Add(pStep1);

            // Schritt 2
            Panel pStep2 = CreateStepRow(
                "🔒", "2. Windows-Sicherheitsupdates",
                "Wichtiger Schutz gegen Viren, Sicherheitslücken und Systemfehler.",
                out lblStep2Title, out lblStep2Desc, out lblStep2Badge, 78
            );
            cardPanel.Controls.Add(pStep2);

            // Schritt 3
            Panel pStep3 = CreateStepRow(
                "🏬", "3. Microsoft Store Apps",
                "Rechner, Fotos, Windows-Sicherheit und vorinstallierte Apps.",
                out lblStep3Title, out lblStep3Desc, out lblStep3Badge, 144
            );
            cardPanel.Controls.Add(pStep3);

            // 4. FORTSCHRITT
            progressBar = new ProgressBar();
            progressBar.Location = new Point(24, 412);
            progressBar.Size = new Size(815, 24);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;
            progressBar.AccessibleName = "Fortschrittsbalken";

            lblProgressInfo = new Label();
            lblProgressInfo.Text = "Tipp: Sie können den Computer währenddessen weiter nutzen. Taste F1 liest den Status vor.";
            lblProgressInfo.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            lblProgressInfo.ForeColor = Color.FromArgb(100, 116, 139);
            lblProgressInfo.Location = new Point(24, 440);
            lblProgressInfo.Size = new Size(815, 22);
            lblProgressInfo.TextAlign = ContentAlignment.MiddleCenter;

            // 5. UNTERE AKTIONEN & OPTIONEN
            chkSpeech = new CheckBox();
            chkSpeech.Text = "Sprachausgabe (Vorlesen) aktiviert (&S)";
            chkSpeech.Checked = true;
            chkSpeech.Location = new Point(24, 470);
            chkSpeech.Size = new Size(360, 28);
            chkSpeech.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            chkSpeech.AccessibleName = "Sprachausgabe aktivieren oder deaktivieren. Tastenkombination Alt + S.";
            chkSpeech.CheckedChanged += (s, e) => {
                if (chkSpeech.Checked) Speak("Sprachausgabe eingeschaltet.");
            };

            chkAutoRestart = new CheckBox();
            chkAutoRestart.Text = "Computer nach Abschluss automatisch neu starten (&N)";
            chkAutoRestart.Checked = false;
            chkAutoRestart.Location = new Point(24, 500);
            chkAutoRestart.Size = new Size(460, 28);
            chkAutoRestart.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            chkAutoRestart.AccessibleName = "Computer nach Abschluss automatisch neu starten. Tastenkombination Alt + N.";

            btnDetails = new Button();
            btnDetails.Text = "Details anzeigen (&D)";
            btnDetails.Location = new Point(24, 536);
            btnDetails.Size = new Size(150, 36);
            btnDetails.Font = new Font("Segoe UI", 10F);
            btnDetails.Click += BtnDetails_Click;
            btnDetails.AccessibleName = "Technische Details anzeigen oder ausblenden. Tastenkombination Alt + D.";

            btnCheckAppUpdate = new Button();
            btnCheckAppUpdate.Text = "🔄 Update prüfen (&U)";
            btnCheckAppUpdate.Location = new Point(184, 536);
            btnCheckAppUpdate.Size = new Size(160, 36);
            btnCheckAppUpdate.Font = new Font("Segoe UI", 10F);
            btnCheckAppUpdate.Click += (s, e) => { TriggerAppUpdateCheck(true); };
            btnCheckAppUpdate.AccessibleName = "Auf Updates für dieses Programm prüfen. Tastenkombination Alt + U.";

            // NEUER BUTTON: PROGRAMME INSTALLIEREN
            btnInstallApps = new Button();
            btnInstallApps.Text = "➕  PROGRAMME INSTALLIEREN (&I)";
            btnInstallApps.Location = new Point(354, 530);
            btnInstallApps.Size = new Size(230, 58);
            btnInstallApps.BackColor = Color.FromArgb(30, 58, 138); // Deep Navy
            btnInstallApps.ForeColor = Color.White;
            btnInstallApps.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnInstallApps.FlatStyle = FlatStyle.Flat;
            btnInstallApps.FlatAppearance.BorderSize = 0;
            btnInstallApps.Cursor = Cursors.Hand;
            btnInstallApps.AccessibleName = "Programme installieren. Tastenkombination Alt + I.";
            btnInstallApps.AccessibleDescription = "Öffnet ein Fenster zur einfachen Installation von NVDA, LibreOffice, Thunderbird, Chrome, Firefox und Antigravity.";
            btnInstallApps.Click += BtnInstallApps_Click;

            // HAUPT-BUTTON: AKTUALISIEREN
            btnStart = new Button();
            btnStart.Text = "▶  JETZT ALLES AKTUALISIEREN";
            btnStart.Location = new Point(594, 530);
            btnStart.Size = new Size(245, 58);
            btnStart.BackColor = Color.FromArgb(22, 163, 74); // Green
            btnStart.ForeColor = Color.White;
            btnStart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Cursor = Cursors.Hand;
            btnStart.AccessibleName = "Jetzt alles aktualisieren";
            btnStart.AccessibleDescription = "Startet die vollständige Aktualisierung von Programmen, Windows Updates und Store Apps.";
            btnStart.Click += BtnStart_Click;

            // 6. LOG-BOX
            txtLog = new TextBox();
            txtLog.Location = new Point(24, 595);
            txtLog.Size = new Size(815, 95);
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.FromArgb(15, 23, 42);
            txtLog.ForeColor = Color.FromArgb(56, 189, 248);
            txtLog.Font = new Font("Consolas", 10F);
            txtLog.Visible = false;
            txtLog.AccessibleName = "Technisches Protokoll";

            this.Controls.Add(txtLog);
            this.Controls.Add(btnStart);
            this.Controls.Add(btnInstallApps);
            this.Controls.Add(btnCheckAppUpdate);
            this.Controls.Add(btnDetails);
            this.Controls.Add(chkAutoRestart);
            this.Controls.Add(chkSpeech);
            this.Controls.Add(lblProgressInfo);
            this.Controls.Add(progressBar);
            this.Controls.Add(cardPanel);
            this.Controls.Add(statusBanner);
            this.Controls.Add(headerPanel);

            this.KeyDown += MainForm_KeyDown;
        }

        private Panel CreateStepRow(string icon, string title, string desc, out Label outTitle, out Label outDesc, out Label outBadge, int top)
        {
            Panel row = new Panel();
            row.Location = new Point(14, top);
            row.Size = new Size(785, 56);
            row.BackColor = Color.FromArgb(248, 250, 252);
            row.BorderStyle = BorderStyle.FixedSingle;

            Label lblIcon = new Label();
            lblIcon.Text = icon;
            lblIcon.Font = new Font("Segoe UI", 18F);
            lblIcon.Location = new Point(8, 8);
            lblIcon.Size = new Size(40, 38);
            lblIcon.TextAlign = ContentAlignment.MiddleCenter;
            row.Controls.Add(lblIcon);

            outTitle = new Label();
            outTitle.Text = title;
            outTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            outTitle.ForeColor = Color.FromArgb(30, 41, 59);
            outTitle.Location = new Point(54, 6);
            outTitle.Size = new Size(560, 24);
            row.Controls.Add(outTitle);

            outDesc = new Label();
            outDesc.Text = desc;
            outDesc.Font = new Font("Segoe UI", 10F);
            outDesc.ForeColor = Color.FromArgb(100, 116, 139);
            outDesc.Location = new Point(54, 28);
            outDesc.Size = new Size(560, 22);
            row.Controls.Add(outDesc);

            outBadge = new Label();
            outBadge.Text = "Bereit";
            outBadge.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            outBadge.ForeColor = Color.FromArgb(100, 116, 139);
            outBadge.Location = new Point(620, 14);
            outBadge.Size = new Size(150, 26);
            outBadge.TextAlign = ContentAlignment.MiddleRight;
            row.Controls.Add(outBadge);

            row.AccessibleRole = AccessibleRole.ListItem;
            row.AccessibleName = title + ". Status: Bereit.";

            return row;
        }

        private void InitSpeech()
        {
            try
            {
                synth = new SpeechSynthesizer();
                foreach (InstalledVoice v in synth.GetInstalledVoices())
                {
                    if (v.Enabled && v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase))
                    {
                        synth.SelectVoice(v.VoiceInfo.Name);
                        break;
                    }
                }
                synth.Rate = 0;
            }
            catch {}
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            btnStart.Focus();

            Speak(
                "Willkommen beim einfachen Computer-Aktualisierer. " +
                "Drücken Sie die Eingabetaste, um alle Updates zu starten, oder Alt plus I, um empfohlene Programme wie NVDA oder LibreOffice zu installieren."
            );

            TriggerAppUpdateCheck(false);
        }

        public void Speak(string text)
        {
            if (chkSpeech != null && !chkSpeech.Checked) return;

            try
            {
                if (synth != null)
                {
                    synth.SpeakAsyncCancelAll();
                    synth.SpeakAsync(text);
                }
            }
            catch {}
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                string statusText = lblStatusMsg.Text;
                string stepInfo = string.Format(
                    "Aktueller Zustand: {0}. Schritt 1, Programme: {1}. Schritt 2, Windows Updates: {2}. Schritt 3, Store Apps: {3}.",
                    statusText, lblStep1Badge.Text, lblStep2Badge.Text, lblStep3Badge.Text
                );
                Speak(stepInfo);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                BtnDetails_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F3)
            {
                BtnInstallApps_Click(this, EventArgs.Empty);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                TriggerAppUpdateCheck(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (isFinished || !isUpdating)
                {
                    this.Close();
                }
            }
        }

        private void BtnDetails_Click(object sender, EventArgs e)
        {
            if (txtLog.Visible)
            {
                txtLog.Visible = false;
                this.Height = 750;
                btnDetails.Text = "Details anzeigen (&D)";
                Speak("Technische Details ausgeblendet.");
            }
            else
            {
                txtLog.Visible = true;
                this.Height = 850;
                btnDetails.Text = "Details verbergen (&D)";
                txtLog.Focus();
                Speak("Technische Details eingeblendet.");
            }
        }

        #region App-Installer Dialog

        private void BtnInstallApps_Click(object sender, EventArgs e)
        {
            using (AppInstallerDialog dlg = new AppInstallerDialog(this))
            {
                dlg.ShowDialog(this);
            }
        }

        #endregion

        #region Selbst-Aktualisierung (Auto-Updater)

        private void TriggerAppUpdateCheck(bool isManual)
        {
            Thread t = new Thread(() => CheckForAppUpdateWorker(isManual));
            t.IsBackground = true;
            t.Start();
        }

        private void CheckForAppUpdateWorker(bool isManual)
        {
            Log(string.Format("Prüfe auf App-Updates (aktuelle Version: {0})...", APP_VERSION));
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Tls12;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "ComputerAktualisierer/" + APP_VERSION);
                    wc.Encoding = Encoding.UTF8;
                    string json = wc.DownloadString(UPDATE_METADATA_URL);

                    Match mVer = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                    Match mUrl = Regex.Match(json, "\"download_url\"\\s*:\\s*\"([^\"]+)\"");

                    if (mVer.Success && mUrl.Success)
                    {
                        string remoteVerStr = mVer.Groups[1].Value.Trim();
                        string downloadUrl = mUrl.Groups[1].Value.Trim();

                        Version currentVer = new Version(APP_VERSION);
                        Version remoteVer = new Version(remoteVerStr);

                        if (remoteVer > currentVer)
                        {
                            Log(string.Format("Neue App-Version gefunden: {0}! Starte automatische Aktualisierung...", remoteVerStr));
                            this.BeginInvoke(new Action(() => {
                                lblStatusIcon.Text = "🚀";
                                lblStatusMsg.Text = string.Format("Neue App-Version {0} gefunden! Wird automatisch aktualisiert...", remoteVerStr);
                                statusBanner.BackColor = Color.FromArgb(238, 242, 255);
                                lblStatusMsg.ForeColor = Color.FromArgb(67, 56, 202);
                            }));

                            Speak(string.Format("Eine neue Version {0} des Computer-Aktualisierers wurde gefunden und wird jetzt automatisch geladen.", remoteVerStr));

                            string appPath = Application.ExecutablePath;
                            string appDir = Path.GetDirectoryName(appPath);
                            string tempNewExe = Path.Combine(appDir, "Computer-Aktualisierer.new.exe");

                            wc.DownloadFile(downloadUrl, tempNewExe);
                            Log("Download der neuen Version erfolgreich abgeschlossen.");

                            string updaterBat = Path.Combine(appDir, "self_updater.bat");
                            string batScript = string.Format(
                                "@echo off\r\n" +
                                "timeout /t 1 /nobreak >nul\r\n" +
                                ":waitloop\r\n" +
                                "copy /y \"{0}\" \"{1}\" >nul\r\n" +
                                "if errorlevel 1 (\r\n" +
                                "  timeout /t 1 /nobreak >nul\r\n" +
                                "  goto waitloop\r\n" +
                                ")\r\n" +
                                "del /f /q \"{0}\" >nul\r\n" +
                                "start \"\" \"{1}\"\r\n" +
                                "(goto) 2>nul & del /f /q \"%~f0\"\r\n",
                                tempNewExe, appPath
                            );

                            File.WriteAllText(updaterBat, batScript, Encoding.Default);

                            ProcessStartInfo psi = new ProcessStartInfo(updaterBat);
                            psi.CreateNoWindow = true;
                            psi.UseShellExecute = false;
                            Process.Start(psi);

                            this.BeginInvoke(new Action(() => {
                                Application.Exit();
                            }));
                            return;
                        }
                        else
                        {
                            Log(string.Format("App ist auf dem neuesten Stand (v{0}).", APP_VERSION));
                            if (isManual)
                            {
                                Speak("Die App ist bereits auf dem neuesten Stand.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Hinweis beim App-Update-Check: " + ex.Message);
            }
        }

        #endregion

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (isFinished)
            {
                this.Close();
                return;
            }

            if (isUpdating) return;

            isUpdating = true;
            btnStart.Enabled = false;
            btnStart.Text = "⏳  WIRD AKTUALISIERT...";
            btnStart.BackColor = Color.FromArgb(148, 163, 184);
            btnInstallApps.Enabled = false;
            btnCheckAppUpdate.Enabled = false;
            chkAutoRestart.Enabled = false;

            lblStatusIcon.Text = "⏳";
            lblStatusMsg.Text = "Aktualisierung läuft... Bitte lassen Sie den PC eingeschaltet.";
            statusBanner.BackColor = Color.FromArgb(254, 243, 199);
            lblStatusMsg.ForeColor = Color.FromArgb(146, 64, 14);

            try { SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED); } catch {}

            Speak(
                "Die Aktualisierung wurde gestartet. " +
                "Bitte schalten Sie den Computer nicht aus. " +
                "Sie können den Computer normal weiter nutzen oder sich eine Tasse Kaffee holen."
            );

            Thread workerThread = new Thread(RunUpdateProcess);
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        public void Log(string text)
        {
            string line = string.Format("[{0}] {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), text);
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => {
                    txtLog.AppendText(line);
                }));
            }
            else
            {
                txtLog.AppendText(line);
            }
        }

        private void UpdateStepUI(int step, string badgeText, Color badgeColor, int progress, string detailText)
        {
            this.BeginInvoke(new Action(() => {
                if (step == 1) { lblStep1Badge.Text = badgeText; lblStep1Badge.ForeColor = badgeColor; }
                else if (step == 2) { lblStep2Badge.Text = badgeText; lblStep2Badge.ForeColor = badgeColor; }
                else if (step == 3) { lblStep3Badge.Text = badgeText; lblStep3Badge.ForeColor = badgeColor; }

                progressBar.Value = progress;
                lblProgressInfo.Text = detailText;
            }));
        }

        private void RunUpdateProcess()
        {
            Log("Starte gesamten Aktualisierungsprozess...");

            // ==============================================================================
            // SCHRITT 1: WINGET (PROGRAMME)
            // ==============================================================================
            UpdateStepUI(1, "Wird geprüft...", Color.FromArgb(37, 99, 235), 10, "Schritt 1 von 3: Programme werden auf Aktualisierungen überprüft...");
            Speak("Schritt 1 von 3: Installierte Programme wie Webbrowser und Tools werden überprüft.");
            Log("Schritt 1: Prüfe Programme über winget...");

            string wingetPath = FindWinget();
            if (!string.IsNullOrEmpty(wingetPath))
            {
                try
                {
                    Log("Aktualisiere winget Katalogquellen...");
                    RunProcessAndLog(wingetPath, "source update");

                    List<PackageItem> packages = GetUpgradablePackages(wingetPath);
                    Log(string.Format("Gefundene Programmaktualisierungen: {0}", packages.Count));

                    if (packages.Count > 0)
                    {
                        Speak(string.Format("{0} Programmaktualisierungen gefunden. Aktualisierungen werden gestartet.", packages.Count));

                        int currentIdx = 0;
                        int successCount = 0;

                        foreach (PackageItem pkg in packages)
                        {
                            currentIdx++;
                            int progressPct = 10 + (int)((float)currentIdx / packages.Count * 25);
                            string stepMsg = string.Format("Schritt 1: Aktualisiere ({0}/{1}): {2}...", currentIdx, packages.Count, pkg.Name);
                            UpdateStepUI(1, string.Format("Lade ({0}/{1})...", currentIdx, packages.Count), Color.FromArgb(37, 99, 235), progressPct, stepMsg);
                            
                            Log(string.Format("--> Aktualisiere [{0}/{1}]: {2} (ID: {3})...", currentIdx, packages.Count, pkg.Name, pkg.Id));
                            Speak(string.Format("Aktualisiere {0}...", pkg.Name));

                            string upgradeArgs = string.Format(
                                "upgrade --id \"{0}\" --include-unknown --accept-package-agreements --accept-source-agreements --disable-interactivity",
                                pkg.Id
                            );

                            int exitCode = RunProcessAndLog(wingetPath, upgradeArgs);
                            if (exitCode == 0)
                            {
                                successCount++;
                                Log(string.Format("[Erfolg] {0} erfolgreich aktualisiert.", pkg.Name));
                            }
                            else
                            {
                                Log(string.Format("[Hinweis] {0} konnte nicht automatisch aktualisiert werden (Exit-Code: {1}). Wird übersprungen.", pkg.Name, exitCode));
                            }
                        }

                        Log(string.Format("Schritt 1 beendet: {0} von {1} Programmen aktualisiert.", successCount, packages.Count));
                        UpdateStepUI(1, "Aktuell ✓", Color.FromArgb(22, 163, 74), 35, string.Format("Schritt 1 abgeschlossen: {0} Programme aktualisiert!", successCount));
                    }
                    else
                    {
                        Log("Alle Programme sind bereits auf dem neuesten Stand.");
                        UpdateStepUI(1, "Bereits aktuell ✓", Color.FromArgb(22, 163, 74), 35, "Programme sind bereits aktuell!");
                    }
                }
                catch (Exception ex)
                {
                    Log("Hinweis bei Schritt 1: " + ex.Message);
                    UpdateStepUI(1, "Fertig ✓", Color.FromArgb(22, 163, 74), 35, "Schritt 1 beendet.");
                }
            }
            else
            {
                Log("winget wurde nicht gefunden. Schritt 1 übersprungen.");
                UpdateStepUI(1, "Übersprungen", Color.Gray, 35, "winget nicht gefunden.");
            }

            Thread.Sleep(1000);

            // ==============================================================================
            // SCHRITT 2: WINDOWS UPDATES
            // ==============================================================================
            UpdateStepUI(2, "Suche Updates...", Color.FromArgb(37, 99, 235), 45, "Schritt 2 von 3: Suche nach Windows-Sicherheitsupdates...");
            Speak("Schritt 2 von 3: Suche nach Windows-Sicherheitsupdates läuft.");
            Log("Schritt 2: Suche nach Windows Updates...");

            try
            {
                Type uSessionType = Type.GetTypeFromProgID("Microsoft.Update.Session");
                if (uSessionType != null)
                {
                    dynamic uSession = Activator.CreateInstance(uSessionType);
                    dynamic searcher = uSession.CreateUpdateSearcher();
                    dynamic searchResult = searcher.Search("IsInstalled=0 and Type='Software' and IsHidden=0");

                    int count = searchResult.Updates.Count;
                    Log("Gefundene Windows Updates: " + count);

                    if (count > 0)
                    {
                        Speak(string.Format("Es wurden {0} Windows-Updates gefunden. Downloads werden gestartet.", count));
                        UpdateStepUI(2, string.Format("Lade ({0})...", count), Color.FromArgb(37, 99, 235), 60, string.Format("Schritt 2 von 3: {0} Windows-Updates werden heruntergeladen...", count));

                        Type colType = Type.GetTypeFromProgID("Microsoft.Update.UpdateColl") 
                                    ?? Type.GetTypeFromProgID("Microsoft.Update.UpdateCollection");

                        if (colType != null)
                        {
                            dynamic toDownload = Activator.CreateInstance(colType);

                            for (int i = 0; i < count; i++)
                            {
                                dynamic up = searchResult.Updates[i];
                                Log("Windows Update: " + up.Title);
                                toDownload.Add(up);
                            }

                            dynamic downloader = uSession.CreateUpdateDownloader();
                            downloader.Updates = toDownload;
                            Log("Lade Windows-Updates herunter...");
                            downloader.Download();

                            UpdateStepUI(2, "Installiere...", Color.FromArgb(37, 99, 235), 75, "Schritt 2 von 3: Windows-Updates werden installiert...");
                            Speak("Windows-Updates werden jetzt installiert.");

                            dynamic toInstall = Activator.CreateInstance(colType);
                            for (int i = 0; i < count; i++)
                            {
                                dynamic up = searchResult.Updates[i];
                                if (up.IsDownloaded) toInstall.Add(up);
                            }

                            if (toInstall.Count > 0)
                            {
                                dynamic installer = uSession.CreateUpdateInstaller();
                                installer.Updates = toInstall;
                                dynamic instResult = installer.Install();

                                Log("Windows Update Ergebnis: " + instResult.ResultCode);
                                if (instResult.RebootRequired)
                                {
                                    rebootRequired = true;
                                    Log("Ein Neustart von Windows ist nach Abschluss erforderlich.");
                                }
                            }

                            UpdateStepUI(2, "Aktuell ✓", Color.FromArgb(22, 163, 74), 80, "Schritt 2 abgeschlossen: Windows Updates installiert!");
                        }
                        else
                        {
                            Log("UpdateCollection ProgID nicht gefunden. Verwende System-Fallback.");
                            TriggerWindowsUpdateFallback();
                            UpdateStepUI(2, "Angestoßen ✓", Color.FromArgb(22, 163, 74), 80, "Windows Update wurde angestoßen.");
                        }
                    }
                    else
                    {
                        Log("Windows ist bereits auf dem neuesten Stand.");
                        UpdateStepUI(2, "Bereits aktuell ✓", Color.FromArgb(22, 163, 74), 80, "Windows ist bereits auf dem neuesten Stand!");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Hinweis bei Windows Updates: " + ex.Message);
                TriggerWindowsUpdateFallback();
                UpdateStepUI(2, "Beendet ✓", Color.FromArgb(22, 163, 74), 80, "Windows-Updates Prüfung beendet.");
            }

            Thread.Sleep(1000);

            // ==============================================================================
            // SCHRITT 3: MICROSOFT STORE APPS
            // ==============================================================================
            UpdateStepUI(3, "Wird geprüft...", Color.FromArgb(37, 99, 235), 85, "Schritt 3 von 3: Microsoft Store Apps werden überprüft...");
            Speak("Schritt 3 von 3: Windows Store Apps werden auf Aktualisierungen geprüft.");
            Log("Schritt 3: Microsoft Store Apps aktualisieren...");

            try
            {
                if (!string.IsNullOrEmpty(wingetPath))
                {
                    RunProcessAndLog(wingetPath, "upgrade --all --source msstore --accept-package-agreements --accept-source-agreements --disable-interactivity");
                }
                UpdateStepUI(3, "Aktuell ✓", Color.FromArgb(22, 163, 74), 100, "Alle Aktualisierungen erfolgreich beendet!");
            }
            catch (Exception ex)
            {
                Log("Hinweis bei Store Apps: " + ex.Message);
                UpdateStepUI(3, "Fertig ✓", Color.FromArgb(22, 163, 74), 100, "Store-Apps Prüfung beendet.");
            }

            Log("Aktualisierungsvorgang komplett abgeschlossen.");

            try { SetThreadExecutionState(ES_CONTINUOUS); } catch {}
            try { System.Media.SystemSounds.Asterisk.Play(); } catch {}

            // Abschluss-Meldung
            this.BeginInvoke(new Action(() => {
                isFinished = true;
                isUpdating = false;

                lblStatusIcon.Text = "✅";
                statusBanner.BackColor = Color.FromArgb(220, 252, 231);
                lblStatusMsg.ForeColor = Color.FromArgb(21, 128, 61);

                if (rebootRequired)
                {
                    lblStatusMsg.Text = "Geschafft! Alles ist aktuell. Bitte starten Sie Ihren Computer demnächst neu.";
                }
                else
                {
                    lblStatusMsg.Text = "Geschafft! Ihr Computer ist jetzt vollständig sicher und auf dem neuesten Stand.";
                }

                btnStart.Text = "✓  FERTIG (FENSTER SCHLIESSEN)";
                btnStart.BackColor = Color.FromArgb(21, 128, 61);
                btnStart.Enabled = true;
                btnInstallApps.Enabled = true;
                btnCheckAppUpdate.Enabled = true;
                btnStart.Focus();

                string finishSpeech = rebootRequired
                    ? "Geschafft! Alle Programme und Windows sind jetzt aktualisiert. Bitte starten Sie Ihren Computer bei Gelegenheit neu. Drücken Sie die Eingabetaste, um das Fenster zu schließen."
                    : "Geschafft! Alle Programme und Windows sind jetzt auf dem neuesten Stand. Sie können das Fenster mit der Eingabetaste schließen.";

                Speak(finishSpeech);

                if (chkAutoRestart.Checked && rebootRequired)
                {
                    lblProgressInfo.Text = "Der Computer wird in 30 Sekunden automatisch neu gestartet...";
                    Thread.Sleep(3000);
                    Process.Start("shutdown.exe", "/r /t 30 /c \"Automatischer Neustart nach Updates\"");
                }
            }));
        }

        private List<PackageItem> GetUpgradablePackages(string wingetPath)
        {
            List<PackageItem> result = new List<PackageItem>();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(wingetPath, "upgrade --include-unknown");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.StandardOutputEncoding = Encoding.UTF8;

                List<string> lines = new List<string>();
                using (Process p = Process.Start(psi))
                {
                    while (!p.StandardOutput.EndOfStream)
                    {
                        lines.Add(p.StandardOutput.ReadLine());
                    }
                    p.WaitForExit(60000);
                }

                int idColStart = -1;
                int verColStart = -1;

                for (int i = 0; i < lines.Count; i++)
                {
                    string l = lines[i];
                    if (l.Contains("ID") && l.Contains("Version"))
                    {
                        idColStart = l.IndexOf("ID");
                        verColStart = l.IndexOf("Version");
                        continue;
                    }

                    if (idColStart > 0 && verColStart > idColStart)
                    {
                        if (l.StartsWith("---") || l.StartsWith("===")) continue;
                        if (l.Contains("Aktualisierungen verf") || l.Contains("upgrades available")) break;

                        if (l.Length > verColStart)
                        {
                            string name = l.Substring(0, idColStart).Trim();
                            string id = l.Substring(idColStart, verColStart - idColStart).Trim();
                            string rest = l.Substring(verColStart).Trim();

                            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name))
                            {
                                result.Add(new PackageItem { Name = name, Id = id, Version = rest });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Fehler beim Ermitteln der Update-Liste: " + ex.Message);
            }
            return result;
        }

        public int RunProcessAndLog(string fileName, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.StandardErrorEncoding = Encoding.UTF8;

                using (Process p = Process.Start(psi))
                {
                    while (!p.StandardOutput.EndOfStream)
                    {
                        string line = p.StandardOutput.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Log(line);
                        }
                    }
                    string err = p.StandardError.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        Log(err);
                    }
                    p.WaitForExit(300000);
                    return p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Log("Prozessfehler: " + ex.Message);
                return -1;
            }
        }

        private void TriggerWindowsUpdateFallback()
        {
            try
            {
                Log("Starte Windows Update Hintergrunddienst (USOClient)...");
                Process.Start(new ProcessStartInfo("usoclient.exe", "StartInteractiveScan") { CreateNoWindow = true, UseShellExecute = false });
            }
            catch {}
        }

        public string FindWinget()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string standardPath = Path.Combine(localAppData, @"Microsoft\WindowsApps\winget.exe");
            if (File.Exists(standardPath)) return standardPath;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("where.exe", "winget.exe");
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using (Process p = Process.Start(psi))
                {
                    string res = p.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(res) && File.Exists(res.Trim())) return res.Trim();
                }
            }
            catch {}

            return "winget.exe";
        }
    }

    // ==============================================================================
    // DIALOG: PROGRAMME INSTALLIEREN (Barrierefrei)
    // ==============================================================================
    public class AppInstallerDialog : Form
    {
        private MainForm mainForm;
        private List<CheckBox> checkBoxes = new List<CheckBox>();
        private List<InstallableApp> availableApps = new List<InstallableApp>();
        private Button btnInstallSelected;
        private Button btnSelectAll;
        private Button btnClose;
        private ProgressBar dlgProgress;
        private Label lblDlgStatus;
        private bool isInstalling = false;

        public AppInstallerDialog(MainForm parent)
        {
            this.mainForm = parent;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Wichtige Programme installieren (Barrierefrei)";
            this.Size = new Size(720, 640);
            this.MinimumSize = new Size(680, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 11F);
            this.KeyPreview = true;
            this.AccessibleRole = AccessibleRole.Dialog;
            this.AccessibleName = "Fenster zum Installieren empfohlener Programme";

            // App-Liste initialisieren
            availableApps.Add(new InstallableApp {
                Icon = "👁️",
                DisplayName = "NVDA (Screenreader / Vorleser)",
                Description = "Kostenloser Bildschirm-Vorleser für Blinde und Sehbehinderte.",
                WingetId = "NVAccess.NVDA",
                IsCheckedDefault = true
            });
            availableApps.Add(new InstallableApp {
                Icon = "📝",
                DisplayName = "LibreOffice",
                Description = "Kostenloses Schreibprogramm und Tabellen (Word & Excel Alternative).",
                WingetId = "TheDocumentFoundation.LibreOffice",
                IsCheckedDefault = false
            });
            availableApps.Add(new InstallableApp {
                Icon = "✉️",
                DisplayName = "Mozilla Thunderbird",
                Description = "Einfaches, sicheres E-Mail-Programm für den PC.",
                WingetId = "Mozilla.Thunderbird",
                IsCheckedDefault = false
            });
            availableApps.Add(new InstallableApp {
                Icon = "🌐",
                DisplayName = "Google Chrome",
                Description = "Beliebter, schneller und sicherer Webbrowser.",
                WingetId = "Google.Chrome",
                IsCheckedDefault = false
            });
            availableApps.Add(new InstallableApp {
                Icon = "🦊",
                DisplayName = "Mozilla Firefox",
                Description = "Freier, datenschutzfreundlicher Webbrowser.",
                WingetId = "Mozilla.Firefox",
                IsCheckedDefault = false
            });
            availableApps.Add(new InstallableApp {
                Icon = "🚀",
                DisplayName = "Google Antigravity",
                Description = "Offizielle Google Antigravity Entwicklungsumgebung.",
                WingetId = "Google.Antigravity",
                IsCheckedDefault = false
            });

            // Header
            Panel head = new Panel();
            head.Dock = DockStyle.Top;
            head.Height = 70;
            head.BackColor = Color.FromArgb(30, 58, 138);
            head.Padding = new Padding(16, 12, 16, 12);

            Label lblTitle = new Label();
            lblTitle.Text = "➕  Wichtige Programme installieren";
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(16, 10);
            head.Controls.Add(lblTitle);

            Label lblSub = new Label();
            lblSub.Text = "Wählen Sie die Programme aus, die Sie auf Ihrem Computer installieren möchten.";
            lblSub.Font = new Font("Segoe UI", 10.5F);
            lblSub.ForeColor = Color.FromArgb(224, 231, 255);
            lblSub.AutoSize = true;
            lblSub.Location = new Point(18, 40);
            head.Controls.Add(lblSub);

            this.Controls.Add(head);

            // Container für Checkboxen
            Panel listPanel = new Panel();
            listPanel.Location = new Point(20, 85);
            listPanel.Size = new Size(665, 360);
            listPanel.AutoScroll = true;
            listPanel.BackColor = Color.White;
            listPanel.BorderStyle = BorderStyle.FixedSingle;

            int topPos = 12;
            for (int i = 0; i < availableApps.Count; i++)
            {
                InstallableApp app = availableApps[i];

                Panel itemPanel = new Panel();
                itemPanel.Location = new Point(12, topPos);
                itemPanel.Size = new Size(625, 52);
                itemPanel.BackColor = Color.FromArgb(248, 250, 252);
                itemPanel.BorderStyle = BorderStyle.FixedSingle;

                CheckBox cb = new CheckBox();
                cb.Text = string.Format("{0}  {1}", app.Icon, app.DisplayName);
                cb.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                cb.Checked = app.IsCheckedDefault;
                cb.Location = new Point(12, 6);
                cb.Size = new Size(600, 24);
                cb.Tag = app;
                cb.AccessibleName = app.DisplayName + ". " + app.Description;
                checkBoxes.Add(cb);
                itemPanel.Controls.Add(cb);

                Label lblDesc = new Label();
                lblDesc.Text = app.Description;
                lblDesc.Font = new Font("Segoe UI", 9.5F);
                lblDesc.ForeColor = Color.FromArgb(100, 116, 139);
                lblDesc.Location = new Point(38, 30);
                lblDesc.Size = new Size(570, 18);
                itemPanel.Controls.Add(lblDesc);

                listPanel.Controls.Add(itemPanel);
                topPos += 58;
            }

            this.Controls.Add(listPanel);

            // Fortschritt & Status
            dlgProgress = new ProgressBar();
            dlgProgress.Location = new Point(20, 455);
            dlgProgress.Size = new Size(665, 20);
            dlgProgress.Minimum = 0;
            dlgProgress.Maximum = 100;
            dlgProgress.Value = 0;
            this.Controls.Add(dlgProgress);

            lblDlgStatus = new Label();
            lblDlgStatus.Text = "Wählen Sie Programme aus und drücken Sie auf 'Ausgewählte Programme installieren'.";
            lblDlgStatus.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            lblDlgStatus.ForeColor = Color.FromArgb(71, 85, 105);
            lblDlgStatus.Location = new Point(20, 480);
            lblDlgStatus.Size = new Size(665, 24);
            lblDlgStatus.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblDlgStatus);

            // Knöpfe unten
            btnSelectAll = new Button();
            btnSelectAll.Text = "Alle auswählen";
            btnSelectAll.Location = new Point(20, 515);
            btnSelectAll.Size = new Size(140, 48);
            btnSelectAll.Font = new Font("Segoe UI", 10.5F);
            btnSelectAll.Click += (s, e) => {
                bool newState = !checkBoxes[0].Checked;
                foreach (CheckBox cb in checkBoxes) cb.Checked = newState;
                btnSelectAll.Text = newState ? "Keine auswählen" : "Alle auswählen";
                mainForm.Speak(newState ? "Alle Programme ausgewählt." : "Auswahl aufgehoben.");
            };
            this.Controls.Add(btnSelectAll);

            btnClose = new Button();
            btnClose.Text = "Schließen (Esc)";
            btnClose.Location = new Point(170, 515);
            btnClose.Size = new Size(130, 48);
            btnClose.Font = new Font("Segoe UI", 10.5F);
            btnClose.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnClose);

            btnInstallSelected = new Button();
            btnInstallSelected.Text = "▶  AUSGEWÄHLTE JETZT INSTALLIEREN";
            btnInstallSelected.Location = new Point(310, 512);
            btnInstallSelected.Size = new Size(375, 54);
            btnInstallSelected.BackColor = Color.FromArgb(22, 163, 74); // Green
            btnInstallSelected.ForeColor = Color.White;
            btnInstallSelected.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnInstallSelected.FlatStyle = FlatStyle.Flat;
            btnInstallSelected.FlatAppearance.BorderSize = 0;
            btnInstallSelected.Cursor = Cursors.Hand;
            btnInstallSelected.AccessibleName = "Ausgewählte Programme jetzt installieren";
            btnInstallSelected.Click += BtnInstallSelected_Click;
            this.Controls.Add(btnInstallSelected);

            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Escape && !isInstalling) {
                    this.Close();
                }
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            btnInstallSelected.Focus();
            mainForm.Speak("Wichtige Programme installieren geöffnet. Drücken Sie Enter, um die ausgewählten Programme zu installieren.");
        }

        private void BtnInstallSelected_Click(object sender, EventArgs e)
        {
            if (isInstalling) return;

            List<InstallableApp> toInstall = new List<InstallableApp>();
            foreach (CheckBox cb in checkBoxes)
            {
                if (cb.Checked && cb.Tag is InstallableApp)
                {
                    toInstall.Add((InstallableApp)cb.Tag);
                }
            }

            if (toInstall.Count == 0)
            {
                MessageBox.Show("Bitte wählen Sie mindestens ein Programm zum Installieren aus.", "Kein Programm gewählt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                mainForm.Speak("Bitte wählen Sie mindestens ein Programm aus.");
                return;
            }

            isInstalling = true;
            btnInstallSelected.Enabled = false;
            btnSelectAll.Enabled = false;
            btnClose.Enabled = false;
            btnInstallSelected.Text = "⏳  WIRD INSTALLIERT...";
            btnInstallSelected.BackColor = Color.FromArgb(148, 163, 184);

            mainForm.Speak(string.Format("Starte Installation von {0} Programmen. Bitte warten.", toInstall.Count));

            Thread t = new Thread(() => RunInstallationWorker(toInstall));
            t.IsBackground = true;
            t.Start();
        }

        private void RunInstallationWorker(List<InstallableApp> apps)
        {
            string wingetPath = mainForm.FindWinget();
            int current = 0;
            int success = 0;

            foreach (InstallableApp app in apps)
            {
                current++;
                int pct = (int)((float)current / apps.Count * 100);

                this.BeginInvoke(new Action(() => {
                    lblDlgStatus.Text = string.Format("Installiere [{0}/{1}]: {2}...", current, apps.Count, app.DisplayName);
                    dlgProgress.Value = pct;
                }));

                mainForm.Log(string.Format("Starte Installation von {0} (ID: {1})...", app.DisplayName, app.WingetId));
                mainForm.Speak(string.Format("Installiere {0}...", app.DisplayName));

                string installArgs = string.Format(
                    "install --id \"{0}\" --accept-package-agreements --accept-source-agreements --disable-interactivity",
                    app.WingetId
                );

                int exitCode = mainForm.RunProcessAndLog(wingetPath, installArgs);
                if (exitCode == 0)
                {
                    success++;
                    mainForm.Log(string.Format("[Erfolg] {0} wurde erfolgreich installiert.", app.DisplayName));
                }
                else
                {
                    mainForm.Log(string.Format("[Hinweis] Installation von {0} beendet mit Exit-Code: {1}.", app.DisplayName, exitCode));
                }
            }

            try { System.Media.SystemSounds.Asterisk.Play(); } catch {}

            this.BeginInvoke(new Action(() => {
                isInstalling = false;
                dlgProgress.Value = 100;
                lblDlgStatus.Text = string.Format("Fertig! {0} von {1} Programmen wurden erfolgreich installiert.", success, apps.Count);

                btnInstallSelected.Text = "✓  FERTIG";
                btnInstallSelected.BackColor = Color.FromArgb(22, 163, 74);
                btnInstallSelected.Enabled = true;
                btnSelectAll.Enabled = true;
                btnClose.Enabled = true;
                btnClose.Focus();

                string finishMsg = string.Format("Fertig! {0} von {1} Programmen wurden installiert.", success, apps.Count);
                mainForm.Speak(finishMsg);
            }));
        }
    }
}
