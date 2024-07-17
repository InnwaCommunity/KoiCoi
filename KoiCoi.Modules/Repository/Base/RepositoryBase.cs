using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Modules.Repository.Base;


public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected AppDbContext RepositoryContext { get; set; }
    public string _OldObjString { get; set; }

    public RepositoryBase(AppDbContext repositoryContext)
    {
        this.RepositoryContext = repositoryContext;
    }

    public T FindByID(int ID)
    {

        T obj;

        obj = this.RepositoryContext.Set<T>().Find(ID);

        return obj;
    }

    //need to call this function in the correct order of composite primary key 
    public T FindByCompositeID(int ID1, int ID2)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2);

        return obj;
    }

    //need to call this function in the correct order of composite primary key 
    public T FindByCompositeID(int ID1, int ID2, int ID3)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3);

        return obj;
    }
    public IEnumerable<T> FindAll()
    {
        return this.RepositoryContext.Set<T>();
    }

    public IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression)
    {
        return this.RepositoryContext.Set<T>().Where(expression);
    }

    public bool AnyByCondition(Expression<Func<T, bool>> expression)
    {
        return this.RepositoryContext.Set<T>().Any(expression);
    }

    public dynamic ExecuteRawQuery(string selectClause, string whereClause, dynamic objParams = null, JObject queryFilter = null)
    {
        string sqlCommand = "";
        DataTable res = new DataTable();
        DataTable resTotalCount = new DataTable();
        if (objParams != null && objParams.gridState.page != null)
        {
            int pageIndex = Convert.ToInt32(objParams.gridState.page);
            int pageSize = Convert.ToInt32(objParams.gridState.pageSize);
            int startRow = (pageIndex - 1) * pageSize;
            int endRow = pageSize;

            if (queryFilter == null)
            {
                queryFilter = new JObject();
            }
            queryFilter.Add("@startRow", startRow);
            queryFilter.Add("@endRow", endRow);

            sqlCommand = selectClause + " " + whereClause + " limit @startRow,@endRow;";
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            sqlCommand = "SELECT count(*) as TotalRecord " + whereClause;
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

        }
        else
        {
            sqlCommand = selectClause + " " + whereClause;
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            sqlCommand = "SELECT count(*) as TotalRecord " + whereClause;
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);
        }

        return new { data = res, total = resTotalCount.Rows[0]["TotalRecord"] };
    }

    // add by psm
    public dynamic ExecuteRawQueryForEventLog(string selectClause, string whereClause, dynamic objParams = null, JObject queryFilter = null, string leaveEngineFailClause = "")
    {
        string sqlCommand = "";
        DataTable res = new DataTable();
        DataTable resTotalCount = new DataTable();
        if (objParams != null && objParams.gridState.page != null)
        {
            int pageIndex = Convert.ToInt32(objParams.gridState.page);
            int pageSize = Convert.ToInt32(objParams.gridState.pageSize);
            int startRow = (pageIndex - 1) * pageSize;
            int endRow = pageSize;

            if (queryFilter == null)
            {
                queryFilter = new JObject();
            }
            queryFilter.Add("@startRow", startRow);
            queryFilter.Add("@endRow", endRow);

            sqlCommand = selectClause + " " + whereClause + " limit @startRow,@endRow;";
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            whereClause = whereClause.ToString().Replace("FormattedLogDateTime", "ev.LogDateTime");
            sqlCommand = "SELECT count(*) as TotalRecord " + whereClause;

            if (sqlCommand.Contains("LeaveBalanceEngineTask"))
            {
                sqlCommand = leaveEngineFailClause;
            }
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

        }
        else
        {
            sqlCommand = selectClause + " " + whereClause;
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            whereClause = whereClause.ToString().Replace("FormattedLogDateTime", "ev.LogDateTime");
            sqlCommand = "SELECT count(*) as TotalRecord " + whereClause;

            if (sqlCommand.Contains("LeaveBalanceEngineTask"))
            {
                sqlCommand = leaveEngineFailClause;
            }
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);
        }

        return new { data = res, total = resTotalCount.Rows[0]["TotalRecord"] };
    }

    public dynamic ExecuteRawQueryWithDistinct(string selectClause, string whereClause, dynamic objParams = null, JObject queryFilter = null)
    {
        string sqlCommand = "";
        DataTable res = new DataTable();
        DataTable resTotalCount = new DataTable();
        if (objParams != null)
        {
            int pageIndex = Convert.ToInt32(objParams.gridState.page);
            int pageSize = Convert.ToInt32(objParams.gridState.pageSize);
            int startRow = (pageIndex - 1) * pageSize;
            int endRow = pageSize;

            if (queryFilter == null)
            {
                queryFilter = new JObject();
            }
            queryFilter.Add("@startRow", startRow);
            queryFilter.Add("@endRow", endRow);

            sqlCommand = selectClause + " " + whereClause + " limit @startRow,@endRow;";
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            sqlCommand = "SELECT count(distinct emp.EmployeeID) as TotalRecord " + whereClause;
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

        }
        else
        {
            sqlCommand = selectClause + " " + whereClause;
            res = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);

            sqlCommand = "SELECT count(distinct emp.EmployeeID) as TotalRecord " + whereClause;
            resTotalCount = RepositoryContext.RunExecuteRawQuery(sqlCommand, queryFilter);
        }

        return new { data = res, total = resTotalCount.Rows[0]["TotalRecord"] };
    }
    public void Create(dynamic entity, bool flush = true)
    {
        entity.SetEventLogMessage(this.SetOldObjectToString(entity));
        this.RepositoryContext.Set<T>().Add(entity);
        if (flush) this.Save();
    }

    public void CreateRange(dynamic entity, bool flush = true)
    {
        this.RepositoryContext.Set<T>().AddRange(entity);
        if (flush) this.Save();
    }

    public void Update(dynamic entity, bool flush = true)
    {
        entity.SetEventLogMessage(this.GetUpdateEventLogString(entity));
        this.RepositoryContext.Set<T>().Update(entity);
        if (flush)
        {
            this.Save();

        }
    }

    public void UpdateRange(dynamic entity, bool flush = true)
    {
        //entity.SetEventLogMessage(this.GetUpdateEventLogString(entity));
        this.RepositoryContext.Set<T>().UpdateRange(entity);
        if (flush) this.Save();
    }

    public void Delete(dynamic entity, bool flush = true)
    {
        entity.SetEventLogMessage(this.SetOldObjectToString(entity));
        this.RepositoryContext.Set<T>().Remove(entity);
        if (flush) this.Save();
    }

    public void DeleteRange(dynamic entity, bool flush = true)
    {
        //entity.SetEventLogMessage(this.SetOldObjectToString(entity));
        this.RepositoryContext.Set<T>().RemoveRange(entity);
        if (flush) this.Save();
    }

    public void DiscardChanges()
    {
        this.RepositoryContext.ChangeTracker.Entries()
        .Where(e => e.Entity != null).ToList()
        .ForEach(e => e.State = EntityState.Detached);

    }

    public void Save()
    {
        this.RepositoryContext.SaveChanges();
    }

    public string SetOldObjectToString(dynamic OldObj)
    {
        _OldObjString = "";
        JObject _duplicateObj = JObject.FromObject(OldObj);
        var _List = _duplicateObj.ToObject<Dictionary<string, object>>();
        foreach (var item in _List)
        {
            var name = item.Key;
            var val = item.Value;
            string msg = name + " : " + val + "\r\n";
            _OldObjString += msg;
        }
        return _OldObjString;
    }

    public String GetUpdateEventLogString(dynamic entity)
    {
        PropertyValues oldObj;
        string _OldObjString = "";
        try
        {
            oldObj = this.RepositoryContext.Entry(entity).OriginalValues;
            if (oldObj == null) return "";
            JObject _newObj = JObject.FromObject(entity);
            var _newList = _newObj.ToObject<Dictionary<string, object>>();

            foreach (var item in oldObj.Properties)
            {
                string name = item.Name;
                var val = oldObj[name] != null ? oldObj[name].ToString().Trim() : "";
                var newval = _newList.GetValueOrDefault(name) != null ? _newList.GetValueOrDefault(name).ToString().Trim() : "";

                string msg = "";
                if (val != newval) msg = name + " : " + val + " >>> " + newval + "\r\n";
                _OldObjString += msg;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(DateTime.Now.ToString() + " - " + "Exception :" + ex.Message);
        }
        return _OldObjString;
    }
    //need to call this function in the correct order of composite primary key 
    public T FindByCompositeID(int ID1, int ID2, int ID3, int ID4, string ID5)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3, ID4, ID5);

        return obj;
    }
    public T FindByCompositeID(int ID1, int ID2, string ID3)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3);

        return obj;
    }

    public T FindByCompositeID(int ID1, int ID2, int ID3, int ID4)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3, ID4);

        return obj;
    }

    public T FindByCompositeID(string ID1, int ID2, int ID3)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3);

        return obj;
    }
    //need to call this function in the correct order of composite primary key 
    public T FindByCompositeID(int ID1, int ID2, int ID3, int ID4, int ID5)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3, ID4, ID5);

        return obj;
    }
    public T FindByCompositeID(string ID1, int ID2)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2);

        return obj;
    }

    public T FindByCompositeID(DateTime ID1, int ID2, int ID3)
    {
        T obj;
        obj = this.RepositoryContext.Set<T>().Find(ID1, ID2, ID3);

        return obj;
    }
}
public static class Extensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
      => dict.TryGetValue(key, out var value) ? value : default(TValue);
}
