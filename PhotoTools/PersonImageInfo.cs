using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoTools
{
    [Serializable]
    public class PersonImageInfo
    {
        public string FileName { get { return FullPath.Substring(FullPath.LastIndexOf(@"\") + 1); } }
        public string DirectoryPath { get { return FullPath.Substring(0, FullPath.LastIndexOf(@"\") + 1); } }
        /// <summary>
        /// 图片全路径
        /// </summary>
        public string FullPath { get; set; }
        /// <summary>
        /// 在逃人员编号
        /// </summary>
        public string ZTRYBH { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public string Index { get; set; }
        /// <summary>
        /// 后缀名
        /// </summary>
        public string Suffix { get; set; }
    }
}
