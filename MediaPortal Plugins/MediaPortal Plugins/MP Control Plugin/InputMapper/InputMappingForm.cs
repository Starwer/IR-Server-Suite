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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using IrssUtils;
using IrssUtils.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.Plugins
{
  /// <summary>
  /// Summary description for ButtonMappingForm.
  /// </summary>
  internal class InputMappingForm : Form
  {
    private readonly ArrayList actionList = new ArrayList();
    private readonly List<string> _pluginList = new List<string>();

    private readonly string[] fullScreenList = new string[] {"Fullscreen", "No Fullscreen"};
    private readonly string inputClassName;
    private readonly string[] layerList = new string[] {"all", "1", "2"};
    private readonly Array nativeActionList = Enum.GetValues(typeof (Action.ActionType));

    private readonly string[] nativePlayerList = new string[] {"TV", "DVD", "MEDIA"};

    private readonly string[] nativePowerList = new string[] {"EXIT", "REBOOT", "SHUTDOWN", "STANDBY", "HIBERNATE"};

    private readonly string[] nativeProcessList = new string[] {"CLOSE", "KILL"};
    private readonly Array nativeWindowsList = Enum.GetValues(typeof (GUIWindow.Window));
    private readonly string[] playerList = new string[] {"TV is running", "DVD is playing", "Media is playing"};

    private readonly string[] powerList = new string[]
                                            {
                                              "Exit MediaPortal", "Reboot Windows", "Shutdown Windows",
                                              "Standby Windows"
                                              , "Hibernate Windows"
                                            };

    private readonly string[] processList = new string[] {"Close Process", "Kill Process"};

    private readonly string[] soundList = new string[] {"none", "back.wav", "click.wav", "cursor.wav"};
    private readonly ArrayList windowsList = new ArrayList();
    private readonly ArrayList windowsListFiltered = new ArrayList();

    private bool changedSettings;
    private MPRadioButton radioButtonPlugin;


    /// <summary>
    /// Required designer variable.
    /// </summary>
    //private Container components;
    private string[] keyList = new string[]
                                 {
                                   "{BACKSPACE}", "{BREAK}", "{CAPSLOCK}", "{DELETE}", "{DOWN}", "{END}", "{ENTER}",
                                   "{ESC}",
                                   "{HELP}", "{HOME}", "{INSERT}", "{LEFT}", "{NUMLOCK}", "{PGDN}", "{PGUP}", "{PRTSC}",
                                   "{RIGHT}", "{SCROLLLOCK}", "{TAB}", "{UP}", "{F1}", "{F2}", "{F3}", "{F4}", "{F5}",
                                   "{F6}",
                                   "{F7}", "{F8}", "{F9}", "{F10}", "{F11}", "{F12}", "{F13}", "{F14}", "{F15}", "{F16}"
                                   ,
                                   "{ADD}", "{SUBTRACT}", "{MULTIPLY}", "{DIVIDE}"
                                 };

    public InputMappingForm(string name)
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      foreach (GUIWindow.Window wnd in nativeWindowsList)
      {
        if (wnd.ToString().IndexOf("DIALOG") == -1)
          switch ((int) Enum.Parse(typeof (GUIWindow.Window), wnd.ToString()))
          {
            case (int) GUIWindow.Window.WINDOW_ARTIST_INFO:
            case (int) GUIWindow.Window.WINDOW_DIALOG_DATETIME:
            case (int) GUIWindow.Window.WINDOW_DIALOG_EXIF:
            case (int) GUIWindow.Window.WINDOW_DIALOG_FILE:
            case (int) GUIWindow.Window.WINDOW_DIALOG_FILESTACKING:
            case (int) GUIWindow.Window.WINDOW_DIALOG_MENU:
            case (int) GUIWindow.Window.WINDOW_DIALOG_MENU_BOTTOM_RIGHT:
            case (int) GUIWindow.Window.WINDOW_DIALOG_NOTIFY:
            case (int) GUIWindow.Window.WINDOW_DIALOG_OK:
            case (int) GUIWindow.Window.WINDOW_DIALOG_PROGRESS:
            case (int) GUIWindow.Window.WINDOW_DIALOG_RATING:
            case (int) GUIWindow.Window.WINDOW_DIALOG_SELECT:
            case (int) GUIWindow.Window.WINDOW_DIALOG_SELECT2:
            case (int) GUIWindow.Window.WINDOW_DIALOG_TEXT:
            case (int) GUIWindow.Window.WINDOW_DIALOG_TVGUIDE:
            case (int) GUIWindow.Window.WINDOW_DIALOG_YES_NO:
            case (int) GUIWindow.Window.WINDOW_INVALID:
            case (int) GUIWindow.Window.WINDOW_MINI_GUIDE:
            case (int) GUIWindow.Window.WINDOW_TV_CROP_SETTINGS:
            case (int) GUIWindow.Window.WINDOW_MSNOSD:
            case (int) GUIWindow.Window.WINDOW_MUSIC:
            case (int) GUIWindow.Window.WINDOW_MUSIC_COVERART_GRABBER_RESULTS:
            case (int) GUIWindow.Window.WINDOW_MUSIC_INFO:
            case (int) GUIWindow.Window.WINDOW_OSD:
            case (int) GUIWindow.Window.WINDOW_TOPBAR:
              //case (int)GUIWindow.Window.WINDOW_TOPBARHOME:
            case (int) GUIWindow.Window.WINDOW_TVMSNOSD:
            case (int) GUIWindow.Window.WINDOW_TVOSD:
            case (int) GUIWindow.Window.WINDOW_TVZAPOSD:
            case (int) GUIWindow.Window.WINDOW_VIDEO_ARTIST_INFO:
            case (int) GUIWindow.Window.WINDOW_VIDEO_INFO:
            case (int) GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD:
            case (int) GUIWindow.Window.WINDOW_VIRTUAL_WEB_KEYBOARD:
              break;
            default:
              windowsListFiltered.Add(GetFriendlyName(wnd.ToString()));
              break;
          }
        windowsList.Add(GetFriendlyName(wnd.ToString()));
      }

      foreach (Action.ActionType actn in nativeActionList)
        actionList.Add(GetFriendlyName(actn.ToString()));

      LoadPluginList();

      comboBoxSound.DataSource = soundList;
      comboBoxLayer.DataSource = layerList;
      inputClassName = name;
      LoadMapping(inputClassName + ".xml", false);
      headerLabel.Caption = inputClassName;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    //protected override void Dispose(bool disposing)
    //{
    //  if (disposing)
    //  {
    //    if (components != null)
    //    {
    //      components.Dispose();
    //    }
    //  }
    //  base.Dispose(disposing);
    //}
    private void CloseThread()
    {
      Thread.Sleep(200);
      Close();
    }

    private void LoadMapping(string xmlFile, bool defaults)
    {
      try
      {
        groupBoxLayer.Enabled = false;
        groupBoxCondition.Enabled = false;
        groupBoxAction.Enabled = false;
        treeMapping.Nodes.Clear();
        XmlDocument doc = new XmlDocument();
        string path = Config.GetFolder(Config.Dir.Base) + "\\InputDeviceMappings\\defaults\\" + xmlFile;
        if (!defaults && File.Exists(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile)))
          path = Config.GetFile(Config.Dir.CustomInputDevice, xmlFile);
        if (!File.Exists(path))
        {
          MessageBox.Show(
            "Can't locate mapping file " + xmlFile + "\n\nMake sure it exists in /InputDeviceMappings/defaults",
            "Mapping file missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
          buttonUp.Enabled =
            buttonDown.Enabled =
            buttonNew.Enabled = buttonRemove.Enabled = buttonDefault.Enabled = buttonApply.Enabled = false;
          ShowInTaskbar = true;
          WindowState = FormWindowState.Minimized;
          Thread closeThread = new Thread(CloseThread);
          closeThread.Start();
          return;
        }
        doc.Load(path);
        XmlNodeList listRemotes = doc.DocumentElement.SelectNodes("/mappings/remote");

        foreach (XmlNode nodeRemote in listRemotes)
        {
          TreeNode remoteNode = new TreeNode(nodeRemote.Attributes["family"].Value);
          remoteNode.Tag = new Data("REMOTE", null, nodeRemote.Attributes["family"].Value);
          XmlNodeList listButtons = nodeRemote.SelectNodes("button");
          foreach (XmlNode nodeButton in listButtons)
          {
            TreeNode buttonNode = new TreeNode(nodeButton.Attributes["name"].Value);
            buttonNode.Tag = new Data("BUTTON", nodeButton.Attributes["name"].Value, nodeButton.Attributes["code"].Value);
            remoteNode.Nodes.Add(buttonNode);

            TreeNode layer1Node = new TreeNode("Layer 1");
            TreeNode layer2Node = new TreeNode("Layer 2");
            TreeNode layerAllNode = new TreeNode("All Layers");
            layer1Node.Tag = new Data("LAYER", null, "1");
            layer2Node.Tag = new Data("LAYER", null, "2");
            layerAllNode.Tag = new Data("LAYER", null, "0");
            layer1Node.ForeColor = Color.DimGray;
            layer2Node.ForeColor = Color.DimGray;
            layerAllNode.ForeColor = Color.DimGray;

            XmlNodeList listActions = nodeButton.SelectNodes("action");

            foreach (XmlNode nodeAction in listActions)
            {
              string conditionString = String.Empty;
              string commandString = String.Empty;

              string condition = nodeAction.Attributes["condition"].Value.ToUpper();
              string conProperty = nodeAction.Attributes["conproperty"].Value; // .ToUpper()
              string command = nodeAction.Attributes["command"].Value.ToUpper();
              string cmdProperty = nodeAction.Attributes["cmdproperty"].Value; // .ToUpper()
              string sound = String.Empty;
              XmlAttribute soundAttribute = nodeAction.Attributes["sound"];
              if (soundAttribute != null)
                sound = soundAttribute.Value;
              bool gainFocus = false;
              XmlAttribute focusAttribute = nodeAction.Attributes["focus"];
              if (focusAttribute != null)
                gainFocus = Convert.ToBoolean(focusAttribute.Value);
              int layer = Convert.ToInt32(nodeAction.Attributes["layer"].Value);

              #region Conditions

              switch (condition)
              {
                case "WINDOW":
                  {
                    conProperty = conProperty.ToUpper();
                    try
                    {
                      conditionString =
                        GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(conProperty)));
                    }
                    catch
                    {
                      conditionString = conProperty;
                    }
                    break;
                  }
                case "FULLSCREEN":
                  conProperty = conProperty.ToUpper();
                  if (conProperty == "TRUE")
                    conditionString = "Fullscreen";
                  else
                    conditionString = "No Fullscreen";
                  break;
                case "PLAYER":
                  conProperty = conProperty.ToUpper();
                  conditionString = playerList[Array.IndexOf(nativePlayerList, conProperty)];
                  break;
                case "PLUGIN":
                  conditionString = conProperty;
                  break;
                case "*":
                  conditionString = "No Condition";
                  break;
              }

              #endregion

              #region Commands

              switch (command)
              {
                case "ACTION":
                  commandString = "Action \"" +
                                  GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(cmdProperty))) +
                                  "\"";
                  break;
                case "KEY":
                  commandString = "Key \"" + cmdProperty + "\"";
                  break;
                case "WINDOW":
                  {
                    try
                    {
                      commandString = "Window \"" +
                                      GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window),
                                                                   Convert.ToInt32(cmdProperty))) + "\"";
                    }
                    catch
                    {
                      commandString = "Window \"" + cmdProperty + "\"";
                    }
                    break;
                  }
                case "TOGGLE":
                  commandString = "Toggle Layer";
                  break;
                case "POWER":
                  commandString = powerList[Array.IndexOf(nativePowerList, cmdProperty)];
                  break;
                case "PROCESS":
                  commandString = processList[Array.IndexOf(nativeProcessList, cmdProperty)];
                  break;
                case "BLAST":
                  commandString = cmdProperty;
                  break;
              }

              #endregion

              TreeNode conditionNode = new TreeNode(conditionString);
              conditionNode.Tag = new Data("CONDITION", condition, conProperty);
              if (commandString == "Action \"Key Pressed\"")
              {
                string cmdKeyChar = nodeAction.Attributes["cmdkeychar"].Value;
                string cmdKeyCode = nodeAction.Attributes["cmdkeycode"].Value;
                TreeNode commandNode = new TreeNode(String.Format("Key Pressed: {0} [{1}]", cmdKeyChar, cmdKeyCode));

                Key key = new Key(Convert.ToInt32(cmdKeyChar), Convert.ToInt32(cmdKeyCode));

                commandNode.Tag = new Data("COMMAND", "KEY", key, gainFocus);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }
              else
              {
                TreeNode commandNode = new TreeNode(commandString);
                commandNode.Tag = new Data("COMMAND", command, cmdProperty, gainFocus);
                commandNode.ForeColor = Color.DarkGreen;
                conditionNode.ForeColor = Color.Blue;
                conditionNode.Nodes.Add(commandNode);
              }

              TreeNode soundNode = new TreeNode(sound);
              soundNode.Tag = new Data("SOUND", null, sound);
              if (String.IsNullOrEmpty(sound))
                soundNode.Text = "No Sound";
              soundNode.ForeColor = Color.DarkRed;
              conditionNode.Nodes.Add(soundNode);

              if (layer == 1) layer1Node.Nodes.Add(conditionNode);
              if (layer == 2) layer2Node.Nodes.Add(conditionNode);
              if (layer == 0) layerAllNode.Nodes.Add(conditionNode);
            }
            if (layer1Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer1Node);
            if (layer2Node.Nodes.Count > 0) buttonNode.Nodes.Add(layer2Node);
            if (layerAllNode.Nodes.Count > 0) buttonNode.Nodes.Add(layerAllNode);
          }
          treeMapping.Nodes.Add(remoteNode);
          if (listRemotes.Count == 1)
            remoteNode.Expand();
        }
        changedSettings = false;
      }
      catch (Exception ex)
      {
        Log.Error(ex);
        File.Delete(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile));
        LoadMapping(xmlFile, true);
      }
    }

    private bool SaveMapping(string xmlFile)
    {
#if !DEBUG
      try
#endif
      {
        Directory.CreateDirectory(Config.GetFolder(Config.Dir.CustomInputDevice));
      }
#if !DEBUG
      catch
      {
        Log.Info("MAP: Error accessing directory \"InputDeviceMappings\\custom\"");
      }

      //try
#endif
      {
        using (
          XmlTextWriter writer = new XmlTextWriter(Config.GetFile(Config.Dir.CustomInputDevice, xmlFile), Encoding.UTF8)
          )
        {
          writer.Formatting = Formatting.Indented;
          writer.Indentation = 1;
          writer.IndentChar = (char) 9;
          writer.WriteStartDocument(true);
          writer.WriteStartElement("mappings"); // <mappings>
          writer.WriteAttributeString("version", "3");
          if (treeMapping.Nodes.Count > 0)
            foreach (TreeNode remoteNode in treeMapping.Nodes)
            {
              writer.WriteStartElement("remote"); // <remote>
              writer.WriteAttributeString("family", (string) ((Data) remoteNode.Tag).Value);
              if (remoteNode.Nodes.Count > 0)
                foreach (TreeNode buttonNode in remoteNode.Nodes)
                {
                  writer.WriteStartElement("button"); // <button>
                  writer.WriteAttributeString("name", (string) ((Data) buttonNode.Tag).Parameter);
                  writer.WriteAttributeString("code", (string) ((Data) buttonNode.Tag).Value);

                  if (buttonNode.Nodes.Count > 0)
                    foreach (TreeNode layerNode in buttonNode.Nodes)
                    {
                      foreach (TreeNode conditionNode in layerNode.Nodes)
                      {
                        string layer;
                        string condition;
                        string conProperty;
                        string command = String.Empty;
                        string cmdProperty = String.Empty;
                        string cmdKeyChar = String.Empty;
                        string cmdKeyCode = String.Empty;
                        string sound = String.Empty;
                        bool focus = false;
                        foreach (TreeNode commandNode in conditionNode.Nodes)
                        {
                          switch (((Data) commandNode.Tag).Type)
                          {
                            case "COMMAND":
                              {
                                command = (string) ((Data) commandNode.Tag).Parameter;
                                focus = ((Data) commandNode.Tag).Focus;
                                if (command != "KEY")
                                  cmdProperty = ((Data) commandNode.Tag).Value.ToString();
                                else
                                {
                                  command = "ACTION";
                                  Key key = (Key) ((Data) commandNode.Tag).Value;
                                  cmdProperty = "93";
                                  cmdKeyChar = key.KeyChar.ToString();
                                  cmdKeyCode = key.KeyCode.ToString();
                                }
                              }
                              break;
                            case "SOUND":
                              sound = (string) ((Data) commandNode.Tag).Value;
                              break;
                          }
                        }
                        condition = (string) ((Data) conditionNode.Tag).Parameter;
                        conProperty = ((Data) conditionNode.Tag).Value.ToString();
                        layer = Convert.ToString(((Data) layerNode.Tag).Value);
                        writer.WriteStartElement("action"); // <action>
                        writer.WriteAttributeString("layer", layer);
                        writer.WriteAttributeString("condition", condition);
                        writer.WriteAttributeString("conproperty", conProperty);
                        writer.WriteAttributeString("command", command);
                        writer.WriteAttributeString("cmdproperty", cmdProperty);
                        if (cmdProperty == Convert.ToInt32(Action.ActionType.ACTION_KEY_PRESSED).ToString())
                        {
                          if (!String.IsNullOrEmpty(cmdKeyChar))
                          {
                            writer.WriteAttributeString("cmdkeychar", cmdKeyChar);
                          }
                          else
                          {
                            writer.WriteAttributeString("cmdkeychar", "0");
                          }
                          if (!String.IsNullOrEmpty(cmdKeyCode))
                          {
                            writer.WriteAttributeString("cmdkeycode", cmdKeyCode);
                          }
                          else
                          {
                            writer.WriteAttributeString("cmdkeychar", "0");
                          }
                        }
                        if (!String.IsNullOrEmpty(sound))
                          writer.WriteAttributeString("sound", sound);
                        if (focus)
                          writer.WriteAttributeString("focus", focus.ToString());
                        writer.WriteEndElement(); // </action>
                      }
                    }
                  writer.WriteEndElement(); // </button>
                }
              writer.WriteEndElement(); // </remote>
            }
          writer.WriteEndElement(); // </mapping>
          writer.WriteEndDocument();
        }
        changedSettings = false;
        return true;
      }
#if !DEBUG
      //catch (Exception ex)
      //{
      //  Log.Info("MAP: Error saving mapping to XML file: {0}", ex.Message);
      //  return false;
      //}
#endif
    }

    private void LoadPluginList()
    {
      _pluginList.Clear();

      string path = Config.GetFile(Config.Dir.Config, "MediaPortal.xml");

      if (!File.Exists(path))
      {
        _pluginList.Add("Music");
        return;
      }

      XmlDocument doc = new XmlDocument();
      doc.Load(path);



      //TreeNode remoteNode = new TreeNode(nodeRemote.Attributes["family"].Value);
      //remoteNode.Tag = new Data("REMOTE", null, nodeRemote.Attributes["family"].Value);
      //XmlNodeList listButtons = nodeRemote.SelectNodes("button");
      //foreach (XmlNode nodeButton in listButtons)
      //{
      //  TreeNode buttonNode = new TreeNode(nodeButton.Attributes["name"].Value);
      //  buttonNode.Tag = new Data("BUTTON", nodeButton.Attributes["name"].Value, nodeButton.Attributes["code"].Value);
      //  remoteNode.Nodes.Add(buttonNode);

      XmlNode plugins = null;
      XmlNodeList listSections = doc.DocumentElement.SelectNodes("/profile/section");
      foreach (XmlNode nodeSection in listSections)
        if (nodeSection.Attributes["name"].Value == "plugins")
        {
          plugins = nodeSection;
          break;
        }

      if (plugins == null)
      {
        _pluginList.Add("Music");
        return;
      }

      foreach (XmlNode nodePlugin in plugins.ChildNodes)
        _pluginList.Add(nodePlugin.Attributes["name"].Value);
    }

    private TreeNode getNode(string type)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data) node.Tag;
      if (data.Type == type)
        return node;

      #region Find Node

      switch (type)
      {
        case "COMMAND":
          if ((data.Type == "SOUND") || (data.Type == "KEY"))
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data) subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data) subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          break;
        case "SOUND":
          if ((data.Type == "COMMAND") || (data.Type == "KEY"))
          {
            node = node.Parent;
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data) subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          else if (data.Type == "CONDITION")
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              data = (Data) subNode.Tag;
              if (data.Type == type)
                return subNode;
            }
          }
          break;
        case "CONDITION":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent;
          break;
        case "LAYER":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent;
          break;
        case "BUTTON":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent.Parent;
          else if (data.Type == "LAYER")
            return node.Parent;
          break;
        case "REMOTE":
          if ((data.Type == "SOUND") || (data.Type == "COMMAND") || (data.Type == "KEY"))
            return node.Parent.Parent.Parent.Parent;
          else if (data.Type == "CONDITION")
            return node.Parent.Parent.Parent;
          else if (data.Type == "LAYER")
            return node.Parent.Parent;
          else if (data.Type == "BUTTON")
            return node.Parent;
          break;
      }

      #endregion

      return null;
    }

    private void treeMapping_AfterSelect(object sender, TreeViewEventArgs e)
    {
      if (e.Action == TreeViewAction.Unknown)
        return;

      TreeNode node = e.Node;
      Data data = (Data) node.Tag;
      switch (data.Type)
      {
        case "REMOTE":
        case "BUTTON":
          groupBoxLayer.Enabled = false;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          comboBoxLayer.Text = "All Layers";
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          return;
        case "LAYER":
          groupBoxLayer.Enabled = true;
          groupBoxCondition.Enabled = false;
          groupBoxAction.Enabled = false;
          comboBoxCondProperty.Text = "none";
          comboBoxCmdProperty.Text = "none";
          comboBoxSound.Text = "none";
          comboBoxLayer.SelectedIndex = Convert.ToInt32(data.Value);
          return;
        case "COMMAND":
        case "SOUND":
        case "KEY":
        case "CONDITION":
          {
            groupBoxCondition.Enabled = true;
            groupBoxAction.Enabled = true;
            groupBoxLayer.Enabled = true;
            if ((data.Type == "COMMAND") || (data.Type == "SOUND"))
            {
              comboBoxLayer.SelectedIndex = Convert.ToInt32(((Data) node.Parent.Parent.Tag).Value);
              node = node.Parent;
              data = (Data) node.Tag;
            }
            else
              comboBoxLayer.SelectedIndex = Convert.ToInt32(((Data) node.Parent.Tag).Value);

            switch ((string) data.Parameter)
            {
              case "WINDOW":
                {
                  comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDown;

                  radioButtonWindow.Checked = true;
                  comboBoxCondProperty.Enabled = true;

                  string friendlyName;
                  try
                  {
                    friendlyName = GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value)));
                  }
                  catch
                  {
                    friendlyName = Convert.ToInt32(data.Value).ToString();
                  }
                  UpdateCombo(ref comboBoxCondProperty, windowsList, friendlyName);
                  break;
                }
              case "FULLSCREEN":
                comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;

                radioButtonFullscreen.Checked = true;
                comboBoxCondProperty.Enabled = true;
                if (Convert.ToBoolean(data.Value))
                  UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
                else
                  UpdateCombo(ref comboBoxCondProperty, fullScreenList, "No Fullscreen");
                break;
              case "PLAYER":
                comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;

                radioButtonPlaying.Checked = true;
                comboBoxCondProperty.Enabled = true;
                UpdateCombo(ref comboBoxCondProperty, playerList,
                            playerList[Array.IndexOf(nativePlayerList, (string)data.Value)]);
                break;
              case "PLUGIN":
                comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDown;

                radioButtonPlugin.Checked = true;
                comboBoxCondProperty.Enabled = true;
                UpdateCombo(ref comboBoxCondProperty, _pluginList.ToArray(), (string)data.Value);
                break;
              case "*":
                comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;

                comboBoxCondProperty.Text = "none";
                radioButtonNoCondition.Checked = true;
                comboBoxCondProperty.Enabled = false;
                comboBoxCondProperty.Items.Clear();
                break;
            }
            foreach (TreeNode typeNode in node.Nodes)
            {
              data = (Data) typeNode.Tag;
              switch (data.Type)
              {
                case "SOUND":
                  if (!String.IsNullOrEmpty(data.Value as string))
                    comboBoxSound.SelectedItem = data.Value;
                  else
                    comboBoxSound.SelectedItem = "none";
                  break;
                case "COMMAND":
                  checkBoxGainFocus.Checked = data.Focus;
                  switch ((string) data.Parameter)
                  {
                    case "ACTION":
                      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                      radioButtonAction.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
                      UpdateCombo(ref comboBoxCmdProperty, actionList,
                                  GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(data.Value))));
                      break;
                    case "KEY":
                      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                      radioButtonAction.Checked = true;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = true;
                      textBoxKeyChar.Text = ((Key) data.Value).KeyChar.ToString();
                      textBoxKeyCode.Text = ((Key) data.Value).KeyCode.ToString();
                      comboBoxCmdProperty.Enabled = true;
                      UpdateCombo(ref comboBoxCmdProperty, actionList, "Key Pressed");
                      break;
                    case "WINDOW":
                      {
                        comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDown;
                        radioButtonActWindow.Checked = true;
                        comboBoxSound.Enabled = true;
                        comboBoxCmdProperty.Enabled = true;
                        textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                        textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;

                        string friendlyName;
                        try
                        {
                          friendlyName =
                            GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value)));
                        }
                        catch
                        {
                          friendlyName = Convert.ToInt32(data.Value).ToString();
                        }
                        UpdateCombo(ref comboBoxCmdProperty, windowsListFiltered, friendlyName);
                        break;
                      }
                    case "TOGGLE":
                      radioButtonToggle.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = false;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
                      comboBoxCmdProperty.Items.Clear();
                      comboBoxCmdProperty.Text = String.Empty;
                      break;
                    case "POWER":
                      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                      radioButtonPower.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
                      UpdateCombo(ref comboBoxCmdProperty, powerList,
                                  powerList[Array.IndexOf(nativePowerList, (string) data.Value)]);
                      break;
                    case "PROCESS":
                      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                      radioButtonProcess.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
                      UpdateCombo(ref comboBoxCmdProperty, processList,
                                  processList[Array.IndexOf(nativeProcessList, (string) data.Value)]);
                      break;
                    case "BLAST":
                      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
                      radioButtonBlast.Checked = true;
                      comboBoxSound.Enabled = true;
                      comboBoxCmdProperty.Enabled = true;
                      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
                      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
                      UpdateCombo(ref comboBoxCmdProperty, MPControlPlugin.GetFileList(true), (string) data.Value);
                      break;
                  }
                  break;
              }
            }
          }
          break;
      }
    }

    private static void UpdateCombo(ref MPComboBox comboBox, Array list, string hilight)
    {
      comboBox.Items.Clear();
      foreach (object item in list)
        comboBox.Items.Add(item.ToString());
      comboBox.Text = hilight;
      comboBox.SelectedItem = hilight;
      comboBox.Enabled = true;
    }

    private static void UpdateCombo(ref MPComboBox comboBox, ArrayList list, string hilight)
    {
      UpdateCombo(ref comboBox, list.ToArray(), hilight);
    }

    private static string GetFriendlyName(string name)
    {
      if ((name.IndexOf("ACTION") != -1) || (name.IndexOf("WINDOW") != -1))
        name = name.Substring(7);

      bool upcase = true;
      string newName = String.Empty;

      foreach (char c in name)
      {
        if (c == '_')
        {
          newName += " ";
          upcase = true;
        }
        else if (upcase)
        {
          newName += c.ToString();
          upcase = false;
        }
        else
        {
          newName += c.ToString().ToLower();
        }
      }

      CleanAbbreviation(ref newName, "TV");
      CleanAbbreviation(ref newName, "DVD");
      CleanAbbreviation(ref newName, "UI");
      CleanAbbreviation(ref newName, "Guide");
      CleanAbbreviation(ref newName, "MSN");
      CleanAbbreviation(ref newName, "OSD");
      CleanAbbreviation(ref newName, "LCD");
      CleanAbbreviation(ref newName, "EPG");
      CleanAbbreviation(ref newName, "DVBC");
      CleanAbbreviation(ref newName, "DVBS");
      CleanAbbreviation(ref newName, "DVBT");

      return newName;
    }

    private static string GetWindowName(string friendlyName)
    {
      return "WINDOW_" + friendlyName.Replace(' ', '_').ToUpper();
    }

    private static string GetActionName(string friendlyName)
    {
      string actionName = String.Empty;

      try
      {
        if (Enum.Parse(typeof (Action.ActionType), "ACTION_" + friendlyName.Replace(' ', '_').ToUpper()) != null)
          actionName = "ACTION_" + friendlyName.Replace(' ', '_').ToUpper();
      }
      catch (ArgumentException)
      {
        try
        {
          if (Enum.Parse(typeof (Action.ActionType), friendlyName.Replace(' ', '_').ToUpper()) != null)
            actionName = friendlyName.Replace(' ', '_').ToUpper();
        }
        catch (ArgumentException)
        {
        }
      }

      return actionName;
    }


    private static void CleanAbbreviation(ref string name, string abbreviation)
    {
      int index = name.ToUpper().IndexOf(abbreviation.ToUpper());
      if (index != -1)
        name = name.Substring(0, index) + abbreviation + name.Substring(index + abbreviation.Length);
    }

    private void radioButtonWindow_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDown;
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      node.Tag = new Data("CONDITION", "WINDOW", "0");
      UpdateCombo(ref comboBoxCondProperty, windowsList, GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), 0)));
      node.Text = (string) comboBoxCondProperty.SelectedItem;
      changedSettings = true;
    }

    private void radioButtonFullscreen_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      node.Tag = new Data("CONDITION", "FULLSCREEN", "true");
      UpdateCombo(ref comboBoxCondProperty, fullScreenList, "Fullscreen");
      node.Text = (string) comboBoxCondProperty.SelectedItem;
      changedSettings = true;
    }

    private void radioButtonPlaying_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      node.Tag = new Data("CONDITION", "PLAYER", "TV");
      node.Text = playerList[0];
      UpdateCombo(ref comboBoxCondProperty, playerList, playerList[0]);
      changedSettings = true;
    }

    private void radioButtonPlugin_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDown;
      comboBoxCondProperty.Enabled = true;
      TreeNode node = getNode("CONDITION");
      node.Tag = new Data("CONDITION", "PLUGIN", _pluginList[0]);
      UpdateCombo(ref comboBoxCondProperty, _pluginList.ToArray(), _pluginList[0]);
      node.Text = comboBoxCondProperty.Text;
      changedSettings = true;
    }

    private void radioButtonNoCondition_Click(object sender, EventArgs e)
    {
      comboBoxCondProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxCondProperty.Enabled = false;
      comboBoxCondProperty.Items.Clear();
      comboBoxCondProperty.Text = "none";
      TreeNode node = getNode("CONDITION");
      node.Tag = new Data("CONDITION", "*", null);
      node.Text = "No Condition";
      changedSettings = true;
    }

    private void radioButtonAction_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "ACTION", "7");
      node.Tag = data;
      UpdateCombo(ref comboBoxCmdProperty, actionList,
                  GetFriendlyName(Enum.GetName(typeof (Action.ActionType), Convert.ToInt32(data.Value))));
      node.Text = "Action \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonActWindow_Click(object sender, EventArgs e)
    {
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDown;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "WINDOW", "0");
      node.Tag = data;

      string friendlyName;
      try
      {
        friendlyName = GetFriendlyName(Enum.GetName(typeof (GUIWindow.Window), Convert.ToInt32(data.Value)));
      }
      catch
      {
        friendlyName = Convert.ToInt32(data.Value).ToString();
      }
      UpdateCombo(ref comboBoxCmdProperty, windowsListFiltered, friendlyName);

      node.Text = "Window \"" + comboBoxCmdProperty.Text + "\"";
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonToggle_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = false;
      comboBoxCmdProperty.Items.Clear();
      comboBoxCmdProperty.Text = "none";
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "TOGGLE", "-1");
      node.Tag = data;
      node.Text = "Toggle Layer";
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonPower_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      node.Tag = new Data("COMMAND", "POWER", "EXIT");
      node.Text = powerList[0];
      UpdateCombo(ref comboBoxCmdProperty, powerList, powerList[0]);
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonProcess_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      comboBoxCmdProperty.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = true;
      TreeNode node = getNode("COMMAND");
      node.Tag = new Data("COMMAND", "PROCESS", "CLOSE");
      node.Text = processList[0];
      UpdateCombo(ref comboBoxCmdProperty, processList, processList[0]);
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      if (changedSettings)
        SaveMapping(inputClassName + ".xml");
      Close();
    }

    private void buttonApply_Click(object sender, EventArgs e)
    {
      if (changedSettings)
        SaveMapping(inputClassName + ".xml");
    }

    private void buttonUp_Click(object sender, EventArgs e)
    {
      bool expanded = false;
      TreeNode node = treeMapping.SelectedNode;
      if (((Data) node.Tag).Type != "BUTTON")
        expanded = node.IsExpanded;
      if ((((Data) node.Tag).Type == "COMMAND") || (((Data) node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((Data) node.Tag).Type != "BUTTON") && (((Data) node.Tag).Type != "CONDITION"))
        return;
      if (node.Index > 0)
      {
        int index = node.Index - 1;
        TreeNode tmpNode = (TreeNode) node.Clone();
        TreeNode parentNode = node.Parent;
        node.Remove();
        if (expanded)
          tmpNode.Expand();
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
      changedSettings = true;
    }

    private void buttonDown_Click(object sender, EventArgs e)
    {
      bool expanded = false;
      TreeNode node = treeMapping.SelectedNode;
      if (((Data) node.Tag).Type != "BUTTON")
        expanded = node.IsExpanded;
      if ((((Data) node.Tag).Type == "COMMAND") || (((Data) node.Tag).Type == "SOUND"))
      {
        node = node.Parent;
        expanded = true;
      }
      if ((((Data) node.Tag).Type != "BUTTON") && (((Data) node.Tag).Type != "CONDITION"))
        return;
      if (node.Index < node.Parent.Nodes.Count - 1)
      {
        int index = node.Index + 1;
        TreeNode tmpNode = (TreeNode) node.Clone();
        TreeNode parentNode = node.Parent;
        node.Remove();
        if (expanded)
          tmpNode.Expand();
        parentNode.Nodes.Insert(index, tmpNode);
        treeMapping.SelectedNode = tmpNode;
      }
      changedSettings = true;
    }

    private void buttonRemove_Click(object sender, EventArgs e)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data) node.Tag;
      if ((data.Type == "COMMAND") || (data.Type == "SOUND") || (data.Type == "CONDITION"))
      {
        node = getNode("CONDITION");
        data = (Data) node.Tag;
      }
      DialogResult result = MessageBox.Show(this, "Are you sure you want to remove this " + data.Type.ToLower() + "?",
                                            "Remove " + data.Type.ToLower(),
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button2);
      if (result == DialogResult.Yes)
      {
        node.Remove();
        changedSettings = true;
      }
    }


    private void buttonNew_Click(object sender, EventArgs e)
    {
      TreeNode node = treeMapping.SelectedNode;
      Data data = (Data) node.Tag;

      TreeNode newLayer = new TreeNode("All Layers");
      newLayer.Tag = new Data("LAYER", null, "0");
      newLayer.ForeColor = Color.DimGray;

      TreeNode newCondition = new TreeNode("No Condition");
      newCondition.Tag = new Data("CONDITION", "*", "-1");
      newCondition.ForeColor = Color.Blue;

      TreeNode newCommand = new TreeNode("Action \"Select Item\"");
      newCommand.Tag = new Data("COMMAND", "ACTION", "7");
      newCommand.ForeColor = Color.DarkGreen;

      TreeNode newSound = new TreeNode("No Sound");
      newSound.Tag = new Data("SOUND", String.Empty, String.Empty);
      newSound.ForeColor = Color.DarkRed;

      switch (data.Type)
      {
        case "LAYER":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          newLayer.Nodes.Add(newCondition);
          node.Parent.Nodes.Add(newLayer);
          newLayer.Expand();
          treeMapping.SelectedNode = newLayer;
          break;
        case "CONDITION":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          node.Parent.Nodes.Add(newCondition);
          newCondition.Expand();
          treeMapping.SelectedNode = newCondition;
          break;
        case "COMMAND":
        case "SOUND":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          node.Parent.Parent.Nodes.Add(newCondition);
          newCondition.Expand();
          treeMapping.SelectedNode = newCondition;
          break;
        case "BUTTON":
          newCondition.Nodes.Add(newCommand);
          newCondition.Nodes.Add(newSound);
          newLayer.Nodes.Add(newCondition);
          node.Nodes.Add(newLayer);
          newLayer.Expand();
          treeMapping.SelectedNode = newLayer;
          break;
        default:
          //NewButtonForm newButtonForm = new NewButtonForm();
          //newButtonForm.ShowDialog();
          //if (newButtonForm.Accepted)
          //{
          //  Log.Info("Name: {0}", newButtonForm.ButtonName);
          //  Log.Info("Code: {0}", newButtonForm.ButtonCode);
          //}
          break;
      }
      changedSettings = true;

      treeMapping_AfterSelect(this, new TreeViewEventArgs(treeMapping.SelectedNode, TreeViewAction.ByKeyboard));
    }

    private void buttonDefault_Click(object sender, EventArgs e)
    {
      string fileName = inputClassName + ".xml";
      string filePath = Config.GetFile(Config.Dir.CustomInputDevice, fileName);

      if (File.Exists(filePath))
        File.Delete(filePath);

      LoadMapping(fileName, true);
    }

    private void textBoxKeyCode_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!char.IsNumber(e.KeyChar) && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void textBoxKeyChar_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (!char.IsNumber(e.KeyChar) && e.KeyChar != 8)
      {
        e.Handled = true;
      }
    }

    private void comboBoxLayer_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = getNode("LAYER");
      node.Tag = new Data("LAYER", null, comboBoxLayer.SelectedIndex);
      if (comboBoxLayer.SelectedIndex == 0)
        node.Text = "All Layers";
      else
        node.Text = "Layer " + comboBoxLayer.SelectedIndex;
      changedSettings = true;
    }

    private void comboBoxCondProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      //FIXME: chefkoch 2009-06-17 : not sure why this should be done only for DropDownList
      //if (comboBoxCondProperty.DropDownStyle == ComboBoxStyle.DropDownList)
      ConditionPropChanged();
    }

    private void comboBoxCmdProperty_SelectionChangeCommitted(object sender, EventArgs e)
    {
      if (comboBoxCmdProperty.DropDownStyle == ComboBoxStyle.DropDownList)
        CommandPropChanged();
    }

    private void comboBoxSound_SelectionChangeCommitted(object sender, EventArgs e)
    {
      TreeNode node = getNode("SOUND");
      node.Text = (string) comboBoxSound.SelectedItem;
      if (node.Text == "none")
      {
        node.Tag = new Data("SOUND", null, String.Empty);
        node.Text = "No Sound";
      }
      else
        node.Tag = new Data("SOUND", null, comboBoxSound.SelectedItem);
      changedSettings = true;
    }

    private void textBoxKeyChar_KeyUp(object sender, KeyEventArgs e)
    {
      string keyChar = textBoxKeyChar.Text;
      string keyCode = textBoxKeyCode.Text;
      TreeNode node = getNode("COMMAND");
      if (String.IsNullOrEmpty(keyChar))
        keyChar = "0";
      if (String.IsNullOrEmpty(keyCode))
        keyCode = "0";
      Key key = new Key(Convert.ToInt32(keyChar), Convert.ToInt32(keyCode));
      node.Tag = new Data("COMMAND", "KEY", key);
      node.Text = String.Format("Key Pressed: {0} [{1}]", keyChar, keyCode);
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void textBoxKeyCode_KeyUp(object sender, KeyEventArgs e)
    {
      textBoxKeyChar_KeyUp(sender, e);
    }

    private void labelExpand_Click(object sender, EventArgs e)
    {
      if (treeMapping.SelectedNode == null)
        treeMapping.Select();
      treeMapping.SelectedNode.ExpandAll();
    }

    private void checkBoxGainFocus_CheckedChanged(object sender, EventArgs e)
    {
      TreeNode node = getNode("COMMAND");
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void radioButtonBlast_Click(object sender, EventArgs e)
    {
      textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
      textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      comboBoxSound.Enabled = true;
      comboBoxCmdProperty.Enabled = false;
      comboBoxCmdProperty.Items.Clear();
      comboBoxCmdProperty.Text = "none";
      TreeNode node = getNode("COMMAND");
      Data data = new Data("COMMAND", "BLAST", String.Empty);
      node.Tag = data;
      node.Text = "";
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      UpdateCombo(ref comboBoxCmdProperty, MPControlPlugin.GetFileList(true), String.Empty);
      changedSettings = true;
    }

    private void comboBoxCondProperty_KeyUp(object sender, KeyEventArgs e)
    {
      //FIXME: chefkoch 2009-06-17 : not sure why this should be done only for DropDownList
      //if (e.KeyCode == Keys.Enter && comboBoxCondProperty.DropDownStyle == ComboBoxStyle.DropDown)
      if (e.KeyCode == Keys.Enter)
        ConditionPropChanged();
    }

    private void comboBoxCmdProperty_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter && comboBoxCmdProperty.DropDownStyle == ComboBoxStyle.DropDown)
        CommandPropChanged();
    }

    private void CommandPropChanged()
    {
      if ((string) comboBoxCmdProperty.SelectedItem == "Key Pressed")
        textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = true;
      else
      {
        textBoxKeyChar.Enabled = textBoxKeyCode.Enabled = false;
        textBoxKeyChar.Text = textBoxKeyCode.Text = String.Empty;
      }

      TreeNode node = getNode("COMMAND");
      Data data = (Data) node.Tag;
      switch ((string) data.Parameter)
      {
        case "ACTION":
          if ((string) comboBoxCmdProperty.SelectedItem != "Key Pressed")
          {
            node.Tag = new Data("COMMAND", "ACTION",
                                (int)
                                Enum.Parse(typeof (Action.ActionType),
                                           GetActionName((string) comboBoxCmdProperty.SelectedItem)));
            node.Text = "Action \"" + (string) comboBoxCmdProperty.SelectedItem + "\"";
          }
          else
          {
            textBoxKeyChar.Text = "0";
            textBoxKeyCode.Text = "0";
            Key key = new Key(Convert.ToInt32(textBoxKeyChar.Text), Convert.ToInt32(textBoxKeyCode.Text));
            node.Tag = new Data("COMMAND", "KEY", key);
            node.Text = String.Format("Key Pressed: {0} [{1}]", textBoxKeyChar.Text, textBoxKeyCode.Text);
          }
          break;
        case "WINDOW":
          {
            int windowID;
            try
            {
              windowID = (int) Enum.Parse(typeof (GUIWindow.Window), GetWindowName(comboBoxCmdProperty.Text));
            }
            catch
            {
              windowID = Convert.ToInt32(comboBoxCmdProperty.Text);
            }

            node.Tag = new Data("COMMAND", "WINDOW", windowID);
            node.Text = "Window \"" + comboBoxCmdProperty.Text + "\"";
            break;
          }
        case "POWER":
          node.Tag = new Data("COMMAND", "POWER",
                              nativePowerList[Array.IndexOf(powerList, (string) comboBoxCmdProperty.SelectedItem)]);
          node.Text = (string) comboBoxCmdProperty.SelectedItem;
          break;
        case "PROCESS":
          node.Tag = new Data("COMMAND", "PROCESS",
                              nativeProcessList[Array.IndexOf(processList, (string) comboBoxCmdProperty.SelectedItem)]);
          node.Text = (string) comboBoxCmdProperty.SelectedItem;
          break;
        case "BLAST":
          {
            string text = (string) comboBoxCmdProperty.SelectedItem;
            if (text.StartsWith(Common.CmdPrefixBlast, StringComparison.InvariantCultureIgnoreCase))
            {
              BlastCommand blastCommand = new BlastCommand(
                MPControlPlugin.BlastIR,
                Common.FolderIRCommands,
                MPControlPlugin.TransceiverInformation.Ports,
                text.Substring(Common.CmdPrefixBlast.Length));

              if (blastCommand.ShowDialog(this) == DialogResult.OK)
              {
                string command = Common.CmdPrefixBlast + blastCommand.CommandString;
                node.Tag = new Data("COMMAND", "BLAST", command);
                node.Text = command;
              }
            }
            else if (text.StartsWith(Common.CmdPrefixMacro, StringComparison.InvariantCultureIgnoreCase))
            {
              node.Tag = new Data("COMMAND", "BLAST", text);
              node.Text = text;
            }
            break;
          }
      }
      ((Data) node.Tag).Focus = checkBoxGainFocus.Checked;
      changedSettings = true;
    }

    private void ConditionPropChanged()
    {
      TreeNode node = getNode("CONDITION");
      Data data = (Data) node.Tag;
      switch ((string) data.Parameter)
      {
        case "WINDOW":
          {
            int windowID;
            try
            {
              windowID = (int) Enum.Parse(typeof (GUIWindow.Window), GetWindowName(comboBoxCondProperty.Text));
            }
            catch
            {
              windowID = Convert.ToInt32(comboBoxCondProperty.Text);
            }

            node.Tag = new Data("CONDITION", "WINDOW", windowID);
            node.Text = comboBoxCondProperty.Text;
            break;
          }
        case "FULLSCREEN":
          if ((string) comboBoxCondProperty.SelectedItem == "Fullscreen")
            node.Tag = new Data("CONDITION", "FULLSCREEN", "true");
          else
            node.Tag = new Data("CONDITION", "FULLSCREEN", "false");
          node.Text = (string) comboBoxCondProperty.SelectedItem;
          break;
        case "PLAYER":
          {
            node.Tag = new Data("CONDITION", "PLAYER",
                                nativePlayerList[Array.IndexOf(playerList, (string)comboBoxCondProperty.SelectedItem)]);
            node.Text = (string)comboBoxCondProperty.SelectedItem;
            break;
          }
        case "PLUGIN":
          {
            node.Tag = new Data("CONDITION", "PLUGIN", comboBoxCondProperty.Text);
            node.Text = comboBoxCondProperty.Text;
            break;
          }
        case "*":
          break;
      }
      changedSettings = true;
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputMappingForm));
      this.treeMapping = new System.Windows.Forms.TreeView();
      this.labelExpand = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonDefault = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonRemove = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonDown = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonUp = new MediaPortal.UserInterface.Controls.MPButton();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.buttonApply = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonOk = new MediaPortal.UserInterface.Controls.MPButton();
      this.buttonCancel = new MediaPortal.UserInterface.Controls.MPButton();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.groupBoxAction = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonBlast = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.checkBoxGainFocus = new MediaPortal.UserInterface.Controls.MPCheckBox();
      this.textBoxKeyCode = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.textBoxKeyChar = new MediaPortal.UserInterface.Controls.MPTextBox();
      this.radioButtonProcess = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.labelSound = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBoxSound = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.radioButtonAction = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonActWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonToggle = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPower = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCmdProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxCondition = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.radioButtonPlugin = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonWindow = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonFullscreen = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonPlaying = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.radioButtonNoCondition = new MediaPortal.UserInterface.Controls.MPRadioButton();
      this.comboBoxCondProperty = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.groupBoxLayer = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.comboBoxLayer = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.labelLayer = new MediaPortal.UserInterface.Controls.MPLabel();
      this.buttonNew = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBoxAction.SuspendLayout();
      this.groupBoxCondition.SuspendLayout();
      this.groupBoxLayer.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeMapping
      // 
      this.treeMapping.AllowDrop = true;
      this.treeMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.treeMapping.FullRowSelect = true;
      this.treeMapping.HideSelection = false;
      this.treeMapping.Location = new System.Drawing.Point(16, 56);
      this.treeMapping.Name = "treeMapping";
      this.treeMapping.Size = new System.Drawing.Size(312, 347);
      this.treeMapping.TabIndex = 0;
      this.treeMapping.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeMapping_AfterSelect);
      // 
      // labelExpand
      // 
      this.labelExpand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.labelExpand.AutoSize = true;
      this.labelExpand.Location = new System.Drawing.Point(328, 386);
      this.labelExpand.Name = "labelExpand";
      this.labelExpand.Size = new System.Drawing.Size(13, 13);
      this.labelExpand.TabIndex = 5;
      this.labelExpand.Text = "+";
      this.labelExpand.Click += new System.EventHandler(this.labelExpand_Click);
      // 
      // buttonDefault
      // 
      this.buttonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDefault.Location = new System.Drawing.Point(268, 454);
      this.buttonDefault.Name = "buttonDefault";
      this.buttonDefault.Size = new System.Drawing.Size(75, 23);
      this.buttonDefault.TabIndex = 11;
      this.buttonDefault.Text = "Reset";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // buttonRemove
      // 
      this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonRemove.Location = new System.Drawing.Point(272, 409);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(56, 20);
      this.buttonRemove.TabIndex = 4;
      this.buttonRemove.Text = "Remove";
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
      // 
      // buttonDown
      // 
      this.buttonDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonDown.Location = new System.Drawing.Point(97, 409);
      this.buttonDown.Name = "buttonDown";
      this.buttonDown.Size = new System.Drawing.Size(56, 20);
      this.buttonDown.TabIndex = 2;
      this.buttonDown.Text = "Down";
      this.buttonDown.UseVisualStyleBackColor = true;
      this.buttonDown.Click += new System.EventHandler(this.buttonDown_Click);
      // 
      // buttonUp
      // 
      this.buttonUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonUp.Location = new System.Drawing.Point(16, 409);
      this.buttonUp.Name = "buttonUp";
      this.buttonUp.Size = new System.Drawing.Size(56, 20);
      this.buttonUp.TabIndex = 1;
      this.buttonUp.Text = "Up";
      this.buttonUp.UseVisualStyleBackColor = true;
      this.buttonUp.Click += new System.EventHandler(this.buttonUp_Click);
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 444);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(328, 2);
      this.beveledLine1.TabIndex = 9;
      // 
      // buttonApply
      // 
      this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonApply.Location = new System.Drawing.Point(346, 454);
      this.buttonApply.Name = "buttonApply";
      this.buttonApply.Size = new System.Drawing.Size(75, 23);
      this.buttonApply.TabIndex = 12;
      this.buttonApply.Text = "Apply";
      this.buttonApply.UseVisualStyleBackColor = true;
      this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
      // 
      // buttonOk
      // 
      this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOk.Location = new System.Drawing.Point(426, 454);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(75, 23);
      this.buttonOk.TabIndex = 13;
      this.buttonOk.Text = "OK";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(505, 454);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 14;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(16, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(558, 24);
      this.headerLabel.TabIndex = 15;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      // 
      // groupBoxAction
      // 
      this.groupBoxAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxAction.Controls.Add(this.radioButtonBlast);
      this.groupBoxAction.Controls.Add(this.checkBoxGainFocus);
      this.groupBoxAction.Controls.Add(this.textBoxKeyCode);
      this.groupBoxAction.Controls.Add(this.label1);
      this.groupBoxAction.Controls.Add(this.textBoxKeyChar);
      this.groupBoxAction.Controls.Add(this.radioButtonProcess);
      this.groupBoxAction.Controls.Add(this.labelSound);
      this.groupBoxAction.Controls.Add(this.comboBoxSound);
      this.groupBoxAction.Controls.Add(this.radioButtonAction);
      this.groupBoxAction.Controls.Add(this.radioButtonActWindow);
      this.groupBoxAction.Controls.Add(this.radioButtonToggle);
      this.groupBoxAction.Controls.Add(this.radioButtonPower);
      this.groupBoxAction.Controls.Add(this.comboBoxCmdProperty);
      this.groupBoxAction.Enabled = false;
      this.groupBoxAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxAction.Location = new System.Drawing.Point(350, 233);
      this.groupBoxAction.Name = "groupBoxAction";
      this.groupBoxAction.Size = new System.Drawing.Size(224, 211);
      this.groupBoxAction.TabIndex = 8;
      this.groupBoxAction.TabStop = false;
      this.groupBoxAction.Text = "Action";
      // 
      // radioButtonBlast
      // 
      this.radioButtonBlast.AutoSize = true;
      this.radioButtonBlast.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonBlast.Location = new System.Drawing.Point(112, 68);
      this.radioButtonBlast.Name = "radioButtonBlast";
      this.radioButtonBlast.Size = new System.Drawing.Size(102, 17);
      this.radioButtonBlast.TabIndex = 5;
      this.radioButtonBlast.Text = "Blast IR / Macro";
      this.radioButtonBlast.UseVisualStyleBackColor = true;
      this.radioButtonBlast.Click += new System.EventHandler(this.radioButtonBlast_Click);
      // 
      // checkBoxGainFocus
      // 
      this.checkBoxGainFocus.AutoSize = true;
      this.checkBoxGainFocus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.checkBoxGainFocus.Location = new System.Drawing.Point(24, 92);
      this.checkBoxGainFocus.Name = "checkBoxGainFocus";
      this.checkBoxGainFocus.Size = new System.Drawing.Size(78, 17);
      this.checkBoxGainFocus.TabIndex = 6;
      this.checkBoxGainFocus.Text = "Gain Focus";
      this.checkBoxGainFocus.UseVisualStyleBackColor = true;
      this.checkBoxGainFocus.CheckedChanged += new System.EventHandler(this.checkBoxGainFocus_CheckedChanged);
      // 
      // textBoxKeyCode
      // 
      this.textBoxKeyCode.BorderColor = System.Drawing.Color.Empty;
      this.textBoxKeyCode.Enabled = false;
      this.textBoxKeyCode.Location = new System.Drawing.Point(152, 152);
      this.textBoxKeyCode.MaxLength = 3;
      this.textBoxKeyCode.Name = "textBoxKeyCode";
      this.textBoxKeyCode.Size = new System.Drawing.Size(48, 20);
      this.textBoxKeyCode.TabIndex = 10;
      this.textBoxKeyCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyCode_KeyUp);
      this.textBoxKeyCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyCode_KeyPress);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(24, 156);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(28, 13);
      this.label1.TabIndex = 8;
      this.label1.Text = "Key:";
      // 
      // textBoxKeyChar
      // 
      this.textBoxKeyChar.BorderColor = System.Drawing.Color.Empty;
      this.textBoxKeyChar.Enabled = false;
      this.textBoxKeyChar.Location = new System.Drawing.Point(72, 152);
      this.textBoxKeyChar.MaxLength = 3;
      this.textBoxKeyChar.Name = "textBoxKeyChar";
      this.textBoxKeyChar.Size = new System.Drawing.Size(80, 20);
      this.textBoxKeyChar.TabIndex = 9;
      this.textBoxKeyChar.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxKeyChar_KeyUp);
      this.textBoxKeyChar.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxKeyChar_KeyPress);
      // 
      // radioButtonProcess
      // 
      this.radioButtonProcess.AutoSize = true;
      this.radioButtonProcess.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonProcess.Location = new System.Drawing.Point(24, 68);
      this.radioButtonProcess.Name = "radioButtonProcess";
      this.radioButtonProcess.Size = new System.Drawing.Size(62, 17);
      this.radioButtonProcess.TabIndex = 4;
      this.radioButtonProcess.Text = "Process";
      this.radioButtonProcess.UseVisualStyleBackColor = true;
      this.radioButtonProcess.Click += new System.EventHandler(this.radioButtonProcess_Click);
      // 
      // labelSound
      // 
      this.labelSound.AutoSize = true;
      this.labelSound.Location = new System.Drawing.Point(24, 184);
      this.labelSound.Name = "labelSound";
      this.labelSound.Size = new System.Drawing.Size(41, 13);
      this.labelSound.TabIndex = 11;
      this.labelSound.Text = "Sound:";
      // 
      // comboBoxSound
      // 
      this.comboBoxSound.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSound.ForeColor = System.Drawing.Color.DarkRed;
      this.comboBoxSound.Location = new System.Drawing.Point(72, 181);
      this.comboBoxSound.Name = "comboBoxSound";
      this.comboBoxSound.Size = new System.Drawing.Size(128, 21);
      this.comboBoxSound.TabIndex = 12;
      this.comboBoxSound.SelectionChangeCommitted += new System.EventHandler(this.comboBoxSound_SelectionChangeCommitted);
      // 
      // radioButtonAction
      // 
      this.radioButtonAction.AutoSize = true;
      this.radioButtonAction.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonAction.Location = new System.Drawing.Point(24, 20);
      this.radioButtonAction.Name = "radioButtonAction";
      this.radioButtonAction.Size = new System.Drawing.Size(54, 17);
      this.radioButtonAction.TabIndex = 0;
      this.radioButtonAction.Text = "Action";
      this.radioButtonAction.UseVisualStyleBackColor = true;
      this.radioButtonAction.Click += new System.EventHandler(this.radioButtonAction_Click);
      // 
      // radioButtonActWindow
      // 
      this.radioButtonActWindow.AutoSize = true;
      this.radioButtonActWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonActWindow.Location = new System.Drawing.Point(112, 20);
      this.radioButtonActWindow.Name = "radioButtonActWindow";
      this.radioButtonActWindow.Size = new System.Drawing.Size(63, 17);
      this.radioButtonActWindow.TabIndex = 1;
      this.radioButtonActWindow.Text = "Window";
      this.radioButtonActWindow.UseVisualStyleBackColor = true;
      this.radioButtonActWindow.Click += new System.EventHandler(this.radioButtonActWindow_Click);
      // 
      // radioButtonToggle
      // 
      this.radioButtonToggle.AutoSize = true;
      this.radioButtonToggle.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonToggle.Location = new System.Drawing.Point(112, 44);
      this.radioButtonToggle.Name = "radioButtonToggle";
      this.radioButtonToggle.Size = new System.Drawing.Size(86, 17);
      this.radioButtonToggle.TabIndex = 3;
      this.radioButtonToggle.Text = "Toggle Layer";
      this.radioButtonToggle.UseVisualStyleBackColor = true;
      this.radioButtonToggle.Click += new System.EventHandler(this.radioButtonToggle_Click);
      // 
      // radioButtonPower
      // 
      this.radioButtonPower.AutoSize = true;
      this.radioButtonPower.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPower.Location = new System.Drawing.Point(24, 44);
      this.radioButtonPower.Name = "radioButtonPower";
      this.radioButtonPower.Size = new System.Drawing.Size(80, 17);
      this.radioButtonPower.TabIndex = 2;
      this.radioButtonPower.Text = "Powerdown";
      this.radioButtonPower.UseVisualStyleBackColor = true;
      this.radioButtonPower.Click += new System.EventHandler(this.radioButtonPower_Click);
      // 
      // comboBoxCmdProperty
      // 
      this.comboBoxCmdProperty.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxCmdProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCmdProperty.ForeColor = System.Drawing.Color.DarkGreen;
      this.comboBoxCmdProperty.Location = new System.Drawing.Point(24, 120);
      this.comboBoxCmdProperty.Name = "comboBoxCmdProperty";
      this.comboBoxCmdProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCmdProperty.Sorted = true;
      this.comboBoxCmdProperty.TabIndex = 7;
      this.comboBoxCmdProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxCmdProperty_SelectionChangeCommitted);
      this.comboBoxCmdProperty.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxCmdProperty_KeyUp);
      // 
      // groupBoxCondition
      // 
      this.groupBoxCondition.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.groupBoxCondition.Controls.Add(this.radioButtonPlugin);
      this.groupBoxCondition.Controls.Add(this.radioButtonWindow);
      this.groupBoxCondition.Controls.Add(this.radioButtonFullscreen);
      this.groupBoxCondition.Controls.Add(this.radioButtonPlaying);
      this.groupBoxCondition.Controls.Add(this.radioButtonNoCondition);
      this.groupBoxCondition.Controls.Add(this.comboBoxCondProperty);
      this.groupBoxCondition.Enabled = false;
      this.groupBoxCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxCondition.Location = new System.Drawing.Point(350, 106);
      this.groupBoxCondition.Name = "groupBoxCondition";
      this.groupBoxCondition.Size = new System.Drawing.Size(224, 120);
      this.groupBoxCondition.TabIndex = 7;
      this.groupBoxCondition.TabStop = false;
      this.groupBoxCondition.Text = "Condition";
      // 
      // radioButtonPlugin
      // 
      this.radioButtonPlugin.AutoSize = true;
      this.radioButtonPlugin.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPlugin.Location = new System.Drawing.Point(24, 67);
      this.radioButtonPlugin.Name = "radioButtonPlugin";
      this.radioButtonPlugin.Size = new System.Drawing.Size(104, 17);
      this.radioButtonPlugin.TabIndex = 5;
      this.radioButtonPlugin.Text = "Plugin is enabled";
      this.radioButtonPlugin.UseVisualStyleBackColor = true;
      this.radioButtonPlugin.Click += new System.EventHandler(this.radioButtonPlugin_Click);
      // 
      // radioButtonWindow
      // 
      this.radioButtonWindow.AutoSize = true;
      this.radioButtonWindow.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonWindow.Location = new System.Drawing.Point(24, 20);
      this.radioButtonWindow.Name = "radioButtonWindow";
      this.radioButtonWindow.Size = new System.Drawing.Size(63, 17);
      this.radioButtonWindow.TabIndex = 0;
      this.radioButtonWindow.Text = "Window";
      this.radioButtonWindow.UseVisualStyleBackColor = true;
      this.radioButtonWindow.Click += new System.EventHandler(this.radioButtonWindow_Click);
      // 
      // radioButtonFullscreen
      // 
      this.radioButtonFullscreen.AutoSize = true;
      this.radioButtonFullscreen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonFullscreen.Location = new System.Drawing.Point(112, 20);
      this.radioButtonFullscreen.Name = "radioButtonFullscreen";
      this.radioButtonFullscreen.Size = new System.Drawing.Size(72, 17);
      this.radioButtonFullscreen.TabIndex = 1;
      this.radioButtonFullscreen.Text = "Fullscreen";
      this.radioButtonFullscreen.UseVisualStyleBackColor = true;
      this.radioButtonFullscreen.Click += new System.EventHandler(this.radioButtonFullscreen_Click);
      // 
      // radioButtonPlaying
      // 
      this.radioButtonPlaying.AutoSize = true;
      this.radioButtonPlaying.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonPlaying.Location = new System.Drawing.Point(24, 44);
      this.radioButtonPlaying.Name = "radioButtonPlaying";
      this.radioButtonPlaying.Size = new System.Drawing.Size(58, 17);
      this.radioButtonPlaying.TabIndex = 2;
      this.radioButtonPlaying.Text = "Playing";
      this.radioButtonPlaying.UseVisualStyleBackColor = true;
      this.radioButtonPlaying.Click += new System.EventHandler(this.radioButtonPlaying_Click);
      // 
      // radioButtonNoCondition
      // 
      this.radioButtonNoCondition.AutoSize = true;
      this.radioButtonNoCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.radioButtonNoCondition.Location = new System.Drawing.Point(112, 44);
      this.radioButtonNoCondition.Name = "radioButtonNoCondition";
      this.radioButtonNoCondition.Size = new System.Drawing.Size(85, 17);
      this.radioButtonNoCondition.TabIndex = 3;
      this.radioButtonNoCondition.Text = "No Condition";
      this.radioButtonNoCondition.UseVisualStyleBackColor = true;
      this.radioButtonNoCondition.Click += new System.EventHandler(this.radioButtonNoCondition_Click);
      // 
      // comboBoxCondProperty
      // 
      this.comboBoxCondProperty.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxCondProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxCondProperty.ForeColor = System.Drawing.Color.Blue;
      this.comboBoxCondProperty.Location = new System.Drawing.Point(24, 90);
      this.comboBoxCondProperty.Name = "comboBoxCondProperty";
      this.comboBoxCondProperty.Size = new System.Drawing.Size(176, 21);
      this.comboBoxCondProperty.Sorted = true;
      this.comboBoxCondProperty.TabIndex = 4;
      this.comboBoxCondProperty.SelectionChangeCommitted += new System.EventHandler(this.comboBoxCondProperty_SelectionChangeCommitted);
      this.comboBoxCondProperty.KeyUp += new System.Windows.Forms.KeyEventHandler(this.comboBoxCondProperty_KeyUp);
      // 
      // groupBoxLayer
      // 
      this.groupBoxLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBoxLayer.Controls.Add(this.comboBoxLayer);
      this.groupBoxLayer.Controls.Add(this.labelLayer);
      this.groupBoxLayer.Enabled = false;
      this.groupBoxLayer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
      this.groupBoxLayer.Location = new System.Drawing.Point(350, 48);
      this.groupBoxLayer.Name = "groupBoxLayer";
      this.groupBoxLayer.Size = new System.Drawing.Size(224, 52);
      this.groupBoxLayer.TabIndex = 6;
      this.groupBoxLayer.TabStop = false;
      this.groupBoxLayer.Text = "Layer";
      // 
      // comboBoxLayer
      // 
      this.comboBoxLayer.BorderColor = System.Drawing.Color.Empty;
      this.comboBoxLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxLayer.ForeColor = System.Drawing.Color.DimGray;
      this.comboBoxLayer.Location = new System.Drawing.Point(80, 20);
      this.comboBoxLayer.Name = "comboBoxLayer";
      this.comboBoxLayer.Size = new System.Drawing.Size(121, 21);
      this.comboBoxLayer.TabIndex = 1;
      this.comboBoxLayer.SelectionChangeCommitted += new System.EventHandler(this.comboBoxLayer_SelectionChangeCommitted);
      // 
      // labelLayer
      // 
      this.labelLayer.AutoSize = true;
      this.labelLayer.Location = new System.Drawing.Point(24, 23);
      this.labelLayer.Name = "labelLayer";
      this.labelLayer.Size = new System.Drawing.Size(36, 13);
      this.labelLayer.TabIndex = 0;
      this.labelLayer.Text = "Layer:";
      // 
      // buttonNew
      // 
      this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.buttonNew.Location = new System.Drawing.Point(189, 409);
      this.buttonNew.Name = "buttonNew";
      this.buttonNew.Size = new System.Drawing.Size(56, 20);
      this.buttonNew.TabIndex = 3;
      this.buttonNew.Text = "New";
      this.buttonNew.UseVisualStyleBackColor = true;
      this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
      // 
      // InputMappingForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.AutoScroll = true;
      this.ClientSize = new System.Drawing.Size(590, 487);
      this.Controls.Add(this.labelExpand);
      this.Controls.Add(this.treeMapping);
      this.Controls.Add(this.buttonDefault);
      this.Controls.Add(this.buttonRemove);
      this.Controls.Add(this.buttonNew);
      this.Controls.Add(this.buttonDown);
      this.Controls.Add(this.buttonUp);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.buttonApply);
      this.Controls.Add(this.buttonOk);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.groupBoxAction);
      this.Controls.Add(this.groupBoxCondition);
      this.Controls.Add(this.groupBoxLayer);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(598, 509);
      this.Name = "InputMappingForm";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MediaPortal - Setup";
      this.groupBoxAction.ResumeLayout(false);
      this.groupBoxAction.PerformLayout();
      this.groupBoxCondition.ResumeLayout(false);
      this.groupBoxCondition.PerformLayout();
      this.groupBoxLayer.ResumeLayout(false);
      this.groupBoxLayer.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    #region Nested type: Data

    private class Data
    {
      private readonly string type;
      private object dataValue;
      private bool focus;
      private object parameter;

      public Data(object newType, object newParameter, object newValue)
      {
        if (newValue == null)
          newValue = String.Empty;
        if (newParameter == null)
          newParameter = String.Empty;
        type = (string) newType;
        dataValue = newValue;
        parameter = newParameter;
      }

      public Data(object newType, object newParameter, object newValue, bool newFocus)
      {
        if (newValue == null)
          newValue = String.Empty;
        if (newParameter == null)
          newParameter = String.Empty;
        type = (string) newType;
        dataValue = newValue;
        parameter = newParameter;
        focus = newFocus;
      }

      public string Type
      {
        get { return type; }
      }

      public object Value
      {
        get { return dataValue; }
        set { dataValue = value; }
      }

      public object Parameter
      {
        get { return parameter; }
        set { parameter = value; }
      }

      public bool Focus
      {
        get { return focus; }
        set { focus = value; }
      }
    }

    #endregion

    #region Controls

    private MPBeveledLine beveledLine1;
    private MPButton buttonApply;
    private MPButton buttonCancel;
    private MPButton buttonDefault;
    private MPButton buttonDown;
    private MPButton buttonNew;
    private MPButton buttonOk;
    private MPButton buttonRemove;
    private MPButton buttonUp;
    private MPCheckBox checkBoxGainFocus;
    private MPComboBox comboBoxCmdProperty;
    private MPComboBox comboBoxCondProperty;
    private MPComboBox comboBoxLayer;
    private MPComboBox comboBoxSound;
    private MPGroupBox groupBoxAction;
    private MPGroupBox groupBoxCondition;
    private MPGroupBox groupBoxLayer;
    private MPGradientLabel headerLabel;
    private MPLabel label1;
    private MPLabel labelExpand;
    private MPLabel labelLayer;
    private MPLabel labelSound;
    private MPRadioButton radioButtonAction;
    private MPRadioButton radioButtonActWindow;
    private MPRadioButton radioButtonBlast;
    private MPRadioButton radioButtonFullscreen;
    private MPRadioButton radioButtonNoCondition;
    private MPRadioButton radioButtonPlaying;
    private MPRadioButton radioButtonPower;
    private MPRadioButton radioButtonProcess;
    private MPRadioButton radioButtonToggle;
    private MPRadioButton radioButtonWindow;
    private MPTextBox textBoxKeyChar;
    private MPTextBox textBoxKeyCode;
    private TreeView treeMapping;

    #endregion

    //    private TreeNode tn;
    //
    //    private void treeMapping_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
    //    {
    //      tn=e.Item as TreeNode;
    //      DoDragDrop(e.Item.ToString(), DragDropEffects.Move);
    //    }
    //
    //    private void treeMapping_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
    //    {
    //      Point pt = new Point(e.X,e.Y); 
    //      pt = treeMapping.PointToClient(pt); 
    //      TreeNode ParentNode = treeMapping.GetNodeAt(pt); 
    //      ParentNode.Nodes.Add(tn.Text); // this copies the node 
    //      tn.Remove(); // need to remove the original version of the node 
    //    }
    //
    //    private void treeMapping_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
    //    {
    //      e.Effect=DragDropEffects.Move;
    //    }
  }
}