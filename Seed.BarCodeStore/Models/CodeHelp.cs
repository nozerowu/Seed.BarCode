﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using Seed.BarCodeStore.Dao;


namespace Seed.StockOutScan.Models
{
    public class CodeHelp
    {
        //导入DT930扫描器数据

        //  1.去除前后空白2.判断长度大于10的为准确单号+条码3.截取字符串一直到空格处为单号4.截取最后空格一直到末尾为大号
        //  2.判断单号+条码是否已经存在于数据库中，没有才存入
        //  2014-08-20
        public string ReadDt930(string url, string newUrl)
        {
            List<NcHandSaleInfo> list = new List<NcHandSaleInfo>(); 
            foreach (var line in File.ReadAllLines(url))
            {
                NcHandSaleInfo info = new NcHandSaleInfo();
                if (line.ToString().Trim().Length > 10)
                {
                    var str = line.Trim();
                    int len = str.IndexOf(' ', 0);
                    info.SaleOrder = str.Substring(0, len).Trim();
                    len = str.LastIndexOf(' ');
                    info.BigCode = str.Substring(len, str.Length - len).Trim();
                }
                list.Add(info);
            }
            using (var db = SugarDao.GetInstance())
            {
                db.InsertRange(list);
            }
            File.Copy(url, newUrl + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
            File.Delete(url);
            return "已导入 " + list.Count + " 条数据";
        }

        //导入PT850扫描器数据

        //  1.去除前后空白2.判断长度大于5的为准确单号或条码3.截取字符串有｛的1到空白位置为单号4.截取无其它标识的为大号
        //  2.判断单号+条码是否已经存在于数据库中，没有才存入
        //  2014-08-20
        public string ReadPt850(string url, string newUrl)
        {
            string str1 = "";
            List<NcHandSaleInfo> list = new List<NcHandSaleInfo>(); 
            foreach (var line in File.ReadAllLines(url))
            {
                NcHandSaleInfo info = new NcHandSaleInfo();
                if (line.Trim().Length > 5)
                {
                    var str = line.Trim();
                    if (str.IndexOf(')', 0) > 0 || str.IndexOf('>', 0) > 0)
                    {
                        continue;
                    }
                    else if (str.IndexOf('}', 0) > 0)
                    {
                        var len = str.IndexOf(' ', 0);
                        str1 = str.Substring(1, len).Trim();
                        continue;
                    }
                    else
                    {
                        var str2 = str.Trim();
                        info.SaleOrder = str1;
                        info.BigCode = str2;
                        list.Add(info);
                    }
                }
            }
            using (var db = SugarDao.GetInstance())
            {
                db.InsertRange(list);
            }
            File.Copy(url, newUrl + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
            File.Delete(url);
            return "已导入 " + list.Count + " 条数据";
        }


        public void WriteTxt(string url)
        {
            //覆盖
            File.WriteAllText(url, @"C:\Dt900\down\wenjian.txt", Encoding.Default);
            //追加
            //  File.AppendAllText(url, "i love u\n", Encoding.Default);
        }

        public void CreatFile(string url)
        {
            if (!System.IO.Directory.Exists(url))
            {
                System.IO.Directory.CreateDirectory(url);
            }
        }

        public void WriteToXls(DataTable dt)
        {
            int cols = dt.Columns.Count;
            int rows = dt.Rows.Count;
            string strNow;

            HSSFWorkbook hssfworkbook = new HSSFWorkbook();

            ISheet sheet1 = hssfworkbook.CreateSheet("Sheet1");
            HSSFRow headerRow = sheet1.CreateRow(1) as HSSFRow;

            IRow row1 = sheet1.CreateRow(0);

            foreach (DataColumn column in dt.Columns)
            {
                row1.CreateCell(column.Ordinal).SetCellValue(column.ColumnName);
            }

            for (int i = 1; i <= rows; i++)
            {
                IRow row = sheet1.CreateRow(i);

                for (int m = 0; m < cols; m++)
                    row.CreateCell(m).SetCellValue(dt.Rows[i - 1][m].ToString());

            }

            strNow = "f:/" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm") + ".xls";
            using (FileStream fs = File.OpenWrite(strNow))
            {
                hssfworkbook.Write(fs);
            }

        }
    }
}
