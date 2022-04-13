// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Text;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    internal static class Commands
    {
        internal static ReadOnlyMemory<byte> LineTerminator = Encoding.UTF8.GetBytes("\r\n");

        internal static ReadOnlyMemory<byte> DollarSymbol = Encoding.UTF8.GetBytes("$");
        internal static ReadOnlyMemory<byte> AsterixSymbol = Encoding.UTF8.GetBytes("*");
        internal static ReadOnlyMemory<byte> ColonSymbol = Encoding.UTF8.GetBytes(":");
        internal static ReadOnlyMemory<byte> PlusSymbol = Encoding.UTF8.GetBytes("+");
        internal static ReadOnlyMemory<byte> MinusSymbol = Encoding.UTF8.GetBytes("-");

        internal static ReadOnlyMemory<byte> LLen = Encoding.UTF8.GetBytes("LLEN");
        
        internal static ReadOnlyMemory<byte> Number0 = Encoding.UTF8.GetBytes("0");
        internal static ReadOnlyMemory<byte> Number1 = Encoding.UTF8.GetBytes("1");
        internal static ReadOnlyMemory<byte> Number2 = Encoding.UTF8.GetBytes("2");
        internal static ReadOnlyMemory<byte> Number3 = Encoding.UTF8.GetBytes("3");
        internal static ReadOnlyMemory<byte> Number4 = Encoding.UTF8.GetBytes("4");
        internal static ReadOnlyMemory<byte> Number5 = Encoding.UTF8.GetBytes("5");
        internal static ReadOnlyMemory<byte> Number6 = Encoding.UTF8.GetBytes("6");
        internal static ReadOnlyMemory<byte> Number7 = Encoding.UTF8.GetBytes("7");
        internal static ReadOnlyMemory<byte> Number8 = Encoding.UTF8.GetBytes("8");
        internal static ReadOnlyMemory<byte> Number9 = Encoding.UTF8.GetBytes("9");
        internal static ReadOnlyMemory<byte> Number10 = Encoding.UTF8.GetBytes("10");
        
        internal static ReadOnlyMemory<byte> Quit = Encoding.UTF8.GetBytes("QUIT");
        internal static ReadOnlyMemory<byte> Auth = Encoding.UTF8.GetBytes("AUTH");
        internal static ReadOnlyMemory<byte> Exists = Encoding.UTF8.GetBytes("EXISTS");
        internal static ReadOnlyMemory<byte> Del = Encoding.UTF8.GetBytes("DEL");
        internal static ReadOnlyMemory<byte> Type = Encoding.UTF8.GetBytes("TYPE");
        internal static ReadOnlyMemory<byte> Keys = Encoding.UTF8.GetBytes("KEYS");
        internal static ReadOnlyMemory<byte> RandomKey = Encoding.UTF8.GetBytes("RANDOMKEY");
        internal static ReadOnlyMemory<byte> Rename = Encoding.UTF8.GetBytes("RENAME");
        internal static ReadOnlyMemory<byte> RenameNx = Encoding.UTF8.GetBytes("RENAMENX");
        internal static ReadOnlyMemory<byte> PExpire = Encoding.UTF8.GetBytes("PEXPIRE");
        internal static ReadOnlyMemory<byte> PExpireAt = Encoding.UTF8.GetBytes("PEXPIREAT");
        internal static ReadOnlyMemory<byte> DbSize = Encoding.UTF8.GetBytes("DBSIZE");
        internal static ReadOnlyMemory<byte> Expire = Encoding.UTF8.GetBytes("EXPIRE");
        internal static ReadOnlyMemory<byte> ExpireAt = Encoding.UTF8.GetBytes("EXPIREAT");
        internal static ReadOnlyMemory<byte> Ttl = Encoding.UTF8.GetBytes("TTL");
        internal static ReadOnlyMemory<byte> PTtl = Encoding.UTF8.GetBytes("PTTL");
        internal static ReadOnlyMemory<byte> Select = Encoding.UTF8.GetBytes("SELECT");
        internal static ReadOnlyMemory<byte> FlushDb = Encoding.UTF8.GetBytes("FLUSHDB");
        internal static ReadOnlyMemory<byte> FlushAll = Encoding.UTF8.GetBytes("FLUSHALL");
        internal static ReadOnlyMemory<byte> Ping = Encoding.UTF8.GetBytes("PING");
        internal static ReadOnlyMemory<byte> Echo = Encoding.UTF8.GetBytes("ECHO");

        internal static ReadOnlyMemory<byte> Save = Encoding.UTF8.GetBytes("SAVE");
        internal static ReadOnlyMemory<byte> BgSave = Encoding.UTF8.GetBytes("BGSAVE");
        internal static ReadOnlyMemory<byte> LastSave = Encoding.UTF8.GetBytes("LASTSAVE");
        internal static ReadOnlyMemory<byte> Shutdown = Encoding.UTF8.GetBytes("SHUTDOWN");
        internal static ReadOnlyMemory<byte> NoSave = Encoding.UTF8.GetBytes("NOSAVE");
        internal static ReadOnlyMemory<byte> BgRewriteAof = Encoding.UTF8.GetBytes("BGREWRITEAOF");

        internal static ReadOnlyMemory<byte> Info = Encoding.UTF8.GetBytes("INFO");
        internal static ReadOnlyMemory<byte> SlaveOf = Encoding.UTF8.GetBytes("SLAVEOF");
        internal static ReadOnlyMemory<byte> No = Encoding.UTF8.GetBytes("NO");
        internal static ReadOnlyMemory<byte> One = Encoding.UTF8.GetBytes("ONE");
        internal static ReadOnlyMemory<byte> ResetStat = Encoding.UTF8.GetBytes("RESETSTAT");
        internal static ReadOnlyMemory<byte> Rewrite = Encoding.UTF8.GetBytes("REWRITE");
        internal static ReadOnlyMemory<byte> Time = Encoding.UTF8.GetBytes("TIME");
        internal static ReadOnlyMemory<byte> Segfault = Encoding.UTF8.GetBytes("SEGFAULT");
        internal static ReadOnlyMemory<byte> Sleep = Encoding.UTF8.GetBytes("SLEEP");
        internal static ReadOnlyMemory<byte> Dump = Encoding.UTF8.GetBytes("DUMP");
        internal static ReadOnlyMemory<byte> Restore = Encoding.UTF8.GetBytes("RESTORE");
        internal static ReadOnlyMemory<byte> Migrate = Encoding.UTF8.GetBytes("MIGRATE");
        internal static ReadOnlyMemory<byte> Move = Encoding.UTF8.GetBytes("MOVE");
        internal static ReadOnlyMemory<byte> Object = Encoding.UTF8.GetBytes("OBJECT");
        internal static ReadOnlyMemory<byte> IdleTime = Encoding.UTF8.GetBytes("IDLETIME");
        internal static ReadOnlyMemory<byte> Monitor = Encoding.UTF8.GetBytes("MONITOR"); //missing
        internal static ReadOnlyMemory<byte> Debug = Encoding.UTF8.GetBytes("DEBUG"); //missing
        internal static ReadOnlyMemory<byte> Config = Encoding.UTF8.GetBytes("CONFIG"); //missing
        internal static ReadOnlyMemory<byte> Client = Encoding.UTF8.GetBytes("CLIENT");
        internal static ReadOnlyMemory<byte> List = Encoding.UTF8.GetBytes("LIST");
        internal static ReadOnlyMemory<byte> Kill = Encoding.UTF8.GetBytes("KILL");
        internal static ReadOnlyMemory<byte> Addr = Encoding.UTF8.GetBytes("ADDR");
        internal static ReadOnlyMemory<byte> Id = Encoding.UTF8.GetBytes("ID");
        internal static ReadOnlyMemory<byte> SkipMe = Encoding.UTF8.GetBytes("SKIPME");
        internal static ReadOnlyMemory<byte> SetName = Encoding.UTF8.GetBytes("SETNAME");
        internal static ReadOnlyMemory<byte> GetName = Encoding.UTF8.GetBytes("GETNAME");
        internal static ReadOnlyMemory<byte> Pause = Encoding.UTF8.GetBytes("PAUSE");
        internal static ReadOnlyMemory<byte> Role = Encoding.UTF8.GetBytes("ROLE");

        internal static ReadOnlyMemory<byte> StrLen = Encoding.UTF8.GetBytes("STRLEN");
        internal static ReadOnlyMemory<byte> Set = Encoding.UTF8.GetBytes("SET");
        internal static ReadOnlyMemory<byte> Get = Encoding.UTF8.GetBytes("GET");
        internal static ReadOnlyMemory<byte> GetSet = Encoding.UTF8.GetBytes("GETSET");
        internal static ReadOnlyMemory<byte> MGet = Encoding.UTF8.GetBytes("MGET");
        internal static ReadOnlyMemory<byte> SetNx = Encoding.UTF8.GetBytes("SETNX");
        internal static ReadOnlyMemory<byte> SetEx = Encoding.UTF8.GetBytes("SETEX");
        internal static ReadOnlyMemory<byte> Persist = Encoding.UTF8.GetBytes("PERSIST");
        internal static ReadOnlyMemory<byte> PSetEx = Encoding.UTF8.GetBytes("PSETEX");
        internal static ReadOnlyMemory<byte> MSet = Encoding.UTF8.GetBytes("MSET");
        internal static ReadOnlyMemory<byte> MSetNx = Encoding.UTF8.GetBytes("MSETNX");
        internal static ReadOnlyMemory<byte> Incr = Encoding.UTF8.GetBytes("INCR");
        internal static ReadOnlyMemory<byte> IncrBy = Encoding.UTF8.GetBytes("INCRBY");
        internal static ReadOnlyMemory<byte> IncrByFloat = Encoding.UTF8.GetBytes("INCRBYFLOAT");
        internal static ReadOnlyMemory<byte> Decr = Encoding.UTF8.GetBytes("DECR");
        internal static ReadOnlyMemory<byte> DecrBy = Encoding.UTF8.GetBytes("DECRBY");
        internal static ReadOnlyMemory<byte> Append = Encoding.UTF8.GetBytes("APPEND");
        internal static ReadOnlyMemory<byte> GetRange = Encoding.UTF8.GetBytes("GETRANGE");
        internal static ReadOnlyMemory<byte> SetRange = Encoding.UTF8.GetBytes("SETRANGE");
        internal static ReadOnlyMemory<byte> GetBit = Encoding.UTF8.GetBytes("GETBIT");
        internal static ReadOnlyMemory<byte> SetBit = Encoding.UTF8.GetBytes("SETBIT");
        internal static ReadOnlyMemory<byte> BitCount = Encoding.UTF8.GetBytes("BITCOUNT");

        internal static ReadOnlyMemory<byte> Scan = Encoding.UTF8.GetBytes("SCAN");
        internal static ReadOnlyMemory<byte> SScan = Encoding.UTF8.GetBytes("SSCAN");
        internal static ReadOnlyMemory<byte> HScan = Encoding.UTF8.GetBytes("HSCAN");
        internal static ReadOnlyMemory<byte> ZScan = Encoding.UTF8.GetBytes("ZSCAN");
        internal static ReadOnlyMemory<byte> Match = Encoding.UTF8.GetBytes("MATCH");
        internal static ReadOnlyMemory<byte> Count = Encoding.UTF8.GetBytes("COUNT");

        internal static ReadOnlyMemory<byte> HSet = Encoding.UTF8.GetBytes("HSET");
        internal static ReadOnlyMemory<byte> HSetNx = Encoding.UTF8.GetBytes("HSETNX");
        internal static ReadOnlyMemory<byte> HGet = Encoding.UTF8.GetBytes("HGET");
        internal static ReadOnlyMemory<byte> HMSet = Encoding.UTF8.GetBytes("HMSET");
        internal static ReadOnlyMemory<byte> HMGet = Encoding.UTF8.GetBytes("HMGET");
        internal static ReadOnlyMemory<byte> HIncrBy = Encoding.UTF8.GetBytes("HINCRBY");
        internal static ReadOnlyMemory<byte> HIncrByFloat = Encoding.UTF8.GetBytes("HINCRBYFLOAT");
        internal static ReadOnlyMemory<byte> HExists = Encoding.UTF8.GetBytes("HEXISTS");
        internal static ReadOnlyMemory<byte> HDel = Encoding.UTF8.GetBytes("HDEL");
        internal static ReadOnlyMemory<byte> HLen = Encoding.UTF8.GetBytes("HLEN");
        internal static ReadOnlyMemory<byte> HKeys = Encoding.UTF8.GetBytes("HKEYS");
        internal static ReadOnlyMemory<byte> HVals = Encoding.UTF8.GetBytes("HVALS");
        internal static ReadOnlyMemory<byte> HGetAll = Encoding.UTF8.GetBytes("HGETALL");

        internal static ReadOnlyMemory<byte> Sort = Encoding.UTF8.GetBytes("SORT");

        internal static ReadOnlyMemory<byte> Watch = Encoding.UTF8.GetBytes("WATCH");
        internal static ReadOnlyMemory<byte> UnWatch = Encoding.UTF8.GetBytes("UNWATCH");
        internal static ReadOnlyMemory<byte> Multi = Encoding.UTF8.GetBytes("MULTI");
        internal static ReadOnlyMemory<byte> Exec = Encoding.UTF8.GetBytes("EXEC");
        internal static ReadOnlyMemory<byte> Discard = Encoding.UTF8.GetBytes("DISCARD");

        internal static ReadOnlyMemory<byte> Subscribe = Encoding.UTF8.GetBytes("SUBSCRIBE");
        internal static ReadOnlyMemory<byte> UnSubscribe = Encoding.UTF8.GetBytes("UNSUBSCRIBE");
        internal static ReadOnlyMemory<byte> PSubscribe = Encoding.UTF8.GetBytes("PSUBSCRIBE");
        internal static ReadOnlyMemory<byte> PUnSubscribe = Encoding.UTF8.GetBytes("PUNSUBSCRIBE");
        internal static ReadOnlyMemory<byte> Publish = Encoding.UTF8.GetBytes("PUBLISH");

        internal static ReadOnlyMemory<byte> Cluster = Encoding.UTF8.GetBytes("CLUSTER");
        
        internal static ReadOnlyMemory<byte> Script = Encoding.UTF8.GetBytes("SCRIPT");
        internal static ReadOnlyMemory<byte> Load = Encoding.UTF8.GetBytes("LOAD");
        internal static ReadOnlyMemory<byte> Flush = Encoding.UTF8.GetBytes("FLUSH");

        internal static ReadOnlyMemory<byte> Eval = Encoding.UTF8.GetBytes("EVAL");
        internal static ReadOnlyMemory<byte> EvalSha = Encoding.UTF8.GetBytes("EVALSHA");
        
    }
}
