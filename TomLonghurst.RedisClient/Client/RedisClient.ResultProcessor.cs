using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient
    {

        public SuccessResultProcessor SuccessResultProcessor => new SuccessResultProcessor();
        public DataResultProcessor DataResultProcessor => new DataResultProcessor();
        public IntegerResultProcessor IntegerResultProcessor => new IntegerResultProcessor();
        public FloatResultProcessor FloatResultProcessor => new FloatResultProcessor();
        public ArrayResultProcessor ArrayResultProcessor => new ArrayResultProcessor();
        public WordResultProcessor WordResultProcessor => new WordResultProcessor();

    }
}