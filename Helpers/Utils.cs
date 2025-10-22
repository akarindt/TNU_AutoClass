using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNU_AutoClass.Helpers
{
    public static class Utils
    {

        public static string GetConfigValue(string _configName, string _defaultValue, string _configFilePath = "config.txt")
        {
            var configDict = ReadConfig(_configFilePath);
            return configDict.GetValueOrDefault(_configName, _defaultValue);
        }

        public static Dictionary<string, string> ReadConfig(string _configFilePath = "config.txt")
        {
            var configDict = new Dictionary<string, string>();

            try
            {
                if (!File.Exists(_configFilePath))
                {
                    Console.WriteLine($"Config file not found: {_configFilePath}");
                    return configDict;
                }

                string[] lines = File.ReadAllLines(_configFilePath, Encoding.UTF8);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#") || line.TrimStart().StartsWith("//"))
                        continue;

                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex > 0 && equalsIndex < line.Length - 1)
                    {
                        string key = line.Substring(0, equalsIndex).Trim();
                        string value = line.Substring(equalsIndex + 1).Trim();

                        if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                            (value.StartsWith("'") && value.EndsWith("'")))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }

                        configDict[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading config file: {ex.Message}");
            }

            return configDict;
        }

        public static bool UpdateConfigValue(string _key, string _value, string _configFilePath = "config.txt")
        {
            try
            {
                var configDict = ReadConfig(_configFilePath);
                configDict[_key] = _value;
                return WriteConfig(configDict, _configFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating config value: {ex.Message}");
                return false;
            }
        }

        public static bool WriteConfig(Dictionary<string, string> _configDict, string _configFilePath = "config.txt")
        {
            try
            {
                string? directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = new List<string>();
                foreach (var kvp in _configDict)
                {
                    string escapedValue = kvp.Value;
                    if (kvp.Value.Contains("=") || kvp.Value.Contains("#") || kvp.Value.Contains("//") ||
                        kvp.Value.StartsWith(" ") || kvp.Value.EndsWith(" "))
                    {
                        escapedValue = $"\"{kvp.Value}\"";
                    }

                    lines.Add($"{kvp.Key}={escapedValue}");
                }

                File.WriteAllLines(_configFilePath, lines, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing config file: {ex.Message}");
                return false;
            }
        }
    }
}
