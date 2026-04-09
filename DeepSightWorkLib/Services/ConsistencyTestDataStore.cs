using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepSightModel;
using DeepSightTool;
using Newtonsoft.Json;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// 一致性测试数据持久化服务 - 管理AppData中的数据集和测试历史
    /// </summary>
    public class ConsistencyTestDataStore
    {
        private static readonly string RootDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DeepSightAI", "ConsistencyTests");

        private static readonly string DatasetsFile = Path.Combine(RootDir, "datasets.json");
        private static readonly string HistoryDir = Path.Combine(RootDir, "history");

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly object _lock = new object();

        public ConsistencyTestDataStore()
        {
            EnsureDirectories();
        }

        private void EnsureDirectories()
        {
            try
            {
                if (!Directory.Exists(RootDir)) Directory.CreateDirectory(RootDir);
                if (!Directory.Exists(HistoryDir)) Directory.CreateDirectory(HistoryDir);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error("创建一致性测试目录失败", ex);
            }
        }

        #region 数据集管理

        /// <summary>
        /// 加载所有数据集
        /// </summary>
        public List<ConsistencyTestDataset> LoadDatasets()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(DatasetsFile)) return new List<ConsistencyTestDataset>();
                    var json = File.ReadAllText(DatasetsFile);
                    return JsonConvert.DeserializeObject<List<ConsistencyTestDataset>>(json, JsonSettings)
                           ?? new List<ConsistencyTestDataset>();
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("加载数据集失败", ex);
                    return new List<ConsistencyTestDataset>();
                }
            }
        }

        /// <summary>
        /// 保存所有数据集
        /// </summary>
        public bool SaveDatasets(List<ConsistencyTestDataset> datasets)
        {
            lock (_lock)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(datasets, JsonSettings);
                    File.WriteAllText(DatasetsFile, json);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error("保存数据集失败", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 添加或更新数据集
        /// </summary>
        public bool SaveDataset(ConsistencyTestDataset dataset)
        {
            var datasets = LoadDatasets();
            var idx = datasets.FindIndex(d => d.Id == dataset.Id);
            if (idx >= 0)
            {
                dataset.UpdateTime = DateTime.Now;
                datasets[idx] = dataset;
            }
            else
            {
                datasets.Add(dataset);
            }
            return SaveDatasets(datasets);
        }

        /// <summary>
        /// 删除数据集
        /// </summary>
        public bool DeleteDataset(string datasetId)
        {
            var datasets = LoadDatasets();
            datasets.RemoveAll(d => d.Id == datasetId);
            // 同时删除关联的历史记录
            DeleteHistory(datasetId);
            return SaveDatasets(datasets);
        }

        /// <summary>
        /// 根据ID获取数据集
        /// </summary>
        public ConsistencyTestDataset GetDataset(string datasetId)
        {
            return LoadDatasets().FirstOrDefault(d => d.Id == datasetId);
        }

        #endregion

        #region 历史记录管理

        private string GetHistoryPath(string datasetId)
        {
            return Path.Combine(HistoryDir, $"{datasetId}.json");
        }

        /// <summary>
        /// 加载指定数据集的测试历史
        /// </summary>
        public ConsistencyTestHistory LoadHistory(string datasetId)
        {
            lock (_lock)
            {
                try
                {
                    var path = GetHistoryPath(datasetId);
                    if (!File.Exists(path)) return new ConsistencyTestHistory { DatasetId = datasetId };
                    var json = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<ConsistencyTestHistory>(json, JsonSettings)
                           ?? new ConsistencyTestHistory { DatasetId = datasetId };
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载历史记录失败: {datasetId}", ex);
                    return new ConsistencyTestHistory { DatasetId = datasetId };
                }
            }
        }

        /// <summary>
        /// 保存测试历史
        /// </summary>
        public bool SaveHistory(ConsistencyTestHistory history)
        {
            lock (_lock)
            {
                try
                {
                    var path = GetHistoryPath(history.DatasetId);
                    var json = JsonConvert.SerializeObject(history, JsonSettings);
                    File.WriteAllText(path, json);
                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"保存历史记录失败: {history.DatasetId}", ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// 添加一轮测试结果到历史
        /// </summary>
        public bool AddRound(string datasetId, ConsistencyTestRound round)
        {
            var history = LoadHistory(datasetId);
            var dataset = GetDataset(datasetId);
            history.DatasetName = dataset?.Name ?? "";
            round.RoundNumber = history.Rounds.Count + 1;
            history.Rounds.Add(round);
            return SaveHistory(history);
        }

        /// <summary>
        /// 删除历史记录
        /// </summary>
        public void DeleteHistory(string datasetId)
        {
            lock (_lock)
            {
                try
                {
                    var path = GetHistoryPath(datasetId);
                    if (File.Exists(path)) File.Delete(path);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"删除历史记录失败: {datasetId}", ex);
                }
            }
        }

        /// <summary>
        /// 获取所有数据集的历史记录
        /// </summary>
        public List<ConsistencyTestHistory> LoadAllHistories()
        {
            var datasets = LoadDatasets();
            var histories = new List<ConsistencyTestHistory>();
            foreach (var ds in datasets)
            {
                var h = LoadHistory(ds.Id);
                if (h.Rounds.Count > 0) histories.Add(h);
            }
            return histories;
        }

        #endregion
    }
}

