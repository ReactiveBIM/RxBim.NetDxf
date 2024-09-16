At the moment, the table may be written to a file without a table block definition but not read.

The table has the following properties:
* Layer: specifies the table layer
* Position: position
* Direction: direction along X or Y
* RowsCount: number of rows, default is 1 so as not to break the DXF document
* ColumnsCount: number of columns, default is 1 so as not to break the DXF document
* Cells: list of all table cells
* RowHeights: list of row heights, must match the number of rows
* ColumnWidths: list of column widths, must match the number of columns
* TextStyle: text style, default is "Standard" to avoid exceptions
* HorizontalMargin: horizontal margin for all table cells
* VerticalMargin: vertical margin for all table cells
* Block: temporarily needed to fix document reading error. It will need to be removed after changing the implementation of the ReadAcadTable method in the DxfReader class.


The table consists of cells with the following properties:
* Type: type of cell (Text or Block)
* Alignment: content alignment inside the cell
* Rotation: content rotation of the cell
* Text: text value
* TextHeight: text height
* Insert: insert with a block for the cell of type “Block”
* IsMerged: merged cell indicator, default is false
* MergedCellBorderWidth: number of horizontally merged cells
* MergedCellBorderHeight: number of vertically merged cells

A few features:
* You can omit specifying the width and height of the cells, and only set margins, then the cells will automatically adjust to the content and align to the maximum width and height.
* If the cells contain only text data, you can omit the cell type.
* If the data in the cells is always centered, you can omit the Alignment.
* You can set a single value for RowHeights that will apply to all rows in the table.
* You can set a single value for ColumnWidths that will apply to all columns in the table.
* Direction is set to X by default.
* Rotation is 0 by default, i.e., aligned with the X-axis.

First, let's create an empty list to add cells to and a cell builder so that we don't have to recreate it every time.

    var cells = new List<Cell>();
    var cellBuilder = CellBuilder.Empty();

Creating data for the header cells. Since they are merged, the first one should indicate the number of horizontally merged cells, while the others should be set to 1.

    var titleCellsData = new HeaderCellInformation[]
    {
        new("Название таблицы", 5),
        new(string.Empty, 1),
        new(string.Empty, 1),
        new(string.Empty, 1),
        new(string.Empty, 1)
    };

Adding them to the list. All cells will have the IsMerged property set to true.    
    cells.AddRange(
        titleCellsData.Select(
            data => cellBuilder
                .IsMerged(true)
                .WithHorizontalMergedCellsCount(data.MergedCellsCount)
                .WithText(data.Text)
                .WithTextHeight(250)
                .Build()));

Creating cells for column headers. These are just string values.    

    var headerRowCellsData = new[] 
    { 
        "Заголовок", 
        "Заголовок",
        "Заголовок",
        "Заголовок",
        "Заголовок" 
    };

    cells.AddRange(
        headerRowCellsData.Select(
            data => cellBuilder
                .WithText(data)
                .WithTextHeight(250)
                .Build()));

Next come the data cells. Here we specify the cell type, cell content (text or Insert containing a Block), as well as the merged cell indicator, the number of merged cells, and the content rotation angle.
The table has 5 columns, so the first 5 cells will be written to the first row, and the next 5 to the second row. This should be taken into account for vertical cell merging.

        var cellsData = new CellInformation[]
        {
            new(CellType.Text, null!, "Вертикальные данные на 2 ячейки", true, 2, 90),
            new(CellType.Text, null!, "Данные", false, 1, 0),
            new(CellType.Text, null!, string.Empty, false, 1, 0),
            new(CellType.Text, null!, "Данные", false, 1, 90),
            new(CellType.Block, new Insert(), string.Empty, false, 1, 0),
            new(CellType.Text, null!, string.Empty, true, 1, 0),
            new(CellType.Text, null!, string.Empty, false, 1, 0),
            new(CellType.Text, null!, "Данные", false, 1, 0),
            new(CellType.Block, new Insert(), string.Empty, false, 1, 0),
            new(CellType.Text, null!, "Данные", false, 1, 90)
        };

        cells.AddRange(
            cellsData.Select(
                data => cellBuilder
                    .WithType(data.Type)
                    .IsMerged(data.IsMerged)
                    .WithVerticalMergedCellsCount(data.MergedCellsCount)
                    .WithText(data.Text)
                    .WithTextHeight(250)
                    .WithInsert(data.Insert)
                    .WithRotation(data.Rotation)
                    .Build()));

Creating the table. Here, we specify the layer, position, the number of rows (including the header), and the column names. Row heights are specified for each row, or you can set one value for all rows. The same applies to column widths. In this example, the row heights will be different, and the column widths will be the same.

We then pass the previously created cells and text style. You can set the vertical and horizontal margins for the entire table. You can also specify the Direction if you need to rotate along the Y-axis. By default, it is set to UnitX.

    document.Add(() =>
        TableBuilder
            .Empty()
            .WithLayer(new Layer("Пример таблицы"))
            .WithPosition(new netDxf.Vector2(0, 0))
            .WithRowsCount(4)
            .WithColumnsCount(5)
            .WithRowHeights([500, 1000, 1500, 2000])
            .WithColumnWidths([3000])
            .WithCells(cells)
            .WithTextStyle(textStyle)
            .Build());

Records for cells that store their descriptions:
    
    private record HeaderCellInformation(string Text, int MergedCellsCount);

    private record CellInformation(CellType Type, Insert Insert, string Text, bool IsMerged, int MergedCellsCount, double Rotation);
 

