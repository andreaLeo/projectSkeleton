

namespace Domain.Infrastructure.Messaging.HTTP
{
    public class DownloadProgressReport
    {
        /// <summary>
        /// 
        /// </summary>
        public int PercentageComplete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long BytesReceived { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long TotalBytesToReceive { get; set; }

    }
}
