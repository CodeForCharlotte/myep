using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Syncfusion.XlsIO;

namespace Site
{

    public class Excel : IDisposable
    {

        private readonly ExcelEngine _excel;
        private readonly IWorkbook _workbook;
        private readonly IWorksheet _sheet;
        private int _row, _col;

        public Excel()
        {
            _excel = new ExcelEngine();
            _workbook = _excel.Excel.Workbooks.Create(1);
            _workbook.Version = ExcelVersion.Excel97to2003;
            _sheet = _workbook.Worksheets[0];
            _row = 1;
            _col = 1;
        }

        public Excel(string path, ExcelVersion? version = null)
        {
            _excel = new ExcelEngine();
            _workbook = version == null
                        ? _excel.Excel.Workbooks.Open(path)
                        : _excel.Excel.Workbooks.Open(path, version.Value);
            _sheet = _workbook.Worksheets[0];
            _row = 1;
            _col = 1;
        }

        public Excel(Stream donorfile, ExcelVersion? version = null)
        {
            _excel = new ExcelEngine();
            _workbook = version == null
                        ? _excel.Excel.Workbooks.Open(donorfile)
                        : _excel.Excel.Workbooks.Open(donorfile, version.Value);
            _sheet = _workbook.Worksheets[0];
            _row = 1;
            _col = 1;
        }

        public IWorkbook Workbook
        {
            get { return _workbook; }
        }

        public object this[int r, int c]
        {
            get { return Get(r, c); }
            set { Set(r, c, value); }
        }

        public object Get(int row, int col)
        {
            return _sheet[row, col].Value2;
        }

        public void Set(int row, int col, object value)
        {
            if (value is string)
            {
                _sheet[row, col].Text = (string)value;
            }
            else
            {
                _sheet[row, col].Value2 = value;
            }
        }

        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }

        public int Col
        {
            get { return _col; }
            set { _col = value; }
        }

        public int Rows
        {
            get { return _sheet.Rows.Length; }
        }

        public int Columns
        {
            get { return _sheet.Columns.Length; }
        }

        public void NextRow()
        {
            Row++;
            Col = 1;
        }

        public void Formula(int row, int col, string formula)
        {
            _sheet[row, col].Formula = formula;
        }

        public void AddRow(params object[] values)
        {
            _row = _row == 1 ? _sheet.Rows.Length + 1 : _row + 1;
            _col = 1;
            AddToRow(values);
        }

        public void AddToRow(params object[] values)
        {
            foreach (var value in values)
            {
                Set(_row, _col++, value);
            }
        }

        public void AddList<T>(IEnumerable<T> list)
        {
            if (list == null) return;
            var props = list.GetType().GetGenericArguments()[0].GetProperties().Where(x => x.CanConvertFrom<string>());
            foreach (var prop in props)
            {
                AddToRow(prop.Name);
            }
            AddRow();
            foreach (var obj in list)
            {
                foreach (var prop in props)
                {
                    AddToRow(prop.GetValue(obj, null));
                }
                AddRow();
            }
        }

        public IRange Column(int i)
        {
            if (_sheet.Columns.Length < i) return null;
            return _sheet.Columns[i - 1];
        }

        public IRange GetRow(int i)
        {
            if (_sheet.Rows.Length < i) return null;
            return _sheet.Rows[i - 1];
        }

        public IRange GetCell(int row, int col)
        {
            return _sheet[row, col];
        }

        public IRange GetCells(int row, int col, int lastRow, int lastCol)
        {
            return _sheet[row, col, lastRow, lastCol];
        }

        public void AutofitColumns(params int[] cols)
        {
            if (cols.Length == 0)
            {
                //all
                foreach (var c in 1.upto(_sheet.Columns.Length))
                {
                    _sheet.AutofitColumn(c);
                }
            }
            else
            {
                foreach (var c in cols)
                {
                    _sheet.AutofitColumn(c);
                }
            }
        }

        public void SetWidth(int c, int width)
        {
            var column = Column(c);
            if (column == null) return;
            column.ColumnWidth = width;
            column.CellStyle.WrapText = true;
        }

