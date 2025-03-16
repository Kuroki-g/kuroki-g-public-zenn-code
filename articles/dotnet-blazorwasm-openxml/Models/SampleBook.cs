using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;

namespace Models;

sealed class SampleBook : IDisposable
{
    private SampleBook(string filePath)
    {
        _document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        _workbookPart = _document.AddWorkbookPart();
        _workbookPart.Workbook = new Workbook();
        _sheets = _workbookPart.Workbook.AppendChild<Sheets>(new Sheets());

        // シート作成のための準備
        _worksheetPart = _workbookPart.AddNewPart<WorksheetPart>();
        _worksheetPart.Worksheet = new Worksheet(new SheetData());

    }

    private SpreadsheetDocument _document;

    private WorkbookPart _workbookPart;

    private WorksheetPart _worksheetPart;

    private Sheets _sheets;

    /// <summary>
    /// サンプルExcelのBookを新規作成する。
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static SampleBook CreateBook(string filePath) {
        var book = new SampleBook(filePath);
        book.CreateSheet("シート1");

        return book;
    }

    public void CreateSheet(string sheetName)
    {

        // シートを作成し追加する
        var max = (uint)(_sheets.Count() + 1);
        var sheet = new Sheet {
            Id = _workbookPart.GetIdOfPart(_worksheetPart),
            SheetId = max,
            Name = sheetName
        };
        _sheets.Append(sheet);
    }

    /// <summary>
    /// 現在のbookを保存し、streamに渡すためのバイト列を生成する。
    /// </summary>
    public async Task<byte[]> SaveAndGetBytesAsync()
    {
        _workbookPart.Workbook.Save();
        
        using var stream = new MemoryStream();
        _document.Clone(stream);

        return stream.ToArray();
    }

    /// <summary>
    /// Book全体の保存を行う。
    /// </summary>
    public void Save() {
        _workbookPart.Workbook.Save();
    }
    
    private bool _isDisposed = false;

    public void Dispose() => Dispose(true);   

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _document?.Dispose();
            }
            _isDisposed = true;
        }
    }
}
