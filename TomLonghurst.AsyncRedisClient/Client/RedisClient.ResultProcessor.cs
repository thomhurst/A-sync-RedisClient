using TomLonghurst.AsyncRedisClient.Models;
using TomLonghurst.AsyncRedisClient.Models.ResultProcessors;

namespace TomLonghurst.AsyncRedisClient.Client
{
    public partial class RedisClient
    {
        internal GenericAbstractAbstractResultProcessor GenericAbstractAbstractResultProcessor => new GenericAbstractAbstractResultProcessor(); 
        internal EmptyAbstractAbstractResultProcessor EmptyAbstractAbstractResultProcessor => new EmptyAbstractAbstractResultProcessor();
        internal SuccessAbstractAbstractResultProcessor SuccessAbstractAbstractResultProcessor => new SuccessAbstractAbstractResultProcessor();
        internal DataAbstractAbstractResultProcessor DataAbstractAbstractResultProcessor => new DataAbstractAbstractResultProcessor();
        internal IntegerAbstractAbstractResultProcessor IntegerAbstractAbstractResultProcessor => new IntegerAbstractAbstractResultProcessor();
        internal FloatAbstractAbstractResultProcessor FloatAbstractAbstractResultProcessor => new FloatAbstractAbstractResultProcessor();
        internal ArrayAbstractAbstractResultProcessor ArrayAbstractAbstractResultProcessor => new ArrayAbstractAbstractResultProcessor();
        internal WordAbstractAbstractResultProcessor WordAbstractAbstractResultProcessor => new WordAbstractAbstractResultProcessor();

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