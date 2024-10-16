// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using TomLonghurst.AsyncRedisClient.Models.Commands;
using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Constants;

internal static class Commands
{
    internal static readonly IRedisCommand? Quit = "QUIT".ToRedisCommand();
    internal static readonly IRedisEncodable Auth = "AUTH".ToRedisEncoded();
    internal static readonly IRedisEncodable Exists = "EXISTS".ToRedisEncoded();
    internal static readonly IRedisEncodable Del = "DEL".ToRedisEncoded();
    internal static readonly IRedisEncodable Type = "TYPE".ToRedisEncoded();
    internal static readonly IRedisEncodable Keys = "KEYS".ToRedisEncoded();
    internal static readonly IRedisEncodable RandomKey = "RANDOMKEY".ToRedisEncoded();
    internal static readonly IRedisEncodable Rename = "RENAME".ToRedisEncoded();
    internal static readonly IRedisEncodable RenameNx = "RENAMENX".ToRedisEncoded();
    internal static readonly IRedisEncodable PExpire = "PEXPIRE".ToRedisEncoded();
    internal static readonly IRedisEncodable PExpireAt = "PEXPIREAT".ToRedisEncoded();
    internal static readonly IRedisCommand? DbSize = "DBSIZE".ToRedisCommand();
    internal static readonly IRedisEncodable Expire = "EXPIRE".ToRedisEncoded();
    internal static readonly IRedisEncodable ExpireAt = "EXPIREAT".ToRedisEncoded();
    internal static readonly IRedisEncodable Ttl = "TTL".ToRedisEncoded();
    internal static readonly IRedisEncodable PTtl = "PTTL".ToRedisEncoded();
    internal static readonly IRedisEncodable Select = "SELECT".ToRedisEncoded();
    internal static readonly IRedisCommand? FlushDb = "FLUSHDB".ToRedisCommand();
    internal static readonly IRedisCommand? FlushAll = "FLUSHALL".ToRedisCommand();
    internal static readonly IRedisCommand? Ping = "PING".ToRedisCommand();
    internal static readonly IRedisEncodable Echo = "ECHO".ToRedisEncoded();

    internal static readonly IRedisEncodable Save = "SAVE".ToRedisEncoded();
    internal static readonly IRedisCommand? BgSave = "BGSAVE".ToRedisCommand();
    internal static readonly IRedisEncodable LastSave = "LASTSAVE".ToRedisEncoded();
    internal static readonly IRedisCommand? Shutdown = "SHUTDOWN".ToRedisCommand();
    internal static readonly IRedisEncodable NoSave = "NOSAVE".ToRedisEncoded();
    internal static readonly IRedisEncodable BgRewriteAof = "BGREWRITEAOF".ToRedisEncoded();

    internal static readonly IRedisCommand? Info = "INFO".ToRedisCommand();
    internal static readonly IRedisEncodable SlaveOf = "SLAVEOF".ToRedisEncoded();
    internal static readonly IRedisEncodable No = "NO".ToRedisEncoded();
    internal static readonly IRedisEncodable One = "ONE".ToRedisEncoded();
    internal static readonly IRedisEncodable ResetStat = "RESETSTAT".ToRedisEncoded();
    internal static readonly IRedisEncodable Rewrite = "REWRITE".ToRedisEncoded();
    internal static readonly IRedisEncodable Time = "TIME".ToRedisEncoded();
    internal static readonly IRedisEncodable Segfault = "SEGFAULT".ToRedisEncoded();
    internal static readonly IRedisEncodable Sleep = "SLEEP".ToRedisEncoded();
    internal static readonly IRedisEncodable Dump = "DUMP".ToRedisEncoded();
    internal static readonly IRedisEncodable Restore = "RESTORE".ToRedisEncoded();
    internal static readonly IRedisEncodable Migrate = "MIGRATE".ToRedisEncoded();
    internal static readonly IRedisEncodable Move = "MOVE".ToRedisEncoded();
    internal static readonly IRedisEncodable Object = "OBJECT".ToRedisEncoded();
    internal static readonly IRedisEncodable IdleTime = "IDLETIME".ToRedisEncoded();
    internal static readonly IRedisEncodable Monitor = "MONITOR".ToRedisEncoded(); //missing
    internal static readonly IRedisEncodable Debug = "DEBUG".ToRedisEncoded(); //missing
    internal static readonly IRedisEncodable Config = "CONFIG".ToRedisEncoded(); //missing
    internal static readonly IRedisEncodable Client = "CLIENT".ToRedisEncoded();
    internal static readonly IRedisEncodable List = "LIST".ToRedisEncoded();
    internal static readonly IRedisEncodable Kill = "KILL".ToRedisEncoded();
    internal static readonly IRedisEncodable Addr = "ADDR".ToRedisEncoded();
    internal static readonly IRedisEncodable Id = "ID".ToRedisEncoded();
    internal static readonly IRedisEncodable SkipMe = "SKIPME".ToRedisEncoded();
    internal static readonly IRedisEncodable SetName = "SETNAME".ToRedisEncoded();
    internal static readonly IRedisEncodable GetName = "GETNAME".ToRedisEncoded();
    internal static readonly IRedisEncodable Pause = "PAUSE".ToRedisEncoded();
    internal static readonly IRedisEncodable Role = "ROLE".ToRedisEncoded();

