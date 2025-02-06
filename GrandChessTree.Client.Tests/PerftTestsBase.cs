using GrandChessTree.Shared;

namespace GrandChessTree.Client.Tests
{
    public unsafe class PerftTestsBase
    {
        public static bool IsInitialized = false;

        public PerftTestsBase()
        {
            if (!IsInitialized)
            {
                IsInitialized = true; 
                Perft.HashTable = Perft.AllocateHashTable();
            }
        }
    }
}