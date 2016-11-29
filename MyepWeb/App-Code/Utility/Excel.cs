using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Site
{
    public class Excel : IDisposable
    {
        private ExcelPackage _excel;
        private ExcelWorksheet _worksheet;

        public Excel()
        {
            _excel = new ExcelPackage();
            _worksheet = _excel.Workbook.Worksheets.Add("Sheet1");
            Row = 0;
            NextRow();
        }

        public Excel(Stream stream)
        {
            _excel = new ExcelPackage();
            _excel.Load(stream);
            _worksheet = _excel.Workbook.Worksheets[1];
            Row = 0;
            NextRow();
        }

        public int Row { get; set; }
        public int Col { get; set; }
        public int Rows => _worksheet.Dimension.End.Row;
        public int Cols => _worksheet.Dimension.End.Column;

        public object this[int row, int col]
        {
            get { return _worksheet.Cells[row, col].Value; }
            set { _worksheet.Cells[row, col].Value = value; }
        }

        public T Get<T>(int row, int col, T def = default(T))
        {
            return (T)Get(typeof(T), row, col, def);
        }

        public object Get(Type type, int row, int col, object def = null)
        {
            if (type == typeof(DateTime))
            {
                return _worksheet.Cells[row, col].GetValue<DateTime>();
            }
            else if (type == typeof(DateTime?))
            {
                var date = _worksheet.Cells[row, col].GetValue<DateTime>();
                return date.HasValue() ? date : def;
            }
            return this[row, col].To(type, def);
        }

        public List<string> GetWorksheets()
        {
            return _excel
                .Workbook
                .Worksheets
                .Select(x => x.Name)
                .ToList();
        }

        public bool NextSheet(string name, bool create = true)
        {
            if (_worksheet != null && _worksheet.Name == "Sheet1")
            {
                _worksheet.Name = name;
                return true;
            }

            _worksheet = _excel.Workbook.Worksheets[name];
            if (_worksheet == null)
            {
                if (create)
                {
                    _worksheet = _excel.Workbook.Worksheets.Add(name);
                }
                else
                {
                    return false;
                }
            }

            Row = 0;
            NextRow();
            return true;
        }

        public void NextCol(params object[] values)
        {
            foreach (var value in values)
            {
                Set(Row, Col++, value);
            }
        }

        public void NextCol(Style style, params object[] values)
        {
            foreach (var value in values)
            {
                Set(Row, Col++, value, style);
            }
        }

        public void NextRow(params object[] values)
        {
            Row++;
            Col = 1;
            NextCol(values);
        }

        public void NextRow(Style style, params object[] values)
        {
            Row++;
            Col = 1;
            NextCol(style, values);
        }

        public void NextSumRow(int fromCol, int toCol, Style style = null)
        {
            Sum(Row, Col++, Row, fromCol, Row, toCol);
            Format(style);
        }

        public void NextSumCol(int fromRow, int toRow, Style style = null)
        {
            Sum(Row, Col++, fromRow, Col - 1, toRow, Col - 1);
            Format(style);
        }

        public void NextFormula(string formula, Style style = null)
        {
            _worksheet.Cells[Row, Col++].Formula = formula;
            Format(style);
        }

        public void Set(int row, int col, object value, Style style = null)
        {
            _worksheet.Cells[row, col].Value = value;
            Format(style);
        }

        public void FormatAll(Style style)
        {
            Format(_worksheet.Cells, style);
        }

        public void Format(Style style, int? row = null, int? col = null)
        {
            if (row == null) row = Row;
            if (col == null) col = Col - 1;
            Format(_worksheet.Cells[row.Value, col.Value], style);
        }

        public void FormatRow(Style style, int fromCol, int toCol)
        {
            Format(_worksheet.Cells[Row, fromCol, Row, toCol], style);
        }

        public void FormatRow(Style style, int? toCol = null)
        {
            if (toCol == null) toCol = Col - 1;
            Format(_worksheet.Cells[Row, 1, Row, toCol.Value], style);
        }

        public void FormatCol(Style style, int col, int? fromRow = 1, int? toRow = null)
        {
            if (fromRow == null) fromRow = 1;
            if (toRow == null) toRow = Row;
            Format(_worksheet.Cells[fromRow.Value, col, toRow.Value, col], style);
        }

        public void FormatCols(Style style, int col, int toCol, int? fromRow = 1, int? toRow = null)
        {
            if (fromRow == null) fromRow = 1;
            if (toRow == null) toRow = Row;
            Format(_worksheet.Cells[fromRow.Value, col, toRow.Value, toCol], style);
        }

        private void Format(ExcelRange cells, Style style)
        {
            if (cells == null || style == null) return;
            if (style.Format != null) cells.Style.Numberformat.Format = style.Format;
            if (style.Bold != null) cells.Style.Font.Bold = style.Bold.Value;
            if (style.Color != null) cells.Style.Font.Color.SetColor(style.Color.Value);
            if (style.BgColor != null)
            {
                cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cells.Style.Fill.BackgroundColor.SetColor(style.BgColor.Value);
            }
            if (style.Merge.HasValue) cells.Merge = style.Merge.Value;
            if (style.FontSize.HasValue) cells.Style.Font.Size = style.FontSize.Value;
            if (style.Indent.HasValue) cells.Style.Indent = style.Indent.Value;
            if (style.Wrap.HasValue) cells.Style.WrapText = style.Wrap.Value;
            if (style.HAlign.HasValue)
            {
                if (style.HAlign.Value == HAlign.Left) cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                if (style.HAlign.Value == HAlign.Center) cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                if (style.HAlign.Value == HAlign.Right) cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
            if (style.Top == true) cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            if (style.Width.HasValue) cells.Start.Column.UpTo(cells.End.Column).Each(x => _worksheet.Column(x).Width = style.Width.Value);
        }

        public string Address(int col)
        {
            return _worksheet.Cells[Row, col].Address;
        }

        public string Address(int row, int col)
        {
            return _worksheet.Cells[row, col].Address;
        }

        public void Sum(int row, int col, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (toRow >= fromRow && toCol >= fromCol)
            {
                _worksheet.Cells[row, col].Formula = "SUM(" + _worksheet.Cells[fromRow, fromCol, toRow, toCol].Address + ")";
            }
        }

        public void AutoFitColumns()
        {
            _worksheet.Cells.AutoFitColumns();
        }

        public void AutoFitColumns(double? minWidth = null, double? maxWidth = null, params int[] cols)
        {
            foreach (var col in cols)
            {
                if (minWidth == null)
                {
                    _worksheet.Column(col).AutoFit();
                }
                else if (maxWidth == null)
                {
                    _worksheet.Column(col).AutoFit(minWidth.Value);
                }
                else
                {
                    _worksheet.Column(col).AutoFit(minWidth.Value, maxWidth.Value);
                }
            }
        }

        public Stream GetStream()
        {
            var memoryStream = new MemoryStream();
            _excel.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public void Save(string path)
        {
            _excel.SaveAs(new FileInfo(path));
        }

        public void Dispose()
        {
            if (_excel != null)
            {
                _excel.Dispose();
                _excel = null;
            }

            if (_worksheet != null)
            {
                _worksheet.Dispose();
                _worksheet = null;
            }
        }

        public class Style
        {
            public Color? Color { get; set; }
            public Color? BgColor { get; set; }
            public bool? Bold { get; set; }
            public string Format { get; set; }
            public bool? Merge { get; set; }
            public float? FontSize { get; set; }
            public int? Indent { get; set; }
            public bool? Wrap { get; set; }
            public HAlign? HAlign { get; set; }
            public bool? Top { get; set; }
            public double? Width { get; set; }
        };

        public const string FMT_CUR = "_($* #,##0_);_($* (#,##0);_($* \"-\"??_);_(@_)";
        public const string FMT_CUR2 = "_($* #,##0.00_);_($* (#,##0.00);_(* \"-\"??_);_(@_)";
        public const string FMT_DATE = "m/d/yyyy";
        public const string FMT_PERC = "0%";

        public static Excel Create<T>(IEnumerable<T> items)
        {
            var excel = new Excel();
            var props = typeof(T).GetProperties().Where(x => x.PropertyType.IsSimpleType()).ToList();

            foreach (var prop in props)
            {
                excel.NextCol(prop.Name);
            }
            excel.NextRow();

            foreach (var item in items)
            {
                foreach (var prop in props)
                {
                    excel.NextCol(item.Get(prop));
                }
                excel.NextRow();
            }

            excel.AutoFitColumns();
            return excel;
        }

        public List<T> GetRow<T>(int row, int startCol = 1)
        {
            return _worksheet
                .Cells[row, startCol, row, Cols]
                .Select(x => x.Text.To<T>())
                .ToList();
        }

        public Dictionary<string, int> GetHeaders()
        {
            return 1.UpTo(Cols)
                .Select(x => Ext.KeyValuePair(Get<string>(1, x), x))
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key.Replace(" ", ""), x => x.Value);
        }

        public List<T> GetList<T>() where T : new()
        {
            var list = new List<T>();
            var headers = 1.UpTo(Cols).ToDict(x => Get<string>(1, x).Or(), x => x);
            var properties = typeof(T).GetProperties().Where(x => headers.ContainsKey(x.Name)).ToDictionary(x => x.Name);
            foreach (var row in 2.UpTo(Rows))
            {
                var item = new T();
                foreach (var header in headers)
                {
                    var prop = properties.Get(header.Key);
                    if (prop == null)
                        continue;

                    item.Set(prop, Get(prop.PropertyType, row, header.Value));
                }
                list.Add(item);
            }
            return list;
        }

        public static List<T> Load<T>(string path) where T : new()
        {
            using (var stream = File.OpenRead(path))
            {
                using (var excel = new Excel(stream))
                {
                    return excel.GetList<T>();
                }
            }
        }

        public enum HAlign
        {
            Left, Center, Right
        }

        public bool IsRowEmpty(int row)
        {
            var values = (object[,])_worksheet.Cells[row, 1, row, Cols].Value;
            return !values.Cast<object>().Any(x => x.Or().HasValue());
        }
    };
}