    internal static readonly IRedisEncodable StrLen = "STRLEN".ToRedisEncoded();
    internal static readonly IRedisEncodable Set = "SET".ToRedisEncoded();
    internal static readonly IRedisEncodable Get = "GET".ToRedisEncoded();
    internal static readonly IRedisEncodable GetSet = "GETSET".ToRedisEncoded();
    internal static readonly IRedisEncodable MGet = "MGET".ToRedisEncoded();
    internal static readonly IRedisEncodable SetNx = "SETNX".ToRedisEncoded();
    internal static readonly IRedisEncodable SetEx = "SETEX".ToRedisEncoded();
    internal static readonly IRedisEncodable Persist = "PERSIST".ToRedisEncoded();
    internal static readonly IRedisEncodable PSetEx = "PSETEX".ToRedisEncoded();
    internal static readonly IRedisEncodable MSet = "MSET".ToRedisEncoded();
    internal static readonly IRedisEncodable MSetNx = "MSETNX".ToRedisEncoded();
    internal static readonly IRedisEncodable Incr = "INCR".ToRedisEncoded();
    internal static readonly IRedisEncodable IncrBy = "INCRBY".ToRedisEncoded();
    internal static readonly IRedisEncodable IncrByFloat = "INCRBYFLOAT".ToRedisEncoded();
    internal static readonly IRedisEncodable Decr = "DECR".ToRedisEncoded();
    internal static readonly IRedisEncodable DecrBy = "DECRBY".ToRedisEncoded();
    internal static readonly IRedisEncodable Append = "APPEND".ToRedisEncoded();
    internal static readonly IRedisEncodable GetRange = "GETRANGE".ToRedisEncoded();
    internal static readonly IRedisEncodable SetRange = "SETRANGE".ToRedisEncoded();
    internal static readonly IRedisEncodable GetBit = "GETBIT".ToRedisEncoded();
    internal static readonly IRedisEncodable SetBit = "SETBIT".ToRedisEncoded();
    internal static readonly IRedisEncodable BitCount = "BITCOUNT".ToRedisEncoded();

    internal static readonly IRedisEncodable Scan = "SCAN".ToRedisEncoded();
    internal static readonly IRedisEncodable SScan = "SSCAN".ToRedisEncoded();
    internal static readonly IRedisEncodable HScan = "HSCAN".ToRedisEncoded();
    internal static readonly IRedisEncodable ZScan = "ZSCAN".ToRedisEncoded();
    internal static readonly IRedisEncodable Match = "MATCH".ToRedisEncoded();
    internal static readonly IRedisEncodable Count = "COUNT".ToRedisEncoded();

    internal static readonly IRedisEncodable HSet = "HSET".ToRedisEncoded();
    internal static readonly IRedisEncodable HSetNx = "HSETNX".ToRedisEncoded();
    internal static readonly IRedisEncodable HGet = "HGET".ToRedisEncoded();
    internal static readonly IRedisEncodable HMSet = "HMSET".ToRedisEncoded();
    internal static readonly IRedisEncodable HMGet = "HMGET".ToRedisEncoded();
    internal static readonly IRedisEncodable HIncrBy = "HINCRBY".ToRedisEncoded();
    internal static readonly IRedisEncodable HIncrByFloat = "HINCRBYFLOAT".ToRedisEncoded();
    internal static readonly IRedisEncodable HExists = "HEXISTS".ToRedisEncoded();
    internal static readonly IRedisEncodable HDel = "HDEL".ToRedisEncoded();
    internal static readonly IRedisEncodable HLen = "HLEN".ToRedisEncoded();
    internal static readonly IRedisEncodable HKeys = "HKEYS".ToRedisEncoded();
    internal static readonly IRedisEncodable HVals = "HVALS".ToRedisEncoded();
    internal static readonly IRedisEncodable HGetAll = "HGETALL".ToRedisEncoded();

    internal static readonly IRedisEncodable Sort = "SORT".ToRedisEncoded();

    internal static readonly IRedisEncodable Watch = "WATCH".ToRedisEncoded();
    internal static readonly IRedisEncodable UnWatch = "UNWATCH".ToRedisEncoded();
    internal static readonly IRedisEncodable Multi = "MULTI".ToRedisEncoded();
    internal static readonly IRedisEncodable Exec = "EXEC".ToRedisEncoded();
    internal static readonly IRedisEncodable Discard = "DISCARD".ToRedisEncoded();

    internal static readonly IRedisEncodable Subscribe = "SUBSCRIBE".ToRedisEncoded();
    internal static readonly IRedisEncodable UnSubscribe = "UNSUBSCRIBE".ToRedisEncoded();
    internal static readonly IRedisEncodable PSubscribe = "PSUBSCRIBE".ToRedisEncoded();
    internal static readonly IRedisEncodable PUnSubscribe = "PUNSUBSCRIBE".ToRedisEncoded();
    internal static readonly IRedisEncodable Publish = "PUBLISH".ToRedisEncoded();

    internal static readonly IRedisCommand? ClusterInfo = "CLUSTER INFO".ToRedisCommand();

    internal static readonly IRedisCommand? ScriptFlush = "SCRIPT FLUSH".ToRedisCommand();

    internal static readonly IEnumerable<IRedisEncodable> ScriptLoad =
        new List<IRedisEncodable> {{"SCRIPT".ToRedisEncoded()}, "LOAD".ToRedisEncoded()};
        
    internal static readonly IRedisEncodable Script = "SCRIPT".ToRedisEncoded();
    internal static readonly IRedisEncodable Load = "LOAD".ToRedisEncoded();

    internal static readonly IRedisEncodable Eval = "EVAL".ToRedisEncoded();
    internal static readonly IRedisEncodable EvalSha = "EVALSHA".ToRedisEncoded();
        
}