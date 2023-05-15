using System.Security.Cryptography;

namespace Folder_Sync
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Input: [Source path] [Replica path] [Interval in seconds] [Log file path]");
                return;
            }

            string sourceFolder = args[0];
            string replicaFolder = args[1];
            int interval = int.Parse(args[2]);
            string logFilePath = args[3];

            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine("Source folder does not exist!");
                return;
            }

            if (!Directory.Exists(replicaFolder))
            {
                Console.WriteLine("Replica folder does not exist. Creating new folder...");
                Directory.CreateDirectory(replicaFolder);
            }

            Console.WriteLine("Starting synchronization. Press Ctrl+C to stop.");

            while (true)
            {
                SyncFolders(sourceFolder, replicaFolder, logFilePath);
                Thread.Sleep(interval * 1000);
            }
        }

        static void SyncFolders(string source, string replica, string logFilePath)
        {
            using (StreamWriter logFile = new StreamWriter(logFilePath, true))
            {
                logFile.WriteLine(DateTime.Now.ToString() + " - Starting synchronization.");

                // get files in source folder
                string[] sourceFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories);

                foreach (string sourceFile in sourceFiles)
                {
                    string relativePath = sourceFile.Substring(source.Length);
                    string replicaFile = replica + relativePath;

                    if (File.Exists(replicaFile))
                    {
                        if (AreFilesEqual(sourceFile, replicaFile))
                        {
                            // files are identical, no action needed
                            continue;
                        }
                        else
                        {
                            // file exists in replica but is different, overwrite it
                            logFile.WriteLine(DateTime.Now.ToString() + " - Updating file: " + replicaFile);
                            Console.WriteLine("Updating file: " + replicaFile);
                            File.Copy(sourceFile, replicaFile, true);
                        }
                    }
                    else
                    {
                        // file does not exist in replica, copy it
                        logFile.WriteLine(DateTime.Now.ToString() + " - Creating file: " + replicaFile);
                        Console.WriteLine("Creating file: " + replicaFile);
                        string replicaDir = Path.GetDirectoryName(replicaFile);
                        if (!Directory.Exists(replicaDir))
                        {
                            Directory.CreateDirectory(replicaDir);
                        }
                        File.Copy(sourceFile, replicaFile, true);
                    }
                }

                // remove files from replica that don't exist in source
                string[] replicaFiles = Directory.GetFiles(replica, "*", SearchOption.AllDirectories);

                foreach (string replicaFile in replicaFiles)
                {
                    string relativePath = replicaFile.Substring(replica.Length);
                    string sourceFile = source + relativePath;

                    if (!File.Exists(sourceFile))
                    {
                        logFile.WriteLine(DateTime.Now.ToString() + " - Deleting file: " + replicaFile);
                        Console.WriteLine("Deleting file: " + replicaFile);
                        File.Delete(replicaFile);
                    }
                }

                logFile.WriteLine(DateTime.Now.ToString() + " - Synchronization complete.");
                Console.WriteLine("Synchronization complete.");
            }
        }

        static bool AreFilesEqual(string file1, string file2)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream1 = File.OpenRead(file1))
                using (var stream2 = File.OpenRead(file2))
                {
                    byte[] hash1 = md5.ComputeHash(stream1);
                    byte[] hash2 = md5.ComputeHash(stream2);
                    for (int i = 0; i < hash1.Length; i++)
                    {
                        if (hash1[i] != hash2[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }
    }
}