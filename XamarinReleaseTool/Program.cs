using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XamarinReleaseTool.FileNaming;

namespace XamarinReleaseTool
{
    internal class Program
    {
        private static string workingDirectory = ConfigurationManager.AppSettings["WorkingFolder"];
        private static string fileNamePrefix = ConfigurationManager.AppSettings["FileName"];
        private static string targets = "/t:Clean;Rebuild;SignAndroidPackage";
        private static string[] allConfigurations = ConfigurationManager.AppSettings["AllConfigurations"].Split(',');
        private static string[] allConfigurationAlias = ConfigurationManager.AppSettings["AllConfigurationsAlias"].Split(',');
        private static string outputDir = (string)null;
        private static string configurationName = "All";
        private static bool renameOnly;
        private static string msBuildPath;
        private static bool noTimestamp;

        private static string _appDir;

        public static string AppDir
        {
            get
            {
                if (string.IsNullOrEmpty(_appDir))
                {
                    _appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _appDir;
            }
        }

        private static void Debug(string str)
        {
            const string lines = "-------------------------------------------";
            //Console.WriteLine(lines);
            Console.WriteLine(str);
            //Console.WriteLine(lines);
            //Console.WriteLine();
        }

        private static int Main(string[] args)
        {
            var startTime = DateTime.Now;
            Debug(string.Format("Build started: {0}", startTime));
            Debug(string.Format("Executing on path {0}", AppDir));
            // load the settings
            LoadSettings();

            // parse arguments
            if (!ParseArguments(args))
            {
                Debug(string.Format("Build finsihed: {0}, time: {1}", DateTime.Now, DateTime.Now.Subtract(startTime)));

                return 1;
            }

            // if All, build all configurations
            // otherwise, build the given
            if (configurationName.Equals("All"))
            {
                foreach (string str in allConfigurations)
                {
                    configurationName = str;
                    if (!BuildApk())
                    {
                        Debug(string.Format("Build finsihed: {0}, time: {1}", DateTime.Now, DateTime.Now.Subtract(startTime)));

                        return 1;
                    }

                    RenameFiles();
                }
            }
            else
            {
                if (!BuildApk())
                {
                    Debug(string.Format("Build finsihed: {0}, time: {1}", DateTime.Now, DateTime.Now.Subtract(startTime)));

                    return 1;
                }
                RenameFiles();
            }

            // all went well
            Debug(string.Format("Build finsihed: {0}, time: {1}", DateTime.Now, DateTime.Now.Subtract(startTime)));

            return 0;
        }

        private static bool BuildApk()
        {

            Console.WriteLine("Building APK...");

            // only build if not rename only
            if (!renameOnly)
            {
                // check the ms build path
                if (msBuildPath == null)
                {
                    Console.WriteLine("When using build, make sure to either set a Environment var [MSBuildExe] pointing to your MSBuild executable,");
                    Console.WriteLine("or add a local.properties file to this application folder with:");
                    Console.WriteLine("msbuildexe=[Full path to MS Build executable]");
                    return false;
                }

                // check if the msbuild exe exists
                if (!File.Exists(msBuildPath))
                {
                    Console.WriteLine("MSbuild not found on: " + msBuildPath);
                    return false;
                }

                // create arguments
                string arguments = string.Format("MK.Solar.csproj /p:Configuration={0} {1}", (object)configurationName, (object)targets);
                Console.WriteLine("Build APK using: " + msBuildPath);
                Console.WriteLine("Arguments: " + arguments);

                // try the build
                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo()
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardOutput = false,
                        UseShellExecute = false,
                        WorkingDirectory = workingDirectory,
                        FileName = msBuildPath,
                        Arguments = arguments
                    };

                    using (Process process = Process.Start(processStartInfo))
                    {
                        //
                        // Read in all the text from the process with the StreamReader.
                        //
                        using (StreamReader reader = process.StandardOutput)
                        {
                            string result = reader.ReadToEnd();
                            Console.Write(result);
                        }

                        //process.WaitForExit();
                    }

                    // wait for the full build to finish
                    //process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    return false;
                }
            }

            // build done
            Console.WriteLine("Build done.");
            return true;
        }

