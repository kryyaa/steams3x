using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.IO.Compression;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Http;

namespace steamsex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string ExtractGameId(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return null;

            if (Regex.IsMatch(userInput, @"^\d+$"))
                return userInput;

            Regex[] patterns = new Regex[]
            {
                new Regex(@"store\.steampowered\.com/app/(\d+)"),
                new Regex(@"steamcommunity\.com/app/(\d+)"),
                new Regex(@"app/(\d+)")
            };

            foreach (var pattern in patterns)
            {
                Match match = pattern.Match(userInput);
                if (match.Success)
                    return match.Groups[1].Value;
            }

            return null;
        }

        private string FindSteamPath()
        {
            List<string> possiblePaths = new List<string>
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Steam")
            };

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                {
                    if (key != null)
                    {
                        string installPath = key.GetValue("InstallPath") as string;
                        if (!string.IsNullOrEmpty(installPath))
                            possiblePaths.Insert(0, installPath);
                    }
                }
            }
            catch { }

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    if (key != null)
                    {
                        string installPath = key.GetValue("InstallPath") as string;
                        if (!string.IsNullOrEmpty(installPath))
                            possiblePaths.Insert(0, installPath);
                    }
                }
            }
            catch { }

            foreach (string path in possiblePaths)
            {
                string steamExe = Path.Combine(path, "steam.exe");
                if (File.Exists(steamExe))
                    return path;
            }

            return null;
        }

        private string GetStpluginPath(string steamPath)
        {
            if (string.IsNullOrEmpty(steamPath))
                return null;

            string configPath = Path.Combine(steamPath, "config");
            string stpluginPath = Path.Combine(configPath, "stplug-in");

            return stpluginPath;
        }

        private string GetDepotCachePath(string steamPath)
        {
            if (string.IsNullOrEmpty(steamPath))
                return null;

            return Path.Combine(steamPath, "depotcache");
        }

        private bool IsSteamRunning()
        {
            Process[] processes = Process.GetProcessesByName("steam");
            return processes.Length > 0;
        }

        private void KillSteam()
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName("steam"))
                    process.Kill();

                foreach (Process process in Process.GetProcessesByName("steamwebhelper"))
                    process.Kill();

                foreach (Process process in Process.GetProcessesByName("steamservice"))
                    process.Kill();

                Thread.Sleep(3000);
            }
            catch { }
        }

        private bool RestartSteam(string steamPath)
        {
            if (string.IsNullOrEmpty(steamPath))
                return false;

            string steamExe = Path.Combine(steamPath, "steam.exe");
            if (!File.Exists(steamExe))
                return false;

            try
            {
                Process.Start(steamExe);
                Thread.Sleep(3000);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private List<string> ExtractManifestIdsFromLua(string luaFilePath)
        {
            List<string> manifestNames = new List<string>();

            try
            {
                string content = File.ReadAllText(luaFilePath, Encoding.UTF8);
                string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                Regex manifestRegex = new Regex(@"setManifestid\s*\(\s*(\d+)\s*,\s*""(\d+)""\s*\)", RegexOptions.IgnoreCase);

                foreach (string line in lines)
                {
                    Match match = manifestRegex.Match(line);
                    if (match.Success)
                    {
                        string depotId = match.Groups[1].Value;
                        string manifestId = match.Groups[2].Value;
                        string manifestName = $"{depotId}_{manifestId}.manifest";
                        manifestNames.Add(manifestName);
                    }
                }
            }
            catch { }

            return manifestNames;
        }

        private bool ProcessNewApi(string appId, string steamPath, string stpluginPath, WebClient client)
        {
            try
            {
                string newApiUrl = $"https://codeload.github.com/SteamAutoCracks/ManifestHub/zip/refs/heads/{appId}";
                string archivePath = Path.Combine(Path.GetTempPath(), $"temp_{appId}_new.zip");

                client.DownloadFile(newApiUrl, archivePath);

                FileInfo fileInfo = new FileInfo(archivePath);
                if (fileInfo.Length < 1024)
                {
                    File.Delete(archivePath);
                    return false;
                }

                string extractPath = Path.Combine(Path.GetTempPath(), $"temp_extract_{appId}");
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                Directory.CreateDirectory(extractPath);

                ZipFile.ExtractToDirectory(archivePath, extractPath);

                string[] allFiles = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

                bool hasLuaFiles = false;
                bool hasManifestFiles = false;

                foreach (string file in allFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string ext = Path.GetExtension(file).ToLower();

                    if (ext == ".lua")
                    {
                        hasLuaFiles = true;
                        string destPath = Path.Combine(stpluginPath, fileName);
                        File.Copy(file, destPath, true);
                    }
                    else if (ext == ".manifest")
                    {
                        hasManifestFiles = true;
                        string depotCachePath = GetDepotCachePath(steamPath);
                        if (!Directory.Exists(depotCachePath))
                            Directory.CreateDirectory(depotCachePath);

                        string destPath = Path.Combine(depotCachePath, fileName);
                        File.Copy(file, destPath, true);
                    }
                }

                Directory.Delete(extractPath, true);
                File.Delete(archivePath);

                return hasLuaFiles || hasManifestFiles;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"New API error for {appId}: {ex.Message}");
                return false;
            }
        }

        private bool ProcessOldApi(string appId, string stpluginPath, WebClient client)
        {
            try
            {
                string baseUrl = "https://kernelos.org";
                string apiUrl = $"{baseUrl}/games/download.php?gen=1&id={appId}";

                string jsonResponse = client.DownloadString(apiUrl);
                var data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (data.url == null)
                    return false;

                string downloadUrl = baseUrl + data.url;
                string archivePath = Path.Combine(Path.GetTempPath(), $"temp_{appId}_old.zip");

                client.DownloadFile(downloadUrl, archivePath);

                bool foundLua = false;
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                        {
                            string extractPath = Path.Combine(stpluginPath, entry.Name);
                            entry.ExtractToFile(extractPath, true);
                            foundLua = true;
                        }
                    }
                }

                File.Delete(archivePath);
                return foundLua;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Old API error for {appId}: {ex.Message}");
                return false;
            }
        }

        private void installButton_Click(object sender, EventArgs e)
        {
            string[] links = linkTextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (links.Length == 0)
            {
                MessageBox.Show("введите хотя бы одну ссылку!");
                return;
            }

            string steamPath = null;

            if (AutoDetectSteam.Checked)
            {
                steamPath = FindSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    MessageBox.Show("steam не найден автоматически! Попробуйте выбрать папку вручную.", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                steamPath = SelectSteamFolder();
                if (string.IsNullOrEmpty(steamPath))
                {
                    return;
                }
            }

            string stpluginPath = GetStpluginPath(steamPath);
            if (string.IsNullOrEmpty(stpluginPath))
            {
                MessageBox.Show("не удалось определить путь stplug-in");
                return;
            }

            if (!Directory.Exists(stpluginPath))
                Directory.CreateDirectory(stpluginPath);

            string depotCachePath = GetDepotCachePath(steamPath);
            if (!Directory.Exists(depotCachePath))
                Directory.CreateDirectory(depotCachePath);

            if (IsSteamRunning())
            {
                KillSteam();
            }

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers["User-Agent"] = "Mozilla/5.0";

                try
                {
                    string[] dllUrls = new string[]
                    {
                "https://github.com/kryyaa/steams3x/releases/download/repair/xinput1_4.dll",
                "https://github.com/kryyaa/steams3x/releases/download/repair/python311.dll",
                "https://github.com/kryyaa/steams3x/releases/download/repair/millennium.dll"
                    };

                    foreach (string dllUrl in dllUrls)
                    {
                        try
                        {
                            string fileName = Path.GetFileName(new Uri(dllUrl).LocalPath);
                            string dllPath = Path.Combine(steamPath, fileName);

                            client.DownloadFile(dllUrl, dllPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"не удалось загрузить {Path.GetFileName(new Uri(dllUrl).LocalPath)}: {ex.Message}",
                                "предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ошибка при загрузке DLL: {ex.Message}", "предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                int successCount = 0;
                int failCount = 0;

                foreach (string link in links)
                {
                    string appId = ExtractGameId(link);
                    if (string.IsNullOrEmpty(appId))
                    {
                        MessageBox.Show($"не удалось извлечь ID из ссылки: {link}");
                        failCount++;
                        continue;
                    }

                    try
                    {
                        bool newApiSuccess = ProcessNewApi(appId, steamPath, stpluginPath, client);

                        if (!newApiSuccess)
                        {
                            bool oldApiSuccess = ProcessOldApi(appId, stpluginPath, client);
                            if (!oldApiSuccess)
                            {
                                MessageBox.Show($"игра {appId} не найдена ни в одном источнике");
                                failCount++;
                            }
                            else
                            {
                                successCount++;
                            }
                        }
                        else
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ошибка при установке для {appId}: {ex.Message}");
                        failCount++;
                    }
                }

                RestartSteam(steamPath);
                MessageBox.Show($"установка завершена!\nуспешно: {successCount}\nне удалось: {failCount}\nDLL файлы загружены\nsteam перезапущен.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] links = linkTextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (links.Length == 0)
            {
                MessageBox.Show("введите хотя бы одну ссылку!", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string steamPath = null;

            if (AutoDetectSteam.Checked)
            {
                steamPath = FindSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    MessageBox.Show("steam не найден автоматически! Попробуйте выбрать папку вручную.", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                steamPath = SelectSteamFolder();
                if (string.IsNullOrEmpty(steamPath))
                {
                    return;
                }
            }

            string stpluginPath = GetStpluginPath(steamPath);
            if (!Directory.Exists(stpluginPath))
            {
                MessageBox.Show("папка stplug-in не найдена", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsSteamRunning())
            {
                KillSteam();
                Thread.Sleep(2000);
            }

            int totalRemoved = 0;
            string depotCachePath = GetDepotCachePath(steamPath);

            foreach (string link in links)
            {
                string appId = ExtractGameId(link);
                if (string.IsNullOrEmpty(appId))
                {
                    MessageBox.Show($"не удалось извлечь ID из ссылки: {link}", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    continue;
                }

                try
                {
                    string[] luaFiles = Directory.GetFiles(stpluginPath, "*.lua");
                    List<string> manifestsToDelete = new List<string>();

                    foreach (string luaFile in luaFiles)
                    {
                        try
                        {
                            string content = File.ReadAllText(luaFile, Encoding.UTF8);
                            if (content.Contains(appId) || Path.GetFileName(luaFile).Contains(appId))
                            {
                                List<string> manifests = ExtractManifestIdsFromLua(luaFile);
                                manifestsToDelete.AddRange(manifests);

                                File.Delete(luaFile);
                                totalRemoved++;
                            }
                        }
                        catch { }
                    }

                    if (Directory.Exists(depotCachePath))
                    {
                        foreach (string manifestName in manifestsToDelete)
                        {
                            try
                            {
                                string manifestPath = Path.Combine(depotCachePath, manifestName);
                                if (File.Exists(manifestPath))
                                {
                                    File.Delete(manifestPath);
                                    totalRemoved++;
                                }
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ошибка при удалении для {appId}: {ex.Message}", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            RestartSteam(steamPath);
            MessageBox.Show($"удалено всего {totalRemoved} файлов. steam перезапущен.", "готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void repairButton_Click(object sender, EventArgs e)
        {
            string steamPath = null;

            if (AutoDetectSteam.Checked)
            {
                steamPath = FindSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    MessageBox.Show("steam не найден автоматически! Попробуйте выбрать папку вручную.", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                steamPath = SelectSteamFolder();
                if (string.IsNullOrEmpty(steamPath))
                {
                    return;
                }
            }

            string stpluginPath = GetStpluginPath(steamPath);
            if (!Directory.Exists(stpluginPath))
            {
                MessageBox.Show("папка stplug-in не найдена", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsSteamRunning())
            {
                KillSteam();
                Thread.Sleep(2000);
            }

            string tempDir = Path.Combine(stpluginPath, "_temp_repair");
            string tempManifestsDir = Path.Combine(Path.GetTempPath(), "_temp_manifests_repair");
            string depotCachePath = GetDepotCachePath(steamPath);
            Dictionary<string, List<string>> luaToManifests = new Dictionary<string, List<string>>();

            try
            {
                Directory.CreateDirectory(tempDir);
                Directory.CreateDirectory(tempManifestsDir);

                string[] luaFiles = Directory.GetFiles(stpluginPath, "*.lua");
                List<string> backedUpFiles = new List<string>();
                List<string> backedUpManifests = new List<string>();

                foreach (string file in luaFiles)
                {
                    string fileName = Path.GetFileName(file);

                    List<string> manifests = ExtractManifestIdsFromLua(file);
                    luaToManifests[fileName] = manifests;

                    string dest = Path.Combine(tempDir, fileName);
                    File.Copy(file, dest, true);
                    backedUpFiles.Add(fileName);
                }

                if (Directory.Exists(depotCachePath))
                {
                    foreach (var kvp in luaToManifests)
                    {
                        foreach (string manifestName in kvp.Value)
                        {
                            try
                            {
                                string manifestPath = Path.Combine(depotCachePath, manifestName);
                                if (File.Exists(manifestPath))
                                {
                                    string backupPath = Path.Combine(tempManifestsDir, manifestName);
                                    File.Copy(manifestPath, backupPath, true);
                                    backedUpManifests.Add(manifestName);
                                }
                            }
                            catch { }
                        }
                    }
                }

                foreach (string file in luaFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                if (Directory.Exists(depotCachePath))
                {
                    foreach (var kvp in luaToManifests)
                    {
                        foreach (string manifestName in kvp.Value)
                        {
                            try
                            {
                                string manifestPath = Path.Combine(depotCachePath, manifestName);
                                if (File.Exists(manifestPath))
                                {
                                    File.Delete(manifestPath);
                                }
                            }
                            catch { }
                        }
                    }
                }

                try
                {
                    string dllUrl = "https://github.com/kryyaa/steams3x/releases/download/repair/xinput1_4.dll";
                    string dllPath = Path.Combine(steamPath, "xinput1_4.dll");

                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers["User-Agent"] = "Mozilla/5.0";
                        webClient.DownloadFile(dllUrl, dllPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"не удалось загрузить xinput1_4.dll: {ex.Message}", "предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (RestartSteam(steamPath))
                {
                    Thread.Sleep(25000);

                    if (IsSteamRunning())
                    {
                        KillSteam();
                        Thread.Sleep(2000);
                    }

                    int restoredLuaCount = 0;
                    foreach (string fileName in backedUpFiles)
                    {
                        try
                        {
                            string src = Path.Combine(tempDir, fileName);
                            string dst = Path.Combine(stpluginPath, fileName);
                            File.Copy(src, dst, true);
                            restoredLuaCount++;
                        }
                        catch { }
                    }

                    int restoredManifestCount = 0;
                    if (Directory.Exists(depotCachePath))
                    {
                        foreach (string manifestName in backedUpManifests)
                        {
                            try
                            {
                                string src = Path.Combine(tempManifestsDir, manifestName);
                                string dst = Path.Combine(depotCachePath, manifestName);
                                File.Copy(src, dst, true);
                                restoredManifestCount++;
                            }
                            catch { }
                        }
                    }

                    RestartSteam(steamPath);
                    MessageBox.Show($"восстановлено {restoredLuaCount} lua файлов и {restoredManifestCount} манифестов\nxinput1_4.dll загружена успешно", "готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ошибка при ремонте: {ex.Message}", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                    if (Directory.Exists(tempManifestsDir))
                        Directory.Delete(tempManifestsDir, true);
                }
                catch { }
            }
        }

        private string SelectSteamFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Выберите папку установки Steam";
                folderDialog.ShowNewFolderButton = false;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = folderDialog.SelectedPath;
                    string steamExe = Path.Combine(selectedPath, "steam.exe");

                    if (File.Exists(steamExe))
                    {
                        return selectedPath;
                    }
                    else
                    {
                        MessageBox.Show("В выбранной папке не найден steam.exe!", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }

                return null;
            }
        }

        private void linkTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void openTgChannelButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://t.me/kryyaasoft");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"не удалось открыть ссылку: {ex.Message}", "ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}