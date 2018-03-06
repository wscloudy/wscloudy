/********************************************************************************
** auth： 邹云
** date： 2017-3-23
** desc： ORCL数据库连接与操作类（需要数据库客户端）
** Ver.@  V1.0.1
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
//new Thread(new ThreadStart()).Start();

namespace CreatsoftPrgram {
	public class DataBaseOper
    {
        public bool lastStatus = false;//纪录最后一次操作是否成功
        private MSSQLController.SqlDBHelper MssqlClient;
        public static string PreTableName = "dc_steq_YNSH_"; //表前缀
        //int locker = 0;//本地数据库线程锁

        /// <summary>
        /// 执行sql语句时的操作类型，已失效
        /// </summary>
        public enum SqlType
        {
            /// <summary>
            /// 执行常规sql语句
            /// </summary>
            NormalWrite = 1,
            /// <summary>
            /// 执行无日志记录的sql语句，并在失败的时候在本地不存档
            /// </summary>
            WriteWithoutLogRestore = 2,
            /// <summary>
            /// 在新的数据库连接中执行sql语句，并在失败的时候在本地不存档
            /// </summary>
            WriteWithNewWithoutRestore = 3
        }

        /// <summary>
        /// 创建数据库操作器
        /// </summary>
        /// <param name="connectString">完整的数据库连接字符串</param>
        public DataBaseOper(string connectString, string preTableName) {
            MSSqlInit(connectString);
            PreTableName = preTableName;
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void MSSqlInit(string connectString)
        {
            try
            {
                MssqlClient = new MSSQLController.SqlDBHelper(connectString);
                if (MssqlClient == null)
                {
                    Log.logger.Info("数据库没有初始化！");
                }
                string errstr = "";
                if (!MssqlClient.ConnectTest(out errstr))
                {
                    Log.logger.Error("数据库连接失败：" + errstr);
                }
            }
            catch (Exception ex)
            {
                Log.logger.Error("实例化数据库类失败", ex);
            }
        }

        public string GetID()
        {
            Random random = new Random();
            return DateTime.Now.ToString("yyyyMMddHmmss") + DateTime.Now.Millisecond.ToString() + random.Next(999).ToString().PadLeft(3,'0');
        }

		/// <summary>
		/// 向数据库插入行
		/// </summary>
		/// <param name="table">表</param>
		/// <param name="value">插入行的字段值，用","隔开,形如"FS-CH-001,1758.1,2016-4-28 10:11:58,0"</param>
		public bool OrclUpdate(string table, string value, SqlType sqlType) {
			int count = 0;
            string sqlstr = "";

            OleDbParameter[] parameters = null;
            try {
				string[] values = value.Split(new char[]{','});
                if (table.Contains("PH"))
                {
                    sqlstr = "insert into [" + table + "](OBJID, OBJSTATUS, MODIFYDATE, phValue, decTime, decpointCode) " +
                            "values(" + GetID() + ", @values0,@values1,@values2,@values3,@values4)";
                    parameters = new OleDbParameter[5];
                    parameters[0] = new OleDbParameter("@values0", System.Data.OleDb.OleDbType.Integer);
                    parameters[1] = new OleDbParameter("@values1", System.Data.OleDb.OleDbType.Date);
                    parameters[2] = new OleDbParameter("@values2", System.Data.OleDb.OleDbType.Single);
                    parameters[3] = new OleDbParameter("@values3", System.Data.OleDb.OleDbType.Date);
                    parameters[4] = new OleDbParameter("@values4", System.Data.OleDb.OleDbType.VarChar, 200);
                    parameters[0].Value = 0;
                    parameters[1].Value = System.DateTime.Parse(values[1]);
                    parameters[2].Value = values[3];
                    parameters[3].Value = System.DateTime.Parse(values[1]);
                    int cutIndex = table.IndexOf("_PH");
                    parameters[4].Value = table.Remove(cutIndex, table.Length - cutIndex).Remove(0, PreTableName.Length);
                    Log.logger.Info("decpointCode:" + parameters[4].Value.ToString() + "|" + table + "|" + cutIndex.ToString() + "|" + PreTableName);
                    count = MssqlClient.ExecuteNonQuery(sqlstr, parameters);
                    //OrclUpdateStatus("corr_measurepoint", values[0] + ",0", sqlType);//同步一次status
                }
                else if (table.Contains("Ind"))
                {
                    sqlstr = "insert into [" + table + "](OBJID, OBJSTATUS, MODIFYDATE, lossValue, decTime, decpointCode) " +
                            "values(" + GetID() + ", @values0,@values1,@values2,@values3,@values4)";
                    parameters = new OleDbParameter[5];
                    parameters[0] = new OleDbParameter("@values0", System.Data.OleDb.OleDbType.Integer);
                    parameters[1] = new OleDbParameter("@values1", System.Data.OleDb.OleDbType.Date);
                    parameters[2] = new OleDbParameter("@values2", System.Data.OleDb.OleDbType.Single);
                    parameters[3] = new OleDbParameter("@values3", System.Data.OleDb.OleDbType.Date);
                    parameters[4] = new OleDbParameter("@values4", System.Data.OleDb.OleDbType.VarChar, 200);
                    parameters[0].Value = 0;
                    parameters[1].Value = System.DateTime.Parse(values[1]);
                    parameters[2].Value = values[3];
                    parameters[3].Value = System.DateTime.Parse(values[1]);
                    int cutIndex = table.IndexOf("_Ind");
                    parameters[4].Value = table.Remove(cutIndex, table.Length - cutIndex).Remove(0, PreTableName.Length);
                    Log.logger.Info("decpointCode:" + parameters[4].Value.ToString() + "|" + table + "|" + cutIndex.ToString() + "|" + PreTableName);
                    count = MssqlClient.ExecuteNonQuery(sqlstr, parameters);
                }
                Log.logger.Info(table + "上传成功：" + sqlstr);   //debug
                if (count > 0)
                {
                    lastStatus = true;
                    return true;
                }
				else
                {
                    lastStatus = false;
                    return false;
                }
					
			} 
			catch (Exception ex)
            {
                Log.logger.Error("数据库插入失败", ex);
                lastStatus = false;
                return false;
			}
		}

        /// <summary>
		/// 向数据库更新测点状态
		/// </summary>
		/// <param name="table">表</param>
		/// <param name="values[]">values[0]@测点标识，values[1]@status</param>
		/// <param name="status">测点标识和状态值，形如"FS-CJY-001,0",或"FS-CH-001,1"</param>
//		public bool OrclUpdateStatus(string table, string status, SqlType sqlType)
//        {
//            string[] values = status.Split(new char[] { ',' });
//            string sqlstr;
//            OleDbParameter[] parameters;
//            if (Equals(values[1], "0") || Equals(values[1], "1"))
//            {
//                try
//                {
//                    switch (table)
//                    {
//                        case "corr_measurepoint":
//                            sqlstr = "update corr_measurepoint set status = @values1 where hardcode=@values0";
//                            parameters = new OleDbParameter[2];
//                            parameters[0] = new OleDbParameter("@values1", System.Data.OleDb.OleDbType.Variant, 1);
//                            parameters[1] = new OleDbParameter("@values0", System.Data.OleDb.OleDbType.VarChar, 200);
//                            parameters[0].Value = values[1];
//                            parameters[1].Value = values[0];
//                            MssqlClient.ExecuteNonQuery(sqlstr, parameters);
//                            break;
//                        case "cw_circulatewaterdata":
//                            //sqlCommad = "update cw_circulatewaterdata set status =" + values[1] + "where hardcode='" + values[0] + "' ";
//                            //conn.CommandText = sqlCommad;
//                            //count = conn.ExecuteNonQuery();
//                            break;
//                    }
//                    return true;
//                }
//                catch (Exception ex)
//                {
//                    switch (table)
//                    {
//                        case "corr_measurepoint":
//                            break;
//                    }
//                    if (sqlType == SqlType.WriteWithNewWithoutRestore || sqlType == SqlType.WriteWithNewWithoutRestore)
//                    {
//                        Log.logger.Error("oracle更新状态失败！", ex);
//                    }
//#if DEBUG
//                    Log.logger.Error("OrclUpdateStatus！", ex);
//#endif
//                    lastStatus = false;
//                    return false;
//                }
//            }
//            else
//            {
//                return true;
//            }
//        }

        //获取当前连接状态
        public bool Status() {
            return lastStatus;
        }
	}
}
