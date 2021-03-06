﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace InventorConfig
{
    public class ConfigEngine
    {
        private string _configRaw;
        private bool _closeInventorAfterCompletion;
        private Configuration Config { get; set; }
        private Inventor.Application App { get; set; }

        public void LoadConfig(string _configPath, bool test = false)
        {
            _configRaw = GetFileContents(_configPath);
            Config = DeserializeConfiguration();

            if (test)
                return;

            DecideIfWeShouldCloseInventorAfterCompletion();
            App = GetInventorInstance();
            Config.LoadConfigurationIntoInventor(App);
            CloseInventorIfRequired();
        }

        public void WriteConfig(string _configPath)
        {
            DecideIfWeShouldCloseInventorAfterCompletion();
            App = GetInventorInstance();

            Config = new Configuration();
            Config.GetConfigurationFromInventor(App);
            SerializeConfiguration(Config, _configPath);

            CloseInventorIfRequired();
        }

        private string GetFileContents(string _configPath)
        {
            try
            {
                return File.ReadAllText(_configPath);
            }
            catch (Exception e)
            {
                throw new SystemException("There was an error reading the json configuration file from disk.  Process was aborted, press any key to continue...", e);
            }
        }

        private Configuration DeserializeConfiguration()
        {
            try
            {
                return JsonConvert.DeserializeObject< Configuration>(_configRaw);
            }
            catch (Exception e)
            {
                throw new SystemException("The configuration was invalid, please verify json file syntax.  Process was aborted, press any key to continue...", e);
            }
        }

        private Inventor.Application GetInventorInstance()
        {
            try
            {
                var i = InventorInstance.GetInventorAppReference();
                return i;
            }
            catch (Exception e)
            {
                throw new SystemException("The Inventor application could not be started on this computer.  Is it installed?  Process aborted, press any key to continue...", e);
            }
        }

        private void DecideIfWeShouldCloseInventorAfterCompletion()
        {
            if (InventorInstance.NumberOfRunningInventorInstances() == 0)
            { _closeInventorAfterCompletion = true; }
        }

        private void CloseInventorIfRequired()
        {
            if (_closeInventorAfterCompletion)
            {
                App.Quit();
                GC.WaitForPendingFinalizers();
            }
        }


        private void SerializeConfiguration(Configuration config, string outputFile)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            StreamWriter sw = new StreamWriter(outputFile);
            JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, config);
            writer.Close();
        }
    }
}