using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient
    {
        internal GenericResultProcessor GenericResultProcessor => new(); 
        internal EmptyResultProcessor EmptyResultProcessor => new();
        internal SuccessResultProcessor SuccessResultProcessor => new();
        internal DataResultProcessor DataResultProcessor => new();
        internal IntegerResultProcessor IntegerResultProcessor => new();
        internal FloatResultProcessor FloatResultProcessor => new();
        internal ArrayResultProcessor ArrayResultProcessor => new();
        internal WordResultProcessor WordResultProcessor => new();

    }

    public enum ResponseType
    {
        Empty,
        Word,
        ComplexString,
        Integer,
        Float,
        Array
    }
}