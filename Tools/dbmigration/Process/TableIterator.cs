using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using toolr.common;

namespace dbmigration.Process
{
    public class TableIterator
    {
        private ILogger _logger;
        private IgnoreList _ignoreList;
        private string _connString;
        private string _databaseName;
        private const string cmdStrSchema = @"select distinct TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES where TABLE_CATALOG = '[catalog]'";

        private const string cmdStr = @" SELECT c.TABLE_SCHEMA, c.TABLE_NAME, PK.PKs, COLUMN_NAME, case when DATA_TYPE in ('char', 'varchar', 'nvarchar') 
        then DATA_TYPE + '(' + case cast(CHARACTER_MAXIMUM_LENGTH as nvarchar) when '-1' then 'max' else cast(CHARACTER_MAXIMUM_LENGTH as nvarchar) end + ')' 
        else 
            case when DATA_TYPE in ('numeric') 
				then COLUMN_NAME + '(' + cast(NUMERIC_PRECISION as nvarchar)  + ',' + cast(NUMERIC_SCALE as nvarchar) + ')'  
				else DATA_TYPE 
            end
    end as 'DATA_TYPE',
    case when IS_NULLABLE='YES' then 1 else 0 end as 'IS_NULLABLE'
		
	FROM INFORMATION_SCHEMA.COLUMNS C 
	inner join INFORMATION_SCHEMA.TABLES T on t.TABLE_NAME = c.TABLE_NAME  and t.TABLE_CATALOG = c.TABLE_CATALOG  and t.TABLE_SCHEMA = c.TABLE_SCHEMA   
	left outer join 
	(SELECT distinct T.TABLE_SCHEMA, T.TABLE_NAME, STUFF((SELECT ',' + COLUMN_NAME	
		from 
		INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, 
		INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
		WHERE 
		T.TABLE_NAME = TAB.TABLE_NAME
		AND T.TABLE_SCHEMA = TAB.TABLE_SCHEMA
		AND Col.CONSTRAINT_NAME = Tab.CONSTRAINT_NAME
		AND Col.TABLE_SCHEMA = TAB.TABLE_SCHEMA
		AND Col.TABLE_NAME = Tab.TABLE_NAME
        and Col.TABLE_CATALOG = Tab.TABLE_CATALOG
		AND CONSTRAINT_TYPE = 'PRIMARY KEY'
		FOR XML PATH('')), 1, 1,'') AS PKs
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T) PK on PK.TABLE_NAME = T.TABLE_NAME and PK.TABLE_SCHEMA = T.TABLE_SCHEMA and t.TABLE_CATALOG = c.TABLE_CATALOG

	where TABLE_TYPE='BASE TABLE' and C.TABLE_CATALOG = '[catalog]'
	ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, C.ORDINAL_POSITION";

        public TableIterator(string connString, string[] ignoreList, ILogger logger) 
        {
            _ignoreList = new IgnoreList(ignoreList);
            _connString = connString;
            _logger = logger;
            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder(connString);
            _databaseName = connStr.InitialCatalog;

        }

        public List<string> GetSchemas()
        {
            List<string> results = new List<string>();
            SqlConnection conn = new SqlConnection(_connString);
            _logger.Log("Start Reading DB Schemas: " + _databaseName, EventType.Info);
            _logger.Log("");
            SqlCommand cmd = new SqlCommand(cmdStrSchema.Replace("[catalog]", _databaseName), conn);
            try
            {
                conn.Open();
                IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                {
                    results.Add(dr.GetString(0));
                }
                dr.Close();
                _logger.Log("");
                _logger.Log("End Reading DB Schemas: " + _databaseName, EventType.Info);
            }
            catch (Exception ex)
            {
                _logger.Log("Exception: " + ex.ToString(), EventType.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return results;
        }

        public List<TableInfo> GetTables()
        {
            List<TableInfo> results = new List<TableInfo>();
            SqlConnection conn = new SqlConnection(_connString);
            _logger.Log("Start Reading DB Tables: " + _databaseName, EventType.Info);
            _logger.Log("");
            SqlCommand cmd = new SqlCommand(cmdStr.Replace("[catalog]", _databaseName), conn);
            try
            {
                conn.Open();
                IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                string lastTableName = "";
                TableInfo ti = new TableInfo();
                string[] Pks = new string[0];
                while (dr.Read())
                {                   
                    var tableName = dr.GetString(0) + "." + dr.GetString(1);                    
                    if (tableName != lastTableName)
                    {                        
                        ti = new TableInfo();
                        ti.Database = _databaseName;
                        ti.Name = dr.GetString(1);
                        ti.Schema = dr.GetString(0);
                        if (!dr.IsDBNull(2))
                            Pks = dr.GetString(2).Split(',').ToArray();
                        else 
                            Pks = new string[0];
                        ti.Fields = new List<FieldInfo>();
                        
                        lastTableName = tableName;
                        if (_ignoreList.IgnoreTable(ti))
                        {
                            _logger.Log("Reading Table: " + tableName + ", Ignored.", EventType.Warning);
                            continue;
                        } else
                        {
                            _logger.Log("Reading Table: " + tableName, EventType.None);
                        }
                        results.Add(ti);
                    }
                    if (ExistField(ti.Fields, new FieldInfo() { Name = dr.GetString(3) }))
                        _logger.Log("Duplicate Field " + dr.GetString(3), EventType.Error);
                    ti.Fields.Add(new FieldInfo() { Name = dr.GetString(3), Type = dr.GetString(4), Nullable = dr.GetInt32(5)==1, IsPK = Pks.Contains(dr.GetString(3)) });
                }
                dr.Close();
                _logger.Log("");
                _logger.Log("End Reading DB Tables: " + _databaseName, EventType.Info);
            }
            catch (Exception ex)
            {
                _logger.Log("Exception: " + ex.ToString(), EventType.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
            return results;
        }

        private bool ExistField(List<FieldInfo> list, FieldInfo field)
        {
            return list.Exists(e => e.Name == field.Name);
        }
    }
}
