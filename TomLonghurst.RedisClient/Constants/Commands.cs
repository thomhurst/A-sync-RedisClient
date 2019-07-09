// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace TomLonghurst.RedisClient.Constants
{
    internal static class Commands
    {
        internal const string Quit = "QUIT";
        internal const string Auth = "AUTH";
        internal const string Exists = "EXISTS";
        internal const string Del = "DEL";
        internal const string Type = "TYPE";
        internal const string Keys = "KEYS";
        internal const string RandomKey = "RANDOMKEY";
        internal const string Rename = "RENAME";
        internal const string RenameNx = "RENAMENX";
        internal const string PExpire = "PEXPIRE";
        internal const string PExpireAt = "PEXPIREAT";
        internal const string DbSize = "DBSIZE";
        internal const string Expire = "EXPIRE";
        internal const string ExpireAt = "EXPIREAT";
        internal const string Ttl = "TTL";
        internal const string PTtl = "PTTL";
        internal const string Select = "SELECT";
        internal const string FlushDb = "FLUSHDB";
        internal const string FlushAll = "FLUSHALL";
        internal const string Ping = "PING";
        internal const string Echo = "ECHO";

        internal const string Save = "SAVE";
        internal const string BgSave = "BGSAVE";
        internal const string LastSave = "LASTSAVE";
        internal const string Shutdown = "SHUTDOWN";
        internal const string NoSave = "NOSAVE";
        internal const string BgRewriteAof = "BGREWRITEAOF";

        internal const string Info = "INFO";
        internal const string SlaveOf = "SLAVEOF";
        internal const string No = "NO";
        internal const string One = "ONE";
        internal const string ResetStat = "RESETSTAT";
        internal const string Rewrite = "REWRITE";
        internal const string Time = "TIME";
        internal const string Segfault = "SEGFAULT";
        internal const string Sleep = "SLEEP";
        internal const string Dump = "DUMP";
        internal const string Restore = "RESTORE";
        internal const string Migrate = "MIGRATE";
        internal const string Move = "MOVE";
        internal const string Object = "OBJECT";
        internal const string IdleTime = "IDLETIME";
        internal const string Monitor = "MONITOR";        //missing
        internal const string Debug = "DEBUG";            //missing
        internal const string Config = "CONFIG";          //missing
        internal const string Client = "CLIENT";
        internal const string List = "LIST";
        internal const string Kill = "KILL";
        internal const string Addr = "ADDR";
        internal const string Id = "ID";
        internal const string SkipMe = "SKIPME";
        internal const string SetName = "SETNAME";
        internal const string GetName = "GETNAME";
        internal const string Pause = "PAUSE";
        internal const string Role = "ROLE";

        internal const string StrLen = "STRLEN";
        internal const string Set = "SET";
        internal const string Get = "GET";
        internal const string GetSet = "GETSET";
        internal const string MGet = "MGET";
        internal const string SetNx = "SETNX";
        internal const string SetEx = "SETEX";
        internal const string Persist = "PERSIST";
        internal const string PSetEx = "PSETEX";
        internal const string MSet = "MSET";
        internal const string MSetNx = "MSETNX";
        internal const string Incr = "INCR";
        internal const string IncrBy = "INCRBY";
        internal const string IncrByFloat = "INCRBYFLOAT";
        internal const string Decr = "DECR";
        internal const string DecrBy = "DECRBY";
        internal const string Append = "APPEND";
        internal const string GetRange = "GETRANGE";
        internal const string SetRange = "SETRANGE";
        internal const string GetBit = "GETBIT";
        internal const string SetBit = "SETBIT";
        internal const string BitCount = "BITCOUNT";

        internal const string Scan = "SCAN";
        internal const string SScan = "SSCAN";
        internal const string HScan = "HSCAN";
        internal const string ZScan = "ZSCAN";
        internal const string Match = "MATCH";
        internal const string Count = "COUNT";

        internal const string HSet = "HSET";
        internal const string HSetNx = "HSETNX";
        internal const string HGet = "HGET";
        internal const string HMSet = "HMSET";
        internal const string HMGet = "HMGET";
        internal const string HIncrBy = "HINCRBY";
        internal const string HIncrByFloat = "HINCRBYFLOAT";
        internal const string HExists = "HEXISTS";
        internal const string HDel = "HDEL";
        internal const string HLen = "HLEN";
        internal const string HKeys = "HKEYS";
        internal const string HVals = "HVALS";
        internal const string HGetAll = "HGETALL";

        internal const string Sort = "SORT";

        internal const string Watch = "WATCH";
        internal const string UnWatch = "UNWATCH";
        internal const string Multi = "MULTI";
        internal const string Exec = "EXEC";
        internal const string Discard = "DISCARD";

        internal const string Subscribe = "SUBSCRIBE";
        internal const string UnSubscribe = "UNSUBSCRIBE";
        internal const string PSubscribe = "PSUBSCRIBE";
        internal const string PUnSubscribe = "PUNSUBSCRIBE";
        internal const string Publish = "PUBLISH";
    }
}
