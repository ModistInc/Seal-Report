﻿//
// Copyright (c) Seal Report (sealreport@gmail.com), http://www.sealreport.org.
// Licensed under the Seal Report Dual-License version 1.0; you may not use this file except in compliance with the License described at https://github.com/ariacom/Seal-Report.
//
using RazorEngine.Templating;
using Seal.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Seal.Model
{
    /// <summary>
    /// The SecurityProvider defines how the login is done and the security groups are added to the user
    /// </summary>
    public class SecurityProvider
    {
        /// <summary>
        /// Name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current file path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Last modification date time
        /// </summary>
        public DateTime LastModification;

        /// <summary>
        /// Razor script used to perform the login
        /// </summary>
        public string Script { get; set; } = "";

        /// <summary>
        /// Razor script for the 2 Factor Authentication, executed after the login script
        /// </summary>
        public string TwoFAScript { get; set; } = "";

        /// <summary>
        /// Property not used anymore
        /// </summary>
        public bool PromptUserPassword { get; set; } = false;

        /// <summary>
        /// List of SecurityParameter
        /// </summary>
        public List<SecurityParameter> Parameters { get; set; } = new List<SecurityParameter>();

        /// <summary>
        /// Current Configuration
        /// </summary>
        public string Configuration
        {
            get
            {
                string result = "";
                try
                {
                    StreamReader sr = new StreamReader(FilePath);
                    result = sr.ReadToEnd();
                    sr.Close();
                    LastModification = File.GetLastWriteTime(FilePath);
                }
                catch (Exception ex)
                {
                    Error = ex.Message;
                }
                return result;
            }
        }

        /// <summary>
        /// Current error
        /// </summary>
        public string Error { get; set; } = "";

        /// <summary>
        /// List of available SecurityProvider
        /// </summary>
        public static List<SecurityProvider> LoadProviders(string providerFolder)
        {
            List<SecurityProvider> providers = new List<SecurityProvider>();
            foreach (var path in Directory.GetFiles(providerFolder, "*.cshtml"))
            {
                SecurityProvider provider = new SecurityProvider() { Name = Path.GetFileNameWithoutExtension(path) };
                provider.FilePath = path;
                providers.Add(provider);
                provider.ParseConfiguration();
            }
            return providers;
        }

        public void ClearConfiguration()
        {
            Parameters.Clear();
        }


        public string _lastConfiguration = ""; //Cache to avoid re-compilation -> public for Cloning used in Repository.CreateFast()
        public void ParseConfiguration()
        {
            //Parse the file to init the provider
            try
            {
                string configuration = Configuration;
                if (configuration.Replace("\r\n", "\n") != _lastConfiguration.Replace("\r\n", "\n"))
                {
                    ClearConfiguration();
                    RazorHelper.CompileExecute(configuration, this, GetType().Name + "_" + Name, LastModification);
                    _lastConfiguration = configuration;
                }
            }
            catch (TemplateCompilationException ex)
            {
                Error = Helper.GetExceptionMessage(ex);
            }
            catch (Exception ex)
            {
                Error = string.Format("Unexpected error got when parsing security provider.\r\n{0}", ex.Message);
            }
        }
    }
}
