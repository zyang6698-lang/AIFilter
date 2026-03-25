using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// 重点缺陷配置管理器（单例，支持多 profile + 料号映射）
    /// 每个 profile 为独立 JSON 文件，存储在 configs/keydefect/ 目录下。
    /// 料号映射存储在 configs/keydefect_mapping.config.json。
    /// </summary>
    public class KeyDefectConfigManager
    {
        public const string DefaultProfileName = "Default";

        private static readonly Lazy<KeyDefectConfigManager> _lazy =
            new Lazy<KeyDefectConfigManager>(() => new KeyDefectConfigManager());

        public static KeyDefectConfigManager Instance => _lazy.Value;

        private readonly object _lock = new object();

        // profile 名称 → 缓存数据
        private readonly Dictionary<string, ProfileCache> _profileCaches =
            new Dictionary<string, ProfileCache>(StringComparer.OrdinalIgnoreCase);

        // 料号 → profile 名称映射
        private Dictionary<string, string> _productMappings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private KeyDefectConfigManager()
        {
            ConfigPaths.EnsureKeyDefectProfileDirectory();
            MigrateLegacyConfig();
            LoadAllProfiles();
            LoadMappings();
        }

        #region 初始化与迁移

        /// <summary>
        /// 将旧版单文件 keydefect.config.json 迁移为 Default profile
        /// </summary>
        private void MigrateLegacyConfig()
        {
            var legacyPath = ConfigPaths.KeyDefectConfigPath;
            var defaultProfilePath = ConfigPaths.GetKeyDefectProfilePath(DefaultProfileName);

            if (File.Exists(legacyPath) && !File.Exists(defaultProfilePath))
            {
                try
                {
                    File.Copy(legacyPath, defaultProfilePath);
                    LogTextHelper.Info($"已将旧版缺陷配置迁移到 Default profile: {defaultProfilePath}");
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"迁移旧版缺陷配置失败: {ex.Message}");
                }
            }

            // 如果 Default profile 不存在，创建空的
            if (!File.Exists(defaultProfilePath))
            {
                SaveProfileToDisk(DefaultProfileName, new KeyDefectConfig());
            }
        }

        /// <summary>
        /// 加载所有 profile 文件到缓存
        /// </summary>
        private void LoadAllProfiles()
        {
            _profileCaches.Clear();
            var dir = ConfigPaths.KeyDefectProfileDirectory;
            if (!Directory.Exists(dir)) return;

            foreach (var file in Directory.GetFiles(dir, "*.json"))
            {
                var profileName = Path.GetFileNameWithoutExtension(file);
                try
                {
                    var json = File.ReadAllText(file);
                    var config = JsonConvert.DeserializeObject<KeyDefectConfig>(json) ?? new KeyDefectConfig();
                    _profileCaches[profileName] = BuildProfileCache(config);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"加载缺陷配置 profile '{profileName}' 失败: {ex.Message}");
                }
            }

            // 确保 Default 总是存在
            if (!_profileCaches.ContainsKey(DefaultProfileName))
            {
                _profileCaches[DefaultProfileName] = BuildProfileCache(new KeyDefectConfig());
            }
        }

        /// <summary>
        /// 加载料号映射
        /// </summary>
        private void LoadMappings()
        {
            _productMappings.Clear();
            var path = ConfigPaths.KeyDefectMappingPath;
            if (!File.Exists(path)) return;

            try
            {
                var json = File.ReadAllText(path);
                var mappingConfig = JsonConvert.DeserializeObject<ProductDefectMappingConfig>(json);
                if (mappingConfig?.Mappings != null)
                {
                    foreach (var entry in mappingConfig.Mappings)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.ProductSerial))
                            _productMappings[entry.ProductSerial] = entry.ProfileName ?? DefaultProfileName;
                    }
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"加载料号缺陷映射失败: {ex.Message}");
            }
        }

        #endregion

        #region Profile 管理

        /// <summary>
        /// 获取所有可用 profile 名称
        /// </summary>
        public List<string> GetProfileNames()
        {
            lock (_lock)
            {
                return _profileCaches.Keys.OrderBy(n =>
                    n.Equals(DefaultProfileName, StringComparison.OrdinalIgnoreCase) ? "" : n).ToList();
            }
        }

        /// <summary>
        /// 创建新 profile（复制 Default 的缺陷列表）
        /// </summary>
        public bool CreateProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return false;

            lock (_lock)
            {
                if (_profileCaches.ContainsKey(profileName)) return false;

                var defaultConfig = GetCachedConfig(DefaultProfileName);
                var newConfig = new KeyDefectConfig
                {
                    DefectEntries = defaultConfig.DefectEntries
                        .Select(e => new KeyDefectEntry
                        {
                            DefectName = e.DefectName,
                            IsKey = e.IsKey,
                            IsDirectReport = e.IsDirectReport,
                            AutoDiscovered = e.AutoDiscovered
                        }).ToList(),
                    AlarmConfig = new KeyDefectAlarmConfig
                    {
                        Enabled = defaultConfig.AlarmConfig.Enabled,
                        AlarmRatioThreshold = defaultConfig.AlarmConfig.AlarmRatioThreshold,
                        AlarmCountThreshold = defaultConfig.AlarmConfig.AlarmCountThreshold,
                        AlarmCooldownSeconds = defaultConfig.AlarmConfig.AlarmCooldownSeconds
                    }
                };

                if (SaveProfileToDisk(profileName, newConfig))
                {
                    _profileCaches[profileName] = BuildProfileCache(newConfig);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 删除 profile（不能删除 Default）
        /// </summary>
        public bool DeleteProfile(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return false;
            if (profileName.Equals(DefaultProfileName, StringComparison.OrdinalIgnoreCase)) return false;

            lock (_lock)
            {
                var path = ConfigPaths.GetKeyDefectProfilePath(profileName);
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                    _profileCaches.Remove(profileName);

                    // 清除引用该 profile 的映射，回退到 Default
                    var toRemove = _productMappings.Where(kv =>
                        string.Equals(kv.Value, profileName, StringComparison.OrdinalIgnoreCase))
                        .Select(kv => kv.Key).ToList();
                    foreach (var key in toRemove)
                        _productMappings.Remove(key);
                    SaveMappingsToDisk();

                    return true;
                }
                catch (Exception ex)
                {
                    LogTextHelper.Error($"删除 profile '{profileName}' 失败: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region 料号映射

        /// <summary>
        /// 获取指定料号对应的 profile 名称（默认返回 "Default"）
        /// </summary>
        public string GetProfileNameForProduct(string productSerial)
        {
            if (string.IsNullOrWhiteSpace(productSerial)) return DefaultProfileName;
            lock (_lock)
            {
                return _productMappings.TryGetValue(productSerial, out var name) ? name : DefaultProfileName;
            }
        }

        /// <summary>
        /// 获取当前所有料号映射
        /// </summary>
        public Dictionary<string, string> GetAllMappings()
        {
            lock (_lock)
            {
                return new Dictionary<string, string>(_productMappings, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 保存料号映射配置
        /// </summary>
        public bool SaveMappings(Dictionary<string, string> mappings)
        {
            lock (_lock)
            {
                _productMappings = new Dictionary<string, string>(mappings, StringComparer.OrdinalIgnoreCase);
                return SaveMappingsToDisk();
            }
        }

        private bool SaveMappingsToDisk()
        {
            try
            {
                var config = new ProductDefectMappingConfig
                {
                    Mappings = _productMappings.Select(kv => new ProductDefectMappingEntry
                    {
                        ProductSerial = kv.Key,
                        ProfileName = kv.Value
                    }).ToList()
                };
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(ConfigPaths.KeyDefectMappingPath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存料号缺陷映射失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 缺陷查询（按 profile 或按料号）

        /// <summary>
        /// 判断某缺陷名在指定 profile 中是否为重点缺陷
        /// </summary>
        public bool IsKeyDefect(string defectName, string profileName = null)
        {
            if (string.IsNullOrWhiteSpace(defectName)) return false;
            profileName = profileName ?? DefaultProfileName;
            lock (_lock)
            {
                if (_profileCaches.TryGetValue(profileName, out var cache))
                    return cache.KeyDefectNames.Contains(defectName);
                return false;
            }
        }

        /// <summary>
        /// 根据料号判断某缺陷名是否为重点缺陷
        /// </summary>
        public bool IsKeyDefectByProduct(string defectName, string productSerial)
        {
            return IsKeyDefect(defectName, GetProfileNameForProduct(productSerial));
        }

        /// <summary>
        /// 判断某缺陷名在指定 profile 中是否为直报缺陷
        /// </summary>
        public bool IsDirectReport(string defectName, string profileName = null)
        {
            if (string.IsNullOrWhiteSpace(defectName)) return false;
            profileName = profileName ?? DefaultProfileName;
            lock (_lock)
            {
                if (_profileCaches.TryGetValue(profileName, out var cache))
                    return cache.DirectReportNames.Contains(defectName);
                return false;
            }
        }

        /// <summary>
        /// 根据料号判断某缺陷名是否为直报缺陷
        /// </summary>
        public bool IsDirectReportByProduct(string defectName, string productSerial)
        {
            return IsDirectReport(defectName, GetProfileNameForProduct(productSerial));
        }

        /// <summary>
        /// 自动发现新缺陷名称到指定 profile（默认 Default）
        /// </summary>
        public bool AutoDiscoverDefect(string defectName, string profileName = null)
        {
            if (string.IsNullOrWhiteSpace(defectName)) return false;
            profileName = profileName ?? DefaultProfileName;

            lock (_lock)
            {
                if (!_profileCaches.TryGetValue(profileName, out var cache))
                    return false;

                if (cache.Config.DefectEntries.Any(e =>
                    string.Equals(e.DefectName, defectName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                cache.Config.DefectEntries.Add(new KeyDefectEntry
                {
                    DefectName = defectName,
                    IsKey = false,
                    AutoDiscovered = true
                });

                try { SaveProfileToDisk(profileName, cache.Config); }
                catch (Exception ex) { LogTextHelper.Error($"保存缺陷配置 profile '{profileName}' 失败: {ex.Message}"); }

                return true;
            }
        }

        #endregion

        #region 配置读写

        /// <summary>
        /// 获取指定 profile 的缓存配置
        /// </summary>
        public KeyDefectConfig GetCachedConfig(string profileName = null)
        {
            profileName = profileName ?? DefaultProfileName;
            lock (_lock)
            {
                if (_profileCaches.TryGetValue(profileName, out var cache))
                    return cache.Config;
                return new KeyDefectConfig();
            }
        }

        /// <summary>
        /// 保存指定 profile 的配置并刷新缓存
        /// </summary>
        public bool SaveAndReload(KeyDefectConfig config, string profileName = null)
        {
            profileName = profileName ?? DefaultProfileName;
            lock (_lock)
            {
                if (SaveProfileToDisk(profileName, config))
                {
                    _profileCaches[profileName] = BuildProfileCache(config);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取指定 profile 的报警配置
        /// </summary>
        public KeyDefectAlarmConfig GetAlarmConfig(string profileName = null)
        {
            profileName = profileName ?? DefaultProfileName;
            lock (_lock)
            {
                if (_profileCaches.TryGetValue(profileName, out var cache))
                    return cache.Config?.AlarmConfig ?? new KeyDefectAlarmConfig();
                return new KeyDefectAlarmConfig();
            }
        }

        /// <summary>
        /// 重新从磁盘加载所有配置
        /// </summary>
        public void ReloadCache()
        {
            lock (_lock)
            {
                LoadAllProfiles();
                LoadMappings();
            }
        }

        #endregion

        #region 内部工具

        private bool SaveProfileToDisk(string profileName, KeyDefectConfig config)
        {
            try
            {
                ConfigPaths.EnsureKeyDefectProfileDirectory();
                var path = ConfigPaths.GetKeyDefectProfilePath(profileName);
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存缺陷配置 profile '{profileName}' 失败: {ex.Message}");
                return false;
            }
        }

        private static ProfileCache BuildProfileCache(KeyDefectConfig config)
        {
            return new ProfileCache
            {
                Config = config,
                KeyDefectNames = new HashSet<string>(
                    config.DefectEntries.Where(e => e.IsKey).Select(e => e.DefectName),
                    StringComparer.OrdinalIgnoreCase),
                DirectReportNames = new HashSet<string>(
                    config.DefectEntries.Where(e => e.IsDirectReport).Select(e => e.DefectName),
                    StringComparer.OrdinalIgnoreCase)
            };
        }

        /// <summary>
        /// 内部缓存结构
        /// </summary>
        private class ProfileCache
        {
            public KeyDefectConfig Config;
            public HashSet<string> KeyDefectNames;
            public HashSet<string> DirectReportNames;
        }

        #endregion
    }
}

