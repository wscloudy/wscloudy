using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace CreatsoftPrgram
{
    public class AccessDB
    {
        private OleDbConnection conn = null;
        private string connectStr;

        public AccessDB(string pathDB)
        {
            connectStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + pathDB + "'";
            try
            {
                conn = new OleDbConnection(connectStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                Log.logger.Error("ACCESS数据库打开失败！", ex);
            }
        }

        /// <summary>
        /// 判断服务器状态，如果断开则重新连接
        /// </summary>
        public void ReOpen()
        {
            try
            {
                if (conn == null)
                {
                    conn = new OleDbConnection(connectStr);
                    conn.Open();
                }
                else
                {
                    if (conn.State == System.Data.ConnectionState.Closed || conn.State == System.Data.ConnectionState.Broken)
                    {
                        conn.Close();
                        conn.Dispose();
                        conn = null;
                        conn = new OleDbConnection(connectStr);
                        conn.Open();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.logger.Error("ACCESS数据库打开失败！", ex);
            }
        }

        /// <summary>
        /// 执行无返回sql语句
        /// </summary>
        public int SqlCommand(string cmd, OleDbParameter[] parameters)
        {
            int lines = -1;
            OleDbCommand connCommand = null;
            try
            {
                //ReOpen();
                connCommand = new OleDbCommand();
                connCommand.Connection = conn;
                connCommand.CommandText = cmd;
                connCommand.Parameters.Clear();
                foreach (OleDbParameter parameter in parameters)
                {
                    //orcl参数转为sql参数
                    //parameter.ParameterName = parameter.ParameterName.Replace(':', '@');
                    connCommand.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                }
                lines = connCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.logger.Error("ACCESS数据库sql执行失败！", ex);
                ReOpen();
                lines = -2;
            }
            finally
            {
                if (connCommand != null)
                {
                    connCommand.Dispose();
                    connCommand = null;
                }
            }
            return lines;
        }

        /// <summary>
        /// 执行有返回sql语句
        /// </summary>
        public OleDbCommand SqlReadData(string cmd, OleDbParameter[] parameters)
        {
            try
            {
                //ReOpen();
                OleDbCommand connCommand = new OleDbCommand();
                connCommand.Connection = conn;
                connCommand.CommandText = cmd;
                connCommand.Parameters.Clear();
                if (parameters != null)
                {
                    foreach (OleDbParameter parameter in parameters)
                    {
                        connCommand.Parameters.AddWithValue(parameter.ParameterName, parameter.Value);
                    }
                }
                return connCommand;
            }
            catch (Exception ex)
            {
                Log.logger.Error("ACCESS数据库sql执行失败！", ex);
                ReOpen();
                return null;
            }
        }
        
        /// <summary>
        /// 释放数据库占用
        /// </summary>
        public void close()
        {
            try
            {
                if (conn != null)
                {
                    this.conn.Close();
                    this.conn.Dispose();
                }
            }
            catch { }
            finally
            {
                conn = null;
            }
        }
    }
}
