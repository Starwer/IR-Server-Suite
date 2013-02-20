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

using System.Windows.Forms;

namespace IrssCommands.General
{
  /// <summary>
  /// Popup Message command.
  /// </summary>
  public class PopupCommand : Command
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupCommand"/> class.
    /// </summary>
    public PopupCommand()
    {
      InitParameters(3);

      // setting default values
      Parameters[0] = "Hello %USERNAME%.";
      Parameters[1] = "This is your message at %$TIME$%.";
      Parameters[2] = 10.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PopupCommand"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public PopupCommand(string[] parameters) : base(parameters)
    {
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Gets the category of this command.
    /// </summary>
    /// <value>The category of this command.</value>
    public override string Category
    {
      get { return "General Commands"; }
    }

    /// <summary>
    /// Gets the user interface text.
    /// </summary>
    /// <value>User interface text.</value>
    public override string UserInterfaceText
    {
      get { return "Popup Message"; }
    }

    /// <summary>
    /// Gets the edit control to be used within a common edit form.
    /// </summary>
    /// <returns>The edit control.</returns>
    public override BaseCommandConfig GetEditControl()
    {
      return new PopupConfig(Parameters);
    }

    /// <summary>
    /// Execute this command.
    /// </summary>
    /// <param name="variables">The variable list of the calling code.</param>
    public override void Execute(VariableList variables)
    {
      string[] processed = ProcessParameters(variables, Parameters);

      int timeout = int.Parse(processed[2]);

      PopupMessage popup = new PopupMessage(processed[0], processed[1], timeout);
      popup.ShowDialog();
    }

    #endregion Public Methods
  }
}