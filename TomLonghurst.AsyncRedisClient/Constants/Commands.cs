// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using TomLonghurst.AsyncRedisClient.Extensions;

namespace TomLonghurst.AsyncRedisClient.Constants;

internal static class Commands
{
    internal static readonly byte[] Quit = Map("QUIT");
    internal static readonly byte[] Auth = Map("AUTH");
    internal static readonly byte[] Exists = Map("EXISTS");
    internal static readonly byte[] Del = Map("DEL");
    internal static readonly byte[] Type = Map("TYPE");
    internal static readonly byte[] Keys = Map("KEYS");
    internal static readonly byte[] RandomKey = Map("RANDOMKEY");
    internal static readonly byte[] Rename = Map("RENAME");
    internal static readonly byte[] RenameNx = Map("RENAMENX");
    internal static readonly byte[] PExpire = Map("PEXPIRE");
    internal static readonly byte[] PExpireAt = Map("PEXPIREAT");
    internal static readonly byte[] DbSize = Map("DBSIZE");
    internal static readonly byte[] Expire = Map("EXPIRE");
    internal static readonly byte[] ExpireAt = Map("EXPIREAT");
    internal static readonly byte[] Ttl = Map("TTL");
    internal static readonly byte[] PTtl = Map("PTTL");
    internal static readonly byte[] Select = Map("SELECT");
    internal static readonly byte[] FlushDb = Map("FLUSHDB");
    internal static readonly byte[] FlushAll = Map("FLUSHALL");
    internal static readonly byte[] Ping = Map("PING");
    internal static readonly byte[] Echo = Map("ECHO");

    internal static readonly byte[] Save = Map("SAVE");
    internal static readonly byte[] BgSave = Map("BGSAVE");
    internal static readonly byte[] LastSave = Map("LASTSAVE");
    internal static readonly byte[] Shutdown = Map("SHUTDOWN");
    internal static readonly byte[] NoSave = Map("NOSAVE");
    internal static readonly byte[] BgRewriteAof = Map("BGREWRITEAOF");

    internal static readonly byte[] Info = Map("INFO");
    internal static readonly byte[] SlaveOf = Map("SLAVEOF");
    internal static readonly byte[] No = Map("NO");
    internal static readonly byte[] One = Map("ONE");
    internal static readonly byte[] ResetStat = Map("RESETSTAT");
    internal static readonly byte[] Rewrite = Map("REWRITE");
    internal static readonly byte[] Time = Map("TIME");
    internal static readonly byte[] Segfault = Map("SEGFAULT");
    internal static readonly byte[] Sleep = Map("SLEEP");
    internal static readonly byte[] Dump = Map("DUMP");
    internal static readonly byte[] Restore = Map("RESTORE");
    internal static readonly byte[] Migrate = Map("MIGRATE");
    internal static readonly byte[] Move = Map("MOVE");
    internal static readonly byte[] Object = Map("OBJECT");
    internal static readonly byte[] IdleTime = Map("IDLETIME");
    internal static readonly byte[] Monitor = Map("MONITOR"); //missing
    internal static readonly byte[] Debug = Map("DEBUG"); //missing
    internal static readonly byte[] Config = Map("CONFIG"); //missing
    internal static readonly byte[] Client = Map("CLIENT");
    internal static readonly byte[] List = Map("LIST");
    internal static readonly byte[] Kill = Map("KILL");
    internal static readonly byte[] Addr = Map("ADDR");
    internal static readonly byte[] Id = Map("ID");
    internal static readonly byte[] SkipMe = Map("SKIPME");
    internal static readonly byte[] SetName = Map("SETNAME");
    internal static readonly byte[] GetName = Map("GETNAME");
    internal static readonly byte[] Pause = Map("PAUSE");
    internal static readonly byte[] Role = Map("ROLE");

