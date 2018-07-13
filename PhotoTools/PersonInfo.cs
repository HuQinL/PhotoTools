using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoTools
{
    [Serializable]
    public class PersonInfo
    {
        /// 在逃人员编号
        /// </summary>
        public string ZTRYBH { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string XM { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string SFZH { get; set; }
    }
}
