/**
* Copyright (c) 2017 Joseph Shaw & Big Sky Software LLC
* Distributed under the GNU GPL v2. For full terms see the file LICENSE.txt
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePipeLib.Interfaces;

namespace GamePipeLib.Model.Steam
{
    /// <summary>
    /// This class is designed to handle games which have multiple appIds which lead to the same install directory
    /// A few examples would be Arma 2 (33910 & 33900), Torchlight (41500 & 41520), and Medal Of Honor (47790 & 47830)
    /// </summary>
    //TODO update the contents of the bundle when items are removed? Not a common situation at all, and probably harmless?
    public class SteamBundle : ILocalSteamApplication
    {
        private List<string> _removedAppIds = new List<string>();

        public SteamBundle(IEnumerable<ILocalSteamApplication> apps)
        {
            var includedBundles = apps.OfType<SteamBundle>();
            var nonBundles = apps.Except(includedBundles);
            var appsToBundle = nonBundles.Concat(includedBundles.SelectMany(x => x.AppsInBundle));
            try
            {
                _AppsInBundle = appsToBundle.OrderBy(x => Convert.ToInt32(x.AppId)).ToArray();
            }
            catch (Exception)
            {
                _AppsInBundle = appsToBundle.OrderBy(x => x.AppId).ToArray();
            }
            if (_AppsInBundle.Any(x => x.InstallDir.ToLower() != InstallDir.ToLower()))
                throw new ArgumentException("Not all apps have the same install dir");

            UpdateAppId();
        }

        private ILocalSteamApplication[] _AppsInBundle;
        public IEnumerable<ILocalSteamApplication> AppsInBundle
        {
            get
            {
                return _AppsInBundle;
            }
        }

        private string _AppId;
        public string AppId
        {
            get
            {
                return _AppId;
            }
        }

        public string GameName
        {
            get
            {
                return _AppsInBundle[0].GameName;
            }
        }

        public string ImageUrl
        {
            get
            {
                return _AppsInBundle[0].ImageUrl;
            }
        }

        public string InstallDir
        {
            get
            {
                return _AppsInBundle[0].InstallDir;
            }
        }

        public string GameDir
        {
            get
            {
                return _AppsInBundle[0].GameDir;
            }
        }

        private void UpdateAppId()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var app in _AppsInBundle.Where(x => !_removedAppIds.Contains(x.AppId)))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(app.AppId);
            }
            _AppId = sb.ToString();
        }
    }
}
