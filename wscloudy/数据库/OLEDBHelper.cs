using System;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

/// <summary>
/// OLEDB数据库基础类
/// </summary>
namespace wscloudy.OLEDBClient
{
    public class SqlDBHelper
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// 可用于手动控制的内置单线程数据库连接
        /// </summary>
        public OleDbConnection Gconn = null;

        /// <summary>
        /// 自动初始化数据库控制
        /// </summary>
        /// <param name="connect_str">数据库连接字符串</param>
        public SqlDBHelper(string connect_str)
        {
            ConnectionString = connect_str;
            Gconn = new OleDbConnection(ConnectionString);
        }

        /// <summary>
        /// 手动初始化数据库控制（释放已打开的数据库连接）
        /// </summary>
        /// <param name="connect_str">数据库连接字符串</param>
        public void Init(string connect_str)
        {
            if (Gconn != null)
            {
                Close();
                Gconn.Dispose();
                Gconn = null;
            }
            ConnectionString = connect_str;
            Gconn = new OleDbConnection(ConnectionString);
        }

        /// <summary>
        /// 手动关闭单线程数据库连接
        /// </summary>
        public void Close()
        {
            try
            {
                if (Gconn.State == ConnectionState.Open)
                {
                    Gconn.Close();
                }
            }
            catch { }
        }

