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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using IrssUtils;
using MediaPortal.TV.Database;

namespace MediaPortal.Plugins
{
  internal partial class ExternalChannels : Form
  {
    #region Variables

    private StbSetup[] _tvCardStbSetups;
    private TabPage[] _tvCardTabs;

    #endregion Variables

    #region Constructor

    public ExternalChannels()
    {
      InitializeComponent();
    }

    #endregion Constructor

    private void ExternalChannels_Load(object sender, EventArgs e)
    {
      ArrayList cards = new ArrayList();
      TVDatabase.GetCards(ref cards);

      if (cards.Count == 0)
        cards.Add(0);

      _tvCardTabs = new TabPage[cards.Count];
      _tvCardStbSetups = new StbSetup[cards.Count];

      comboBoxCopyFrom.Items.Clear();

      int index = 0;
      foreach (int cardId in cards)
      {
        string cardName = String.Format("TV Card {0}", cardId);

        comboBoxCopyFrom.Items.Add(cardName);

        _tvCardStbSetups[index] = new StbSetup(cardId);
        _tvCardStbSetups[index].Name = "StbSetup" + index;
        _tvCardStbSetups[index].Dock = DockStyle.Fill;

        _tvCardTabs[index] = new TabPage(cardName);
        _tvCardTabs[index].Controls.Add(_tvCardStbSetups[index]);

        tabControlTVCards.TabPages.Add(_tvCardTabs[index]);

        index++;
      }

      comboBoxCopyFrom.SelectedIndex = 0;

      // Setup quick setup combo box
      string[] quickSetupFiles = Directory.GetFiles(Common.FolderSTB, "*.xml", SearchOption.TopDirectoryOnly);
      foreach (string file in quickSetupFiles)
        comboBoxQuickSetup.Items.Add(Path.GetFileNameWithoutExtension(file));

      comboBoxQuickSetup.Items.Add("Clear all");
    }

    private static void ProcessExternalChannelProgram(string runCommand, int currentChannelDigit,
                                                      string fullChannelString)
    {
      string[] commands = Common.SplitRunCommand(runCommand);

      commands[2] = commands[2].Replace("%1", currentChannelDigit.ToString());
      commands[2] = commands[2].Replace("%2", fullChannelString);

      Common.ProcessRunCommand(commands);
    }

    private static void ProcessSerialCommand(string serialCommand, int currentChannelDigit, string fullChannelString)
    {
      string[] commands = Common.SplitSerialCommand(serialCommand);

      commands[0] = commands[0].Replace("%1", currentChannelDigit.ToString());
      commands[0] = commands[0].Replace("%2", fullChannelString);

      Common.ProcessSerialCommand(commands);
    }

    #region Buttons

    private void buttonOK_Click(object sender, EventArgs e)
    {
      try
      {
        foreach (StbSetup setup in _tvCardStbSetups)
        {
          setup.Save();
          TV2BlasterPlugin.GetExternalChannelConfig(setup.CardId).Save();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), "Failed to save external channel setup", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private void buttonTest_Click(object sender, EventArgs e)
    {
      try
      {
        StbSetup setup = _tvCardStbSetups[tabControlTVCards.SelectedIndex];

        int channelTest = Decimal.ToInt32(numericUpDownTest.Value);
        string channel;
        switch (setup.ChannelDigits)
        {
          case 2:
            channel = channelTest.ToString("00");
            break;

          case 3:
            channel = channelTest.ToString("000");
            break;

          case 4:
            channel = channelTest.ToString("0000");
            break;

          default:
            channel = channelTest.ToString();
            break;
        }

        int charVal;
        string command;

        for (int repeatCount = 0; repeatCount <= setup.RepeatChannelCommands; repeatCount++)
        {
          if (repeatCount > 0 && setup.RepeatPauseTime > 0)
            Thread.Sleep(setup.RepeatPauseTime);

          if (setup.UsePreChangeCommand && !String.IsNullOrEmpty(setup.PreChangeCommand))
          {
            TV2BlasterPlugin.ProcessExternalCommand(setup.PreChangeCommand, -1, channel);

            if (setup.PauseTime > 0)
              Thread.Sleep(setup.PauseTime);
          }

          foreach (char digit in channel)
          {
            charVal = digit - 48;

            command = setup.Digits[charVal];
            if (!String.IsNullOrEmpty(command))
            {
              TV2BlasterPlugin.ProcessExternalCommand(command, charVal, channel);

              if (setup.PauseTime > 0)
                Thread.Sleep(setup.PauseTime);
            }
          }

          if (setup.SendSelect && !String.IsNullOrEmpty(setup.SelectCommand))
          {
            TV2BlasterPlugin.ProcessExternalCommand(setup.SelectCommand, -1, channel);

            if (setup.DoubleChannelSelect)
            {
              if (setup.PauseTime > 0)
                Thread.Sleep(setup.PauseTime);

              TV2BlasterPlugin.ProcessExternalCommand(setup.SelectCommand, -1, channel);
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), "Failed to test external channel", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void buttonQuickSet_Click(object sender, EventArgs e)
    {
      string quickSetup = comboBoxQuickSetup.Text;

      if (String.IsNullOrEmpty(quickSetup))
        return;

      try
      {
        _tvCardStbSetups[tabControlTVCards.SelectedIndex].SetToXml(quickSetup);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), "Failed to quick-set external channel setup", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
    }

    private void buttonCopyFrom_Click(object sender, EventArgs e)
    {
      try
      {
        _tvCardStbSetups[tabControlTVCards.SelectedIndex].SetToCard(comboBoxCopyFrom.SelectedIndex + 1);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString(), "Failed to copy external channel setup", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    #endregion Buttons
  }
}