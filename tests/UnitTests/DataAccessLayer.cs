namespace SQL
{
	public class ExecutionScope : global::System.IDisposable
	{
		internal static global::System.Collections.Generic.List<int> RetryableErrors = new global::System.Collections.Generic.List<int>
		{
			53, 601, 615, 913, 921, 922, 923, 924, 926, 927, 941, 955, 956, 983, 976, 978, 979, 982, 983, 1204, 1205, 1214, 1222, 1428, 35201
		};
		public global::System.Data.SqlClient.SqlTransaction Transaction { get; private set; }
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
					() => global::System.Configuration.ConfigurationManager.ConnectionStrings["dataRules"].ConnectionString
				);
				return _connectionString;
			}
			set
			{
				_connectionString = value;
			}
		}
	}
}namespace SQL.TableVariables.common
{
public class AssetFieldDataRow
	{
public global::System.String AssetNumber { get; private set; }		public global::System.Int16? ColumnId { get; private set; }		public global::System.String Value { get; private set; }		public AssetFieldDataRow(global::System.String assetNumber, global::System.Int16? columnId, global::System.String value)
		{
			this.AssetNumber = assetNumber;			this.ColumnId = columnId;			this.Value = value;
		}	}
public class AssetFieldData : global::System.Collections.Generic.List<AssetFieldDataRow>
	{
public AssetFieldData(global::System.Collections.Generic.IEnumerable<AssetFieldDataRow> collection)
			: base(collection)
		{
		}
internal global::System.Data.DataTable GetDataTable()
		{
			var dt = new global::System.Data.DataTable();			dt.Columns.Add("AssetNumber", typeof(System.String));			dt.Columns.Add("ColumnId", typeof(System.Int16));			dt.Columns.Add("Value", typeof(System.String));
			dt.AcceptChanges();			foreach (var item in this)
			{
				var row = dt.NewRow();
				row[0] = item.AssetNumber;				row[1] = item.ColumnId;				row[2] = item.Value;				dt.Rows.Add(row);			}			return dt;
		}

	}
}namespace SQL.Enums.common
{
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
}namespace SQL.Executables.asset
{
public class BatchUpdate
	{
public class ParametersCollection
		{
public global::System.Int16? ClientId { get; private set; }
public global::SQL.TableVariables.common.AssetFieldData AssetData { get; private set; }
public global::System.Boolean? Debug { get; private set; }
public ParametersCollection(global::System.Int16? clientId, global::SQL.TableVariables.common.AssetFieldData assetData, global::System.Boolean? debug)
			{
				this.ClientId = clientId;
				this.AssetData = assetData;
				this.Debug = debug;
			}

		}
public ParametersCollection Parameters { get; private set; }
public global::System.Int32 ReturnValue { get; private set; }
public static async global::System.Threading.Tasks.Task<BatchUpdate> ExecuteAsync(global::System.Int16? clientId, global::SQL.TableVariables.common.AssetFieldData assetData, global::System.Boolean? debug, global::SQL.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
		{
			var retValue = new BatchUpdate();
			{
				var retryCycle = 0;
				while (true)
				{
					global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::SQL.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
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
							cmd.CommandText = "[asset].[BatchUpdate]";
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.SmallInt, 2, global::System.Data.ParameterDirection.Input, true, 5, 0, null, global::System.Data.DataRowVersion.Default, clientId){ });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@AssetData", global::System.Data.SqlDbType.Structured) { TypeName = "[common].[AssetFieldData]", Value = assetData.GetDataTable() });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@Debug", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, true, 1, 0, null, global::System.Data.DataRowVersion.Default, debug){ });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
							await cmd.ExecuteNonQueryAsync();
							retValue.Parameters = new ParametersCollection(clientId, assetData, debug);
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
				}}
		}
/*end*/
public static BatchUpdate Execute(global::System.Int16? clientId, global::SQL.TableVariables.common.AssetFieldData assetData, global::System.Boolean? debug, global::SQL.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
		{
			var retValue = new BatchUpdate();
			{
				var retryCycle = 0;
				while (true)
				{
					global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::SQL.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
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
							cmd.CommandText = "[asset].[BatchUpdate]";
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ClientId", global::System.Data.SqlDbType.SmallInt, 2, global::System.Data.ParameterDirection.Input, true, 5, 0, null, global::System.Data.DataRowVersion.Default, clientId){ });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@AssetData", global::System.Data.SqlDbType.Structured) { TypeName = "[common].[AssetFieldData]", Value = assetData.GetDataTable() });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@Debug", global::System.Data.SqlDbType.Bit, 1, global::System.Data.ParameterDirection.Input, true, 1, 0, null, global::System.Data.DataRowVersion.Default, debug){ });
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
							cmd.ExecuteNonQuery();
							retValue.Parameters = new ParametersCollection(clientId, assetData, debug);
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
				}}
		}
