using DeepSightCommunication;
using DeepSightDB;
using DeepSightTool;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeepSightWorkLib.Services
{
    /// <summary>
    /// LevelDB 状态服务：通过 heartbeat 获取各 LevelDB 服务器上所有 DB 的状态，
    /// 并缓存结果以供 VRS 查询等场景判断目标 DB 是否可用。
    /// </summary>
    public class DbStatusService
    {
        private readonly LevelDbHttpClient _httpDb;

        /// <summary>
        /// 心跳结果缓存：key = URL, value = 心跳响应
        /// </summary>
        private readonly ConcurrentDictionary<string, HeartbeatResponse> _cache
            = new ConcurrentDictionary<string, HeartbeatResponse>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 缓存过期时间
        /// </summary>
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 各 URL 上次心跳时间
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _lastHeartbeatTime
            = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public DbStatusService(LevelDbHttpClient httpDb)
        {
            _httpDb = httpDb ?? throw new ArgumentNullException(nameof(httpDb));
        }

        /// <summary>
        /// 向指定 LevelDB URL 发送 heartbeat，获取该服务器上所有 DB 的状态。
        /// 结果会被缓存，在 TTL 内重复调用不会再次发送请求。
        /// </summary>
        /// <param name="url">LevelDB 服务器 URL（如 http://127.0.0.1:9877）</param>
        /// <param name="forceRefresh">是否强制刷新缓存</param>
        /// <returns>心跳响应，失败返回 null</returns>
        public HeartbeatResponse SendHeartbeat(string url, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // 检查缓存是否有效
            if (!forceRefresh &&
                _cache.TryGetValue(url, out var cached) &&
                _lastHeartbeatTime.TryGetValue(url, out var lastTime) &&
                DateTime.Now - lastTime < CacheTtl)
            {
                return cached;
            }

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                operation = "heartbeat"
            };

            if (!_httpDb.PostJson(url, req, LevelDbOperation.Read, out string response, "心跳检测"))
            {
                LogTextHelper.Warn($"DbStatusService: heartbeat 请求失败 url={url}");
                return null;
            }

            if (string.IsNullOrEmpty(response))
            {
                LogTextHelper.Warn($"DbStatusService: heartbeat 响应为空 url={url}");
                return null;
            }

            try
            {
                var heartbeat = JsonConvert.DeserializeObject<HeartbeatResponse>(response);
                if (heartbeat != null)
                {
                    _cache[url] = heartbeat;
                    _lastHeartbeatTime[url] = DateTime.Now;
                    LogTextHelper.Info($"DbStatusService: heartbeat 成功 url={url}, db_count={heartbeat.DbCount}");
                    return heartbeat;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"DbStatusService: 解析 heartbeat 响应失败 url={url}, ex={ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 异步发送 heartbeat
        /// </summary>
        public async System.Threading.Tasks.Task<HeartbeatResponse> SendHeartbeatAsync(string url, bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (!forceRefresh &&
                _cache.TryGetValue(url, out var cached) &&
                _lastHeartbeatTime.TryGetValue(url, out var lastTime) &&
                DateTime.Now - lastTime < CacheTtl)
            {
                return cached;
            }

            var req = new RootDbInfo
            {
                uniqueKey = Guid.NewGuid().ToString(),
                operation = "heartbeat"
            };

            var result = await _httpDb.PostJsonAsync(url, req, LevelDbOperation.Read, "心跳检测").ConfigureAwait(false);
            if (!result.Success || string.IsNullOrEmpty(result.Response))
            {
                LogTextHelper.Warn($"DbStatusService: heartbeat 异步请求失败 url={url}");
                return null;
            }

            try
            {
                var heartbeat = JsonConvert.DeserializeObject<HeartbeatResponse>(result.Response);
                if (heartbeat != null)
                {
                    _cache[url] = heartbeat;
                    _lastHeartbeatTime[url] = DateTime.Now;
                    LogTextHelper.Info($"DbStatusService: heartbeat 异步成功 url={url}, db_count={heartbeat.DbCount}");
                    return heartbeat;
                }
            }
            catch (Exception ex)
            {
                LogTextHelper.Error($"DbStatusService: 解析 heartbeat 响应失败 url={url}, ex={ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 检查指定 URL 上的某个 DB 是否存在且可读（get_status = true）。
        /// 会先尝试从缓存获取心跳结果，缓存未命中时自动发送 heartbeat。
        /// </summary>
        /// <param name="url">LevelDB 服务器 URL</param>
        /// <param name="dbName">数据库名称</param>
        /// <returns>DB 是否存在且 get_status 为 true</returns>
        public bool IsDbAvailable(string url, string dbName)
        {
            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(dbName))
                return false;

            var heartbeat = SendHeartbeat(url);
            if (heartbeat?.DbStatus == null)
                return false;

            var db = heartbeat.DbStatus.FirstOrDefault(
                d => string.Equals(d.DbName, dbName, StringComparison.OrdinalIgnoreCase));
            return db != null && db.GetStatus;
        }

        /// <summary>
        /// 获取指定 URL 上所有可读 DB 的名称集合。
        /// 先尝试缓存，未命中时自动发送 heartbeat。
        /// </summary>
        /// <param name="url">LevelDB 服务器 URL</param>
        /// <returns>可读 DB 名称的 HashSet（忽略大小写）</returns>
        public HashSet<string> GetAvailableDbNames(string url)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(url))
                return result;

            var heartbeat = SendHeartbeat(url);
            if (heartbeat?.DbStatus == null)
                return result;

            foreach (var db in heartbeat.DbStatus)
            {
                if (db.GetStatus && !string.IsNullOrEmpty(db.DbName))
                    result.Add(db.DbName);
            }

            return result;
        }

        /// <summary>
        /// 清除指定 URL 的缓存
        /// </summary>
        public void ClearCache(string url = null)
        {
            if (url == null)
            {
                _cache.Clear();
                _lastHeartbeatTime.Clear();
            }
            else
            {
                _cache.TryRemove(url, out _);
                _lastHeartbeatTime.TryRemove(url, out _);
            }
        }

        /// <summary>
        /// 预热：对一批 URL 发送 heartbeat 并缓存结果。
        /// 适合在批量查询开始前调用，避免后续逐个查询时的延迟。
        /// </summary>
        /// <param name="urls">需要预热的 URL 集合</param>
        public void WarmUp(IEnumerable<string> urls)
        {
            if (urls == null) return;

            foreach (var url in urls.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    SendHeartbeat(url, forceRefresh: true);
                }
                catch (Exception ex)
                {
                    LogTextHelper.Warn($"DbStatusService.WarmUp 失败 url={url}: {ex.Message}");
                }
            }
        }
    }
}
