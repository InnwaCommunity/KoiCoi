using MySqlConnector;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Data;
using System.Dynamic;
using Dapper;

namespace KoiCoi.Database;


public class BaseDataAccess
{
    public string ConnectionString { get; set; }
    public DbConnection _dbConnection { get; set; }

    public BaseDataAccess(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    public BaseDataAccess(DbConnection dbConnection)
    {
        this._dbConnection = dbConnection;
    }

    private MySqlConnection GetConnection()
    {
        return new MySqlConnection(ConnectionString);
    }

    private DbCommand CreateDbCommand(string Query)
    {
        try
        {
            if (_dbConnection.State == System.Data.ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
            if (_dbConnection.State == System.Data.ConnectionState.Open)
            {
                DbCommand cmd = _dbConnection.CreateCommand();
                cmd.CommandText = Query;
                cmd.CommandType = System.Data.CommandType.Text;
                return cmd;
            }
            else
            {
                throw new Exception("DB Connection failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("CreateDbCommand failed: " + DateTime.Now + ex.Message);
            throw;
        }
    }

    /*
     public dynamic ExecuteReader(string qry)
    {
        var retObject = new List<dynamic>();
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(qry, conn);
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // var dataRow = new ExpandoObject() as IDictionary<string, object>;
                        var dataRow = new DynamicObject();
                        for (var iFiled = 0; iFiled < dataReader.FieldCount; iFiled++)
                        {
                            // one can modify the next line to
                            //   if (dataReader.IsDBNull(iFiled))
                            //       dataRow.Add(dataReader.GetName(iFiled), dataReader[iFiled]);
                            // if one want don't fill the property for NULL
                            dataRow.AddProperty(
                                dataReader.GetName(iFiled),
                                dataReader.IsDBNull(iFiled) ? null : dataReader[iFiled] // use null instead of {}
                            );
                        }

                        retObject.Add(dataRow);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ExecuteReader " + DateTime.Now + ex.Message);
        }
        return retObject;
    }
     */

    public DataTable ExecuteQuery(string query, string queryparams)
    {
        DataTable dt = new DataTable();
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.CommandTimeout = 0;

                    if (queryparams.Trim() != "")
                    {
                        JArray parsedArray = JArray.Parse(queryparams);
                        foreach (JObject parsedObject in parsedArray.Children<JObject>())
                        {
                            string fieldname = "";
                            string fieldvalue = "";
                            foreach (JProperty parsedProperty in parsedObject.Properties())
                            {
                                string propertyName = parsedProperty.Name;
                                if (propertyName.Equals("name"))
                                {
                                    string propertyValue = (string)parsedProperty.Value;
                                    //Console.WriteLine("Name: {0}, Value: {1}", propertyName, propertyValue);
                                    fieldname = propertyValue;
                                }
                                else if (propertyName.Equals("value"))
                                {
                                    string propertyValue = (string)parsedProperty.Value;
                                    //Console.WriteLine("Name: {0}, Value: {1}", propertyName, propertyValue);
                                    fieldvalue = propertyValue;
                                }
                            }
                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dt.Load(dataReader);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + " Error in ExecuteQuery" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return dt;
    }

    public int ExecuteNonQuery(string qry)
    {
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(qry, conn);
                var result = cmd.ExecuteNonQuery();
                //conn.Close();
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return 0;
    }
    public dynamic ExecuteQuery(string qry)
    {
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(qry, conn);
                var result = cmd.ExecuteNonQuery();
                //conn.Close();
                return result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return 0;
    }
    public dynamic ExecuteReportQuery(string query)
    {
        dynamic response = null;
        /*  ExpandoObject queryParams = null;
           if(queryParams == null)
         {
             queryParams = new ExpandoObject();
         } */

        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    //conn.Open();
                    conn.OpenAsync().GetAwaiter().GetResult();
                    MySqlCommand cmd = new MySqlCommand(query, conn);


                    var result = conn.Query(query, new ExpandoObject());
                    return result.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + ex.Message + ex.StackTrace);
                    conn.Close();
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ExecuteReportQuery :" + ex.Message);
        }
        return response;
    }

    public DataTable ExecuteQueryNew(string query, JObject queryparams)
    {
        DataTable dt = new DataTable();
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    //conn.Open();
                    conn.OpenAsync().GetAwaiter().GetResult();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.CommandTimeout = 0;

                    if (queryparams.Count > 0)
                    {
                        foreach (var obj in queryparams)
                        {
                            string fieldname = obj.Key;
                            var fieldvalue = ((JValue)obj.Value).Value;

                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }

                    using (var dataReader = cmd.ExecuteReader())
                    {
                        dt.Load(dataReader);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + ex.Message + ex.StackTrace);
                    conn.Close();
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return dt;
    }

    public int ExecuteNonQueryWithParams(string query, JObject queryparams)
    {
        int result = -1;
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.OpenAsync().GetAwaiter().GetResult();
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    if (queryparams.Count > 0)
                    {
                        foreach (var obj in queryparams)
                        {
                            string fieldname = obj.Key;
                            var fieldvalue = ((JValue)obj.Value).Value;

                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }
                    result = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + ex.Message + ex.StackTrace);
                    conn.Close();
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return result;
    }

    public async Task<DataTable> ExecuteQueryRawAsync(string query, JObject queryparams)
    {
        DataTable dt = new DataTable();
        try
        {
            DbCommand cmd = CreateDbCommand(query);

            cmd.CommandTimeout = 0;
            if (queryparams.Count > 0)
            {
                foreach (var obj in queryparams)
                {
                    string fieldname = obj.Key;
                    var fieldvalue = ((JValue)obj.Value).Value;

                    if (fieldname.Trim() != "")
                    {
                        DbParameter dbParameter = new MySqlConnector.MySqlParameter
                        {
                            ParameterName = fieldname,
                            Value = fieldvalue
                        };
                        cmd.Parameters.Add(dbParameter);
                    }
                }
            }

            using (var dataReader = await cmd.ExecuteReaderAsync())
            {
                dt.Load(dataReader);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return dt;
    }

    public async Task<DataTable> ExecuteQueryAsync(string query, string queryparams)
    {
        DataTable dt = new DataTable();
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    await conn.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.CommandTimeout = 0;

                    if (queryparams.Trim() != "")
                    {
                        JArray parsedArray = JArray.Parse(queryparams);
                        foreach (JObject parsedObject in parsedArray.Children<JObject>())
                        {
                            string fieldname = "";
                            string fieldvalue = "";
                            foreach (JProperty parsedProperty in parsedObject.Properties())
                            {
                                string propertyName = parsedProperty.Name;
                                if (propertyName.Equals("name"))
                                {
                                    string propertyValue = (string)parsedProperty.Value;
                                    fieldname = propertyValue;
                                }
                                else if (propertyName.Equals("value"))
                                {
                                    string propertyValue = (string)parsedProperty.Value!;
                                    fieldvalue = propertyValue;
                                }
                            }
                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }

                    using (var dataReader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(dataReader);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + " Error in ExecuteQuery" + ex.Message + ex.StackTrace);
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return dt;
    }

    public int ExecuteQueryAndResponseEffectedCount(string query, JObject queryparams)
    {
        int effectedCount = 0;
        try
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    //conn.Open();
                    conn.OpenAsync().GetAwaiter().GetResult();
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    if (queryparams.Count > 0)
                    {
                        foreach (var obj in queryparams)
                        {
                            string fieldname = obj.Key;
                            var fieldvalue = ((JValue)obj.Value).Value;

                            if (fieldname.Trim() != "")
                                cmd.Parameters.AddWithValue(fieldname, fieldvalue);
                        }
                    }

                    effectedCount = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(DateTime.Now + ex.Message + ex.StackTrace);
                    conn.Close();
                }
                finally
                {
                    conn.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception :" + ex.Message);
        }
        return effectedCount;
    }
}

