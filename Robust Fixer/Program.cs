using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Win32;

namespace Robust_Fixer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Select your option");
            Console.WriteLine("1: Delete robust folders \n2: Add windows defender exclusion \n3: Execute option 1 and 2 \n4: Disable Hyper-V features \n5: Fix Onedrive Username issue (might break your PC)");
            int selectedOption;
            bool successfullyParsed = int.TryParse(Console.ReadLine(), out selectedOption);

            if (successfullyParsed)
            {
                switch (selectedOption)
                {
                    case 1:
                        //string defaultGtaPath = "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V";
                        Console.WriteLine("Select your GTA directory...");
                        string gtaDirectoryPath = FolderBrowser();
                        if (gtaDirectoryPath != null)
                        {
                            DeleteRobustFolderGtaDirectory(gtaDirectoryPath);
                            DeleteRobustFolderMyDocuments();
                            break;
                        }
                        Console.WriteLine("No path selected...");
                        break;
                    case 2:
                        Console.WriteLine("Select the Robust launcher (RMEngineLauncher)...");
                        string robustLauncherPath = FileBrowser();
                        if (robustLauncherPath != null)
                        {
                            AddRobustLauncherToWindowsDefender(robustLauncherPath);
                            Console.WriteLine("Added an exlcusion");
                            break;
                        }
                        break;
                    case 3:
                        Console.WriteLine("Select your GTA directory...");
                        string gtaDirectoryPath2 = FolderBrowser();
                        if (gtaDirectoryPath2 != null)
                        {
                            DeleteRobustFolderGtaDirectory(gtaDirectoryPath2);
                            DeleteRobustFolderMyDocuments();

                            Console.WriteLine("Step 1 done.");

                            Console.WriteLine("Select the Robust launcher (RMEngineLauncher)...");

                            string robustLauncherPath2 = FileBrowser();
                            if (robustLauncherPath2 != null)
                            {
                                AddRobustLauncherToWindowsDefender(robustLauncherPath2);
                                Console.WriteLine("Added an exlcusion");
                                Console.WriteLine("Step 2 done.");
                                break;
                            }
                            Console.WriteLine("No path selected...");
                            break;
                        }
                        break;
                    case 4:
                        DisableHyperVFeatures();
                        break;
                    case 5:
                        Console.WriteLine("Are you sure you want to continue? Please be aware that this might break your User.");
                        Console.Write("To continue please type 'yes' \n");
                        if (Console.ReadLine() == "yes")
                        {
                            ResetDocuments();
                            Console.WriteLine("Done, please restart computer");
                        }
                        else
                        {
                            Console.WriteLine("You canceled the Request");
                            break;
                        }
                        break;
                }
            }

            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
        }

        public static string FileBrowser()
        {
            OpenFileDialog fbd = new OpenFileDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string launcherLocation = fbd.FileName;
                if (launcherLocation != null)
                {
                    return launcherLocation;
                }
                return null;
            }
            return null;
        }

        public static string FolderBrowser()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string directoryPath = folderBrowserDialog.SelectedPath;
                if (directoryPath != null)
                {
                    return directoryPath;
                }
                return null;
            }
            return null;
        }

        private static void AddRobustLauncherToWindowsDefender(string launcherPath)
        {
            // C&P xd
            string addFolderExclusionCommand = "Add-MpPreference -ExclusionPath \"" + launcherPath + "\"";
            //runspace
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            //pipeline
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(addFolderExclusionCommand);
            pipeline.Commands.Add("Out-String");
            pipeline.Invoke();
            //runspace
            runspace.Close();
        }

        private static void DisableHyperVFeatures()
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            //pipeline
            Pipeline pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript("Disable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All");
            pipeline.Commands.Add("Out-String");
            pipeline.Invoke();
            //runspace
            runspace.Close();
        }

        private static bool DeleteRobustFolderGtaDirectory(string path)
        {
            //string defaultGtaPath = "C:\\Program Files\\Rockstar Games\\Grand Theft Auto V";
            bool doesDirectoryExist = Directory.Exists(path);
            if (doesDirectoryExist)
            {
                string robustGtaFolderPath = Path.Combine(path, "Robust");
                Console.WriteLine("Path: " + path);
                bool doesRobustDirectoryExist = Directory.Exists(robustGtaFolderPath);
                if (doesRobustDirectoryExist)
                {
                    Console.WriteLine("Robust GTA path: " + robustGtaFolderPath);
                    var gtaDiretory = new DirectoryInfo(robustGtaFolderPath);
                    gtaDiretory.Delete(true);
                    Console.WriteLine("The Robust folder in the GTA diretory was removed.");
                    return true;
                }
                Console.WriteLine("The Robust folder in the GTA diretory didn't exist. Nothing has been changed.");
                return false;
            }
            Console.WriteLine("Path doesn't exist.");
            return false;
        }

        private static bool DeleteRobustFolderMyDocuments()
        {
            var pathToMyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string robustMyDocumentsPath = Path.Combine(pathToMyDocuments, "Robust");
            bool doesRobustDirectoryExist = Directory.Exists(robustMyDocumentsPath);
            if (doesRobustDirectoryExist)
            {
                var gtaDiretory = new DirectoryInfo(robustMyDocumentsPath);
                gtaDiretory.Delete(true);
                Console.WriteLine("The Robust folder in the My Documents diretory was removed.");
                return true;
            }
            Console.WriteLine("The Robust folder in the My Documents diretory didn't exist. Nothing has been changed.");
            return false;
        }


        protected static bool ResetDocuments()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\User Shell Folders", true))
                if (regKey != null)
                {
                    regKey.SetValue("Personal", "%USERPROFILE%\\Documents", RegistryValueKind.String);
                    regKey.Close();
                    return true;
                }
            Console.WriteLine("Failed");
            return false;

        }
    }
}
