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

        internal const string PfAdd = "PFADD";
        internal const string PfCount = "PFCOUNT";
        internal const string PfMerge = "PFMERGE";

        internal const string RPush = "RPUSH";
        internal const string LPush = "LPUSH";
        internal const string RPushX = "RPUSHX";
        internal const string LPushX = "LPUSHX";
        internal const string LLen = "LLEN";
        internal const string LRange = "LRANGE";
        internal const string LTrim = "LTRIM";
        internal const string LIndex = "LINDEX";
        internal const string LInsert = "LINSERT";
        internal const string Before = "BEFORE";
        internal const string After = "AFTER";
        internal const string LSet = "LSET";
        internal const string LRem = "LREM";
        internal const string LPop = "LPOP";
        internal const string RPop = "RPOP";
        internal const string BLPop = "BLPOP";
        internal const string BRPop = "BRPOP";
        internal const string RPopLPush = "RPOPLPUSH";
        internal const string BRPopLPush = "BRPOPLPUSH";

        internal const string SAdd = "SADD";
        internal const string SRem = "SREM";
        internal const string SPop = "SPOP";
        internal const string SMove = "SMOVE";
        internal const string SCard = "SCARD";
        internal const string SIsMember = "SISMEMBER";
        internal const string SInter = "SINTER";
        internal const string SInterStore = "SINTERSTORE";
        internal const string SUnion = "SUNION";
        internal const string SUnionStore = "SUNIONSTORE";
        internal const string SDiff = "SDIFF";
        internal const string SDiffStore = "SDIFFSTORE";
        internal const string SMembers = "SMEMBERS";
        internal const string SRandMember = "SRANDMEMBER";

        internal const string ZAdd = "ZADD";
        internal const string ZRem = "ZREM";
        internal const string ZIncrBy = "ZINCRBY";
        internal const string ZRank = "ZRANK";
        internal const string ZRevRank = "ZREVRANK";
        internal const string ZRange = "ZRANGE";
        internal const string ZRevRange = "ZREVRANGE";
        internal const string ZRangeByScore = "ZRANGEBYSCORE";
        internal const string ZRevRangeByScore = "ZREVRANGEBYSCORE";
        internal const string ZCard = "ZCARD";
        internal const string ZScore = "ZSCORE";
        internal const string ZCount = "ZCOUNT";
        internal const string ZRemRangeByRank = "ZREMRANGEBYRANK";
        internal const string ZRemRangeByScore = "ZREMRANGEBYSCORE";
        internal const string ZUnionStore = "ZUNIONSTORE";
        internal const string ZInterStore = "ZINTERSTORE";
        internal const string ZRangeByLex = "ZRANGEBYLEX";
        internal const string ZLexCount = "ZLEXCOUNT";
        internal const string ZRemRangeByLex = "ZREMRANGEBYLEX";

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


        internal const string WithScores = "WITHSCORES";
        internal const string Limit = "LIMIT";
        internal const string By = "BY";
        internal const string Asc = "ASC";
        internal const string Desc = "DESC";
        internal const string Alpha = "ALPHA";
        internal const string Store = "STORE";

        internal const string Eval = "EVAL";
        internal const string EvalSha = "EVALSHA";
        internal const string Script = "SCRIPT";
        internal const string Load = "LOAD";
        internal const string Flush = "FLUSH";
        internal const string Slowlog = "SLOWLOG";

        internal const string Ex = "EX";
        internal const string Px = "PX";
        internal const string Nx = "NX";
        internal const string Xx = "XX";

        // Sentinel commands
        internal const string Sentinel = "SENTINEL";
        internal const string Masters = "masters";
        internal const string Sentinels = "sentinels";
        internal const string Master = "master";
        internal const string Slaves = "slaves";
        internal const string Failover = "failover";
        internal const string GetMasterAddrByName = "get-master-addr-by-name";

        //Geo commands
        internal const string GeoAdd = "GEOADD";
        internal const string GeoDist = "GEODIST";
        internal const string GeoHash = "GEOHASH";
        internal const string GeoPos = "GEOPOS";
        internal const string GeoRadius = "GEORADIUS";
        internal const string GeoRadiusByMember = "GEORADIUSBYMEMBER";

        internal const string WithCoord = "WITHCOORD";
        internal const string WithDist = "WITHDIST";
        internal const string WithHash = "WITHHASH";
    }
}
