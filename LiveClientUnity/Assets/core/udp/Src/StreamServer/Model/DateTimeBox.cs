using System;

namespace StreamServer.Model
{
    public class DateTimeBox
    {
        public readonly DateTime LastUpdated;

        public DateTimeBox(DateTime lastUpdated)
        {
            LastUpdated = lastUpdated;
        }
    }
}
