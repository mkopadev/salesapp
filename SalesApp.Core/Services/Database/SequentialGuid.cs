using System;
using SalesApp.Core.Framework;

namespace SalesApp.Core.Services.Database
{
    [Preserve(AllMembers = true)]
    public class SequentialGuid
    {
        Guid _currentGuid;
        public Guid CurrentGuid
        {
            get
            {
                return _currentGuid;
            }
        }

        private SequentialGuid()
        {
            _currentGuid = Guid.NewGuid();
        }

        private SequentialGuid(Guid previousGuid)
        {
            _currentGuid = previousGuid;
        }

        public static SequentialGuid GuidGen = new SequentialGuid();

        public static SequentialGuid operator ++(SequentialGuid sequentialGuid)
        {
            byte[] bytes = sequentialGuid._currentGuid.ToByteArray();
            for (int mapIndex = 0; mapIndex < 16; mapIndex++)
            {
                int bytesIndex = SqlOrderMap[mapIndex];
                bytes[bytesIndex]++;
                if (bytes[bytesIndex] != 0)
                {
                    break; // No need to increment more significant bytes
                }
            }
            sequentialGuid._currentGuid = new Guid(bytes);
            return sequentialGuid;
        }

        private static int[] _SqlOrderMap = null;
        private static int[] SqlOrderMap
        {
            get
            {
                if (_SqlOrderMap == null)
                {
                    _SqlOrderMap = new int[16] {
                    3, 2, 1, 0, 5, 4, 7, 6, 9, 8, 15, 14, 13, 12, 11, 10
                };
                    // 3 - the least significant byte in Guid ByteArray [for SQL Server ORDER BY clause]
                    // 10 - the most significant byte in Guid ByteArray [for SQL Server ORDERY BY clause]
                }
                return _SqlOrderMap;
            }
        }
    }
}
