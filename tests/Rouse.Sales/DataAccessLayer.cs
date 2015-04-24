namespace Rouse.Sales.Rules
{
    public class ExecutionScope : global::System.IDisposable
    {
        internal static global::System.Collections.Generic.List<int> RetryableErrors = new global::System.Collections.Generic.List<int>
        {
			53, 601, 615, 913, 921, 922, 923, 924, 926, 927, 941, 955, 956, 983, 976, 978, 979, 982, 983, 1204, 1205, 1214, 1222, 1428, 35201
		};
        public global::System.Data.SqlClient.SqlTransaction Transaction
        {
            get;
            private set;
        }
        private readonly global::System.Data.SqlClient.SqlConnection _connection;
        public ExecutionScope()
        {
            this._connection = new global::System.Data.SqlClient.SqlConnection(ConnectionString);
            this._connection.Open();
            this.Transaction = _connection.BeginTransaction();
        }
        public void Commit()
        {
            if (this.Transaction != null)
                this.Transaction.Commit();
        }
        public void Rollback()
        {
            if (this.Transaction != null)
                this.Transaction.Rollback();
        }
        public void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Dispose();
            }
            if (this._connection != null && this._connection.State != System.Data.ConnectionState.Closed)
            {
                this._connection.Close();
                this._connection.Dispose();
            }
        }
        private static global::System.String _connectionString;
        public static global::System.String ConnectionString
        {
            get
            {
                global::System.Threading.LazyInitializer.EnsureInitialized(
                    ref _connectionString,
                    () => global::System.Configuration.ConfigurationManager.ConnectionStrings["calc"].ConnectionString
                );
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
    }
}
namespace Rouse.Sales.Rules.TableVariables.common
{
    public class AssetFieldDataRow
    {
        public global::System.String AssetNumber
        {
            get;
            private set;
        }

        public global::System.Int16? ColumnId
        {
            get;
            private set;
        }

