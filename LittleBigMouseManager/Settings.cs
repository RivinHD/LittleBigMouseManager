using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;


namespace LittleBigMouseManager
{
    static internal class Settings
    {
        public const string fileName = "Manager_Settings.json";
        public class Properties
        {
            public string ProcessName { get; set; }
            public string ProcessPath { get; set; }
            public bool AutoRestart { get; set; }
            public bool RestartOnClose { get; set; }
            public bool RestartOnUnwantedClose { get; set; }
            public bool KillLBM { get; set; }
            public uint SafetyTime { get; set; }
        }
        public readonly static Properties defaultProperties = new Properties
        {
            ProcessName = "LittleBigMouse_Daemon",
            ProcessPath = "C:\\Program Files\\LittleBigMouse\\LittleBigMouse_Daemon.exe",
            AutoRestart = true,
            RestartOnClose = false,
            RestartOnUnwantedClose = true,
            KillLBM = true,
            SafetyTime = 1000,
        };
        public static Properties loadedProperties = null;

        public static void Read()
        {
            if (!File.Exists(fileName))
            {
                loadedProperties = defaultProperties;
                Write(loadedProperties);
                return;
            }
            Stream jsonIn = new StreamReader(fileName, Encoding.UTF8).BaseStream;
            loadedProperties = JsonSerializer.Deserialize<Properties>(jsonIn);
            jsonIn.Close();
        }

        public static void Write(Properties properties)
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            Stream jsonOut = new StreamWriter(fileName, false, Encoding.UTF8).BaseStream;
            JsonSerializer.Serialize(jsonOut, properties, options);
            jsonOut.Close();
        }
    }
}
