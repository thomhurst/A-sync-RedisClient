// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using System;
using System.Text;

namespace TomLonghurst.AsyncRedisClient.Constants
{
    internal static class Commands
    {
        internal static readonly ReadOnlyMemory<byte> LineTerminator = Encoding.UTF8.GetBytes("\r\n");

        internal static readonly ReadOnlyMemory<byte> DollarSymbol = Encoding.UTF8.GetBytes("$");
        internal static readonly ReadOnlyMemory<byte> AsterixSymbol = Encoding.UTF8.GetBytes("*");
        internal static readonly ReadOnlyMemory<byte> ColonSymbol = Encoding.UTF8.GetBytes(":");
        internal static readonly ReadOnlyMemory<byte> PlusSymbol = Encoding.UTF8.GetBytes("+");
        internal static readonly ReadOnlyMemory<byte> MinusSymbol = Encoding.UTF8.GetBytes("-");

        internal static readonly ReadOnlyMemory<byte> LLen = Encoding.UTF8.GetBytes("LLEN");
        
        internal static readonly ReadOnlyMemory<byte> Number0 = Encoding.UTF8.GetBytes("0");
        internal static readonly ReadOnlyMemory<byte> Number1 = Encoding.UTF8.GetBytes("1");
        internal static readonly ReadOnlyMemory<byte> Number2 = Encoding.UTF8.GetBytes("2");
        internal static readonly ReadOnlyMemory<byte> Number3 = Encoding.UTF8.GetBytes("3");
        internal static readonly ReadOnlyMemory<byte> Number4 = Encoding.UTF8.GetBytes("4");
        internal static readonly ReadOnlyMemory<byte> Number5 = Encoding.UTF8.GetBytes("5");
        internal static readonly ReadOnlyMemory<byte> Number6 = Encoding.UTF8.GetBytes("6");
        internal static readonly ReadOnlyMemory<byte> Number7 = Encoding.UTF8.GetBytes("7");
        internal static readonly ReadOnlyMemory<byte> Number8 = Encoding.UTF8.GetBytes("8");
        internal static readonly ReadOnlyMemory<byte> Number9 = Encoding.UTF8.GetBytes("9");
        internal static readonly ReadOnlyMemory<byte> Number10 = Encoding.UTF8.GetBytes("10");
        
        internal static readonly ReadOnlyMemory<byte> Quit = Encoding.UTF8.GetBytes("QUIT");
        internal static readonly ReadOnlyMemory<byte> Auth = Encoding.UTF8.GetBytes("AUTH");
        internal static readonly ReadOnlyMemory<byte> Exists = Encoding.UTF8.GetBytes("EXISTS");
        internal static readonly ReadOnlyMemory<byte> Del = Encoding.UTF8.GetBytes("DEL");
        internal static readonly ReadOnlyMemory<byte> Type = Encoding.UTF8.GetBytes("TYPE");
        internal static readonly ReadOnlyMemory<byte> Keys = Encoding.UTF8.GetBytes("KEYS");
        internal static readonly ReadOnlyMemory<byte> RandomKey = Encoding.UTF8.GetBytes("RANDOMKEY");
        internal static readonly ReadOnlyMemory<byte> Rename = Encoding.UTF8.GetBytes("RENAME");
        internal static readonly ReadOnlyMemory<byte> RenameNx = Encoding.UTF8.GetBytes("RENAMENX");
        internal static readonly ReadOnlyMemory<byte> PExpire = Encoding.UTF8.GetBytes("PEXPIRE");
        internal static readonly ReadOnlyMemory<byte> PExpireAt = Encoding.UTF8.GetBytes("PEXPIREAT");
        internal static readonly ReadOnlyMemory<byte> DbSize = Encoding.UTF8.GetBytes("DBSIZE");
        internal static readonly ReadOnlyMemory<byte> Expire = Encoding.UTF8.GetBytes("EXPIRE");
        internal static readonly ReadOnlyMemory<byte> ExpireAt = Encoding.UTF8.GetBytes("EXPIREAT");
        internal static readonly ReadOnlyMemory<byte> Ttl = Encoding.UTF8.GetBytes("TTL");
        internal static readonly ReadOnlyMemory<byte> PTtl = Encoding.UTF8.GetBytes("PTTL");
        internal static readonly ReadOnlyMemory<byte> Select = Encoding.UTF8.GetBytes("SELECT");
        internal static readonly ReadOnlyMemory<byte> FlushDb = Encoding.UTF8.GetBytes("FLUSHDB");
        internal static readonly ReadOnlyMemory<byte> FlushAll = Encoding.UTF8.GetBytes("FLUSHALL");
        internal static readonly ReadOnlyMemory<byte> Ping = Encoding.UTF8.GetBytes("PING");
        internal static readonly ReadOnlyMemory<byte> Echo = Encoding.UTF8.GetBytes("ECHO");

