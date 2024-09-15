using netDxf.Tables;

namespace netDxf.Entities.Table;

/// <summary>
/// Creates a <see cref="Table"/>.
/// </summary>
public sealed class TableBuilder
{
    private Layer layer;
    private Vector3 position;
    private Vector3 direction = Vector3.UnitX;
    private int rowsCount = 1;
    private int columnsCount = 1;
    private readonly List<Cell> cells = [];
    private readonly List<double> rowHeights = [];
    private readonly List<double> columnWidths = [];
    private TextStyle textStyle;
    private double horizontalMargin;
    private double verticalMargin;
    
    private TableBuilder()
    {
    }
    
    /// <summary>
    /// Creates a <see cref="TableBuilder"/>.
    /// </summary>
    /// <returns></returns>
    public static TableBuilder Empty() => new();
    
    /// <summary>
    /// Sets the table layer.
    /// </summary>
    /// <param name="value">Layer.</param>
    public TableBuilder WithLayer(Layer value)
    {
        layer = value;
        return this;
    }
    
    /// <summary>
    /// Sets the table position.
    /// </summary>
    /// <param name="value">Position.</param>
    public TableBuilder WithPosition(Vector2 value)
    {
        position = new Vector3(value.X, value.Y, 0.0);
        return this;
    }
    
    /// <summary>
    /// Sets the table direction. <see cref="Vector3.UnitX"/> - default.
    /// </summary>
    /// <param name="value">Direction.</param>
    public TableBuilder WithDirection(Vector2 value)
    {
        direction = new Vector3(value.X, value.Y, 0.0);;
        return this;
    }
        
    /// <summary>
    /// Sets the table rows count. Title + header row + other rows.
    /// </summary>
    /// <param name="value">Rows count.</param>
    public TableBuilder WithRowsCount(int value)
    {
        rowsCount = value;
        return this;
    }
    
    /// <summary>
    /// Sets the table columns count.
    /// </summary>
    /// <param name="value">Columns count.</param>
    public TableBuilder WithColumnsCount(int value)
    {
        columnsCount = value;
        return this;
    }
    
    /// <summary>
    /// Adds the table cells.
    /// </summary>
    /// <param name="value">Collection of <see cref="Cell"/>.</param>
    public TableBuilder WithCells(IReadOnlyCollection<Cell> value)
    {
        cells.AddRange(value);
        return this;
    }

    /// <summary>
    /// Sets the table row heights. 1 value per row or one value for all rows.
    /// </summary>
    /// <param name="value">Collection of row heights.</param>
    public TableBuilder WithRowHeights(IReadOnlyList<double> value)
    {
        rowHeights.AddRange(value);
        return this;
    }
    
    /// <summary>
    /// Sets the table column widths. 1 value per row or one value for all rows.
    /// </summary>
    /// <param name="value">Collection of column widths.</param>
    public TableBuilder WithColumnWidths(IReadOnlyList<double> value)
    {
        columnWidths.AddRange(value);
        return this;
    }
    
    /// <summary>
    /// Sets the table text style.
    /// </summary>
    /// <param name="value">Text style.</param>
    public TableBuilder WithTextStyle(TextStyle value)
    {
        textStyle = value;
        return this;
    }
    
    /// <summary>
    /// Sets the table cells horizontal margin.
    /// </summary>
    /// <param name="value">Horizontal margin.</param>
    public TableBuilder WithHorizontalMargin(double value)
    {
        horizontalMargin = value;
        return this;
    }
    
    /// <summary>
    /// Sets the table cells vertical margin.
    /// </summary>
    /// <param name="value">Vertical margin.</param>
    public TableBuilder WithVerticalMargin(double value)
    {
        verticalMargin = value;
        return this;
    }

    /// <summary>
    /// Creates new instance of the <see cref="Table"/>.
    /// </summary>
    public Table Build()
    {
        return new Table
        {
            Layer = layer,
            Position = position,
            Direction = direction,
            RowsCount = rowsCount,
            ColumnsCount = columnsCount,
            Cells = cells,
            RowHeights = rowHeights,
            ColumnWidths = columnWidths,
            TextStyle = textStyle,
            HorizontalMargin = horizontalMargin,
            VerticalMargin = verticalMargin
        };
    }
}