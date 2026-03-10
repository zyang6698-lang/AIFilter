using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepSightTool;
using Newtonsoft.Json;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 机台注册表配置管理器（使用 JSON 格式）
    /// 管理所有机台的基础信息（名称、启用状态、数据源类型），
    /// 与 Agent 专有配置（AVIConfig）解耦。
    /// </summary>
    public class MachineRegistryManager : JsonConfigBase<MachineRegistryConfig>
    {
        /// <summary>
        /// 配置文件路径
        /// </summary>
        protected override string ConfigPath => ConfigPaths.MachineRegistryConfigPath;

        /// <summary>
        /// 跳过基类的自动初始化（因为迁移逻辑需要外部传入 AVIConfig 数据）
        /// 调用方需在构造后手动调用 TryMigrateFromAviConfig 或确认文件已存在
        /// </summary>
        protected override void Initialize()
        {
            ConfigPaths.EnsureConfigDirectory();
            // 不自动创建默认配置，交给 Machine.Init() 控制迁移时机
        }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        protected override MachineRegistryConfig GetDefaultConfig()
        {
            return new MachineRegistryConfig
            {
                Machines = new List<MachineEntry>
                {
                    new MachineEntry
                    {
                        MachineName = DefaultValues.DefaultMachineName,
                        IsEnable = false,
                        DataSourceType = DefaultValues.DefaultDataSourceType
                    }
                }
            };
        }

        /// <summary>
        /// 检查是否存在 Agent 类型的机台
        /// </summary>
        public bool HasAgentMachines()
        {
            if (Read(out var config))
            {
                return config.Machines.Any(m => m.DataSourceType == DataSourceType.Agent && m.IsEnable);
            }
            return false;
        }

        /// <summary>
        /// 根据机台名查找
        /// </summary>
        public MachineEntry FindByName(string machineName)
        {
            if (Read(out var config))
            {
                return config.Machines.FirstOrDefault(m => m.MachineName == machineName);
            }
            return null;
        }

        /// <summary>
        /// 添加或更新机台（按 MachineName 匹配）
        /// </summary>
        public bool AddOrUpdate(MachineEntry entry)
        {
            if (Read(out var config))
            {
                var existing = config.Machines.FirstOrDefault(m => m.MachineName == entry.MachineName);
                if (existing != null)
                {
                    existing.IsEnable = entry.IsEnable;
                    existing.DataSourceType = entry.DataSourceType;
                }
                else
                {
                    config.Machines.Add(entry);
                }
                return Save(config);
            }
            return false;
        }

        /// <summary>
        /// 从现有的 AVIConfig WatchPaths 迁移生成机台注册表
        /// （仅当注册表配置文件不存在时执行）
        /// </summary>
        public bool TryMigrateFromAviConfig(List<WatchPathConfig> watchPaths)
        {
            // 如果注册表文件已存在，不覆盖
            if (File.Exists(ConfigPath))
            {
                return true;
            }

            if (watchPaths == null || watchPaths.Count == 0)
            {
                // 没有可迁移的数据，创建默认配置
                return Save(GetDefaultConfig());
            }

            var config = new MachineRegistryConfig
            {
                Machines = watchPaths.Select(w => new MachineEntry
                {
                    MachineName = w.AviName,
                    IsEnable = w.IsEnable,
                    // 如果 WatchPath 有 APath/BPath 等 Agent 配置，标记为 Agent 类型
                    DataSourceType = !string.IsNullOrEmpty(w.APath) || !string.IsNullOrEmpty(w.BPath)
                        ? DataSourceType.Agent
                        : DataSourceType.LevelDb
                }).ToList()
            };

            LogTextHelper.Info($"从 AVIConfig 迁移生成机台注册表，共 {config.Machines.Count} 个机台");
            return Save(config);
        }
    }
}

