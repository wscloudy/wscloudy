using System;
using System.Data.SqlClient;
using System.Data;

namespace wscloudy.MSSQLClient
{
    /// <summary>
    /// 
    /// </summary>
    public class MSSQLHelper
    {
        /// <summary>
        /// 数据库连接语句
        /// </summary>
        public string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        //public SqlDBHelper()
        //{
        //    ConnectionString = ConfigurationManager.ConnectionStrings["pubsConnectionString"].ConnectionString;
        //}

        /// <summary>
        /// 
        /// </summary>
        public MSSQLHelper(string connStr)
        {
            ConnectionString = connStr;
        }

        /// <summary>
        /// 连接测试
        /// </summary>
        /// <param name="messageStr"></param>
        /// <returns></returns>
        public bool ConnectTest(out string messageStr)
        {
            SqlConnection sqlcnn = new SqlConnection();
            messageStr = "";
            try
            {
                //ConnectionString = ConfigurationManager.ConnectionStrings["pubsConnectionString"].ConnectionString;
                sqlcnn.ConnectionString = ConnectionString;

                if (sqlcnn.State != ConnectionState.Open)
                    sqlcnn.Open();

                if (sqlcnn.State == ConnectionState.Open)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                messageStr = ex.Message;
            }
            finally
            {
                sqlcnn.Close();
            }
            return false;
        }

