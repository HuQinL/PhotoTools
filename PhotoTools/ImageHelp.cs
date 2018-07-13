using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PhotoTools
{
    public class ImageHelp
    {


        /// <summary>
        /// 选择图片文件夹
        /// </summary>
        /// <returns></returns>
        public static string[] SelectImageFolder(ref string path)
        {
            List<string> fileList = new List<string>();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (Directory.Exists(path))
            {
                fbd.SelectedPath = path;
            }
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
                fileList.AddRange(Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly));
                fileList.AddRange(Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly));
                fileList.AddRange(Directory.GetFiles(path, "*.jpeg", SearchOption.TopDirectoryOnly));
                fileList.AddRange(Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly));
            }
            else
            {
                path = null;
                return null;
            }
            return fileList.ToArray();
        }

       
    }
}
