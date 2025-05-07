using ColinApp.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColinApp.Common.Extensions
{
    public class SnowflakeIdGenerator
    {
        private const long Twepoch = 1288834974657L; // 起始时间戳（Twitter原始值，可自定义）
        private const int WorkerIdBits = 5;
        private const int DatacenterIdBits = 5;
        private const int SequenceBits = 12;

        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private const long SequenceMask = -1L ^ (-1L << SequenceBits);

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        public long WorkerId { get; private set; }
        public long DatacenterId { get; private set; }

        private readonly object _lock = new object();

        public SnowflakeIdGenerator(long workerId, long datacenterId)
        {
            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"workerId must be between 0 and {MaxWorkerId}");
            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"datacenterId must be between 0 and {MaxDatacenterId}");

            WorkerId = workerId;
            DatacenterId = datacenterId;
        }

        public long NextId()
        {
            lock (_lock)
            {
                var timestamp = CurrentTimeMillis();

                if (timestamp < _lastTimestamp)
                    throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp}ms");

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                        timestamp = WaitUntilNextMillis(_lastTimestamp);
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << TimestampLeftShift)
                       | (DatacenterId << DatacenterIdShift)
                       | (WorkerId << WorkerIdShift)
                       | _sequence;
            }
        }

        private long WaitUntilNextMillis(long lastTimestamp)
        {
            var timestamp = CurrentTimeMillis();
            while (timestamp <= lastTimestamp)
            {
                Thread.Sleep(0); // 避免CPU空转
                timestamp = CurrentTimeMillis();
            }
            return timestamp;
        }

        private long CurrentTimeMillis() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}


//var snowflake = new SnowflakeIdGenerator(workerId: 1, datacenterId: 1);
//long id = snowflake.NextId();
//Console.WriteLine($"生成的ID: {id}");