        public global::System.String Value
        {
            get;
            private set;
        }
        public AssetFieldDataRow(global::System.String assetNumber, global::System.Int16? columnId, global::System.String value)
        {
            this.AssetNumber = assetNumber;
            this.ColumnId = columnId;
            this.Value = value;
        }
    }
    public class AssetFieldData : global::System.Collections.Generic.List<AssetFieldDataRow>
    {
        public AssetFieldData(global::System.Collections.Generic.IEnumerable<AssetFieldDataRow> collection) : base(collection)
        {
        }
        internal global::System.Data.DataTable GetDataTable()
        {
            var dt = new global::System.Data.DataTable();
            dt.Columns.Add("AssetNumber", typeof(System.String));
            dt.Columns.Add("ColumnId", typeof(System.Int16));
            dt.Columns.Add("Value", typeof(System.String));
            dt.AcceptChanges();
            foreach (var item in this)
            {
                var row = dt.NewRow();
                row[0] = item.AssetNumber;
                row[1] = item.ColumnId;
                row[2] = item.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
namespace Rouse.Sales.Rules.Enums.common
{
    public enum Column_Source
    {
        External = 1,
        Computed = 2
    }
    public enum DataType
    {
        Bool = 1,
        Byte = 2,
        Decimal = 3,
        Int32 = 4,
        Int64 = 5,
        Int16 = 6,
        String = 7,
        DateTime = 8
    }
}
namespace Rouse.Sales.Rules.Executables.RulesRepository
{
    public class Columns_List
    {
        public class ParametersCollection
        {
            public global::System.Int32? ClientId
            {
                get;
                private set;
            }
            public ParametersCollection(global::System.Int32? clientId)
            {
                this.ClientId = clientId;
            }
        }
        public ParametersCollection Parameters
        {
            get;
            private set;
        }
        public global::System.Int32 ReturnValue
        {
            get;
            private set;
        }
        public class Record0
        {
            public global::System.String ColumnName
            {
                get;
                private set;
            }
            public global::System.String DataTypeName
            {
                get;
                private set;
            }
            public global::System.Byte? SourceId
            {
                get;
                private set;
            }
            public Record0(global::System.String columnName, global::System.String dataTypeName, global::System.Byte? sourceId)
            {
                this.ColumnName = columnName;
                this.DataTypeName = dataTypeName;
                this.SourceId = sourceId;
            }
        }
        public global::System.Collections.Generic.List<Record0> Recordset0
        {
            get;
            private set;
        }
        public static async global::System.Threading.Tasks.Task<Columns_List> ExecuteAsync(global::System.Int32? clientId, global::Rouse.Sales.Rules.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
        {
            var retValue = new Columns_List();
            {
                var retryCycle = 0;
                while (true)
                {
                    global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::Rouse.Sales.Rules.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
                    try
                    {
                        if (conn.State != global::System.Data.ConnectionState.Open)
                        {
                            if (executionScope == null)
                            {
                                await conn.OpenAsync();
                            }
                            else
                            {
                                retryCycle = int.MaxValue;
                                throw new global::System.Exception("Execution Scope must have an open connection.");
                            }
                        }
                        using (global::System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = global::System.Data.CommandType.StoredProcedure;
                            if (executionScope != null && executionScope.Transaction != null)
                                cmd.Transaction = executionScope.Transaction;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = "[RulesRepository].[Columns.List]";
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, true, 10, 0, null, global::System.Data.DataRowVersion.Default, clientId)
                            {
                            });
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
                            using (global::System.Data.SqlClient.SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
                                while (await reader.ReadAsync())
                                {
                                    retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : await reader.GetFieldValueAsync<global::System.String>(0), reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<global::System.String>(1), reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<global::System.Byte?>(2)));
                                }
                            }
                            retValue.Parameters = new ParametersCollection(clientId);
                            retValue.ReturnValue = (global::System.Int32)cmd.Parameters["@ReturnValue"].Value;
                            return retValue;
                        }
                    }
                    catch (global::System.Data.SqlClient.SqlException e)
                    {
                        if (retryCycle++ > 9 || !ExecutionScope.RetryableErrors.Contains(e.Number))
                            throw;
                        global::System.Threading.Thread.Sleep(1000);
                    }
                    finally
                    {
                        if (executionScope == null && conn != null)
                        {
                            ((global::System.IDisposable)conn).Dispose();
                        }
                    }
                }
            }
        }/*end*/
        public static Columns_List Execute(global::System.Int32? clientId, global::Rouse.Sales.Rules.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
        {
            var retValue = new Columns_List();
            {
                var retryCycle = 0;
                while (true)
                {
                    global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::Rouse.Sales.Rules.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
                    try
                    {
                        if (conn.State != global::System.Data.ConnectionState.Open)
                        {
                            if (executionScope == null)
                            {
                                conn.Open();
                            }
                            else
                            {
                                retryCycle = int.MaxValue;
                                throw new global::System.Exception("Execution Scope must have an open connection.");
                            }
                        }
                        using (global::System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = global::System.Data.CommandType.StoredProcedure;
                            if (executionScope != null && executionScope.Transaction != null)
                                cmd.Transaction = executionScope.Transaction;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = "[RulesRepository].[Columns.List]";
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, true, 10, 0, null, global::System.Data.DataRowVersion.Default, clientId)
                            {
                            });
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
                            using (global::System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                            {
                                retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
                                while (reader.Read())
                                {
                                    retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : reader.GetFieldValue<global::System.String>(0), reader.IsDBNull(1) ? null : reader.GetFieldValue<global::System.String>(1), reader.IsDBNull(2) ? null : reader.GetFieldValue<global::System.Byte?>(2)));
                                }
                            }
                            retValue.Parameters = new ParametersCollection(clientId);
                            retValue.ReturnValue = (global::System.Int32)cmd.Parameters["@ReturnValue"].Value;
                            return retValue;
                        }
                    }
                    catch (global::System.Data.SqlClient.SqlException e)
                    {
                        if (retryCycle++ > 9 || !ExecutionScope.RetryableErrors.Contains(e.Number))
                            throw;
                        global::System.Threading.Thread.Sleep(1000);
                    }
                    finally
                    {
                        if (executionScope == null && conn != null)
                        {
                            ((global::System.IDisposable)conn).Dispose();
                        }
                    }
                }
            }
        }/*end*/
    }
    public class Variables_List
    {
        public class ParametersCollection
        {
            public global::System.Int32? ClientId
            {
                get;
                private set;
            }
            public ParametersCollection(global::System.Int32? clientId)
            {
                this.ClientId = clientId;
            }
        }
        public ParametersCollection Parameters
        {
            get;
            private set;
        }
        public global::System.Int32 ReturnValue
        {
            get;
            private set;
        }
        public class Record0
        {
            public global::System.String Name
            {
                get;
                private set;
            }
            public global::System.String Formula
            {
                get;
                private set;
            }
            public global::System.String OutputColumnName
            {
                get;
                private set;
            }
            public global::System.String DataTypeName
            {
                get;
                private set;
            }
            public Record0(global::System.String name, global::System.String formula, global::System.String outputColumnName, global::System.String dataTypeName)
            {
                this.Name = name;
                this.Formula = formula;
                this.OutputColumnName = outputColumnName;
                this.DataTypeName = dataTypeName;
            }
        }
        public global::System.Collections.Generic.List<Record0> Recordset0
        {
            get;
            private set;
        }
        public static async global::System.Threading.Tasks.Task<Variables_List> ExecuteAsync(global::System.Int32? clientId, global::Rouse.Sales.Rules.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
        {
            var retValue = new Variables_List();
            {
                var retryCycle = 0;
                while (true)
                {
                    global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::Rouse.Sales.Rules.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
                    try
                    {
                        if (conn.State != global::System.Data.ConnectionState.Open)
                        {
                            if (executionScope == null)
                            {
                                await conn.OpenAsync();
                            }
                            else
                            {
                                retryCycle = int.MaxValue;
                                throw new global::System.Exception("Execution Scope must have an open connection.");
                            }
                        }
                        using (global::System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = global::System.Data.CommandType.StoredProcedure;
                            if (executionScope != null && executionScope.Transaction != null)
                                cmd.Transaction = executionScope.Transaction;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = "[RulesRepository].[Variables.List]";
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, true, 10, 0, null, global::System.Data.DataRowVersion.Default, clientId)
                            {
                            });
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
                            using (global::System.Data.SqlClient.SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
                                while (await reader.ReadAsync())
                                {
                                    retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : await reader.GetFieldValueAsync<global::System.String>(0), reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<global::System.String>(1), reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<global::System.String>(2), reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<global::System.String>(3)));
                                }
                            }
                            retValue.Parameters = new ParametersCollection(clientId);
                            retValue.ReturnValue = (global::System.Int32)cmd.Parameters["@ReturnValue"].Value;
                            return retValue;
                        }
                    }
                    catch (global::System.Data.SqlClient.SqlException e)
                    {
                        if (retryCycle++ > 9 || !ExecutionScope.RetryableErrors.Contains(e.Number))
                            throw;
                        global::System.Threading.Thread.Sleep(1000);
                    }
                    finally
                    {
                        if (executionScope == null && conn != null)
                        {
                            ((global::System.IDisposable)conn).Dispose();
                        }
                    }
                }
            }
        }/*end*/
        public static Variables_List Execute(global::System.Int32? clientId, global::Rouse.Sales.Rules.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
        {
            var retValue = new Variables_List();
            {
                var retryCycle = 0;
                while (true)
                {
                    global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::Rouse.Sales.Rules.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
                    try
                    {
                        if (conn.State != global::System.Data.ConnectionState.Open)
                        {
                            if (executionScope == null)
                            {
                                conn.Open();
                            }
                            else
                            {
                                retryCycle = int.MaxValue;
                                throw new global::System.Exception("Execution Scope must have an open connection.");
                            }
                        }
                        using (global::System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandType = global::System.Data.CommandType.StoredProcedure;
                            if (executionScope != null && executionScope.Transaction != null)
                                cmd.Transaction = executionScope.Transaction;
                            cmd.CommandTimeout = commandTimeout;
                            cmd.CommandText = "[RulesRepository].[Variables.List]";
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.Input, true, 10, 0, null, global::System.Data.DataRowVersion.Default, clientId)
                            {
                            });
                            cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
                            using (global::System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
                            {
                                retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
                                while (reader.Read())
                                {
                                    retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : reader.GetFieldValue<global::System.String>(0), reader.IsDBNull(1) ? null : reader.GetFieldValue<global::System.String>(1), reader.IsDBNull(2) ? null : reader.GetFieldValue<global::System.String>(2), reader.IsDBNull(3) ? null : reader.GetFieldValue<global::System.String>(3)));
                                }
                            }
                            retValue.Parameters = new ParametersCollection(clientId);
                            retValue.ReturnValue = (global::System.Int32)cmd.Parameters["@ReturnValue"].Value;
                            return retValue;
                        }
                    }
                    catch (global::System.Data.SqlClient.SqlException e)
                    {
                        if (retryCycle++ > 9 || !ExecutionScope.RetryableErrors.Contains(e.Number))
                            throw;
                        global::System.Threading.Thread.Sleep(1000);
                    }
                    finally
                    {
                        if (executionScope == null && conn != null)
                        {
                            ((global::System.IDisposable)conn).Dispose();
                        }
                    }
                }
            }
        }/*end*/
    }
}