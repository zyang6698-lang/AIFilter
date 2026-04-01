using DeepSightModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepSightDB.Interfaces
{
    /// <summary>
    /// 鏁版嵁搴撴湇鍔℃帴鍙?
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// 鑾峰彇鏁版嵁搴撻槦鍒楅暱搴?
        /// </summary>
        int GetQueueLength();

        /// <summary>
        /// 淇濆瓨/鏇存柊 PanelSide 鏁版嵁
        /// </summary>
        /// <param name="record">PanelSide 璁板綍</param>
        void SavePanelSide(PanelSideRecord record);

        /// <summary>
        /// 鎵归噺淇濆瓨 PanelSide 鏁版嵁
        /// </summary>
        /// <param name="records">璁板綍闆嗗悎</param>
        /// <param name="batchSize">姣忔壒娆℃彁浜ょ殑璁板綍鏁?/param>
        Task SavePanelSidesBatch(IEnumerable<PanelSideRecord> records, int batchSize = 100);

        /// <summary>
        /// 淇濆瓨鍛樺伐鎶ュ憡
        /// </summary>
        /// <param name="report">鍛樺伐鎶ュ憡</param>
        void SaveEmployeeReport(EmployeeReport report);

        /// <summary>
        /// 鑾峰彇鍛樺伐鎶ュ憡鍒楄〃
        /// </summary>
        /// <param name="start">寮€濮嬫椂闂?/param>
        /// <param name="end">缁撴潫鏃堕棿</param>
        Task<List<EmployeeReport>> GetEmployeeReports(DateTime start, DateTime end);

        /// <summary>
        /// 鑾峰彇鍛樺伐ID鍒楄〃
        /// </summary>
        /// <param name="start">寮€濮嬫椂闂?/param>
        /// <param name="end">缁撴潫鏃堕棿</param>
        Task<List<string>> GetEmployeeIds(DateTime start, DateTime end);

        /// <summary>
        /// 鏍规嵁鏈哄彴鍜屾壒娆¤幏鍙栭潰鏉挎暟鎹?
        /// </summary>
        /// <param name="machineId">鏈哄彴ID</param>
        /// <param name="lotNumber">鎵规鍙?/param>
        Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber);

        /// <summary>
        /// 鑾峰彇闈㈡澘鏁版嵁
        /// </summary>
        /// <param name="start">寮€濮嬫椂闂?/param>
        /// <param name="end">缁撴潫鏃堕棿</param>
        /// <param name="partNumber">鏂欏彿锛堝彲閫夛級</param>
        Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partNumber = null);

        /// <summary>
        /// 浠?CSV 鏁版嵁瀵煎叆
        /// </summary>
        /// <param name="panelsCsvLines">Panels CSV 鏁版嵁琛?/param>
        /// <param name="panelSidesCsvLines">PanelSides CSV 鏁版嵁琛?/param>
        /// <param name="progressCallback">杩涘害鍥炶皟</param>
        Task<(int success, int failed)> ImportFromCsvData(string[] panelsCsvLines, string[] panelSidesCsvLines, Action<int, string> progressCallback = null);

        /// <summary>
        /// 鑾峰彇鎵€鏈夋満鍙癐D
        /// </summary>
        Task<List<string>> GetAllMachineIds();

        /// <summary>
        /// 娓呯┖鎵€鏈夋暟鎹?
        /// </summary>
        Task<bool> ClearAllData();

        /// <summary>
        /// 鍒嗛〉鑾峰彇鏈€杩戠殑Lot鍒楄〃锛堟寜鏈€鏂版娴嬫椂闂村€掑簭锛?
        /// </summary>
        /// <param name="page">椤电爜锛堜粠1寮€濮嬶級</param>
        /// <param name="pageSize">姣忛〉Lot鏁伴噺</param>
        /// <returns>Lot鍙峰垪琛?/returns>
        Task<List<string>> GetRecentLotNumbers(int page, int pageSize);

        /// <summary>
        /// 鑾峰彇鏁版嵁搴撲腑涓嶉噸澶嶇殑Lot鎬绘暟
        /// </summary>
        Task<int> GetTotalLotCount();
    }
}

