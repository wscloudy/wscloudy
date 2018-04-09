using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// 文件、配置文件等操作
/// </summary>
namespace wscloudy.Files
{
    public class OpenIni
    {
		public string Path;
			public OpenIni(string path)
			{
				this.Path = path;
			}

         #region
         /// <summary>
         /// 调用API
         /// </summary>
         /// <param name="section">段落</param>
         /// <param name="key">键</param>
         /// <param name="val">值</param>
         ///  <param name="filePath">路径</param>
         ///  <param name="defVal">读取异常的缺省值</param>



         [DllImport("kernel32")]
         private static extern long WritePrivateProfileString(string section, string key, string val, string filePath); 

          [DllImport("kernel32")]
         private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size,string filePath);


#endregion
          /// <summary>
            /// 写INI文件
            /// </summary>
           /// <param name="section">段落</param>
           /// <param name="key">键</param>
          /// <param name="iValue">值</param>
        public void IniWriteValue(string section, string key, string iValue) 
        {
            WritePrivateProfileString(section, key, iValue, this.Path);
        }

          /// <summary>
          /// 读取INI文件
          /// </summary>
          /// <param name="section">段落</param>
          /// <param name="key">键</param>
          /// <returns>返回的键值</returns>
          public string IniReadValue(string section, string key) 
          { 
              StringBuilder temp = new StringBuilder(255);
              int i = GetPrivateProfileString(section, key, "", temp,255, this.Path);
              string val = temp.ToString();

              return val;
          }

		  public string IniReadValue(string section, string key ,string defult) {
			  StringBuilder temp = new StringBuilder(255);
			  int i = GetPrivateProfileString(section, key, defult , temp, 255, this.Path);
              string val = temp.ToString();

              return val;
		  }



    }
}
