﻿using UnityEngine;

namespace ModTool.Shared
{
    /// <summary>
    /// Class for storing general ModTool settings.
    /// </summary>
    public class ModToolSettings : ScriptableSingleton<ModToolSettings>
    {
        /// <summary>
        /// The product name for the project.
        /// </summary>
        public static string productName => instance._productName;

        /// <summary>
        /// The unity version of the project.
        /// </summary>
        public static string unityVersion => instance._unityVersion;

        /// <summary>
        /// The supported platforms for the project.
        /// </summary>
        public static ModPlatform supportedPlatforms => instance._supportedPlatforms;

        /// <summary>
        /// The types of content that are supported for the project.
        /// </summary>
        public static ModContent supportedContent => instance._supportedContent;

        /// <summary>
        /// Log filter level for the project.
        /// </summary>        
        public static LogLevel logLevel => instance._logLevel;

        [HideInInspector]
        [SerializeField]
        private string _productName;

        [HideInInspector]
        [SerializeField]
        private string _unityVersion;
        
        [HideInInspector]
        [SerializeField]
        private ModPlatform _supportedPlatforms = ModPlatform.Android | ModPlatform.Linux | ModPlatform.OSX | ModPlatform.Windows;

        [HideInInspector]
        [SerializeField]
        private ModContent _supportedContent = ModContent.Code | ModContent.Assets | ModContent.Scenes;

        [HideInInspector]
        [SerializeField]
        private LogLevel _logLevel = LogLevel.Info;
        
        void OnEnable()
        {
            if (string.IsNullOrEmpty(_productName))
                _productName = Application.productName;

            if (string.IsNullOrEmpty(_unityVersion))
                _unityVersion = Application.unityVersion;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            GetInstance();
        }

        public static bool VerifyVersion()
        {
            if (string.IsNullOrEmpty(unityVersion))
            {
                return false;
            }
            return unityVersion.Split('.')[0] == Application.unityVersion.Split('.')[0];
        }

    }
}