        /// <summary>
        /// 手动连接单线程数据库
        /// </summary>
        /// <returns>成功-true，失败-false</returns>
        public bool Open()
        {
            try
            {
                if (Gconn.State != ConnectionState.Open)
                {
                    Gconn.Open();
                    if (Gconn.State == ConnectionState.Open)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 测试数据库连接是否成功,该操作会进行一次完整的连接和端开。
        /// </summary>
        /// <returns>成功返回true，失败返回false</returns>
        public bool Test()
        {
            bool ret = Open();
            Close();
            return ret;
        }

        /// <summary>
        /// 执行SQL语句,返回受影响记录数
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="cmdParms"></param>
        /// <returns>该操作影响的行数</returns>
        public int ExecuteNonQuery(string sql, params IDataParameter[] cmdParms)
        {
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                OleDbCommand sqlcmd = new OleDbCommand();
                try
                {
                    PrepareCommand(sqlcmd, sqlcnn, null, sql, cmdParms);
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    int flag = sqlcmd.ExecuteNonQuery();
                    sqlcnn.Close();
                    //trace.Success("NonQuery:" + flag, OperationLogFlags.ExecuteNonQuery);
                    return flag;
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句,返回受影响记录数
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <returns>该操作影响的行数</returns>
        public int ExecuteNonQuery(string sql)
        {
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                OleDbCommand sqlcmd = new OleDbCommand();
                try
                {
                    sqlcnn.Open();
                    sqlcmd.CommandText = sql;
                    int flag = sqlcmd.ExecuteNonQuery();
                    sqlcnn.Close();
                    return flag;
                }
                finally
                {
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL语句,返回IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms">参数</param>
        /// <returns>SqlDataReader</returns>
        public OleDbDataReader ExecuteReader(string sql, params IDataParameter[] cmdParms)
        {
            OleDbCommand sqlcmd = new OleDbCommand();
            PrepareCommand(sqlcmd, Gconn, null, sql, cmdParms);
            OleDbDataReader sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection);
            return sdr;
        }

        /// <summary>
        /// 执行SQL语句，返回数据表
        /// </summary>
        /// <param name="text">sql文本</param>
        /// <param name="parameters">sql参数</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string text, params OleDbParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(conn.CreateCommand()))
                {
                    conn.Open();
                    adapter.SelectCommand.CommandText = text;
                    if (parameters != null && parameters.Length > 0)
                    {
                        adapter.SelectCommand.Parameters.AddRange(parameters);
                    }
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        /// <summary>
        /// 执行多条SQL语句，返回各个数据表
        /// </summary>
        /// <param name="text">sql文本数组</param>
        /// <returns></returns>
        public DataTable[] ExecuteDataTables(string[] text, string[] name = null)
        {
            if (text == null || text.Length == 0) return new DataTable[0];
            List<DataTable> dts = new List<DataTable>();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == null || text[i] == "") continue;
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(Gconn.CreateCommand()))
                {
                    DataTable dt = new DataTable();
                    adapter.SelectCommand.CommandText = text[i];
                    adapter.Fill(dt);
                    if (name != null)
                    {
                        dt.TableName = name[i];
                    }
                    else
                    {
                        dt.TableName = "other" + i.ToString();
                    }
                    dts.Add(dt);
                }
            }
            return dts.ToArray();
        }

        /// <summary>
        /// 执行SQL，返回结果集中第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params IDataParameter[] cmdParms)
        {
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                OleDbCommand sqlcmd = new OleDbCommand();
                try
                {
                    PrepareCommand(sqlcmd, sqlcnn, null, sql, cmdParms);
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    var result = sqlcmd.ExecuteScalar();
                    //trace.Success("Scalar:" + result, OperationLogFlags.ExecuteScalar);
                    return result;
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL，返回结果集中第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, CommandType commandType, params IDataParameter[] cmdParms)
        {
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                OleDbCommand sqlcmd = new OleDbCommand();
                try
                {
                    PrepareCommand(sqlcmd, sqlcnn, commandType, sql, cmdParms);
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    var result = sqlcmd.ExecuteScalar();
                    //trace.Success("Scalar:" + result, OperationLogFlags.ExecuteScalar);
                    return result;
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL，判断数据是否存在
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns>0（不存在）或1（存在）</returns>
        public int Exists(string sql, params IDataParameter[] cmdParms)
        {
            return (ExecuteScalar(sql, cmdParms) == null) ? 0 : 1;
        }

        /// <summary>
        /// 执行SQL，返回DataSet
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns>DataSet</returns>
        public DataSet GetDataSet(string sql, params IDataParameter[] cmdParms)
        {
            DataSet ds = new DataSet();
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
                try
                {
                    PrepareCommand(sqlcmd, sqlcnn, null, sql, cmdParms);
                    SqlDataAdapter adpt = new SqlDataAdapter(sqlcmd);
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    adpt.Fill(ds);
                    //trace.Success("sda.ds", OperationLogFlags.SqlDataAdapter);
                    return ds;
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 执行SQL，返回内存中数据的一个表
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns>DataTable</returns>
        public DataTable GetDataTable(string sql, params IDataParameter[] cmdParms)
        {
            DataSet ds = new DataSet();
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
                try
                {
                    PrepareCommand(sqlcmd, sqlcnn, null, sql, cmdParms);
                    SqlDataAdapter adpt = new SqlDataAdapter(sqlcmd);
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    adpt.Fill(ds);
                    //trace.Success("sda.dt", OperationLogFlags.SqlDataAdapter);
                    return ds.Tables[0];
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 拼接Sql
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="conn">数据库连接</param>
        /// <param name="trans">事务</param>
        /// <param name="sql">Sql语句</param>
        /// <param name="cmdParms">Sql参数</param>
        public void PrepareCommand(IDbCommand cmd, IDbConnection conn, IDbTransaction trans, string sql, IDataParameter[] cmdParms)
        {
            //打开数据库
            if (conn.State != ConnectionState.Open)
                conn.Open();
            //设置数据库连接
            cmd.Connection = conn;
            //设置Sql语句
            cmd.CommandText = sql;
            //设置事务
            if (trans != null)
                cmd.Transaction = trans;
            //设置Sql执行方式
            cmd.CommandType = CommandType.Text;
            //填充Sql参数
            if (cmdParms != null)
            {
                foreach (OleDbParameter parameter in ConvertParameters(cmdParms))
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 执行SQL语句,返回受影响记录数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="commandType"></param>
        /// <param name="cmdParms"></param>
        /// <returns>该操作影响的行数</returns>
        public int ExecuteNonQuery(string sql, CommandType commandType, params IDataParameter[] cmdParms)
        {
            using (OleDbConnection sqlcnn = new OleDbConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
                try
                {
                    //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
                    PrepareCommand(sqlcmd, sqlcnn, commandType, sql, cmdParms);
                    int flag = sqlcmd.ExecuteNonQuery();
                    sqlcnn.Close();
                    //trace.Success("NonQuery:" + flag, OperationLogFlags.ExecuteNonQuery);
                    return flag;
                }
                finally
                {
                    sqlcmd.Parameters.Clear();
                    if (sqlcnn.State == ConnectionState.Open) sqlcnn.Close();
                }
            }
        }

        /// <summary>
        /// 拼接Sql
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="conn"></param>
        /// <param name="commandType"></param>
        /// <param name="sql"></param>
        /// <param name="cmdParms"></param>
        public void PrepareCommand(IDbCommand cmd, IDbConnection conn, CommandType commandType, string sql, params IDataParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.CommandType = commandType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parameter in ConvertParameters(cmdParms))
                {
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 参数转换
        /// </summary>
        /// <param name="cmdParameters"></param>
        /// <returns></returns>
        public object[] ConvertParameters(IDataParameter[] cmdParameters)
        {
            if (cmdParameters == null)
                return null;

            SqlParameter[] sqlParameters = new SqlParameter[cmdParameters.Length];

            int i = 0;
            foreach (var parms in cmdParameters)
            {
                SqlParameter p = new SqlParameter();
                p.DbType = (DbType)parms.DbType;
                sqlParameters[i] = new SqlParameter(parms.ParameterName, p.SqlDbType)
                {
                    Value = parms.Value,
                    Direction = parms.Direction,
                    IsNullable = parms.IsNullable
                };
                i++;
            }

            return sqlParameters;
        }
    }
}

