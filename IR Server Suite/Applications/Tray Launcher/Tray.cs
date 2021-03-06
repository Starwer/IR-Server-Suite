#region Copyright (C) 2005-2009 Team MediaPortal

// Copyright (C) 2005-2009 Team MediaPortal
// http://www.team-mediaportal.com
// 
// This Program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2, or (at your option)
// any later version.
// 
// This Program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with GNU Make; see the file COPYING.  If not, write to
// the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
// http://www.gnu.org/copyleft/gpl.html

#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using IrssComms;
using IrssUtils;
using TrayLauncher.Properties;

namespace TrayLauncher
{
  /// <summary>
  /// Tray launcher main class.
  /// </summary>
  internal class Tray
  {
    #region Constants

    private const string ProcessCommandThreadName = "ProcessCommand";

    private static readonly string ConfigurationDir = Path.Combine(Common.FolderAppData,
                                                                   "Tray Launcher");

    private static readonly string ConfigurationFile = Path.Combine(ConfigurationDir,
                                                                    "Tray Launcher.xml");

    #endregion Constants

    #region Variables

    private static ClientMessageSink _handleMessage;

    private static bool _registered;
    private readonly Container _container;
    private readonly NotifyIcon _notifyIcon;

    private Client _client;
    private bool _inConfiguration;

    private string _serverHost;
    private string _programFile;
    private bool _autoRun;
    private bool _launchOnLoad;
    private bool _oneInstanceOnly;
    private bool _repeatsFocus;
    private string _launchKeyCode;

    #endregion Variables

    #region Properties

    internal static ClientMessageSink HandleMessage
    {
      get { return _handleMessage; }
      set { _handleMessage = value; }
    }

    internal static bool Registered
    {
      get { return _registered; }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Tray"/> class.
    /// </summary>
    public Tray()
    {
      ContextMenuStrip contextMenu = new ContextMenuStrip();
      contextMenu.Items.Add(new ToolStripLabel("Tray Launcher"));
      contextMenu.Items.Add(new ToolStripSeparator());
      contextMenu.Items.Add(new ToolStripMenuItem("&Launch", null, new EventHandler(ClickLaunch)));
      contextMenu.Items.Add(new ToolStripMenuItem("&Setup", null, new EventHandler(ClickSetup)));
      contextMenu.Items.Add(new ToolStripMenuItem("&Quit", null, new EventHandler(ClickQuit)));

      _container = new Container();

      _notifyIcon = new NotifyIcon(_container);
      _notifyIcon.ContextMenuStrip = contextMenu;
      _notifyIcon.DoubleClick += ClickSetup;

      UpdateTrayIcon("Tray Launcher - Connecting ...", Resources.Icon16Connecting);
    }

    #endregion Constructor

    #region Implementation

    private void UpdateTrayIcon(string text, Icon icon)
    {
      if (String.IsNullOrEmpty(text))
        throw new ArgumentNullException("text");

      if (icon == null)
        throw new ArgumentNullException("icon");

      _notifyIcon.Text = text;
      _notifyIcon.Icon = icon;
    }

    internal bool Start()
    {
      try
      {
        LoadSettings();

        bool didSetup = false;
        if (String.IsNullOrEmpty(_programFile) || String.IsNullOrEmpty(_serverHost))
        {
          if (!Configure())
            return false;

          didSetup = true;
        }

        bool clientStarted = false;

        try
        {
          IPAddress serverIP = Network.GetIPFromName(_serverHost);
          IPEndPoint endPoint = new IPEndPoint(serverIP, Server.DefaultPort);

          clientStarted = StartClient(endPoint);
        }
        catch (Exception ex)
        {
          IrssLog.Error(ex);
          MessageBox.Show("Failed to start IR Server communications, refer to log file for more details.",
                          "Tray Launcher - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          clientStarted = false;
        }

        if (clientStarted)
        {
          _notifyIcon.Visible = true;

          if (!didSetup && _launchOnLoad)
            Launch();

          return true;
        }
        else
        {
          Configure();
        }
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);
      }

      return false;
    }

    private void Stop()
    {
      _notifyIcon.Visible = false;

      try
      {
        if (_registered)
        {
          _registered = false;

          IrssMessage message = new IrssMessage(MessageType.UnregisterClient, MessageFlags.Request);
          _client.Send(message);
        }
      }
      catch
      {
      }

      StopClient();
    }

    private void LoadSettings()
    {
      try
      {
        _autoRun = SystemRegistry.GetAutoRun("Tray Launcher");
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);

        _autoRun = false;
      }

      XmlDocument doc = new XmlDocument();

      try
      {
        doc.Load(ConfigurationFile);
      }
      catch (FileNotFoundException)
      {
        IrssLog.Warn("Configuration file not found, using defaults");

        CreateDefaultSettings();
        return;
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);

        CreateDefaultSettings();
        return;
      }

