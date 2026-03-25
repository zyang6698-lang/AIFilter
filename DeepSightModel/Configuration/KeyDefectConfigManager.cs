using System;
using System.Collections.Generic;
using System.Linq;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 重点缺陷配置管理器（单例 + JSON 持久化）
    /// </summary>
    public class KeyDefectConfigManager : JsonConfigBase<KeyDefectConfig>
    {
        private static readonly Lazy<KeyDefectConfigManager> _lazy =
            new Lazy<KeyDefectConfigManager>(() => new KeyDefectConfigManager());

        public static KeyDefectConfigManager Instance => _lazy.Value;

        // 内存中的快速查找集合（缺陷名 -> 是否重点）
        private readonly object _lock = new object();
        private HashSet<string> _keyDefectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private KeyDefectConfig _cachedConfig;

        protected override string ConfigPath => ConfigPaths.KeyDefectConfigPath;

        private KeyDefectConfigManager()
        {
            ReloadCache();
        }

        protected override KeyDefectConfig GetDefaultConfig()
        {
            return new KeyDefectConfig
            {
                DefectEntries = new List<KeyDefectEntry>(),
                AlarmConfig = new KeyDefectAlarmConfig()
            };
        }

        /// <summary>
        /// 重新从磁盘加载配置到内存缓存
        /// </summary>
        public void ReloadCache()
        {
            lock (_lock)
            {
                _cachedConfig = GetConfig();
                _keyDefectNames = new HashSet<string>(
                    _cachedConfig.DefectEntries
                        .Where(e => e.IsKey)
                        .Select(e => e.DefectName),
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 判断某缺陷名是否为重点缺陷（高性能，使用内存 HashSet）
        /// </summary>
        public bool IsKeyDefect(string defectName)
        {
            if (string.IsNullOrWhiteSpace(defectName)) return false;
            lock (_lock)
            {
                return _keyDefectNames.Contains(defectName);
            }
        }

        /// <summary>
        /// 自动发现新缺陷名称：如果不存在则添加到已知列表（不标记为重点）
        /// </summary>
        /// <returns>true 表示是新发现的缺陷名</returns>
        public bool AutoDiscoverDefect(string defectName)
        {
            if (string.IsNullOrWhiteSpace(defectName)) return false;

            lock (_lock)
            {
                if (_cachedConfig.DefectEntries.Any(e =>
                    string.Equals(e.DefectName, defectName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                _cachedConfig.DefectEntries.Add(new KeyDefectEntry
                {
                    DefectName = defectName,
                    IsKey = false,
                    AutoDiscovered = true
                });

                // 异步保存，不阻塞检测流程
                try { Save(_cachedConfig); }
                catch (Exception ex) { LogTextHelper.Error($"保存重点缺陷配置失败: {ex.Message}"); }

                return true;
            }
        }

        /// <summary>
        /// 获取当前缓存的配置（只读副本）
        /// </summary>
        public KeyDefectConfig GetCachedConfig()
        {
            lock (_lock)
            {
                return _cachedConfig;
            }
        }

        /// <summary>
        /// 保存配置并刷新缓存
        /// </summary>
        public bool SaveAndReload(KeyDefectConfig config)
        {
            lock (_lock)
            {
                if (Save(config))
                {
                    _cachedConfig = config;
                    _keyDefectNames = new HashSet<string>(
                        config.DefectEntries
                            .Where(e => e.IsKey)
                            .Select(e => e.DefectName),
                        StringComparer.OrdinalIgnoreCase);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取报警配置
        /// </summary>
        public KeyDefectAlarmConfig GetAlarmConfig()
        {
            lock (_lock)
            {
                return _cachedConfig?.AlarmConfig ?? new KeyDefectAlarmConfig();
            }
        }
    }
}