        internal static readonly ReadOnlyMemory<byte> Save = Encoding.UTF8.GetBytes("SAVE");
        internal static readonly ReadOnlyMemory<byte> BgSave = Encoding.UTF8.GetBytes("BGSAVE");
        internal static readonly ReadOnlyMemory<byte> LastSave = Encoding.UTF8.GetBytes("LASTSAVE");
        internal static readonly ReadOnlyMemory<byte> Shutdown = Encoding.UTF8.GetBytes("SHUTDOWN");
        internal static readonly ReadOnlyMemory<byte> NoSave = Encoding.UTF8.GetBytes("NOSAVE");
        internal static readonly ReadOnlyMemory<byte> BgRewriteAof = Encoding.UTF8.GetBytes("BGREWRITEAOF");

        internal static readonly ReadOnlyMemory<byte> Info = Encoding.UTF8.GetBytes("INFO");
        internal static readonly ReadOnlyMemory<byte> SlaveOf = Encoding.UTF8.GetBytes("SLAVEOF");
        internal static readonly ReadOnlyMemory<byte> No = Encoding.UTF8.GetBytes("NO");
        internal static readonly ReadOnlyMemory<byte> One = Encoding.UTF8.GetBytes("ONE");
        internal static readonly ReadOnlyMemory<byte> ResetStat = Encoding.UTF8.GetBytes("RESETSTAT");
        internal static readonly ReadOnlyMemory<byte> Rewrite = Encoding.UTF8.GetBytes("REWRITE");
        internal static readonly ReadOnlyMemory<byte> Time = Encoding.UTF8.GetBytes("TIME");
        internal static readonly ReadOnlyMemory<byte> Segfault = Encoding.UTF8.GetBytes("SEGFAULT");
        internal static readonly ReadOnlyMemory<byte> Sleep = Encoding.UTF8.GetBytes("SLEEP");
        internal static readonly ReadOnlyMemory<byte> Dump = Encoding.UTF8.GetBytes("DUMP");
        internal static readonly ReadOnlyMemory<byte> Restore = Encoding.UTF8.GetBytes("RESTORE");
        internal static readonly ReadOnlyMemory<byte> Migrate = Encoding.UTF8.GetBytes("MIGRATE");
        internal static readonly ReadOnlyMemory<byte> Move = Encoding.UTF8.GetBytes("MOVE");
        internal static readonly ReadOnlyMemory<byte> Object = Encoding.UTF8.GetBytes("OBJECT");
        internal static readonly ReadOnlyMemory<byte> IdleTime = Encoding.UTF8.GetBytes("IDLETIME");
        internal static readonly ReadOnlyMemory<byte> Monitor = Encoding.UTF8.GetBytes("MONITOR"); //missing
        internal static readonly ReadOnlyMemory<byte> Debug = Encoding.UTF8.GetBytes("DEBUG"); //missing
        internal static readonly ReadOnlyMemory<byte> Config = Encoding.UTF8.GetBytes("CONFIG"); //missing
        internal static readonly ReadOnlyMemory<byte> Client = Encoding.UTF8.GetBytes("CLIENT");
        internal static readonly ReadOnlyMemory<byte> List = Encoding.UTF8.GetBytes("LIST");
        internal static readonly ReadOnlyMemory<byte> Kill = Encoding.UTF8.GetBytes("KILL");
        internal static readonly ReadOnlyMemory<byte> Addr = Encoding.UTF8.GetBytes("ADDR");
        internal static readonly ReadOnlyMemory<byte> Id = Encoding.UTF8.GetBytes("ID");
        internal static readonly ReadOnlyMemory<byte> SkipMe = Encoding.UTF8.GetBytes("SKIPME");
        internal static readonly ReadOnlyMemory<byte> SetName = Encoding.UTF8.GetBytes("SETNAME");
        internal static readonly ReadOnlyMemory<byte> GetName = Encoding.UTF8.GetBytes("GETNAME");
        internal static readonly ReadOnlyMemory<byte> Pause = Encoding.UTF8.GetBytes("PAUSE");
        internal static readonly ReadOnlyMemory<byte> Role = Encoding.UTF8.GetBytes("ROLE");