      try
      {
        _serverHost = doc.DocumentElement.Attributes["ServerHost"].Value;
      }
      catch
      {
        _serverHost = "localhost";
      }
      try
      {
        _programFile = doc.DocumentElement.Attributes["ProgramFile"].Value;
      }
      catch
      {
        _programFile = String.Empty;
      }
      try
      {
        _launchOnLoad = bool.Parse(doc.DocumentElement.Attributes["LaunchOnLoad"].Value);
      }
      catch
      {
        _launchOnLoad = false;
      }
      try
      {
        _oneInstanceOnly = bool.Parse(doc.DocumentElement.Attributes["OneInstanceOnly"].Value);
      }
      catch
      {
        _oneInstanceOnly = true;
      }
      try
      {
        _repeatsFocus = bool.Parse(doc.DocumentElement.Attributes["RepeatsFocus"].Value);
      }
      catch
      {
        _repeatsFocus = true;
      }
      try
      {
        _launchKeyCode = doc.DocumentElement.Attributes["LaunchKeyCode"].Value;
      }
      catch
      {
        _launchKeyCode = "Start";
      }
    }

    private void SaveSettings()
    {
      try
      {
        if (_autoRun)
          SystemRegistry.SetAutoRun("Tray Launcher", Application.ExecutablePath);
        else
          SystemRegistry.RemoveAutoRun("Tray Launcher");
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);
      }

