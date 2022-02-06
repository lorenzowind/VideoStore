using System.Collections.Generic;

namespace VideoStore.Core.Protocols
{
    public class ResponseErrorMessages
    {
        public List<string> Messages { get; set; }

        public ResponseErrorMessages()
        {
            Messages = new List<string>();
        }
    }

    public class ResponseResult
    {
        public string Title { get; set; }
        public int Status { get; set; }
        public ResponseErrorMessages Errors { get; set; }
        public ResponseResult()
        {
            Errors = new ResponseErrorMessages();
        }
    }
}
