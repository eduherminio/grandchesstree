using System.Collections.Concurrent;
using GrandChessTree.Shared;

namespace GrandChessTree.Client
{
    public class SubTaskHashTable
    {
        private readonly ConcurrentDictionary<ulong, Summary> _dict;
        private readonly ConcurrentQueue<ulong> _keysQueue;
        private readonly int _capacity;

        public SubTaskHashTable(int capacity)
        {
            _capacity = capacity;
            _dict = new ConcurrentDictionary<ulong, Summary>();
            _keysQueue = new ConcurrentQueue<ulong>();
        }

        public void Add(ulong key, Summary value)
        {
            if (_dict.TryAdd(key, value))
            {
                _keysQueue.Enqueue(key);

                if (_dict.Count > _capacity && _keysQueue.TryDequeue(out var oldestKey))
                {
                    _dict.TryRemove(oldestKey, out _);
                }
            }
        }

        public bool TryGetValue(ulong key, out Summary value) => _dict.TryGetValue(key, out value);
    }
 }