      try
      {
        Directory.CreateDirectory(ConfigurationDir);
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);
      }

      try
      {
        using (XmlTextWriter writer = new XmlTextWriter(ConfigurationFile, Encoding.UTF8))
        {
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 1;
          writer.IndentChar = (char) 9;
          writer.WriteStartDocument(true);
          writer.WriteStartElement("settings"); // <settings>

          writer.WriteAttributeString("ServerHost", _serverHost);
          writer.WriteAttributeString("ProgramFile", _programFile);
          writer.WriteAttributeString("LaunchOnLoad", _launchOnLoad.ToString());
          writer.WriteAttributeString("OneInstanceOnly", _oneInstanceOnly.ToString());
          writer.WriteAttributeString("RepeatsFocus", _repeatsFocus.ToString());
          writer.WriteAttributeString("LaunchKeyCode", _launchKeyCode);

          writer.WriteEndElement(); // </settings>
          writer.WriteEndDocument();
        }
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);
      }
    }

    private void CreateDefaultSettings()
    {
      _serverHost = "localhost";
      _programFile = String.Empty;
      _launchOnLoad = false;
      _oneInstanceOnly = true;
      _repeatsFocus = true;
      _launchKeyCode = "Start";

      SaveSettings();
    }

    private void CommsFailure(object obj)
    {
      Exception ex = obj as Exception;

      if (ex != null)
        IrssLog.Error("Communications failure: {0}", ex.Message);
      else
        IrssLog.Error("Communications failure");

      StopClient();

      MessageBox.Show("Please report this error.", "Tray Launcher - Communications failure", MessageBoxButtons.OK,
                      MessageBoxIcon.Error);
    }

    private void Connected(object obj)
    {
      IrssLog.Info("Connected to server");

      UpdateTrayIcon("Tray Launcher", Resources.Icon16);

      IrssMessage message = new IrssMessage(MessageType.RegisterClient, MessageFlags.Request);
      _client.Send(message);
    }

    private void Disconnected(object obj)
    {
      IrssLog.Warn("Communications with server has been lost");

      UpdateTrayIcon("Tray Launcher - Re-Connecting ...", Resources.Icon16Connecting);

      Thread.Sleep(1000);
    }

    private bool StartClient(IPEndPoint endPoint)
    {
      if (_client != null)
        return false;

      ClientMessageSink sink = ReceivedMessage;

      _client = new Client(endPoint, sink);
      _client.CommsFailureCallback = CommsFailure;
      _client.ConnectCallback = Connected;
      _client.DisconnectCallback = Disconnected;

      if (_client.Start())
      {
        return true;
      }
      else
      {
        _client = null;
        return false;
      }
    }

    private void StopClient()
    {
      if (_client == null)
        return;

      _client.Dispose();
      _client = null;

      _registered = false;
    }

    private void ReceivedMessage(IrssMessage received)
    {
      IrssLog.Debug("Received Message \"{0}\"", received.Type);

      try
      {
        switch (received.Type)
        {
          case MessageType.RegisterClient:
            if ((received.Flags & MessageFlags.Success) == MessageFlags.Success)
            {
              //_irServerInfo = IRServerInfo.FromBytes(received.DataAsBytes);
              _registered = true;

              IrssLog.Info("Registered to IR Server");
            }
            else if ((received.Flags & MessageFlags.Failure) == MessageFlags.Failure)
            {
              _registered = false;
              IrssLog.Warn("IR Server refused to register");
            }
            break;

          case MessageType.RemoteEvent:
            string deviceName = received.MessageData[IrssMessage.DEVICE_NAME] as string;
            string keyCode = received.MessageData[IrssMessage.KEY_CODE] as string;

            RemoteHandlerCallback(deviceName, keyCode);
            break;

          case MessageType.ServerShutdown:
            IrssLog.Warn("IR Server Shutdown - Tray Launcher disabled until IR Server returns");
            _registered = false;
            break;

          case MessageType.Error:
            IrssLog.Error("Received error: {0}", received.GetDataAsString());
            break;
        }

        if (_handleMessage != null)
          _handleMessage(received);
      }
      catch (Exception ex)
      {
        IrssLog.Error("ReceivedMessage(): {0}", ex.ToString());
      }
    }

    private void RemoteHandlerCallback(string deviceName, string keyCode)
    {
      IrssLog.Info("Remote Event: {0}", keyCode);

      if (keyCode.Equals(_launchKeyCode, StringComparison.Ordinal))
        Launch();
    }

    private bool Configure()
    {
      Setup setup = new Setup();

      setup.AutoRun = _autoRun;
      setup.ServerHost = _serverHost;
      setup.ProgramFile = _programFile;
      setup.LaunchOnLoad = _launchOnLoad;
      setup.OneInstanceOnly = _oneInstanceOnly;
      setup.RepeatsFocus = _repeatsFocus;
      setup.LaunchKeyCode = _launchKeyCode;

      if (setup.ShowDialog() == DialogResult.OK)
      {
        _autoRun = setup.AutoRun;
        _serverHost = setup.ServerHost;
        _programFile = setup.ProgramFile;
        _launchOnLoad = setup.LaunchOnLoad;
        _oneInstanceOnly = setup.OneInstanceOnly;
        _repeatsFocus = setup.RepeatsFocus;
        _launchKeyCode = setup.LaunchKeyCode;

        SaveSettings();

        return true;
      }

      return false;
    }

    private void ClickSetup(object sender, EventArgs e)
    {
      if (_inConfiguration)
      {
        IrssLog.Info("Setup clicked, but configuration is already open.");
        return;
      }

      _inConfiguration = true;

      if (Configure())
      {
        Stop();
        Thread.Sleep(500);
        Start();
      }

      _inConfiguration = false;
    }

    private void ClickLaunch(object sender, EventArgs e)
    {
      IrssLog.Info("Launch");

      Launch();
    }

    private void ClickQuit(object sender, EventArgs e)
    {
      if (_inConfiguration)
      {
        IrssLog.Info("Can't close application, because configuration is still open.");
        return;
      }

      Stop();

      Application.Exit();
    }

    private void Launch()
    {
      if (_inConfiguration)
      {
        IrssLog.Info("Can't launch target application, in Configuration");
        return;
      }

      try
      {
        // Check for multiple instances
        if (_oneInstanceOnly)
        {
          foreach (Process process in Process.GetProcesses())
          {
            try
            {
              if (Path.GetFileName(process.MainModule.ModuleName).Equals(Path.GetFileName(_programFile),
                                                                         StringComparison.OrdinalIgnoreCase))
              {
                IrssLog.Info("Can't launch target application, program already running");
                if (_repeatsFocus)
                {
                  IrssLog.Info("Attempting to give focus to target application ...");

                  FocusForcer forcer = new FocusForcer(process.Id);
                  forcer.ForceOnce();
                }
                return;
              }
            }
            catch (Win32Exception ex)
            {
              if (ex.ErrorCode != -2147467259) // Ignore "Unable to enumerate the process modules" errors.
                IrssLog.Error(ex);
            }
            catch (Exception ex)
            {
              IrssLog.Error(ex);
            }
          }
        }

        IrssLog.Info("Launching \"{0}\" ...", _programFile);

        string[] launchCommand = new string[]
                                   {
                                     _programFile,
                                     Path.GetDirectoryName(_programFile),
                                     String.Empty,
                                     Enum.GetName(typeof (ProcessWindowStyle), ProcessWindowStyle.Normal),
                                     false.ToString(),
                                     true.ToString(),
                                     false.ToString(),
                                     true.ToString()
                                   };

        Common.ProcessRunCommand(launchCommand);
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex);
        MessageBox.Show(ex.Message, "Tray Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion Implementation
  }
}