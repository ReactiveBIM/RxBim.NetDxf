namespace netDxf.Entities.Table;

/// <summary>
/// Creates a <see cref="Cell"/>.
/// </summary>
public sealed class CellBuilder
{
    private CellType type = CellType.Text;
    private CellAlignment alignment = CellAlignment.MiddleCenter;
    private double rotation;
    private string text = string.Empty;
    private double textHeight;
    private Insert insert;
    private bool isMerged;
    private int horizontalMergedCellsCount = 1;
    private int verticalMergedCellsCount = 1;
    
    private CellBuilder()
    {
    }
    
    /// <summary>
    /// Creates a <see cref="CellBuilder"/>.
    /// </summary>
    public static CellBuilder Empty() => new();

    /// <summary>
    /// Sets the cell type.
    /// </summary>
    /// <param name="value"><see cref="CellType"/>.</param>
    public CellBuilder WithType(CellType value)
    {
        type = value;
        return this;
    }
    
    /// <summary>
    /// Sets the cell alignment.
    /// </summary>
    /// <param name="value"><see cref="CellAlignment"/>.</param>
    public CellBuilder WithAlignment(CellAlignment value)
    {
        alignment = value;
        return this;
    }
    
    /// <summary>
    /// Sets the cell rotation.
    /// </summary>
    /// <param name="value">Rotation.</param>
    public CellBuilder WithRotation(double value)
    {
        rotation = value;
        return this;
    }
    
    /// <summary>
    /// Sets the cell text.
    /// </summary>
    /// <param name="value">Text.</param>
    public CellBuilder WithText(string value)
    {
        text = value;
        return this;
    }
    
    /// <summary>
    /// Sets the cell text height.
    /// </summary>
    /// <param name="value">Text.</param>
    public CellBuilder WithTextHeight(double value)
    {
        textHeight = value;
        return this;
    }

    /// <summary>
    /// Sets the cell insert.
    /// </summary>
    /// <param name="value"><see cref="Insert"/>.</param>
    public CellBuilder WithInsert(Insert value)
    {
        insert = value;
        return this;
    }
    
    /// <summary>
    /// Sets the flag whether the cell is merged.
    /// </summary>
    public CellBuilder IsMerged(bool value)
    {
        isMerged = value;
        return this;
    }
    
    /// <summary>
    /// Sets horizontal merged cells count.
    /// </summary>
    public CellBuilder WithHorizontalMergedCellsCount(int value)
    {
        horizontalMergedCellsCount = value;
        return this;
    }
    
    /// <summary>
    /// Sets vertical merged cells count.
    /// </summary>
    public CellBuilder WithVerticalMergedCellsCount(int value)
    {
        verticalMergedCellsCount = value;
        return this;
    }

    /// <summary>
    /// Creates new instance of the <see cref="Cell"/>.
    /// </summary>
    public Cell Build()
    {
        return new Cell
        {
            Type = type,
            Alignment = alignment,
            Rotation = rotation,
            Text = text,
            TextHeight = textHeight,
            Insert = insert,
            IsMerged = isMerged,
            HorizontalMergedCount = horizontalMergedCellsCount,
            VerticalMergedCount = verticalMergedCellsCount
        };
    }
}