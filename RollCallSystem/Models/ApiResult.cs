using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models
{
    public class ApiResult<T>
    {
        public ApiResult()
        {
            Messages = new List<string>();
            IsSuccess = false;
        }

        public ApiResult(T data, List<string> messages, bool isSuccess)
        {
            Data = data;
            Messages = messages;
            IsSuccess = isSuccess;
        }

        public T Data { get; set; }
        public List<String> Messages { get; set; }
        public bool IsSuccess { get; set; }
    }
}