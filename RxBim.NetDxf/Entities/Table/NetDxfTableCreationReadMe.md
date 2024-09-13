На данный момент таблица записывается в файл, но не читается. 

Таблица имеет следующие свойства:
* Layer - задается слой таблицы
* Position - позиция
* Direction - направление по X или Y
* RowsCount - количество строк, по умолчанию стоит 1, чтобы не ломался dxf документ
* ColumnsCount - количество столбцов, по умолчанию стоит 1, чтобы не ломался dxf документ
* Cells - список всех ячеек таблицы
* RowHeights - список высоты строк, должно соответствовать их кол-ву
* ColumnWidths - список ширины строк, должно соответствовать их кол-ву
* TextStyle - стиль текста, по умолчанию стоит Standard, чтобы не было исключения
* HorizontalMargin - горизонтальный марджин всех ячеек в таблице
* VerticalMargin - вертикальный марджин всех ячеек в таблице
* Block - нужен временно для устранения ошибки чтения документа. После изменения реализации метода ReadAcadTable в классе DxfReader нужно будет удалить.


Таблица состоит из ячеек, которые имеют следующие свойства:
* Type - тип ячейки (Текст или блок)
* Alignment - выравнивание контента внутри ячейки
* Rotation - поворот контента ячейки
* Text - значение текста
* TextHeight - высота текста
* Insert - инсерт с блоком для ячейки с типом “Блок“
* IsMerged - признак объединенной ячейки, по умолчанию false
* MergedCellBorderWidth - количество горизонтально объединенных ячеек
* MergedCellBorderHeight - количество вертикально объединенных ячеек

Несколько фишек:

* Можно не задавать ширину и высоту ячеек, а задать только марджины, тогда ячейки сами подстроятся под контент и выровняются по наибольшей ширине и высоте
* Если в ячейках только текстовые данные, то можно не задавать тип ячейки
* Если данные в ячейках всегда по центру, то можно не задавать Allignment
* Можно задать одно значение для RowHeights, которое применится ко всем строкам в таблице
* Можно задать одно значение для ColumnWidths, которое применится ко всем строкам в таблице
* Direction по умолчанию стоит по Х
* Rotation по умолчанию будет 0, т.е. по Х

Для начала создадим пустой список, куда будем добавлять ячейки,
и билдер ячеек, чтобы не пересоздавать его каждый раз.

    var cells = new List<Cell>();
    var cellBuilder = CellBuilder.Empty();

Создаём данные для ячеек заголовка. Так как они объединённые, в первой должно быть указано количество объединяемых ячеек
по горизонтали, в остальных 1.

    var titleCellsData = new HeaderCellInformation[]
    {
        new("Название таблицы", 5),
        new(string.Empty, 1),
        new(string.Empty, 1),
        new(string.Empty, 1),
        new(string.Empty, 1)
    };

Добавляем в список. У всех ячеек признак объединения будет стоять в true.
    
    cells.AddRange(
        titleCellsData.Select(
            data => cellBuilder
                .IsMerged(true)
                .WithHorizontalMergedCellsCount(data.MergedCellsCount)
                .WithText(data.Text)
                .WithTextHeight(250)
                .Build()));

Создаём ячейки для заголовков столбцов. Здесь просто строковые значения.
    
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

Дальше идут ячейки с данными. 
Здесь указываем тип ячейки, содержимое ячейки (текст либо Insert, который содержит Block),
а так же признак объединения ячеек и их количество, и угол поворота содержимого ячейки.

В таблице 5 колонок, следовательно, первые 5 ячеек запишутся по порядку в первую строку, вторые 5 - во вторую.
Это нужно учитывать при вертикальном объединении ячеек.

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

Создаём таблицу. Здесь указываем слой, позицию, количество строк, включая заголовок, и названия колонок.
Высоту строк указываем для каждой строки, либо можно задать одну для всех строк. То же самое и для ширины колонок.
В данном примере высота строк будет разная, а ширина колонок одинаковая.

Дальше передаем выше созданные ячейки и стиль текста.
Можно задать вертикальный и горизонтальный марджин для всей таблицы.
Также можно указать Direction, если надо развернуть, по оси Y. По дефолту установлено в UnitX.

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

Рекорды для ячеек, в которых хранится их описание.
    
    private record HeaderCellInformation(string Text, int MergedCellsCount);

    private record CellInformation(CellType Type, Insert Insert, string Text, bool IsMerged, int MergedCellsCount, double Rotation);
 

