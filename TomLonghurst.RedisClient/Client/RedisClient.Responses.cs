using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TomLonghurst.RedisClient.Exceptions;
using TomLonghurst.RedisClient.Extensions;
using TomLonghurst.RedisClient.Models;

namespace TomLonghurst.RedisClient.Client
{
    public partial class RedisClient : IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ExpectSuccess()
        {
            var response = ReadLine();
            if (response.StartsWith("-"))
            {
                throw new RedisFailedCommandException(response, _lastCommand);
            }

            return new object();
        }

        private object ExpectSuccess(int count)
        {
            for (var i = 0; i < count; i++)
            {
                ExpectSuccess();
            }
            
            return new object();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ExpectData()
        {
            return ReadData().FromUtf8();
        }
        
        private IList<string> ExpectData(int count)
        {
            var responses = new List<string>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(ExpectData());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ExpectWord()
        {
            var word = ReadLine();

            if (!word.StartsWith("+"))
            {
                throw new UnexpectedRedisResponseException(word);
            }

            return word.Substring(1);
        }
        
        private IList<string> ExpectWord(int count)
        {
            var responses = new List<string>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(ExpectWord());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ExpectInteger()
        {
            var line = ReadLine();

            if (!line.StartsWith(":") || !int.TryParse(line.Substring(1), out var number))
            {
                throw new UnexpectedRedisResponseException(line);
            }

            return number;
        }
        
        private IList<int> ExpectInteger(int count)
        {
            var responses = new List<int>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(ExpectInteger());
            }
            
            return responses;
        }
        
        private float ExpectFloat()
        {
            var floatString = ReadData().FromUtf8();

            if (!float.TryParse(floatString, out var number))
            {
                throw new UnexpectedRedisResponseException(floatString);
            }

            return number;
        }
        
        private IList<float> ExpectFloat(float count)
        {
            var responses = new List<float>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(ExpectFloat());
            }
            
            return responses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<StringRedisValue> ExpectArray()
        {
            var arrayWithCountLine = ReadLine();

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
                results[i] = ReadData();
            }

            return results.ToRedisValues();
        }
        
        private IList<IEnumerable<StringRedisValue>> ExpectArray(int count)
        {
            var responses = new List<IEnumerable<StringRedisValue>>();
            for (var i = 0; i < count; i++)
            {
                responses.Add(ExpectArray());
            }
            
            return responses;
        }
    }
}