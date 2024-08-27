namespace netDxf.Entities.Table;

/// <summary>
/// Represents cell in table <see cref="Table"></see>.
/// </summary>
public sealed class Cell
{
    /// <summary>
    /// The beginning of the cell value.
    /// </summary>
    public const string CellValueBegin = "CELL_VALUE";
    
    /// <summary>
    /// The end of the cell value.
    /// </summary>
    public const string CellValueEnd = "ACVALUE_END";

    /// <summary>
    /// The default value of the cell <see cref="Insert"></see>.
    /// </summary>
    public const double DefaultScale = 1;

    /// <summary>
    /// Gets or sets the cell type.
    /// </summary>
    public CellType Type { get; set; }

    /// <summary>
    /// Gets or sets the cell alignment.
    /// </summary>
    public CellAlignment Alignment { get; set; }
    
    /// <summary>
    /// Gets or sets the content rotation.
    /// </summary>
    public double Rotation { get; set; }

    /// <summary>
    /// Gets or sets the text in the cell.
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Gets or sets the <see cref="Insert"></see> in the cell.
    /// </summary>
    public Insert Insert { get; set; }

    /// <summary>
    /// Gets or sets the text height.
    /// </summary>
    public double TextHeight { get; set; }

    /// <summary>
    /// Gets or sets the flag whether the cell is merged.
    /// </summary>
    public bool IsMerged { get; set; }

    /// <summary>
    /// Gets or sets horizontal merged cells count (applicable only for merged cells).
    /// </summary>
    public int HorizontalMergedCount { get; set; }

    /// <summary>
    /// Gets or sets vertical merged cells count (applicable only for merged cells).
    /// </summary>
    public int VerticalMergedCount { get; set; }
}