        /// <summary>
        /// 执行SQL语句,返回受影响记录数
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="cmdParms"></param>
        /// <returns>该操作影响的行数</returns>
        public int ExecuteNonQuery(string sql, params IDataParameter[] cmdParms)
        {
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
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
        /// 执行SQL语句,返回IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns>IDataReader</returns>
        public IDataReader ExecuteReader(string sql, params IDataParameter[] cmdParms)
        {
            SqlConnection sqlcnn = new SqlConnection(ConnectionString);
            SqlCommand sqlcmd = new SqlCommand();
            PrepareCommand(sqlcmd, sqlcnn, null, sql, cmdParms);
            //var trace = DbExecTrace.Start(sqlcmd, cmdParms);
            SqlDataReader sdr = sqlcmd.ExecuteReader(CommandBehavior.CloseConnection);
            sqlcmd.Parameters.Clear();
            //trace.Success("Reader", OperationLogFlags.ExecuteReader);
            return sdr;
        }

        public DataTable ExecuteDataTable(string text, CommandType type, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(conn.CreateCommand()))
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
        /// 执行SQL语句,返回IDataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns>IDataReader</returns>
        //public IDataReader ExecuteProReader(string sql, params IDataParameter[] cmdParms)
        //{
        //    sql = ODBHelper.ConvertParameterToSql(sql, cmdParms);

        //    return ExecuteReader(sql);
        //}

        /// <summary>
        /// 执行SQL，返回结果集中第一行第一列
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params IDataParameter[] cmdParms)
        {
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
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
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
            {
                SqlCommand sqlcmd = new SqlCommand();
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
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
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
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
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
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        //public int ExecuteSqlTran(Hashtable sqlList)
        //{
        //    using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
        //    {
        //        sqlcnn.Open();
        //        using (SqlTransaction trans = sqlcnn.BeginTransaction())
        //        {
        //            //var trace = DbExecTrace.StartTrans();
        //            SqlCommand sqlcmd = new SqlCommand();
        //            try
        //            {
        //                var list = new List<Action>();
        //                //受影响总条数
        //                int count = 0;
        //                //循环
        //                foreach (DictionaryEntry myDE in sqlList)
        //                {
        //                    string sql = myDE.Key.ToString();
        //                    var cmdParms = (IDataParameter[])ConvertParameters((IDataParameter[])myDE.Value);
        //                    PrepareCommand(sqlcmd, sqlcnn, trans, sql, cmdParms);
        //                    trace.Trace(sqlcmd, cmdParms);
        //                    int val = sqlcmd.ExecuteNonQuery();
        //                    //if (val == 0)
        //                    //{
        //                    //    trace.FailTrans();
        //                    //    trans.Rollback();
        //                    //    return 0;
        //                    //}
        //                    count += val;
        //                    sqlcmd.Parameters.Clear();
        //                    trace.Success("trans:" + count, OperationLogFlags.ExecuteNonQuery);
        //                }
        //                trans.Commit();
        //                trace.SuccessTrans();
        //                list.ForEach(p => p());
        //                list.Clear();
        //                return count;
        //            }
        //            catch (Exception ex)
        //            {
        //                trace.FailTrans();
        //                trans.Rollback();
        //                throw new Exception(ex.Message);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="sqlList"></param>
        /// <returns></returns>
        //public int ExecuteSqlTran(IList<ZJNBLH.Business.ODBHelper.CommandInfo> sqlList)
        //{
        //    using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
        //    {
        //        sqlcnn.Open();
        //        using (SqlTransaction trans = sqlcnn.BeginTransaction())
        //        {
        //            var trace = DbExecTrace.StartTrans();
        //            SqlCommand sqlcmd = new SqlCommand();
        //            try
        //            {
        //                var list = new List<Action>();
        //                //受影响总条数
        //                int count = 0;
        //                //是否跳过下次操作
        //                bool Execution = false;
        //                //循环
        //                foreach (ZJNBLH.Business.ODBHelper.CommandInfo myDE in sqlList)
        //                {
        //                    string sql = myDE.Sql;
        //                    var cmdParms = (IDataParameter[])ConvertParameters(myDE.Parameters);// (AseParameter[])(IDataParameter[])myDE.Parameters;
        //                    PrepareCommand(sqlcmd, sqlcnn, trans, sql, cmdParms);
        //                    int val = 0;//执行Sql返回值
        //                    if (myDE.State == ZJNBLH.Business.ODBHelper.CommandInfo.StateEnum.Skip)//如果判断为空，则跳过下一次
        //                    {
        //                        trace.Trace(sqlcmd, cmdParms);
        //                        val = (int)sqlcmd.ExecuteScalar();
        //                        trace.Success("trans:Scalar:r=" + val, OperationLogFlags.ExecuteScalar);
        //                        if (val == myDE.IsValue)
        //                            Execution = true;
        //                    }
        //                    else if (myDE.State == ZJNBLH.Business.ODBHelper.CommandInfo.StateEnum.Check)//如果判断不为空，则跳过下一次
        //                    {
        //                        trace.Trace(sqlcmd, cmdParms);
        //                        val = Convert.ToInt32(sqlcmd.ExecuteScalar());
        //                        trace.Success("trans:Scalar:r=" + val, OperationLogFlags.ExecuteScalar);
        //                        if (val != myDE.IsValue)
        //                            Execution = true;
        //                    }
        //                    else if (myDE.State == ZJNBLH.Business.ODBHelper.CommandInfo.StateEnum.Beyond)//如果没有超出判断值，则跳过下一次
        //                    {
        //                        trace.Trace(sqlcmd, cmdParms);
        //                        val = (int)sqlcmd.ExecuteScalar();
        //                        trace.Success("trans:Scalar:r=" + val, OperationLogFlags.ExecuteScalar);
        //                        if (val < myDE.IsValue)
        //                            Execution = true;
        //                    }
        //                    else if (myDE.State == ZJNBLH.Business.ODBHelper.CommandInfo.StateEnum.Normal)
        //                    {
        //                        //跳过本次操作
        //                        if (Execution)
        //                        {
        //                            Execution = false;
        //                            sqlcmd.Parameters.Clear();
        //                            continue;
        //                        }
        //                        trace.Trace(sqlcmd, cmdParms);
        //                        val = sqlcmd.ExecuteNonQuery();
        //                        trace.Success("trans:NonQuery:r=" + val, OperationLogFlags.ExecuteNonQuery);
        //                        //if (val == 0)
        //                        //{
        //                        //    trans.Rollback();
        //                        //    return 0;
        //                        //}
        //                        count += val;
        //                    }
        //                    sqlcmd.Parameters.Clear();
        //                }
        //                trans.Commit();
        //                trace.SuccessTrans();
        //                list.ForEach(p => p());
        //                list.Clear();
        //                return count;
        //            }
        //            catch (Exception ex)
        //            {
        //                trans.Rollback();
        //                trace.FailTrans();
        //                return 0;
        //            }
        //        }
        //    }
        //}

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
                foreach (SqlParameter parameter in ConvertParameters(cmdParms))
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
            using (SqlConnection sqlcnn = new SqlConnection(ConnectionString))
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

    /// <summary>
    /// 
    /// </summary>
    //public class SqlDBHelperFactory : IDBHelperFactory
    //{
    //    /// <summary>
    //    /// 创建实体类
    //    /// </summary>
    //    /// <returns></returns>
    //    public IDBHelper CreateHepler()
    //    {
    //        return new SqlDBHelper();
    //    }

    //    /// <summary>
    //    /// 创建实体类
    //    /// </summary>
    //    /// <param name="connStr"></param>
    //    /// <returns></returns>
    //    public IDBHelper CreateHepler(string connStr)
    //    {
    //        return new SqlDBHelper(connStr);
    //    }
    //}
}