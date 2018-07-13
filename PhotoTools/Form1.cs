using Hs.Utility.OpenCv;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PhotoTools
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 在逃编号+姓名+身份证号
        /// </summary>
        List<PersonInfo> nameList = new List<PersonInfo>();
        /// <summary>
        /// 分类好的图片  在逃编号是key,图片地址是value
        /// </summary>
        Dictionary<string, string> dicImage = new Dictionary<string, string>();

        DataTable dataTable = new DataTable();

        static DataTable dtError = new DataTable();

        string pathStr = string.Empty;

        /// <summary>
        /// 暂停功能
        /// </summary>
        bool bl = false;
        public Form1()
        {
            InitializeComponent();
            Init();
            //设置系统主窗体
            this.StartPosition = FormStartPosition.CenterScreen;
        }


        private void Init()
        {
            richTextBox1.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 打开工具" + "\r\n");
            richTextBox1.AppendText("使用说明" + "\r\n");
            richTextBox1.AppendText("1.选择人员信息文件, 仅支持 .xlsx" + "\r\n");
            richTextBox1.AppendText("2.选择相片文件夹" + "\r\n");
            richTextBox1.AppendText("3.点击开始,等待结果..." + "\r\n");
            richTextBox1.AppendText("4.更多内容，请看‘请阅读我.txt’文本" + "\r\n");
            richTextBox1.AppendText("------------------------------------" + "\r\n");
        }
        /// <summary>
        /// 选择文件夹   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            string path = string.Empty;
            var folderList = ImageHelp.SelectImageFolder(ref path);
            //取消选中图片文件
            if (folderList == null)
            {
                return;
            }
            //选中的文件里面   没有图片文件
            if (folderList.Length == 0)
            {
                //清空上次导入的数据
                MessageBox.Show("选择的文件夹无相片，请重新选择！");
                return;
            }
            dicImage?.Clear();

            textBox1.Text = path;

            pathStr = path;
            int i = 0;//不符合命名规则  23位网逃编号

            foreach (var item in folderList)
            {
                string aFirstName = item.Substring(item.LastIndexOf("\\") + 1, (item.LastIndexOf(".") - item.LastIndexOf("\\") - 1)); //文件名
                string aLastName = item.Substring(item.LastIndexOf(".") + 1, (item.Length - item.LastIndexOf(".") - 1)); //扩展名

                string[] str = aFirstName.Split('_');
                if (str.Length != 2 || str[0].Length != 23 || str[1].Length != 1)
                {
                    i++;
                    continue;
                }
                if (!Directory.Exists(path + "\\没选上"))
                {
                    Directory.CreateDirectory(path + "\\没选上");
                }
                if (!Directory.Exists(path + "\\筛选到"))
                {
                    Directory.CreateDirectory(path + "\\筛选到");
                }
                if (dicImage.ContainsKey(str[0]))
                {
                    switch (str[1])
                    {
                        case "2":
                        case "4":
                            ImageMove(dicImage[str[0]], path + "\\没选上\\" + dicImage[str[0]].Substring(dicImage[str[0]].LastIndexOf("\\") + 1, (dicImage[str[0]].LastIndexOf(".") - dicImage[str[0]].LastIndexOf("\\") - 1)) + "." + dicImage[str[0]].Substring(dicImage[str[0]].LastIndexOf(".") + 1, (dicImage[str[0]].Length - dicImage[str[0]].LastIndexOf(".") - 1)));

                            //ImageMove(item, path + "\\筛选到\\" + aFirstName + "." + aLastName);

                            dicImage[str[0]] = item;
                            break;
                        case "5":
                        case "6":
                            ImageMove(item, path + "\\没选上\\" + aFirstName + "." + aLastName);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //ImageMove(item, path + "\\筛选到\\" + aFirstName + "." + aLastName);
                    dicImage.Add(str[0], item);
                }
                #region 大佬给的方法
                //var filterFiles = dicImage.OrderBy(x => x.ZTRYBH).ThenBy(x => x.Index).GroupBy(x => x.ZTRYBH)
                // .Select(g => new { g, count = g.Count() })
                //          .SelectMany(t => t.g.Select(b => b)
                //                              .Zip(Enumerable.Range(1, t.count), (obj, i) => new { obj, rn = i }))
                //                              .Where(x => x.rn == 1).Select(x => x.obj).ToList();
                #endregion
            }
            richTextBox1.AppendText(String.Format("图片文件夹,已读取{0}张图片,筛选出{1}张图片,不符合命名规则{2}张\r\n", folderList.Length, dicImage.Count, i));
            progressBar1.Maximum = dicImage.Count;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            this.label4.Text = progressBar1.Value + "/" + dicImage.Count;
        }



        /// <summary>
        /// 选择Excel表格  获取表格数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "(*.xlsx)|*.xlsx";
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(String.Format("正在读取表格中,请稍等......\r\n"));
                    }));

                    textBox2.Text = openFileDialog1.FileName;

                    dataTable = new DataTable();

                    dataTable = ExcelToTableForXLSX(openFileDialog1.FileName);

                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(String.Format("读取表格完毕,共符合规则{0}行,不符合规则{1}行\r\n", dataTable.Rows.Count, dtError.Rows.Count));
                    }));

                    string path1 = System.IO.Path.GetDirectoryName(openFileDialog1.FileName) + @"\";

                    TableToExcelForXLSX(dataTable, path1 + "\\符合规则.xlsx");

                    TableToExcelForXLSX(dtError, path1 + "\\不符合规则.xlsx");
                    #region 旧版
                    //XSSFWorkbook wb = null;

                    //using (var fs = File.Open(openFileDialog1.FileName, FileMode.Open))
                    //{
                    //    wb = new XSSFWorkbook(fs);
                    //}



                    //var sheet = wb.GetSheetAt(0);
                    //int rowIndex = 0;
                    //int nullrow = 0;
                    //while (true)
                    //{
                    //    rowIndex++;
                    //    if (nullrow > 2) break;
                    //    var row = sheet.GetRow(rowIndex);
                    //    if (row == null)
                    //    {
                    //        nullrow++;
                    //        continue;
                    //    }
                    //    //在逃人员编号 id
                    //    //姓名 name
                    //    //身份证号  code
                    //    //var id = row.GetCell(0).StringCellValue;
                    //    string id = DataCell(row.GetCell(0));
                    //    string name = DataCell(row.GetCell(1));
                    //    string code = DataCell(row.GetCell(2));

                    //    //var name = row.GetCell(1).StringCellValue;
                    //    //var code = row.GetCell(2).StringCellValue;
                    //    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(code))
                    //    {
                    //        //break;
                    //        //richTextBox1.AppendText(String.Format("{0} {1} {2} {3} 姓名，编号，身份证号不能为空\n\r", rowIndex, name, id, code));
                    //        nullrow++;
                    //        continue;
                    //    }
                    //    PersonInfo tuple = new PersonInfo() { ZTRYBH = id, XM = name, SFZH = code };
                    //    if (nameList.Contains(tuple))
                    //    {
                    //        richTextBox1.AppendText(String.Format("已存在{0} {1} {2} {3}\r\n", rowIndex, name, id, code));
                    //        continue;
                    //    }
                    //    if (code.Length != 15 && code.Length != 18)
                    //    {
                    //        richTextBox1.AppendText(String.Format("{0} {1} {2} {3} 不符合身份证号长度\n\r", rowIndex, name, id, code));
                    //        continue;
                    //    }
                    //    nameList.Add(tuple);
                    //}
                    //richTextBox1.AppendText(String.Format("Excel文档,筛选出{0}条记录\r\n", nameList.Count));
                    #endregion
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", ex + "\r\n");
            }

        }


        /// <summary>  
        /// 将Excel文件中的数据读出到DataTable中(xlsx)  
        /// </summary>  
        /// <param name="file"></param>  
        /// <returns></returns>  
        public static DataTable ExcelToTableForXLSX(string file)
        {
            DataTable dt = new DataTable();
            dtError = new DataTable();
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    XSSFWorkbook xssfworkbook = new XSSFWorkbook(fs);
                    ISheet sheet = xssfworkbook.GetSheetAt(0);

                    //表头  
                    IRow header = sheet.GetRow(sheet.FirstRowNum);
                    List<int> columns = new List<int>();
                    for (int i = 0; i < header.LastCellNum; i++)
                    {
                        object obj = GetValueTypeForXLSX(header.GetCell(i) as XSSFCell);
                        if (obj == null || obj.ToString() == string.Empty)
                        {
                            dt.Columns.Add(new DataColumn("Columns" + i.ToString()));
                            dtError.Columns.Add(new DataColumn("Columns" + i.ToString()));
                            //continue;  
                        }
                        else
                        {
                            dt.Columns.Add(new DataColumn(obj.ToString()));
                            dtError.Columns.Add(new DataColumn(obj.ToString()));
                        }
                        columns.Add(i);
                    }
                    //数据  
                    for (int i = sheet.FirstRowNum + 1; i <= sheet.LastRowNum; i++)
                    {
                        DataRow dr = dt.NewRow();
                        bool hasValue = false;
                        foreach (int j in columns)
                        {
                            dr[j] = GetValueTypeForXLSX(sheet.GetRow(i).GetCell(j) as XSSFCell);
                            if (dr[j] != null && dr[j].ToString() != string.Empty)
                            {
                                hasValue = true;
                                if (j == 0 && dr[j].ToString().Length != 23)
                                {
                                    hasValue = false;
                                    break;
                                }
                                if (j == 2 && dr[j].ToString().Length != 15 && dr[j].ToString().Length != 18)
                                {
                                    hasValue = false;
                                    break;
                                }
                            }


                        }
                        if (hasValue)
                        {
                            dt.Rows.Add(dr);
                        }
                        else
                        {
                            dtError.Rows.Add(dr.ItemArray);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", ex + "\r\n");
            }

            return dt;
        }

        /// <summary>  
        /// 将DataTable数据导出到Excel文件中(xlsx)  
        /// </summary>  
        /// <param name="dt"></param>  
        /// <param name="file"></param>  
        public static void TableToExcelForXLSX(DataTable dt, string file)
        {
            XSSFWorkbook xssfworkbook = new XSSFWorkbook();
            ISheet sheet = xssfworkbook.CreateSheet("Sheet1");

            //表头  
            IRow row = sheet.CreateRow(0);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell cell = row.CreateCell(i);
                cell.SetCellValue(dt.Columns[i].ColumnName);
            }

            //数据  
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row1 = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row1.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            //转为字节数组  
            MemoryStream stream = new MemoryStream();
            xssfworkbook.Write(stream);
            var buf = stream.ToArray();

            //保存为Excel文件  
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(buf, 0, buf.Length);
                fs.Flush();
            }
        }

        /// <summary>  
        /// 获取单元格类型(xlsx)  
        /// </summary>  
        /// <param name="cell"></param>  
        /// <returns></returns>  
        private static object GetValueTypeForXLSX(XSSFCell cell)
        {
            if (cell == null)
                return null;
            switch (cell.CellType)
            {
                case CellType.Blank: //BLANK:  
                    return null;
                case CellType.Boolean: //BOOLEAN:  
                    return cell.BooleanCellValue;
                case CellType.Numeric: //NUMERIC:  
                    return cell.NumericCellValue;
                case CellType.String: //STRING:  
                    return cell.StringCellValue;
                case CellType.Error: //ERROR:  
                    return cell.ErrorCellValue;
                case CellType.Formula: //FORMULA:  
                default:
                    return "=" + cell.CellFormula;
            }
        }

        private static string DataCell(ICell cell)
        {
            DataFormatter formatter = new DataFormatter(); //creating formatter using the default locale
            String str = formatter.FormatCellValue(cell);//Returns the formatted value of a cell as a String regardless of the cell type.
            return str;
        }

        /// <summary>
        /// 点击开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataTable == null || dataTable.Rows.Count == 0 || dicImage.Count == 0)
            {
                MessageBox.Show("请选择/重新选择 Excle或者图片文件夹");
                return;
            }

            this.Invoke(new MethodInvoker(delegate ()
            {
                richTextBox1.Text = string.Empty;
                Init();
                this.button2.Enabled = false;
                this.progressBar1.Value = 0;//滚动条清零 2017-1-7 骆东辉
                richTextBox1.AppendText(String.Format("开始处理,共{0}张图片\r\n", dicImage.Count));
            }));

            Thread thread = new Thread(() =>
            {
                try
                {
                    if (!Directory.Exists(pathStr + "\\success"))
                    {
                        Directory.CreateDirectory(pathStr + "\\success");
                    }
                    if (!Directory.Exists(pathStr + "\\文档没查到对应编号"))
                    {
                        Directory.CreateDirectory(pathStr + "\\文档没查到对应编号");
                    }
                    if (!Directory.Exists(pathStr + "\\筛选到"))
                    {
                        Directory.CreateDirectory(pathStr + "\\筛选到");
                    }

                    #region 大佬给的方法
                    //var query = from a in filerImages
                    //            join b in personInfo
                    //                            on a.ZTRYBH equals b.ZTRYBH
                    //            select new { DirectoryPath = Path.Combine(a.DirectoryPath, newFolder), SourcePath = a.FullPath, TargetPath = Path.Combine(a.DirectoryPath, newFolder, $"{b.SFZH}_{b.XM}.{a.Suffix}") };

                    //var queryList = query.ToList();
                    //if (queryList != null && queryList.Any())
                    //{
                    //    foreach (var item in queryList)
                    //    {
                    //        if (!Directory.Exists(item.DirectoryPath))
                    //        {
                    //            Directory.CreateDirectory(item.DirectoryPath);
                    //        }
                    //        System.IO.File.Copy(item.SourcePath, item.TargetPath, true);
                    //    }
                    //}
                    #endregion
                    #region for
                    //for (int i = dicImage.Count - 1; i >= 0; i--)
                    //{
                    //    bool bl = false;
                    //    foreach (var item in nameList)
                    //    {
                    //        if (dicImage.Keys.ElementAtOrDefault(i).Equals(item.Item1))
                    //        {
                    //            bl = true;
                    //            Image img = Image.FromFile(dicImage.Values.ElementAtOrDefault(i));
                    //            Bitmap imgBitmap = new Bitmap(img);
                    //            imgBitmap = ImageCompression(imgBitmap);
                    //            imgBitmap.Save(String.Format(textBox1.Text + "\\success\\{0}_{1}_{2}_{3}{4}", item.Item2, GetSexFromIdCard(item.Item3), item.Item3, GetBrithdayFromIdCard(item.Item3), ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                    //            dicImage.Remove(dicImage.Keys.ElementAtOrDefault(i));
                    //            break;
                    //        }
                    //    }
                    //    this.Invoke(new MethodInvoker(delegate ()
                    //    {
                    //        progressBar1.Value += 1;
                    //        this.label4.Text = progressBar1.Value + "/" + progressBar1.Maximum;
                    //    }));

                    //    if (!bl)
                    //    {
                    //        this.Invoke(new MethodInvoker(delegate ()
                    //        {
                    //            richTextBox1.AppendText(String.Format("图片编号{0},在Excel表格里面,未查到图片的编号，该图片移到‘文档没查到对应编号’文件夹\r\n", dicImage.Keys.ElementAtOrDefault(i)));

                    //            ImageMove(dicImage.Values.ElementAtOrDefault(i), textBox1.Text + "\\文档没查到对应编号\\" + dicImage.Values.ElementAtOrDefault(i).Substring(dicImage.Values.ElementAtOrDefault(i).LastIndexOf("\\") + 1, (dicImage.Values.ElementAtOrDefault(i).LastIndexOf(".") - dicImage.Values.ElementAtOrDefault(i).LastIndexOf("\\") - 1)) + "." + dicImage.Values.ElementAtOrDefault(i).Substring(dicImage.Values.ElementAtOrDefault(i).LastIndexOf(".") + 1, (dicImage.Values.ElementAtOrDefault(i).Length - dicImage.Values.ElementAtOrDefault(i).LastIndexOf(".") - 1)));
                    //        }));
                    //    }
                    //}
                    #endregion
                    foreach (KeyValuePair<String, String> keyValue in dicImage)
                    {
                        bool bl = false;
                        try
                        {
                            foreach (DataRow item in dataTable.Rows)
                            {

                                if (keyValue.Key.Equals(item.ItemArray[0]))
                                {
                                    bl = true;
                                    using (Image img = Image.FromFile(keyValue.Value))
                                    {
                                        Bitmap imgBitmap = new Bitmap(img);
                                        imgBitmap = ImageCompression(imgBitmap);
                                        imgBitmap.Save(String.Format(pathStr + "\\success\\{0}_{1}_{2}_{3}{4}", item[1].ToString(), GetSexFromIdCard(item[2].ToString()), item[2].ToString(), GetBrithdayFromIdCard(item[2].ToString()), ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                                    };

                                    ImageMove(keyValue.Value, String.Format(pathStr + "\\筛选到\\" + keyValue.Value.Substring(keyValue.Value.LastIndexOf("\\") + 1, (keyValue.Value.LastIndexOf(".") - keyValue.Value.LastIndexOf("\\") - 1)) + "." + keyValue.Value.Substring(keyValue.Value.LastIndexOf(".") + 1, (keyValue.Value.Length - keyValue.Value.LastIndexOf(".") - 1))));

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText("log.txt", ex + "\r\n");
                        }


                        #region 旧版
                        //foreach (var item in nameList)
                        //{
                        //    if (keyValue.Key.Equals(item.ZTRYBH))
                        //    {
                        //        bl = true;
                        //        using (Image img = Image.FromFile(keyValue.Value))
                        //        {
                        //            Bitmap imgBitmap = new Bitmap(img);
                        //            imgBitmap = ImageCompression(imgBitmap);
                        //            imgBitmap.Save(String.Format(textBox1.Text + "\\success\\{0}_{1}_{2}_{3}{4}", item.XM, GetSexFromIdCard(item.SFZH), item.SFZH, GetBrithdayFromIdCard(item.SFZH), ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                        //        };

                        //        ImageMove(keyValue.Value, String.Format(textBox1.Text + "\\筛选到\\" + keyValue.Value.Substring(keyValue.Value.LastIndexOf("\\") + 1, (keyValue.Value.LastIndexOf(".") - keyValue.Value.LastIndexOf("\\") - 1)) + "." + keyValue.Value.Substring(keyValue.Value.LastIndexOf(".") + 1, (keyValue.Value.Length - keyValue.Value.LastIndexOf(".") - 1))));

                        //        break;
                        //    }
                        //}
                        #endregion
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            progressBar1.Value += 1;
                            this.label4.Text = progressBar1.Value + "/" + dicImage.Count;
                        }));

                        if (!bl)
                        {
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                richTextBox1.AppendText(String.Format("图片编号{0},在Excel表格里面,未查到图片的编号，该图片移到‘文档没查到对应编号’文件夹\r\n", keyValue.Key));
                            }));
                            ImageMove(keyValue.Value, pathStr + "\\文档没查到对应编号\\" + keyValue.Value.Substring(keyValue.Value.LastIndexOf("\\") + 1, (keyValue.Value.LastIndexOf(".") - keyValue.Value.LastIndexOf("\\") - 1)) + "." + keyValue.Value.Substring(keyValue.Value.LastIndexOf(".") + 1, (keyValue.Value.Length - keyValue.Value.LastIndexOf(".") - 1)));

                        }
                    }
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        richTextBox1.AppendText(String.Format("筛选结束,{0}张图片,处理完毕\r\n", dicImage.Count));
                    }));

                    //建模完成，允许点击
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        this.button2.Enabled = true;
                    }));

                    dicImage?.Clear();
                    //nameList?.Clear();
                }
                catch (Exception ex)
                {
                    File.AppendAllText("log.txt", ex + "\r\n");
                }

            });
            thread.Start();



        }


        /// <summary>
        /// 根据身份证号获取生日
        /// </summary>
        /// <param name="IdCard"></param>
        /// <returns></returns>
        public static string GetBrithdayFromIdCard(string IdCard)
        {
            string rtn = "1900-01-01";
            if (IdCard.Length == 15)
            {
                rtn = IdCard.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            }
            else if (IdCard.Length == 18)
            {
                rtn = IdCard.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            }
            return rtn;
        }

        /// <summary>
        /// 根据身份证获取性别
        /// </summary>
        /// <param name="IdCard"></param>
        /// <returns></returns>
        public static int GetSexFromIdCard(string IdCard)
        {
            int rtn = 0;
            string tmp = "";
            if (IdCard.Length == 15)
            {
                tmp = IdCard.Substring(IdCard.Length - 3);
            }
            else if (IdCard.Length == 18)
            {
                tmp = IdCard.Substring(IdCard.Length - 4);
                tmp = tmp.Substring(0, 3);
            }
            if (!string.IsNullOrEmpty(tmp))
            {
                int sx = int.Parse(tmp);
                int outNum;
                Math.DivRem(sx, 2, out outNum);
                if (outNum == 0)
                {
                    //1是男  2是女
                    rtn = 2;
                }
                else
                {
                    rtn = 1;
                }
            }

            return rtn;
        }

        /// <summary>
        ///  图片压缩  
        /// ps:至于单张跟批量的图片压缩为啥没有合一呢？是因为单张是用户可以框选人脸的，批量是整张图片丢过的,没框选人脸
        /// </summary>
        public Bitmap ImageCompression(Bitmap tmpImg)
        {
            //超过1500*1500时压缩图片 2017-12-27 韩永健
            if (tmpImg.Width > 1500 || tmpImg.Height > 1500)
            {
                tmpImg = ImageHelper.BitmapResize2(tmpImg, 1500, 1500);
            }
            //超过500*500时尝试扣人脸 2017-12-27 韩永健
            if (tmpImg.Width > 500 || tmpImg.Height > 500)
            {
                var listFaceRect = ImageHelper.GetImageFaces(tmpImg, new System.Drawing.Size(130, 130));
                if (listFaceRect != null && listFaceRect.Count > 0)
                {
                    var faceRect = ImageHelper.GetMaxFaceRect(listFaceRect);
                    faceRect = ImageHelper.ConvertFaceRect(faceRect, tmpImg.Width, tmpImg.Height);
                    tmpImg = ImageHelper.CutFacesRect(tmpImg, faceRect);
                }
            }
            //裁剪人脸后，超过500*500时压缩图片 2017-12-27 韩永健
            if (tmpImg.Width > 500 || tmpImg.Height > 500)
            {
                tmpImg = ImageHelper.BitmapResize2(tmpImg, 500, 500);
            }
            return tmpImg;
        }


        /// <summary>
        /// 图片移动
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        private static void ImageMove(string path, string newPath)
        {
            // string fileNew = item.Replace(path, path + "\\筛选到");
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }
            File.Move(path, newPath);
        }
    }
}
