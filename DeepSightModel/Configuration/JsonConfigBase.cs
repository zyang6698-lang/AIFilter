using System;
using System.IO;
using Newtonsoft.Json;
using DeepSightTool;

namespace DeepSightModel.Configuration
{
    /// <summary>
    /// JSON 配置文件读写基类
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    public abstract class JsonConfigBase<T> where T : class, new()
    {
        /// <summary>
        /// 配置文件完整路径
        /// </summary>
        protected abstract string ConfigPath { get; }

        /// <summary>
        /// 旧版配置文件路径（用于迁移，可为 null）
        /// </summary>
        protected virtual string LegacyConfigPath => null;

        /// <summary>
        /// JSON 序列化设置
        /// </summary>
        protected virtual JsonSerializerSettings JsonSettings => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// 构造函数，自动执行初始化
        /// </summary>
        protected JsonConfigBase()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化配置（确保目录存在、迁移旧配置、创建默认配置）
        /// </summary>
        protected virtual void Initialize()
        {
            ConfigPaths.EnsureConfigDirectory();
            TryMigrateLegacyConfig();

            if (!File.Exists(ConfigPath))
            {
                CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 尝试迁移旧版配置文件
        /// </summary>
        protected virtual void TryMigrateLegacyConfig()
        {
            // 子类可以重写此方法实现自定义迁移逻辑
        }

        /// <summary>
        /// 创建默认配置（由子类实现）
        /// </summary>
        protected abstract T GetDefaultConfig();

        /// <summary>
        /// 创建并保存默认配置
        /// </summary>
        protected virtual bool CreateDefaultConfig()
        {
            try
            {
                var defaultConfig = GetDefaultConfig();
                return Save(defaultConfig);
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"创建默认配置失败: {ConfigPath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="config">输出配置对象</param>
        /// <returns>是否成功</returns>
        public virtual bool Read(out T config)
        {
            config = new T();
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return false;
                }

                var json = File.ReadAllText(ConfigPath);
                config = JsonConvert.DeserializeObject<T>(json, JsonSettings) ?? new T();
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"读取配置失败: {ConfigPath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="config">配置对象</param>
        /// <returns>是否成功</returns>
        public virtual bool Save(T config)
        {
            try
            {
                ConfigPaths.EnsureConfigDirectory();
                var json = JsonConvert.SerializeObject(config, JsonSettings);
                File.WriteAllText(ConfigPath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"保存配置失败: {ConfigPath}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取配置（读取失败则返回默认值）
        /// </summary>
        /// <returns>配置对象</returns>
        public virtual T GetConfig()
        {
            if (Read(out var config))
            {
                return config;
            }
            return GetDefaultConfig();
        }
    }
}

