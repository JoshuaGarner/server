﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;
using UlteriusPluginBase;
using UlteriusServer.Utilities;

#endregion

namespace UlteriusServer.Plugins
{
    public class PluginLoader
    {
        public static readonly string PluginPath = Path.Combine(AppEnvironment.DataPath, "Plugins");



        public static List<string> BrokenPlugins = new List<string>();


        public static List<PluginBase> LoadPlugins()
        {
            Console.WriteLine(PluginPath);
            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
                return null;
            }

            var plugins = new List<PluginBase>();
            var installedPlugins = Directory.GetFiles(PluginPath, "*.plugin.dll").ToList();
            foreach (var installedPlugin in installedPlugins)
            {
                var permissionSet = new PermissionSet(PermissionState.None);
                var manifestPath = installedPlugin.Replace(".plugin.dll", ".json");
                if (File.Exists(manifestPath))
                {
                    var manifest = File.ReadAllText(manifestPath);
                    var o = JObject.Parse(manifest);
                    IList<string> keys = o.Properties().Select(p => p.Name).ToList();
                    var perms = new List<string>();
                    foreach (var permission in keys.SelectMany(key => o[key]["Permissions"]))
                    {
                        perms.Add(permission.ToString());
                        permissionSet.AddPermission(PluginPermissions.GetPermissionByName(permission.ToString()));
                    }
                    try
                    {
                        var pluginMan = PluginManager.GetInstance(permissionSet);

                        var plugin = pluginMan.LoadPlugin(Path.GetFullPath(installedPlugin));
                        if (plugin.GUID.ToString() != Guid.Empty.ToString())
                        {
                            plugins.Add(plugin);
                            PluginHandler.PluginPermissions[plugin.GUID.ToString()] = perms;
                        }
                        else
                        {
                            BrokenPlugins.Add(installedPlugin + "|" + "Missing GUID");
                        }
                    }
                    catch (Exception e)
                    {
                        BrokenPlugins.Add(installedPlugin + "|" + e.Message);
                    }
                }
                else
                {
                    BrokenPlugins.Add(installedPlugin + "|" + "No Manifest");
                }
            }
            return plugins;
        }
    }
}