        internal static readonly ReadOnlyMemory<byte> StrLen = Encoding.UTF8.GetBytes("STRLEN");
        internal static readonly ReadOnlyMemory<byte> Set = Encoding.UTF8.GetBytes("SET");
        internal static readonly ReadOnlyMemory<byte> Get = Encoding.UTF8.GetBytes("GET");
        internal static readonly ReadOnlyMemory<byte> GetSet = Encoding.UTF8.GetBytes("GETSET");
        internal static readonly ReadOnlyMemory<byte> MGet = Encoding.UTF8.GetBytes("MGET");
        internal static readonly ReadOnlyMemory<byte> SetNx = Encoding.UTF8.GetBytes("SETNX");
        internal static readonly ReadOnlyMemory<byte> SetEx = Encoding.UTF8.GetBytes("SETEX");
        internal static readonly ReadOnlyMemory<byte> Persist = Encoding.UTF8.GetBytes("PERSIST");
        internal static readonly ReadOnlyMemory<byte> PSetEx = Encoding.UTF8.GetBytes("PSETEX");
        internal static readonly ReadOnlyMemory<byte> MSet = Encoding.UTF8.GetBytes("MSET");
        internal static readonly ReadOnlyMemory<byte> MSetNx = Encoding.UTF8.GetBytes("MSETNX");
        internal static readonly ReadOnlyMemory<byte> Incr = Encoding.UTF8.GetBytes("INCR");
        internal static readonly ReadOnlyMemory<byte> IncrBy = Encoding.UTF8.GetBytes("INCRBY");
        internal static readonly ReadOnlyMemory<byte> IncrByFloat = Encoding.UTF8.GetBytes("INCRBYFLOAT");
        internal static readonly ReadOnlyMemory<byte> Decr = Encoding.UTF8.GetBytes("DECR");
        internal static readonly ReadOnlyMemory<byte> DecrBy = Encoding.UTF8.GetBytes("DECRBY");
        internal static readonly ReadOnlyMemory<byte> Append = Encoding.UTF8.GetBytes("APPEND");
        internal static readonly ReadOnlyMemory<byte> GetRange = Encoding.UTF8.GetBytes("GETRANGE");
        internal static readonly ReadOnlyMemory<byte> SetRange = Encoding.UTF8.GetBytes("SETRANGE");
        internal static readonly ReadOnlyMemory<byte> GetBit = Encoding.UTF8.GetBytes("GETBIT");
        internal static readonly ReadOnlyMemory<byte> SetBit = Encoding.UTF8.GetBytes("SETBIT");
        internal static readonly ReadOnlyMemory<byte> BitCount = Encoding.UTF8.GetBytes("BITCOUNT");

        internal static readonly ReadOnlyMemory<byte> Scan = Encoding.UTF8.GetBytes("SCAN");
        internal static readonly ReadOnlyMemory<byte> SScan = Encoding.UTF8.GetBytes("SSCAN");
        internal static readonly ReadOnlyMemory<byte> HScan = Encoding.UTF8.GetBytes("HSCAN");
        internal static readonly ReadOnlyMemory<byte> ZScan = Encoding.UTF8.GetBytes("ZSCAN");
        internal static readonly ReadOnlyMemory<byte> Match = Encoding.UTF8.GetBytes("MATCH");
        internal static readonly ReadOnlyMemory<byte> Count = Encoding.UTF8.GetBytes("COUNT");

