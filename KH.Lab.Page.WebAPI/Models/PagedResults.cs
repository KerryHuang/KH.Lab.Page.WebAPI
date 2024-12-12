namespace KH.Lab.Page.WebAPI.Models;

/// <summary>
/// 表示分頁結果集。
/// </summary>
/// <typeparam name="T">結果集中項目的類型。</typeparam>
public class PagedResults<T>
{
    /// <summary>
    /// 初始化 <see cref="PagedResults{T}"/> 類別的新執行個體。
    /// </summary>
    /// <param name="totalItems">結果集中的項目總數。</param>
    /// <param name="pageNumber">當前頁碼（默認為 1）。</param>
    /// <param name="pageSize">每頁的項目數（默認為 10）。</param>
    /// <param name="maxNavigationPages">最大導航頁數（默認為 5）。</param>
    public PagedResults(int totalItems, int pageNumber = 1, int pageSize = 10, int maxNavigationPages = 5)
    {
        // 初始化屬性
        TotalItems = totalItems;
        PageSize = pageSize;
        PageNumber = CalculatePageNumber(pageNumber, CalculateTotalPages(totalItems, pageSize));
        MaxNavigationPages = maxNavigationPages;

        // 計算頁範圍
        var (startPage, endPage) = CalculatePageRange(PageNumber, CalculateTotalPages(totalItems, pageSize), maxNavigationPages);
        PageNumbers = Enumerable.Range(startPage, endPage - startPage + 1).ToList();
        StartPage = startPage;
        EndPage = endPage;
    }

    /// <summary>
    /// 獲取或設置當前頁面的項目列表。
    /// </summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 獲取結果集中的項目總數。
    /// </summary>
    public int TotalItems { get; }

    /// <summary>
    /// 獲取每頁的項目數。
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// 獲取當前頁碼。
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// 獲取最大導航頁數。
    /// </summary>
    public int MaxNavigationPages { get; }

    /// <summary>
    /// 獲取結果集中的總頁數。
    /// </summary>
    public int TotalPages => CalculateTotalPages(TotalItems, PageSize);

    /// <summary>
    /// 獲取頁範圍的起始頁碼。
    /// </summary>
    public int StartPage { get; }

    /// <summary>
    /// 獲取頁範圍的結束頁碼。
    /// </summary>
    public int EndPage { get; }

    /// <summary>
    /// 獲取頁範圍內的頁碼列表。
    /// </summary>
    public IReadOnlyList<int> PageNumbers { get; }

    /// <summary>
    /// 根據提供的頁碼和總頁數計算頁碼。
    /// </summary>
    /// <param name="pageNumber">提供的頁碼。</param>
    /// <param name="totalPages">總頁數。</param>
    /// <returns>計算出的頁碼。</returns>
    private static int CalculatePageNumber(int pageNumber, int totalPages) => Math.Min(Math.Max(pageNumber, 1), totalPages);

    /// <summary>
    /// 根據總項目數和每頁項目數計算總頁數。
    /// </summary>
    /// <param name="totalItems">總項目數。</param>
    /// <param name="pageSize">每頁項目數。</param>
    /// <returns>總頁數。</returns>
    private static int CalculateTotalPages(int totalItems, int pageSize) => (int)Math.Ceiling((decimal)totalItems / pageSize);

    /// <summary>
    /// 根據當前頁碼、總頁數和最大導航頁數計算頁範圍。
    /// </summary>
    /// <param name="pageNumber">當前頁碼。</param>
    /// <param name="totalPages">總頁數。</param>
    /// <param name="maxNavigationPages">最大導航頁數。</param>
    /// <returns>包含起始頁碼和結束頁碼的元組。</returns>
    private static (int startPage, int endPage) CalculatePageRange(int pageNumber, int totalPages, int maxNavigationPages)
    {
        // 處理邊界情況
        if (totalPages <= maxNavigationPages)
            return (1, totalPages);

        // 計算頁範圍
        var maxPagesBeforeActualPage = (int)Math.Floor(maxNavigationPages / (decimal)2);
        var maxPagesAfterActualPage = (int)Math.Ceiling(maxNavigationPages / (decimal)2) - 1;

        if (pageNumber <= maxPagesBeforeActualPage)
            return (1, maxNavigationPages);

        if (pageNumber + maxPagesAfterActualPage >= totalPages)
            return (totalPages - maxNavigationPages + 1, totalPages);

        return (pageNumber - maxPagesBeforeActualPage, pageNumber + maxPagesAfterActualPage);
    }
}
