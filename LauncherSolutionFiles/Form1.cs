// Experimentals Launcher for Experimental Games Project
// Michael Anthony Nash 
// Student ID : 23627350
// 11th May 2025 - MIT Licence

// Purpose of Application

// This Launcher will allow the user to automatically download EXPERIMENTALS from an Public Github Repository (https://github.com/T3L3KIN3SISr/EXPERIMENTALS/releases/tag/Builds).
// It'll also attempt to open the Unreal Project (if successful)

using System;
using System.IO;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.NetworkInformation;


namespace ExperimentalsLauncher
{
    public partial class Form1 : Form
    {
        private bool DoesUserHaveUnrealEngine = false; // Boolean isn't really used. 

        public Form1()
        {
            InitializeComponent();
            CheckforUnrealEngine();
        }

        #region Button Triggers
        private void button1_Click_1(object sender, EventArgs e) // Open Self Extraction for Packaged Game
        {
            DownloadGitHubRelease();
        }

        private void button2_Click(object sender, EventArgs e) // Open Unreal Project
        {
            LaunchUnrealEngineProject();
        }
        #endregion

        #region Unreal Engine Verification
        private void CheckforUnrealEngine()
        {
            string[] PotentialUnrealPaths =
            {
                @"C:\Program Files\Epic Games\UE_5.4", // This is the Recommended Version (Unreal Engine 5.4.4)
                @"C:\Program Files\Epic Games\UE_5.1", // You may not have Unreal Engine installed in these directories.
                @"C:\Program Files\Epic Games\UE_5.2", // I am assuming Unreal Engine is installed here.
                @"C:\Program Files\Epic Games\UE_5.3", // If this does not work and you do have Unreal Engine then please open MyProject.uproject manually.
            };

            foreach (string path in PotentialUnrealPaths)
            {
                if (Directory.Exists(path))
                {
                    textBox1.Text = ("Unreal Engine has been detected at: " + path);
                    DoesUserHaveUnrealEngine = true;
                    break;
                }
                else
                {
                    textBox1.Text = ("Unable to find Unreal Engine");
                }
            }
        }

        private void LaunchUnrealEngineProject()
        {
            string defaultDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)); // Checks in the root directory.
            string fullApplicationPath = Path.Combine(defaultDirectory, "MyProject.uproject"); // This will be the root directory

            if (File.Exists(fullApplicationPath) && DoesUserHaveUnrealEngine)
            {
                Process.Start(new ProcessStartInfo(fullApplicationPath) { UseShellExecute = true});
                textBox1.Text = ("Opening Unreal Engine - Successful");
                CloseApplicationAfterSuccess();
            }
            else
            {
                // Attempt to open Unreal Project if the user has Unreal Engine installed in an abnormal path
                Process.Start(new ProcessStartInfo(fullApplicationPath) { UseShellExecute = true });
                textBox1.Text = ("Attempting to open Unreal Project through Windows Shell.");
            }
        }

        #endregion

        private async void DownloadGitHubRelease()
        {
            string githubLink = "https://github.com/T3L3KIN3SISr/EXPERIMENTALS/releases/download/dev/ExperimentalsSFX.exe";
            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExperimentalsSFX.exe");

            try
            {
                textBox1.Text = "Please Wait... Downloading from Github.";
                progressBar1.Visible = true;

                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(githubLink, HttpCompletionOption.ResponseHeadersRead); 
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                using Stream contentStream = await response.Content.ReadAsStreamAsync();
                using FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);

                var buffer = new byte[8192]; // Create the Default Byte for Progress Bar
                long totalRead = 0;
                int bytesRead;

                // progressBar1 - Force Set Values
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;

                    int percent = (int)((totalRead * 100L) / totalBytes.Value);
                    progressBar1.Value = percent;
                }

                textBox1.Text = "Download Complete.";

                #region Attempt to Open Self Extraction Application 

                // 12/05/2025 - When testing on other PC's, the Self Extractor CANNOT to execute.
                // Could be due to Anti-Virus Protection or other Security Settings??

                // The User can locate the Self Extractor in the Base Directory for Manual Execution

                if (File.Exists(targetPath))
                {
                    // Using Command Prompt to open the Executable due to System.Diagnostics being unable to open the targetPath by itself for some reason.
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"\"{targetPath}\"\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process.Start(processInfo);

                    textBox1.Text = "Please Check the Root Directory of this Program.";

                    await Task.Delay(2000);  // 2 Seconds

                    textBox1.Text = "If the Self Extractor has not opened, It will be located in the root directory.";
                }
            }
            catch (Exception)
            {
                textBox1.Text = "Unable to Download from Github. Please try again.";
                progressBar1.Visible = false;
            }
        }
        #endregion

        private async void CloseApplicationAfterSuccess()
        {
            await Task.Delay(5000); // 5 Seconds
            Application.Exit();
        }
    }
}
