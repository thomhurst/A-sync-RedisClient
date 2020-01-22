// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System.Collections.Generic;
using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    internal static class Commands
    {
        internal static readonly string Quit = "QUIT";
        internal static readonly string Auth = "AUTH";
        internal static readonly string Exists = "EXISTS";
        internal static readonly string Del = "DEL";
        internal static readonly string Type = "TYPE";
        internal static readonly string Keys = "KEYS";
        internal static readonly string RandomKey = "RANDOMKEY";
        internal static readonly string Rename = "RENAME";
        internal static readonly string RenameNx = "RENAMENX";
        internal static readonly string PExpire = "PEXPIRE";
        internal static readonly string PExpireAt = "PEXPIREAT";
        internal static readonly string DbSize = "DBSIZE";
        internal static readonly string Expire = "EXPIRE";
        internal static readonly string ExpireAt = "EXPIREAT";
        internal static readonly string Ttl = "TTL";
        internal static readonly string PTtl = "PTTL";
        internal static readonly string Select = "SELECT";
        internal static readonly string FlushDb = "FLUSHDB";
        internal static readonly string FlushAll = "FLUSHALL";
        internal static readonly string Ping = "PING";
        internal static readonly string Echo = "ECHO";

        internal static readonly string Save = "SAVE";
        internal static readonly string BgSave = "BGSAVE";
        internal static readonly string LastSave = "LASTSAVE";
        internal static readonly string Shutdown = "SHUTDOWN";
        internal static readonly string NoSave = "NOSAVE";
        internal static readonly string BgRewriteAof = "BGREWRITEAOF";

        internal static readonly string Info = "INFO";
        internal static readonly string SlaveOf = "SLAVEOF";
        internal static readonly string No = "NO";
        internal static readonly string One = "ONE";
        internal static readonly string ResetStat = "RESETSTAT";
        internal static readonly string Rewrite = "REWRITE";
        internal static readonly string Time = "TIME";
        internal static readonly string Segfault = "SEGFAULT";
        internal static readonly string Sleep = "SLEEP";
        internal static readonly string Dump = "DUMP";
        internal static readonly string Restore = "RESTORE";
        internal static readonly string Migrate = "MIGRATE";
        internal static readonly string Move = "MOVE";
        internal static readonly string Object = "OBJECT";
        internal static readonly string IdleTime = "IDLETIME";
        internal static readonly string Monitor = "MONITOR"; //missing
        internal static readonly string Debug = "DEBUG"; //missing
        internal static readonly string Config = "CONFIG"; //missing
        internal static readonly string Client = "CLIENT";
        internal static readonly string List = "LIST";
        internal static readonly string Kill = "KILL";
        internal static readonly string Addr = "ADDR";
        internal static readonly string Id = "ID";
        internal static readonly string SkipMe = "SKIPME";
        internal static readonly string SetName = "SETNAME";
        internal static readonly string GetName = "GETNAME";
        internal static readonly string Pause = "PAUSE";
        internal static readonly string Role = "ROLE";

        internal static readonly string StrLen = "STRLEN";
        internal static readonly string Set = "SET";
        internal static readonly string Get = "GET";
        internal static readonly string GetSet = "GETSET";
        internal static readonly string MGet = "MGET";
        internal static readonly string SetNx = "SETNX";
        internal static readonly string SetEx = "SETEX";
        internal static readonly string Persist = "PERSIST";
        internal static readonly string PSetEx = "PSETEX";
        internal static readonly string MSet = "MSET";
        internal static readonly string MSetNx = "MSETNX";
        internal static readonly string Incr = "INCR";
        internal static readonly string IncrBy = "INCRBY";
        internal static readonly string IncrByFloat = "INCRBYFLOAT";
        internal static readonly string Decr = "DECR";
        internal static readonly string DecrBy = "DECRBY";
        internal static readonly string Append = "APPEND";
        internal static readonly string GetRange = "GETRANGE";
        internal static readonly string SetRange = "SETRANGE";
        internal static readonly string GetBit = "GETBIT";
        internal static readonly string SetBit = "SETBIT";
        internal static readonly string BitCount = "BITCOUNT";

        internal static readonly string Scan = "SCAN";
        internal static readonly string SScan = "SSCAN";
        internal static readonly string HScan = "HSCAN";
        internal static readonly string ZScan = "ZSCAN";
        internal static readonly string Match = "MATCH";
        internal static readonly string Count = "COUNT";

        internal static readonly string HSet = "HSET";
        internal static readonly string HSetNx = "HSETNX";
        internal static readonly string HGet = "HGET";
        internal static readonly string HMSet = "HMSET";
        internal static readonly string HMGet = "HMGET";
        internal static readonly string HIncrBy = "HINCRBY";
        internal static readonly string HIncrByFloat = "HINCRBYFLOAT";
        internal static readonly string HExists = "HEXISTS";
        internal static readonly string HDel = "HDEL";
        internal static readonly string HLen = "HLEN";
        internal static readonly string HKeys = "HKEYS";
        internal static readonly string HVals = "HVALS";
        internal static readonly string HGetAll = "HGETALL";

        internal static readonly string Sort = "SORT";

        internal static readonly string Watch = "WATCH";
        internal static readonly string UnWatch = "UNWATCH";
        internal static readonly string Multi = "MULTI";
        internal static readonly string Exec = "EXEC";
        internal static readonly string Discard = "DISCARD";

        internal static readonly string Subscribe = "SUBSCRIBE";
        internal static readonly string UnSubscribe = "UNSUBSCRIBE";
        internal static readonly string PSubscribe = "PSUBSCRIBE";
        internal static readonly string PUnSubscribe = "PUNSUBSCRIBE";
        internal static readonly string Publish = "PUBLISH";

        internal static readonly string ClusterInfo = "CLUSTER INFO";

        internal static readonly string ScriptFlush = "SCRIPT FLUSH";

        internal static readonly IEnumerable<string> ScriptLoad =
            new List<string> {{"SCRIPT"}, "LOAD"};
        
        internal static readonly string Script = "SCRIPT";
        internal static readonly string Load = "LOAD";

        internal static readonly string Eval = "EVAL";
        internal static readonly string EvalSha = "EVALSHA";
        
    }
}
