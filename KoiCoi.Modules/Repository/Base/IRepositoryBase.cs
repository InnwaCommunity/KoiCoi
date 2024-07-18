/*using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace KoiCoi.Modules.Repository.Base;


public interface IRepositoryBase<T>
{
    T FindByID(int ID);
    T FindByCompositeID(int ID1, int ID2);
    T FindByCompositeID(int ID1, int ID2, int ID3);
    IEnumerable<T> FindAll();
    IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression);
    bool AnyByCondition(Expression<Func<T, bool>> expression);
    void Create(dynamic entity, bool flush = true);
    void CreateRange(dynamic entity, bool flush = true);
    void Update(dynamic entity, bool flush = true);
    void UpdateRange(dynamic entity, bool flush = true);
    void Delete(dynamic entity, bool flush = true);
    void DeleteRange(dynamic entity, bool flush = true);
    void DiscardChanges();
    void Save();
    T FindByCompositeID(int ID1, int ID2, int ID3, int ID4, string ID5);
    T FindByCompositeID(int ID1, int ID2, int ID3, int ID4);
    T FindByCompositeID(string ID1, int ID2, int ID3);
    T FindByCompositeID(int ID1, int ID2, string ID3);
    T FindByCompositeID(int ID1, int ID2, int ID3, int ID4, int ID5);
    T FindByCompositeID(string ID1, int ID2);
    T FindByCompositeID(DateTime ID1, int ID2, int ID3);

    dynamic ExecuteRawQuery(string selectClause, string whereClause, dynamic objParams, JObject queryFilter = null);
    dynamic ExecuteRawQueryForEventLog(string selectClause, string whereClause, dynamic objParams, JObject queryFilter = null, string leaveEngineFailClause = "");
    dynamic ExecuteRawQueryWithDistinct(string selectClause, string whereClause, dynamic objParams, JObject queryFilter = null);

}


 */