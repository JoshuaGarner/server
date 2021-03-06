﻿#region

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using UlteriusServer.Authentication;
using UlteriusServer.Utilities;
using UlteriusServer.Utilities.Extensions;
using UlteriusServer.Utilities.Security;

#endregion

namespace UlteriusServer.TaskServer

{
    public class Packets
    {
        public List<object> Args = new List<object>();
        private bool encryptionOff = false;

        //  public List<object> args;
        public string Endpoint;
        public PacketType PacketType;
        public string SyncKey;

        public Packets(AuthClient client, string packetData)
        {
            //An entire base64 string is an aes encrypted packet
            if ((bool) Settings.Get("TaskServer").Encryption)
            {
                if (packetData.IsBase64String())
                {
                    try
                    {
                        var keybytes = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(client.AesKey));
                        var iv = Encoding.UTF8.GetBytes(Rsa.SecureStringToString(client.AesIv));
                        var packet = Convert.FromBase64String(packetData);
                        packetData = UlteriusAes.Decrypt(packet, keybytes, iv);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);

                        PacketType = PacketType.InvalidOrEmptyPacket;
                        return;
                    }
                }
                else
                {
                    //the only non encrypted packet allowed is the first handshake
                    try
                    {
                        var validHandshake = JObject.Parse(packetData);
                        var endpoint = validHandshake["endpoint"].ToString().Trim().ToLower();
                        if (!endpoint.Equals("aeshandshake"))
                        {
                            PacketType = PacketType.InvalidOrEmptyPacket;
                            return;
                        }
                        //prevent sending a new aes key pair after a handshake has already taken place
                        if (client.AesShook)
                        {
                            PacketType = PacketType.InvalidOrEmptyPacket;
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        PacketType = PacketType.InvalidOrEmptyPacket;
                        return;
                    }
                }
            }