        private static bool ParseArguments(string[] args)
        {
            Console.WriteLine("Parsing arguments...");
            if (args.Length == 1 && args[0].Equals("-?"))
            {
                ShowHelp();
                return true;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("No release name found, use default [All]");
                configurationName = "All";
            }

            // itereate through the arguments and check
            foreach (string arg in args)
            {
                bool argRecognized = false;

                // configuration
                if (arg.StartsWith("/Configuration="))
                {
                    string[] split = arg.Split('=');
                    if (split.Length == 2)
                    {
                        configurationName = split[1];
                        Console.WriteLine("Building configuration: " + configurationName);
                        argRecognized = true;
                    }
                    else
                    {
                        Console.WriteLine("Error make sure you write the Configuration param as [/Configuration=Name]");
                        return false;
                    }
                }

                // outputdir (relative to running folder)
                if (arg.StartsWith("/OutputDir="))
                {
                    string[] split = arg.Split('=');
                    if (split.Length == 2)
                    {
                        outputDir = split[1];
                        Console.WriteLine("Output Dir: " + outputDir);
                        argRecognized = true;
                    }
                    else
                    {
                        Console.WriteLine("Error make sure you write the Configuration param as [/Configuration=Name]");
                        return false;
                    }
                }

                // rename only
                if (arg.Equals("/RenameOnly"))
                {
                    Console.WriteLine("/RenameOnly found, skip the build.");
                    renameOnly = true;
                    argRecognized = true;
                }

                // do we need the timestamp?
                if (arg.Equals("/NoTimestamp"))
                {
                    Console.WriteLine("/NoTimestamp found, do not add timestamp to the filename.");
                    noTimestamp = true;
                    argRecognized = true;
                }

                // if argument is not recognized, report
                if (!argRecognized)
                {
                    Console.WriteLine("Could not rezognize arg : " + arg);
                }
            }
            return true;
        }

        private static void LoadSettings()
        {
            workingDirectory = null;
            // check working folder
            if (Directory.Exists(@"xamarin\MK.Solar"))
            {
                workingDirectory = @"xamarin\MK.Solar";
            }
            else if (Directory.Exists(@"..\xamarin\MK.Solar"))
            {
                workingDirectory = @"..\xamarin\MK.Solar";
            }
            else if (Directory.Exists(@"..\..\..\xamarin\MK.Solar"))
            {
                workingDirectory = @"..\..\..\xamarin\MK.Solar";
            }

            if (workingDirectory == null)
            {
                throw new Exception("Working directory not found, we need to allow this as an argument if we need to make it work, Not Supported yet...");
            }

            // laod the MS Build from the Env
            msBuildPath = Environment.GetEnvironmentVariable("MSBuildExe");

            if (!File.Exists("local.properties"))
            {
                return;
            }

            // Read properties from local.properties
            foreach (string str in File.ReadAllLines("local.properties"))
            {
                if (str.StartsWith("msbuildexe="))
                    msBuildPath = str.Remove(0, "msbuildexe=".Length);
            }
        }

        private static void RenameFiles()
        {
            Console.WriteLine("Renaming files.");
            string apkPath = workingDirectory + "\\bin\\" + configurationName;
            string manifestFile = workingDirectory + "\\Properties\\AndroidManifest.xml";
            XNamespace androidNs = (XNamespace)"http://schemas.android.com/apk/res/android";


            Console.WriteLine("Rename files in: " + apkPath);
            Console.WriteLine("Reading Android Manifest: " + manifestFile);

            XDocument manifestDoc = XDocument.Load(manifestFile);
            string versionName = manifestDoc.Root.Attribute(androidNs + "versionName").Value;
            string versionCode = manifestDoc.Root.Attribute(androidNs + "versionCode").Value;
            string package = manifestDoc.Root.Attribute((XName)"package").Value;

            Console.WriteLine(string.Format("versionName: {0}, versionCode: {1}, package: {2}", (object)versionName, (object)versionCode, (object)package));

            // check if there is an alias
            string alias = configurationName;
            for (int index = 0; index < allConfigurations.Length; ++index)
            {
                if (allConfigurations[index].Equals(configurationName) && allConfigurationAlias.Length > index)
                {
                    alias = allConfigurationAlias[index];
                }
            }

            // create the new file name
            string replaceFileName = string.Format("{0}-{1}-{2}-{3}{4}",
                fileNamePrefix, alias, versionName, versionCode, noTimestamp ? "" : DateTime.Now.ToString("-yyyyMMdd-HHmmss"));
            foreach (string fileName in Directory.GetFiles(apkPath, package + "*.apk"))
            {
                Name name = new Name(fileNamePrefix, alias, fileName, versionCode, versionName);
                string currentDirectory = AppDir + name.DirectoryPath;
                if (!Directory.Exists(currentDirectory))
                {
                    Directory.CreateDirectory(currentDirectory);
                }
                string pathToApk = currentDirectory + name.Filename;

                // delete file if existing
                if (File.Exists(pathToApk))
                {
                    File.Delete(pathToApk);
                }

                // Copy the file
                Debug(string.Format("Copy [{0}] to [{1}]", (object)fileName, pathToApk));
                File.Copy(fileName, pathToApk);

                Debug(string.Format("Compressing: {0}", pathToApk));

                using (ZipArchive zip = ZipFile.Open(pathToApk + ".zip", ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(pathToApk, name.Filename);
                }
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Required parameters: /Configuration=[Name] /RenameOnly (optional)");
        }
    }
}
