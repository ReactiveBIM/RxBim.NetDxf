using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities.Table;

/// <summary>
/// Represents a table <see cref="EntityObject">entity</see>.
/// </summary>
public sealed class Table : EntityObject
{
    private TextStyle textStyle;
    
    /// <summary>
    /// Initializes a new instance of the <c>Table</c> class.
    /// </summary>
    public Table() : base(EntityType.Table, DxfObjectCode.AcadTable)
    {
    }
    
    /// <summary>
    /// Gets or sets the table block.
    /// </summary>
    public Block Block { get; set; }

    /// <summary>
    /// Gets or sets the table position in world coordinates.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets or sets the table direction.
    /// </summary>
    public Vector3 Direction { get; set; }

    /// <summary>
    /// Gets or sets the table rows count.
    /// </summary>
    public int RowsCount { get; set; }

    /// <summary>
    /// Gets or sets the table columns count.
    /// </summary>
    public int ColumnsCount { get; set; }

    /// <summary>
    /// Gets or sets the table cells.
    /// </summary>
    public List<Cell> Cells { get; set; } = [];
    
    /// <summary>
    /// Gets or sets row height; this value is repeated, 1 value per row or one value for all rows.
    /// </summary>
    public IReadOnlyList<double> RowHeights { get; set; } = [];
    
    /// <summary>
    /// Gets or sets column width; this value is repeated, 1 value per row or one value for all columns.
    /// </summary>
    public IReadOnlyList<double> ColumnWidths { get; set; } = [];

    /// <summary>
    /// Gets or sets the <see cref="TextStyle"></see>.
    /// </summary>
    public TextStyle TextStyle
    {
        get => textStyle;
        set
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            textStyle = OnTextStyleChangedEvent(textStyle, value);
        }
    }
    
    /// <summary>
    /// Gets or sets the table cells horizontal margin.
    /// </summary>
    public double HorizontalMargin { get; set; }
    
    /// <summary>
    /// Gets or sets the table cells vertical margin.
    /// </summary>
    public double VerticalMargin { get; set; }
    
    /// <summary>
    /// Delegate TextStyleChangedEventHandler.
    /// </summary>
    public delegate void TextStyleChangedEventHandler(Table sender, TableObjectChangedEventArgs<TextStyle> e);
    
    /// <summary>
    /// Event TextStyleChangedEventHandler.
    /// </summary>
    public event TextStyleChangedEventHandler TextStyleChanged;
    
    /// <summary>
    /// Changes the table text style.
    /// </summary>
    private TextStyle OnTextStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
    {
        var textStyleChangedEventHandler = TextStyleChanged;
        if (textStyleChangedEventHandler == null)
            return newTextStyle;
        
        var eventArgs = new TableObjectChangedEventArgs<TextStyle>(oldTextStyle, newTextStyle);
        textStyleChangedEventHandler(this, eventArgs);
        
        return eventArgs.NewValue;
    }

    /// <summary>
    /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
    /// </summary>
    /// <param name="transformation">Transformation matrix.</param>
    /// <param name="translation">Translation vector.</param>
    /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
    public override void TransformBy(Matrix3 transformation, Vector3 translation)
    {
        var newPosition = transformation * Position + translation;
        var newNormal = transformation * Normal;
        if (Vector3.Equals(Vector3.Zero, newNormal))
        {
            newNormal = Normal;
        }

        Position = newPosition;
        Normal = newNormal;
    }

    /// <summary>
    /// Creates a new Table that is a copy of the current instance.
    /// </summary>
    /// <returns>A new Table that is a copy of this instance.</returns>
    public override object Clone()
    {
        var entity = new Table
        {
            // EntityObject properties
            Layer = (Layer) Layer.Clone(),
            Linetype = (Linetype) Linetype.Clone(),
            Color = (AciColor) Color.Clone(),
            Lineweight = Lineweight,
            Transparency = (Transparency) Transparency.Clone(),
            LinetypeScale = LinetypeScale,
            Normal = Normal,
            IsVisible = IsVisible,
            
            // Table properties
            Position = Position,
            Direction = Direction,
            RowsCount = RowsCount,
            ColumnsCount = ColumnsCount,
            Cells = Cells,
            RowHeights = RowHeights,
            ColumnWidths = ColumnWidths,
            TextStyle = (TextStyle)textStyle.Clone(),
            HorizontalMargin = HorizontalMargin,
            VerticalMargin = VerticalMargin
        };
        
        foreach (var data in XData.Values)
        {
            entity.XData.Add((XData) data.Clone());
        }

        return entity;
    }
}