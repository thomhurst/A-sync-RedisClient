using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient
    {
        internal GenericResultProcessor GenericResultProcessor => new GenericResultProcessor(); 
        internal EmptyResultProcessor EmptyResultProcessor => new EmptyResultProcessor();
        internal SuccessResultProcessor SuccessResultProcessor => new SuccessResultProcessor();
        internal DataResultProcessor DataResultProcessor => new DataResultProcessor();
        internal IntegerResultProcessor IntegerResultProcessor => new IntegerResultProcessor();
        internal FloatResultProcessor FloatResultProcessor => new FloatResultProcessor();
        internal ArrayResultProcessor ArrayResultProcessor => new ArrayResultProcessor();
        internal WordResultProcessor WordResultProcessor => new WordResultProcessor();

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