/*end*/

	}
}namespace SQL.Executables.calc
{
public class GetSampleData
	{
public class ParametersCollection
		{
public ParametersCollection()
			{
			}

		}
public ParametersCollection Parameters { get; private set; }
public global::System.Int32 ReturnValue { get; private set; }
public class Record0
		{
public global::System.String EquipNo { get; private set; }
public global::System.DateTime? AcquisitionDate { get; private set; }
public global::System.String ClientMake { get; private set; }
public global::System.String ClientModel { get; private set; }
public global::System.Int32? Year { get; private set; }
public global::System.String ForSaleFlag { get; private set; }
public global::System.Decimal? Cost { get; private set; }
public Record0(global::System.String equipNo, global::System.DateTime? acquisitionDate, global::System.String clientMake, global::System.String clientModel, global::System.Int32? year, global::System.String forSaleFlag, global::System.Decimal? cost)
			{
				this.EquipNo = equipNo;
				this.AcquisitionDate = acquisitionDate;
				this.ClientMake = clientMake;
				this.ClientModel = clientModel;
				this.Year = year;
				this.ForSaleFlag = forSaleFlag;
				this.Cost = cost;
			}

		}
public global::System.Collections.Generic.List<Record0> Recordset0{ get; private set; }
public static async global::System.Threading.Tasks.Task<GetSampleData> ExecuteAsync(global::SQL.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
		{
			var retValue = new GetSampleData();
			{
				var retryCycle = 0;
				while (true)
				{
					global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::SQL.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
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
							cmd.CommandText = "[calc].[GetSampleData]";
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
							using (global::System.Data.SqlClient.SqlDataReader reader = await cmd.ExecuteReaderAsync())
							{
								retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
								while (await reader.ReadAsync())
								{
									retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : await reader.GetFieldValueAsync<global::System.String>(0), reader.IsDBNull(1) ? null : await reader.GetFieldValueAsync<global::System.DateTime?>(1), reader.IsDBNull(2) ? null : await reader.GetFieldValueAsync<global::System.String>(2), reader.IsDBNull(3) ? null : await reader.GetFieldValueAsync<global::System.String>(3), reader.IsDBNull(4) ? null : await reader.GetFieldValueAsync<global::System.Int32?>(4), reader.IsDBNull(5) ? null : await reader.GetFieldValueAsync<global::System.String>(5), reader.IsDBNull(6) ? null : await reader.GetFieldValueAsync<global::System.Decimal?>(6)));
								}
							}
							retValue.Parameters = new ParametersCollection();
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
				}}
		}
/*end*/
public static GetSampleData Execute(global::SQL.ExecutionScope executionScope = null, global::System.Int32 commandTimeout = 30)
		{
			var retValue = new GetSampleData();
			{
				var retryCycle = 0;
				while (true)
				{
					global::System.Data.SqlClient.SqlConnection conn = executionScope == null ? new global::System.Data.SqlClient.SqlConnection(global::SQL.ExecutionScope.ConnectionString) : executionScope.Transaction.Connection;
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
							cmd.CommandText = "[calc].[GetSampleData]";
							cmd.Parameters.Add(new global::System.Data.SqlClient.SqlParameter("@ReturnValue", global::System.Data.SqlDbType.Int, 4, global::System.Data.ParameterDirection.ReturnValue, true, 0, 0, null, global::System.Data.DataRowVersion.Default, global::System.DBNull.Value));
							using (global::System.Data.SqlClient.SqlDataReader reader = cmd.ExecuteReader())
							{
								retValue.Recordset0 = new global::System.Collections.Generic.List<Record0>();
								while (reader.Read())
								{
									retValue.Recordset0.Add(new Record0(reader.IsDBNull(0) ? null : reader.GetFieldValue<global::System.String>(0), reader.IsDBNull(1) ? null : reader.GetFieldValue<global::System.DateTime?>(1), reader.IsDBNull(2) ? null : reader.GetFieldValue<global::System.String>(2), reader.IsDBNull(3) ? null : reader.GetFieldValue<global::System.String>(3), reader.IsDBNull(4) ? null : reader.GetFieldValue<global::System.Int32?>(4), reader.IsDBNull(5) ? null : reader.GetFieldValue<global::System.String>(5), reader.IsDBNull(6) ? null : reader.GetFieldValue<global::System.Decimal?>(6)));
								}
							}
							retValue.Parameters = new ParametersCollection();
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
				}}
		}
/*end*/

	}
}