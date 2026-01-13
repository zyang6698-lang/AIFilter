using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeepSightDB.Interfaces
{
    /// <summary>
    /// 数据库服务接口
    /// </summary>
    public interface IDatabaseService : IDisposable
    {
        /// <summary>
        /// 获取数据库队列长度
        /// </summary>
        int GetQueueLength();

        /// <summary>
        /// 保存/更新 PanelSide 数据
        /// </summary>
        /// <param name="record">PanelSide 记录</param>
        void SavePanelSide(PanelSideRecord record);

        /// <summary>
        /// 批量保存 PanelSide 数据
        /// </summary>
        /// <param name="records">记录集合</param>
        /// <param name="batchSize">每批次提交的记录数</param>
        Task SavePanelSidesBatch(IEnumerable<PanelSideRecord> records, int batchSize = 100);

        /// <summary>
        /// 保存员工报告
        /// </summary>
        /// <param name="report">员工报告</param>
        void SaveEmployeeReport(EmployeeReport report);

        /// <summary>
        /// 获取员工报告列表
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        Task<List<EmployeeReport>> GetEmployeeReports(DateTime start, DateTime end);

        /// <summary>
        /// 获取员工ID列表
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        Task<List<string>> GetEmployeeIds(DateTime start, DateTime end);

        /// <summary>
        /// 根据机台和批次获取面板数据
        /// </summary>
        /// <param name="machineId">机台ID</param>
        /// <param name="lotNumber">批次号</param>
        Task<List<PanelDataRecord>> GetPanelsDataByMachineAndLot(string machineId, string lotNumber);

        /// <summary>
        /// 获取面板数据
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="partNumber">料号（可选）</param>
        Task<List<PanelDataRecord>> GetPanelsData(DateTime start, DateTime end, string partNumber = null);

        /// <summary>
        /// 从 CSV 数据导入
        /// </summary>
        /// <param name="panelsCsvLines">Panels CSV 数据行</param>
        /// <param name="panelSidesCsvLines">PanelSides CSV 数据行</param>
        /// <param name="progressCallback">进度回调</param>
        Task<(int success, int failed)> ImportFromCsvData(string[] panelsCsvLines, string[] panelSidesCsvLines, Action<int, string> progressCallback = null);

        /// <summary>
        /// 获取所有机台ID
        /// </summary>
        Task<List<string>> GetAllMachineIds();

        /// <summary>
        /// 清空所有数据
        /// </summary>
        Task<bool> ClearAllData();
    }
}