    internal static readonly byte[] StrLen = Map("STRLEN");
    internal static readonly byte[] Set = Map("SET");
    internal static readonly byte[] Get = Map("GET");
    internal static readonly byte[] GetSet = Map("GETSET");
    internal static readonly byte[] MGet = Map("MGET");
    internal static readonly byte[] SetNx = Map("SETNX");
    internal static readonly byte[] SetEx = Map("SETEX");
    internal static readonly byte[] Persist = Map("PERSIST");
    internal static readonly byte[] PSetEx = Map("PSETEX");
    internal static readonly byte[] MSet = Map("MSET");
    internal static readonly byte[] MSetNx = Map("MSETNX");
    internal static readonly byte[] Incr = Map("INCR");
    internal static readonly byte[] IncrBy = Map("INCRBY");
    internal static readonly byte[] IncrByFloat = Map("INCRBYFLOAT");
    internal static readonly byte[] Decr = Map("DECR");
    internal static readonly byte[] DecrBy = Map("DECRBY");
    internal static readonly byte[] Append = Map("APPEND");
    internal static readonly byte[] GetRange = Map("GETRANGE");
    internal static readonly byte[] SetRange = Map("SETRANGE");
    internal static readonly byte[] GetBit = Map("GETBIT");
    internal static readonly byte[] SetBit = Map("SETBIT");
    internal static readonly byte[] BitCount = Map("BITCOUNT");

    internal static readonly byte[] Scan = Map("SCAN");
    internal static readonly byte[] SScan = Map("SSCAN");
    internal static readonly byte[] HScan = Map("HSCAN");
    internal static readonly byte[] ZScan = Map("ZSCAN");
    internal static readonly byte[] Match = Map("MATCH");
    internal static readonly byte[] Count = Map("COUNT");

    internal static readonly byte[] HSet = Map("HSET");
    internal static readonly byte[] HSetNx = Map("HSETNX");
    internal static readonly byte[] HGet = Map("HGET");
    internal static readonly byte[] HMSet = Map("HMSET");
    internal static readonly byte[] HMGet = Map("HMGET");
    internal static readonly byte[] HIncrBy = Map("HINCRBY");
    internal static readonly byte[] HIncrByFloat = Map("HINCRBYFLOAT");
    internal static readonly byte[] HExists = Map("HEXISTS");
    internal static readonly byte[] HDel = Map("HDEL");
    internal static readonly byte[] HLen = Map("HLEN");
    internal static readonly byte[] HKeys = Map("HKEYS");
    internal static readonly byte[] HVals = Map("HVALS");
    internal static readonly byte[] HGetAll = Map("HGETALL");

    internal static readonly byte[] Sort = Map("SORT");

    internal static readonly byte[] Watch = Map("WATCH");
    internal static readonly byte[] UnWatch = Map("UNWATCH");
    internal static readonly byte[] Multi = Map("MULTI");
    internal static readonly byte[] Exec = Map("EXEC");
    internal static readonly byte[] Discard = Map("DISCARD");

    internal static readonly byte[] Subscribe = Map("SUBSCRIBE");
    internal static readonly byte[] UnSubscribe = Map("UNSUBSCRIBE");
    internal static readonly byte[] PSubscribe = Map("PSUBSCRIBE");
    internal static readonly byte[] PUnSubscribe = Map("PUNSUBSCRIBE");
    internal static readonly byte[] Publish = Map("PUBLISH");

    internal static readonly byte[] ClusterInfo = Map("CLUSTER INFO");

    internal static readonly byte[] ScriptFlush = Map("SCRIPT FLUSH");

    internal static readonly IEnumerable<byte[]> ScriptLoad =
        [
            Map("SCRIPT"), 
            Map("LOAD")
        ];
        
    internal static readonly byte[] Script = Map("SCRIPT");
    internal static readonly byte[] Load = Map("LOAD");

    internal static readonly byte[] Eval = Map("EVAL");
    internal static readonly byte[] EvalSha = Map("EVALSHA");

    private static byte[] Map(ReadOnlySpan<char> input)
    {
        return input.ToUtf8BytesWithTerminator();
    }
}