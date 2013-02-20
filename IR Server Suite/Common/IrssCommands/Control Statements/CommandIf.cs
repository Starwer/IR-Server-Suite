using System;
using System.Windows.Forms;

namespace IrssCommands
{
  /// <summary>
  /// If Statement macro command.
  /// </summary>
  public class CommandIf : Command
  {
    #region Comparisons

    // String comparisons ...
    internal const string IfContains = "CONTAINS";
    internal const string IfEndsWith = "ENDS WITH";
    internal const string IfEquals = "==";

    // Integer comparisons ...
    internal const string IfGreaterThan = ">";
    internal const string IfGreaterThanOrEqual = ">=";
    internal const string IfLessThan = "<";
    internal const string IfLessThanOrEqual = "<=";
    internal const string IfNotEqual = "!=";
    internal const string IfStartsWith = "STARTS WITH";

    #endregion Comparisons

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandIf"/> class.
    /// </summary>
    public CommandIf()
    {
      InitParameters(5);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandIf"/> class.
    /// </summary>
    /// <param name="parameters">The parameters.</param>
    public CommandIf(string[] parameters) : base(parameters)
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
      get { return Processor.CategoryControl; }
    }

    /// <summary>
    /// Gets the user interface text.
    /// </summary>
    /// <value>User interface text.</value>
    public override string UserInterfaceText
    {
      get { return "If Statement"; }
    }

    /// <summary>
    /// Gets the user display text.
    /// </summary>
    /// <value>The user display text.</value>
    public override string UserDisplayText
    {
      get
      {
        if (String.IsNullOrEmpty(Parameters[4]))
          return String.Format("If ({0} {1} {2}) then goto \"{3}\"", Parameters[0], Parameters[1], Parameters[2],
                               Parameters[3]);
        else
          return String.Format("If ({0} {1} {2}) then goto \"{3}\" else goto \"{4}\"", Parameters[0], Parameters[1],
                               Parameters[2], Parameters[3], Parameters[4]);
      }
    }

    /// <summary>
    /// Gets the edit control to be used within a common edit form.
    /// </summary>
    /// <returns>The edit control.</returns>
    public override BaseCommandConfig GetEditControl()
    {
      return new IfConfig(Parameters);
    }

    #endregion Implementation

    #region Static Methods

    /// <summary>
    /// This method will determine if an If Statement evaluates true.
    /// </summary>
    /// <param name="value1">The first value for comparison.</param>
    /// <param name="comparison">The comparison type.</param>
    /// <param name="value2">The second value for comparison.</param>
    /// <returns><c>true</c> if the parameters evaluates true, otherwise <c>false</c>.</returns>
    public static bool Evaluate(string value1, string comparison, string value2)
    {
      int value1AsInt;
      bool value1IsInt = int.TryParse(value1, out value1AsInt);

      int value2AsInt;
      bool value2IsInt = int.TryParse(value2, out value2AsInt);

      bool comparisonResult = false;
      switch (comparison.ToUpperInvariant())
      {
          // Use string comparison ...
        case IfEquals:
          comparisonResult = value1.Equals(value2, StringComparison.OrdinalIgnoreCase);
          break;
        case IfNotEqual:
          comparisonResult = !value1.Equals(value2, StringComparison.OrdinalIgnoreCase);
          break;
        case IfContains:
          comparisonResult = value1.ToUpperInvariant().Contains(value2.ToUpperInvariant());
          break;
        case IfStartsWith:
          comparisonResult = value1.StartsWith(value2, StringComparison.OrdinalIgnoreCase);
          break;
        case IfEndsWith:
          comparisonResult = value1.EndsWith(value2, StringComparison.OrdinalIgnoreCase);
          break;

          // Use integer comparison ...
        case IfGreaterThan:
          if (value1IsInt && value2IsInt)
            comparisonResult = (value1AsInt > value2AsInt);
          break;

        case IfLessThan:
          if (value1IsInt && value2IsInt)
            comparisonResult = (value1AsInt < value2AsInt);
          break;

        case IfGreaterThanOrEqual:
          if (value1IsInt && value2IsInt)
            comparisonResult = (value1AsInt >= value2AsInt);
          break;

        case IfLessThanOrEqual:
          if (value1IsInt && value2IsInt)
            comparisonResult = (value1AsInt <= value2AsInt);
          break;

        default:
          throw new CommandStructureException(String.Format("Invalid variable comparison method: {0}", comparison));
      }

      return comparisonResult;
    }

    #endregion Static Methods
  }
}