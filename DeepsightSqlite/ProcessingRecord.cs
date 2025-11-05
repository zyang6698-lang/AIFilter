using System;

namespace DeepsightSqlite
{
    public class ProcessingRecord
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string BatchInfo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Quantity { get; set; }
    }
}
