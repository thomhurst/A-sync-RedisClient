using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<object> ExpectSuccess()
        {
            var response = await ReadLine();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response, LastCommand);
            }

            return new object();
        }

        private async ValueTask<object> ExpectSuccess(int count)
        {
            for (var i = 0; i < count; i++)
            {
                await ExpectSuccess();
            }
            
            return new object();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<string> ExpectData()
        {
            return (await ReadData()).AsString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<string> ExpectData(bool readToEnd = false)
        {
            return (await ReadData(readToEnd)).AsString();
        }
        
        private async ValueTask<IList<string>> ExpectData(int count)
        {
            var responses = new List<string>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(await ExpectData());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<string> ExpectWord()
        {
            var word = await ReadLine();

            if (!word.StartsWith("+"))
            {
                throw new UnexpectedRedisResponseException(word);
            }

            return word.Substring(1);
        }
        
        private async ValueTask<IList<string>> ExpectWord(int count)
        {
            var responses = new List<string>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(await ExpectWord());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<int> ExpectInteger()
        {
            var line = await ReadLine();

            if (!line.StartsWith(":") || !int.TryParse(line.Substring(1), out var number))
            {
                throw new UnexpectedRedisResponseException(line);
            }

            return number;
        }
        
        private async ValueTask<IList<int>> ExpectInteger(int count)
        {
            var responses = new List<int>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(await ExpectInteger());
            }
            
            return responses;
        }
        
        private async ValueTask<float> ExpectFloat()
        {
            var floatString = (await ReadData()).AsString();

            if (!float.TryParse(floatString, out var number))
            {
                throw new UnexpectedRedisResponseException(floatString);
            }

            return number;
        }
        
        private async ValueTask<IList<float>> ExpectFloat(float count)
        {
            var responses = new List<float>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(await ExpectFloat());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask<IEnumerable<StringRedisValue>> ExpectArray()
        {
            var arrayWithCountLine = await ReadLine();

            if (!arrayWithCountLine.StartsWith("*"))
            {
                throw new UnexpectedRedisResponseException(arrayWithCountLine);
            }

            if (!int.TryParse(arrayWithCountLine.Substring(1), out var count))
            {
                throw new UnexpectedRedisResponseException($"Error getting message count: {arrayWithCountLine}");
            }
            
            var results = new byte [count][];
            for (var i = 0; i < count; i++)
            {
                // Refresh the pipe buffer before 'ReadData' method reads it
                LastAction = "Reading Data Synchronously in ExpectArray";
                if (!_pipe.Input.TryRead(out _readResult))
                {
                    LastAction = "Reading Data Asynchronously in ExpectArray";
                    _readResult = await _pipe.Input.ReadAsync().ConfigureAwait(false);
                }
                
                results[i] = (await ReadData()).ToArray();
            }

            return results.ToRedisValues();
        }
        
        private async ValueTask<IList<IEnumerable<StringRedisValue>>> ExpectArray(int count)
        {
            var responses = new List<IEnumerable<StringRedisValue>>();
            
            for (var i = 0; i < count; i++)
            {
                responses.Add(await ExpectArray());
            }
            
            return responses;
        }
    }
}