            JObject deserializedPacket = null;
            try
            {
                deserializedPacket = JObject.Parse(packetData);
            }
            catch (Exception)
            {
                PacketType = PacketType.InvalidOrEmptyPacket;
                return;
            }
            if (deserializedPacket != null)
            {
                try
                {
                    Endpoint = deserializedPacket["endpoint"].ToString().Trim().ToLower();
                }
                catch (Exception)
                {
                    PacketType = PacketType.InvalidOrEmptyPacket;
                    return;
                }
                try
                {
                    SyncKey = deserializedPacket["synckey"].ToString().Trim();
                }
                catch (Exception)
                {
                    SyncKey = null;
                }
                try
                {
                    Args.AddRange(JArray.Parse(deserializedPacket["args"].ToString()));
                }
                catch (Exception)
                {
                    // no args
                }
                switch (Endpoint)
                {
                    case "authenticate":
                        PacketType = PacketType.Authenticate;
                        break;
                    case "requestgpuinformation":
                        PacketType = PacketType.RequestGpuInformation;
                        break;
                    case "createfiletree":
                        PacketType = PacketType.CreateFileTree;
                        break;
                    case "requestprocessinformation":
                        PacketType = PacketType.RequestProcess;
                        break;
                    case "requestcpuinformation":
                        PacketType = PacketType.RequestCpuInformation;
                        break;
                    case "requestosinformation":
                        PacketType = PacketType.RequestOsInformation;
                        break;
                    case "requestnetworkinformation":
                        PacketType = PacketType.RequestNetworkInformation;
                        break;
                    case "requestsysteminformation":
                        PacketType = PacketType.RequestSystemInformation;
                        break;
                    case "startprocess":
                        PacketType = PacketType.StartProcess;
                        break;
                    case "killprocess":
                        PacketType = PacketType.KillProcess;
                        break;
                    case "generatenewkey":
                        PacketType = PacketType.GenerateNewKey;
                        break;
                    case "togglewebserver":
                        PacketType = PacketType.UseWebServer;
                        break;
                    case "changewebserverport":
                        PacketType = PacketType.ChangeWebServerPort;
                        break;
                    case "changewebfilepath":
                        PacketType = PacketType.ChangeWebFilePath;
                        break;
                    case "startscreenshare":
                        PacketType = PacketType.StartScreenShare;
                        break;
                    case "checkscreenshare":
                        PacketType = PacketType.CheckScreenShare;
                        break;
                    case "stopscreenshare":
                        PacketType = PacketType.StopScreenShare;
                        break;
                    case "changescreensharepass":
                        PacketType = PacketType.ChangeScreenSharePass;
                        break;
                    case "changetaskserverport":
                        PacketType = PacketType.ChangeTaskServerPort;
                        break;
                    case "changescreenshareport":
                        PacketType = PacketType.ChangeScreenSharePort;
                        break;
                    case "changenetworkresolve":
                        PacketType = PacketType.ChangeNetworkResolve;
                        break;
                    case "changeloadplugins":
                        PacketType = PacketType.ChangeLoadPlugins;
                        break;
                    case "changeuseterminal":
                        PacketType = PacketType.ChangeUseTerminal;
                        break;
                    case "getcurrentsettings":
                        PacketType = PacketType.GetCurrentSettings;
                        break;
                    case "geteventlogs":
                        PacketType = PacketType.GetEventLogs;
                        break;
                    case "checkforupdate":
                        PacketType = PacketType.CheckUpdate;
                        break;
                    case "restartserver":
                        PacketType = PacketType.RestartServer;
                        break;
                    case "getwindowsdata":
                        PacketType = PacketType.RequestWindowsInformation;
                        break;
                    case "getactivewindowssnapshots":
                        PacketType = PacketType.GetActiveWindowsSnapshots;
                        break;
                    case "plugin":
                        PacketType = PacketType.Plugin;
                        break;
                    case "getplugins":
                        PacketType = PacketType.GetPlugins;
                        break;
                    case "getbadplugins":
                        PacketType = PacketType.GetBadPlugins;
                        break;
                    case "startcamera":
                        PacketType = PacketType.StartCamera;
                        break;
                    case "stopcamera":
                        PacketType = PacketType.StopCamera;
                        break;
                    case "pausecamera":
                        PacketType = PacketType.PauseCamera;
                        break;
                    case "getcameras":
                        PacketType = PacketType.GetCameras;
                        break;
                    case "getcameraframe":
                        PacketType = PacketType.GetCameraFrame;
                        break;
                    case "startcamerastream":
                        PacketType = PacketType.StartCameraStream;
                        break;
                    case "stopcamerastream":
                        PacketType = PacketType.StopCameraStream;
                        break;
                    case "refreshcameras":
                        PacketType = PacketType.RefreshCameras;
                        break;
                    case "approvefile":
                        PacketType = PacketType.ApproveFile;
                        break;
                    case "requestfile":
                        PacketType = PacketType.RequestFile;
                        break;
                    case "aeshandshake":
                        PacketType = PacketType.AesHandshake;
                        break;
                    case "approveplugin":
                        PacketType = PacketType.ApprovePlugin;
                        break;
                    case "getpendingplugins":
                        PacketType = PacketType.GetPendingPlugins;
                        break;
                    case "removefile":
                        PacketType = PacketType.RemoveFile;
                        break;
                    case "searchfiles":
                        PacketType = PacketType.SearchFiles;
                        break;
                    default:
                        PacketType = PacketType.InvalidOrEmptyPacket;
                        break;
                }
            }
        }
    }
}

public enum PacketType
{
    RequestProcess,
    RequestCpuInformation,
    RequestOsInformation,
    RequestNetworkInformation,
    RequestSystemInformation,
    StartProcess,
    KillProcess,
    GenerateNewKey,
    EmptyApiKey,
    InvalidApiKey,
    InvalidOrEmptyPacket,
    UseWebServer,
    ChangeWebServerPort,
    ChangeWebFilePath,
    ChangeTaskServerPort,
    ChangeNetworkResolve,
    GetCurrentSettings,
    GetEventLogs,
    CheckUpdate,
    RequestWindowsInformation,
    RestartServer,
    GetActiveWindowsSnapshots,
    Authenticate,
    CreateFileTree,
    RequestGpuInformation,
    Plugin,
    ChangeScreenSharePort,
    StartScreenShare,
    ChangeScreenSharePass,
    GetPlugins,
    GetBadPlugins,
    PauseCamera,
    StopCamera,
    StartCamera,
    GetCameras,
    GetCameraFrame,
    RefreshCameras,
    StartCameraStream,
    StopCameraStream,
    ChangeLoadPlugins,
    ChangeUseTerminal,
    AesHandshake,
    ApprovePlugin,
    GetPendingPlugins,
    ApproveFile,
    RequestFile,
    RemoveFile,
    SearchFiles,
    StopScreenShare,
    CheckScreenShare
}