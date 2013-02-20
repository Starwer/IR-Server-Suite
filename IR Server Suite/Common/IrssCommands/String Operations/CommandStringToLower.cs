using System.Globalization;
using System.Windows.Forms;

namespace IrssCommands
{
  /// <summary>
  /// String To Lower macro command.
  /// </summary>
  public class CommandStringToLower : Command
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandStringToLower"/> class.
    /// </summary>
    public CommandStringToLower()
    {
      InitParameters(2);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandStringToLower"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public CommandStringToLower(string[] parameters) : base(parameters)
    {
    }

    #endregion Constructors

    #region Implementation

    /// <summary>
    /// Gets the category of this command.
    /// </summary>
    /// <value>The category of this command.</value>
    public override string Category
    {
      get { return Processor.CategoryString; }
    }

    /// <summary>
    /// Gets the user interface text.
    /// </summary>
    /// <value>User interface text.</value>
    public override string UserInterfaceText
    {
      get { return "String To Lower"; }
    }

    /// <summary>
    /// Gets the edit control to be used within a common edit form.
    /// </summary>
    /// <returns>The edit control.</returns>
    public override BaseCommandConfig GetEditControl()
    {
      return new MathsStringConfig(Parameters);
    }

    /// <summary>
    /// Execute this command.
    /// </summary>
    /// <param name="variables">The variable list of the calling code.</param>
    public override void Execute(VariableList variables)
    {
      string[] processed = ProcessParameters(variables, Parameters);

      string output = processed[0].ToLower(CultureInfo.CurrentCulture);

      variables.VariableSet(processed[1], output);
    }

    #endregion Implementation
  }
}