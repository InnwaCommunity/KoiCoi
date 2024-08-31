using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Modules.Repository;

public static class RepoFunService
{
    public static Pagination getWithPagination<T>(int pageNumber, int pageSize, List<T> newlist)
    {
        int rowCount = newlist.Count;
        int pageCount = rowCount / pageSize;
        if (rowCount % pageSize > 0)
            pageCount++;

        var pagedList = newlist
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        Pagination data = new Pagination
        {
            PageCount = pageCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = pagedList
        };
        return data;
    }
}