        public void AutofitRows(params int[] rows)
        {
            if (rows.Length == 0)
            {
                foreach (var r in 1.upto(_sheet.Rows.Length))
                {
                    _sheet.Rows[r - 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                    _sheet.AutofitRow(r);
                }
            }
            else
            {
                foreach (var r in rows)
                {
                    _sheet.Rows[r - 1].VerticalAlignment = ExcelVAlign.VAlignTop;
                    _sheet.AutofitRow(r);
                }
            }
        }

        public Stream GetStream(ExcelVersion? version = null)
        {
            if (version.HasValue)
            {
                _workbook.Version = version.Value;
            }

            try
            {
                var stream = new MemoryStream();
                _workbook.SaveAs(stream);
                return stream;
            }
            finally
            {
                Dispose();
            }
        }

        public void Save(string path, ExcelVersion? version = null)
        {
            if (version.HasValue) _workbook.Version = version.Value;
            _workbook.SaveAs(path, ExcelSaveType.SaveAsXLS);
        }

        public void Dispose()
        {
            if (_workbook != null) _workbook.Close();
            if (_excel != null) _excel.Dispose();
        }

        public const string FMT_CUR = "_($* #,##0_);_($* (#,##0);_($* \"-\"??_);_(@_)";

        public IStyle AddStyle(string name)
        {
            return _workbook.Styles.Add(name);
        }

        public void SetBorders(IRange cells, ExcelLineStyle style, ExcelKnownColors color, params ExcelBordersIndex[] borders)
        {
            if (cells == null) return;
            foreach (var b in borders)
            {
                cells.CellStyle.Borders[b].LineStyle = style;
                cells.CellStyle.Borders[b].Color = color;
            }
        }

        public Stream AsStream()
        {
            var stream = new MemoryStream();
            _workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public string GetValue(int row, int col)
        {
            return GetValue<string>(row, col);
        }

        public T GetValue<T>(int row, int col)
        {
            var type = typeof(T);
            var cell = _sheet[row, col];

            if (type == typeof(string))
            {
                return (T)(object)cell.Value;
            }
            else if ((type == typeof(DateTime) || type == typeof(DateTime?)) && cell.HasDateTime)
            {
                return (T)(object)cell.DateTime;
            }
            else if ((type == typeof(decimal) || type == typeof(decimal?)) && cell.HasNumber)
            {
                return (T)(object)Convert.ToDecimal(cell.Number);
            }

            return cell.Value.To<T>();
        }

        public string GetDisplay(int row, int col)
        {
            return _sheet[row, col].DisplayText;
        }

        public static Stream Create<T>(IEnumerable<T> items, params string[] properties)
        {
            if (properties == null || properties.Length == 0)
            {
                properties = typeof(T).GetProperties().Select(x => x.Name).ToArray();
            }

            using (var excel = new ExcelEngine())
            {
                var workbook = excel.Excel.Workbooks.Create(1);
                var sheet = workbook.Worksheets[0];

                //write out header
                var row = 1;
                var col = 1;
                foreach (var p in properties)
                {
                    sheet[row, col++].Value2 = p;
                }

                //write out each item
                row++;
                foreach (var item in items)
                {
                    col = 1;
                    foreach (var p in properties)
                    {
                        sheet[row, col++].Value2 = item.Get(p);
                    }
                    row++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream;
            }

        }

        public static Stream Create<T>(IEnumerable<T> items, Action<IWorksheet, int, T> load)
        {
            using (var excel = new ExcelEngine())
            {
                var workbook = excel.Excel.Workbooks.Create(1);
                var sheet = workbook.Worksheets[0];
                var row = 1;

                foreach (var item in items)
                {
                    load(sheet, row, item);
                    row++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream;
            }
        }

        public List<T> ToList<T>() where T : new()
        {
            var props = new List<PropertyInfo>();
            foreach (var col in 1.upto(Columns))
            {
                var prop = typeof(T).GetProperty(this[1, col].Or());
                props.Add(prop);
            }

            var list = new List<T>();
            foreach (var r in 2.upto(Rows))
            {
                var item = new T();
                foreach (var c in 0.upto(props.Count - 1))
                {
                    if (props[c] != null) item.Set(props[c], this[r, c + 1]);
                }
                list.Add(item);
            }

            return list;
        }
    };

    public static class ExtSyncfusion
    {

        public static void AddRow(this IWorksheet sheet, params object[] values)
        {
            var row = sheet.Rows.Length + 1;
            var col = 1;
            foreach (var value in values)
            {
                if (value == null)
                {
                }
                else if (Ext.IsNumeric(value))
                {
                    sheet[row, col].Value2 = value;
                }
                else
                {
                    sheet[row, col].Text = value.ToString();
                }
                col++;
            }
        }

        public static void AutofitColumns(this IWorksheet sheet)
        {
            foreach (var c in 1.upto(sheet.Columns.Length))
            {
                sheet.AutofitColumn(c);
            }
        }

        public static void AddToRow(this IWorksheet sheet, params object[] values)
        {
            var row = sheet.Rows.Length;
            var col = sheet.Rows[row - 1].LastColumn;
            foreach (var value in values)
            {
                sheet[row, col++].Value2 = value;
            }
        }

    };

}
