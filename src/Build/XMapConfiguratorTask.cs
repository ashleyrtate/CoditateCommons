using System;
using System.IO;
using Coditate.Common.XMap;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Coditate.Build
{
    /// <summary>
    /// MSBuild task for updating .NET configuration files using XPath queries defined in a separate mapping file.
    /// </summary>
    /// <remarks>
    /// 
    /// 
    /// </remarks>
    public class XMapConfiguratorTask : Task
    {
        /// <summary>
        /// Default extension for XMap property files.
        /// </summary>
        public const string DefaultPropertyFileExtension = ".xprop";

        /// <summary>
        /// Default extension for XMapping files.
        /// </summary>
        public const string DefaultXMapFileExtension = ".xmap";

        /// <summary>
        /// Gets or sets the default config files.
        /// </summary>
        /// <value>The default config files.</value>
        [Required]
        public ITaskItem[] DefaultConfigFiles { get; set; }

        /// <summary>
        /// Gets or sets the Xmapping name.
        /// </summary>
        /// <value>The Xmapping name.</value>
        [Required]
        public string XMapping { get; set; }

        /// <summary>
        /// Executes this task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            Log.LogMessage("XMapping: \"{0}\"", XMapping);

            foreach (ITaskItem item in DefaultConfigFiles)
            {
                Log.LogMessage("Updating configuration file: \"{0}\"", item.ToString());

                ApplyMapping(item);
            }

            if (Log.HasLoggedErrors)
            {
                return false;
            }
            return true;
        }

        private void ApplyMapping(ITaskItem defaultConfig)
        {
            var defaultConfigFile = new FileInfo(defaultConfig.ItemSpec);
            string configFilePath = Path.ChangeExtension(defaultConfigFile.FullName, ".config");
            string mapFilePath = GetMapFilePath(defaultConfigFile);

            // todo: cache mapping 
            var xmap = new XMap();
            AddProperties(xmap, defaultConfigFile.DirectoryName);

            // todo: add configuration of xmap (how to handle missing properties, multiple mapping matches, no mapping matches, etc.)
            // todo: add report from Xmap of mappings applied for logging to MSBuild logger
            try
            {
                xmap.UpdateNodes(defaultConfigFile.FullName, configFilePath, mapFilePath);
            }
            catch (InvalidXpathExpressionException ex)
            {
                Log.LogError(null, null, null, mapFilePath, 0, 0, 0, 0,
                             "\"{0}\" is an invalid XPath expression. It does not fully resolve to any element or attribute of file \"{1}\"",
                             ex.XPath, defaultConfigFile.FullName);
            }
            catch (PropertyNotRegisteredException ex)
            {
                Log.LogError(null, null, null, mapFilePath, 0, 0, 0, 0,
                             "Property \"{0}\" has not been defined.",
                             ex.Property);
            }
        }

        private void AddProperties(XMap xmap, string propertySearchStartPath)
        {
            FileInfo propertyFile = GetPropertyFilePath(propertySearchStartPath);
            if (propertyFile == null)
            {
                return;
            }

            Log.LogMessage("Using Xmap property file \"{0}\"", propertyFile.FullName);

            var propertyLines = new string[0];
            try
            {
                propertyLines = File.ReadAllLines(propertyFile.FullName);
            }
            catch (Exception ex)
            {
                Log.LogError("Unable to read XMap property file \"{0}\": {1}", propertyFile.FullName, ex.ToString());
            }

            foreach (string propertyLine in propertyLines)
            {
                string trimmedLine, propertyName, propertyValue;
                trimmedLine = propertyLine.Trim();
                if (trimmedLine.Length == 0 || trimmedLine[0] == XMap.CommentEscapeChar)
                {
                    continue;
                }

                ParseProperty(trimmedLine, out propertyName, out propertyValue);
                if (string.IsNullOrEmpty(propertyName) || propertyValue == null)
                {
                    Log.LogError("XMap property file \"{0}\" contains invalid property definition \"{1}\"",
                                 propertyFile.FullName,
                                 propertyLine);
                    continue;
                }
                Log.LogMessage(MessageImportance.Low,
                               "Parsed property \"{0}\" with value \"{1}\" from XMap property file \"{2}\"",
                               propertyName, propertyValue, propertyFile.FullName);

                xmap.AddProperty(propertyName, propertyValue);
            }
        }

        private void ParseProperty(string propertyLine, out string propertyName, out string propertyValue)
        {
            propertyName = null;
            propertyValue = null;
            int equalIndex = propertyLine.IndexOf('=');
            if (equalIndex < 0)
            {
                return;
            }
            propertyName = propertyLine.Substring(0, equalIndex);
            propertyValue = propertyLine.Substring(equalIndex + 1);
            if (propertyValue.StartsWith("\"") && propertyValue.EndsWith("\"") && propertyValue.Length > 1)
            {
                propertyValue = propertyValue.Substring(1, propertyValue.Length - 2);
            }
        }

        private string GetMapFilePath(FileInfo configFile)
        {
            string mapFilePath = Path.Combine(configFile.DirectoryName, XMapping + DefaultXMapFileExtension);

            if (!File.Exists(mapFilePath))
            {
                Log.LogMessage("Found no XMap file \"{0}\" for configuration file \"{1}\"",
                               mapFilePath, configFile.FullName);
                mapFilePath = null;
            }
            else
            {
                Log.LogMessage("Applying XMap file \"{0}\" to configuration file \"{1}\"",
                               mapFilePath, configFile.FullName);
            }

            return mapFilePath;
        }

        private FileInfo GetPropertyFilePath(string propertySearchStartPath)
        {
            var startDirectory = new DirectoryInfo(propertySearchStartPath);
            FileInfo propertyFile = FindPropertyFile(startDirectory);
            return propertyFile;
        }

        private FileInfo FindPropertyFile(DirectoryInfo startDirectory)
        {
            var fileName = XMapping + DefaultPropertyFileExtension;
            Log.LogMessage(MessageImportance.Low, "Searching for property file named \"{0}\" in directory \"{1}\"", fileName, startDirectory.FullName);
            
            FileInfo propertyFile = null;
            FileInfo[] files = startDirectory.GetFiles(fileName,
                                                       SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                propertyFile = files[0];
            }
            if (propertyFile == null && startDirectory.Parent != null)
            {
                propertyFile = FindPropertyFile(startDirectory.Parent);
            }
            return propertyFile;
        }
    }
}