        internal static readonly ReadOnlyMemory<byte> HSet = Encoding.UTF8.GetBytes("HSET");
        internal static readonly ReadOnlyMemory<byte> HSetNx = Encoding.UTF8.GetBytes("HSETNX");
        internal static readonly ReadOnlyMemory<byte> HGet = Encoding.UTF8.GetBytes("HGET");
        internal static readonly ReadOnlyMemory<byte> HMSet = Encoding.UTF8.GetBytes("HMSET");
        internal static readonly ReadOnlyMemory<byte> HMGet = Encoding.UTF8.GetBytes("HMGET");
        internal static readonly ReadOnlyMemory<byte> HIncrBy = Encoding.UTF8.GetBytes("HINCRBY");
        internal static readonly ReadOnlyMemory<byte> HIncrByFloat = Encoding.UTF8.GetBytes("HINCRBYFLOAT");
        internal static readonly ReadOnlyMemory<byte> HExists = Encoding.UTF8.GetBytes("HEXISTS");
        internal static readonly ReadOnlyMemory<byte> HDel = Encoding.UTF8.GetBytes("HDEL");
        internal static readonly ReadOnlyMemory<byte> HLen = Encoding.UTF8.GetBytes("HLEN");
        internal static readonly ReadOnlyMemory<byte> HKeys = Encoding.UTF8.GetBytes("HKEYS");
        internal static readonly ReadOnlyMemory<byte> HVals = Encoding.UTF8.GetBytes("HVALS");
        internal static readonly ReadOnlyMemory<byte> HGetAll = Encoding.UTF8.GetBytes("HGETALL");

        internal static readonly ReadOnlyMemory<byte> Sort = Encoding.UTF8.GetBytes("SORT");

        internal static readonly ReadOnlyMemory<byte> Watch = Encoding.UTF8.GetBytes("WATCH");
        internal static readonly ReadOnlyMemory<byte> UnWatch = Encoding.UTF8.GetBytes("UNWATCH");
        internal static readonly ReadOnlyMemory<byte> Multi = Encoding.UTF8.GetBytes("MULTI");
        internal static readonly ReadOnlyMemory<byte> Exec = Encoding.UTF8.GetBytes("EXEC");
        internal static readonly ReadOnlyMemory<byte> Discard = Encoding.UTF8.GetBytes("DISCARD");

        internal static readonly ReadOnlyMemory<byte> Subscribe = Encoding.UTF8.GetBytes("SUBSCRIBE");
        internal static readonly ReadOnlyMemory<byte> UnSubscribe = Encoding.UTF8.GetBytes("UNSUBSCRIBE");
        internal static readonly ReadOnlyMemory<byte> PSubscribe = Encoding.UTF8.GetBytes("PSUBSCRIBE");
        internal static readonly ReadOnlyMemory<byte> PUnSubscribe = Encoding.UTF8.GetBytes("PUNSUBSCRIBE");
        internal static readonly ReadOnlyMemory<byte> Publish = Encoding.UTF8.GetBytes("PUBLISH");

        internal static readonly ReadOnlyMemory<byte> Cluster = Encoding.UTF8.GetBytes("CLUSTER");
        
        internal static readonly ReadOnlyMemory<byte> Script = Encoding.UTF8.GetBytes("SCRIPT");
        internal static readonly ReadOnlyMemory<byte> Load = Encoding.UTF8.GetBytes("LOAD");
        internal static readonly ReadOnlyMemory<byte> Flush = Encoding.UTF8.GetBytes("FLUSH");

        internal static readonly ReadOnlyMemory<byte> Eval = Encoding.UTF8.GetBytes("EVAL");
        internal static readonly ReadOnlyMemory<byte> EvalSha = Encoding.UTF8.GetBytes("EVALSHA");
        
    }
}
