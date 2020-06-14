using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class AcknowledgementResponse<T> : BaseResponse
    {
        public AcknowledgementResponse() { }
        public T Data { get; set; }
    }
}