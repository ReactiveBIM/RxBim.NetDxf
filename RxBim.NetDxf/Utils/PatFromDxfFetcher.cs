using System.Globalization;

namespace netDxf.Utils;

/// <summary>
/// Extracts the content of the pat file from a dxf file.
/// </summary>
public class PatFromDxfFetcher
{
    private const string AngleParameter = "53";
    private const string BasePointXParameter = "43";
    private const string BasePointYParameter = "44";
    private const string OffsetXParameter = "45";
    private const string OffsetYParameter = "46";
    private const string DashLengthCounterParameter = "79";
    private const string DashLengthParameter = "49";
    private const string LinesCountParameter = "78";
    private const string FloatFormat = "0.000";

    /// <summary>
    /// Extracts the content of the pat file from a Stream containing a dxf file.
    /// </summary>
    /// <param name="dxfDocument">Stream containing the dxf file.</param>
    /// <param name="hatchNameFromDxf">Hatch name from the dxf file.</param>
    /// <param name="newHatchName">New hatch name.</param>
    public Stream FetchPat(Stream dxfDocument, string hatchNameFromDxf, string newHatchName)
    {
        var hatchPattern = GetHatchPattern(dxfDocument, hatchNameFromDxf, newHatchName);
        var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        foreach (var hatchLine in hatchPattern)
        {
            streamWriter.WriteLineAsync(hatchLine);
        }

        return memoryStream;
    }

    /// <summary>
    /// Extracts the content of the pat file from a dxf file and put it to specified file.
    /// </summary>
    /// <param name="dxfFilePath">Dxf file path.</param>
    /// <param name="patFilePath">Pat file path.</param>
    /// <param name="hatchNameFromDxf">Hatch name from the dxf file.</param>
    /// <param name="newHatchName">New hatch name.</param>
    public void FetchPat(string dxfFilePath, string patFilePath, string hatchNameFromDxf, string newHatchName)
    {
        using var fsSource = new FileStream(dxfFilePath, FileMode.Open, FileAccess.Read);
        var hatchPattern = GetHatchPattern(fsSource, hatchNameFromDxf, newHatchName);
        using var streamWriter = new StreamWriter(patFilePath);
        foreach (var hatchLine in hatchPattern)
        {
            streamWriter.WriteLineAsync(hatchLine);
        }
    }

    private List<string> GetHatchPattern(Stream dxfStream, string hatchNameFromDxf, string newHatchName)
    {
        var res = new List<string> { GenerateName(newHatchName) };

        var lines = ReadLines(new StreamReader(dxfStream));
        var linesEnumerator = lines.GetEnumerator();
        MoveToHatchLabel(hatchNameFromDxf, linesEnumerator);
        var hatchPatternLinesCount = GetHatchPatternLines(linesEnumerator);

        linesEnumerator.MoveNext();
        for (var i = 0; i < hatchPatternLinesCount; i++)
        {
            res.Add(GetHatchPatternLine(linesEnumerator));
        }

        return res;
    }

    private void MoveToHatchLabel(string hatchNameFromDxf, IEnumerator<string> linesEnumerator)
    {
        while (linesEnumerator.MoveNext())
        {
            var current = GetCurrentOrThrow(linesEnumerator).Trim();
            if (current == hatchNameFromDxf)
            {
                break;
            }
        }
    }

    private int GetHatchPatternLines(IEnumerator<string> linesEnumerator)
    {
        while (linesEnumerator.MoveNext())
        {
            var current = GetCurrentOrThrow(linesEnumerator).Trim();
            if (current == LinesCountParameter)
            {
                break;
            }
        }

        linesEnumerator.MoveNext();
        return int.Parse(GetCurrentOrThrow(linesEnumerator).Trim());
    }

    private string GetHatchPatternLine(IEnumerator<string> linesEnumerator)
    {
        var listOfNumbers = new List<string>();

        var angle = ProcessParameterValue(linesEnumerator, listOfNumbers, AngleParameter);
        var angleInRadians = DegreesToRadians(angle);
        ProcessParameterValue(linesEnumerator, listOfNumbers, BasePointXParameter);
        ProcessParameterValue(linesEnumerator, listOfNumbers, BasePointYParameter);
        var offsetX = ProcessParameterValue(linesEnumerator, listOfNumbers, OffsetXParameter, needToAppend: false);
        var offsetY = ProcessParameterValue(linesEnumerator, listOfNumbers, OffsetYParameter, needToAppend: false);
        var shift = (offsetX * Math.Cos(angleInRadians) + offsetY * Math.Sin(angleInRadians)).ToString(FloatFormat,
            CultureInfo.InvariantCulture);
        var offset = (offsetY * Math.Cos(angleInRadians) - offsetX * Math.Sin(angleInRadians)).ToString(FloatFormat,
            CultureInfo.InvariantCulture);
        listOfNumbers.Add(shift);
        listOfNumbers.Add(offset);

        ProcessParameterValue(linesEnumerator, listOfNumbers, DashLengthCounterParameter, needToAppend: false);
        ProcessParameterValue(linesEnumerator, listOfNumbers, DashLengthParameter);
        ProcessParameterValue(linesEnumerator, listOfNumbers, DashLengthParameter);

        return string.Join(", ", listOfNumbers);
    }

    private float ProcessParameterValue(
        IEnumerator<string> linesEnumerator,
        List<string> listOfNumbers,
        string parameterName,
        bool needToAppend = true)
    {
        if (GetCurrentOrThrow(linesEnumerator).Trim() != parameterName)
        {
            throw new InvalidOperationException($"{parameterName} expected");
        }

        linesEnumerator.MoveNext();
        var paramValue = GetCurrentOrThrow(linesEnumerator).Trim();
        var floatVal = float.Parse(paramValue, CultureInfo.InvariantCulture);
        if (needToAppend)
        {
            listOfNumbers.Add(floatVal.ToString(FloatFormat, CultureInfo.InvariantCulture));
        }

        linesEnumerator.MoveNext();
        return floatVal;
    }

    private string GenerateName(string newHatchName) => $"*{newHatchName}, description placeholder";

    private IEnumerable<string> ReadLines(StreamReader streamReader)
    {
        while (!streamReader.EndOfStream)
        {
            yield return streamReader.ReadLine()!;
        }
    }
    
    private string GetCurrentOrThrow(IEnumerator<string> linesEnumerator)
    {
        if (linesEnumerator.Current == null)
        {
            throw new InvalidOperationException("The file stream was unexpectedly interrupted");
        }
        return linesEnumerator.Current;
    }

    private float DegreesToRadians(float angleInDegrees)
    {
        return (float)(Math.PI / 180 * angleInDegrees);
    }
}