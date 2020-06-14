using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RollCallSystem.Models.ModelResponse
{
    public class BaseResponse
    {
        public Boolean isSuccess { get; set; }
        public List<string> errorMessage = new List<string>();
        public List<string> successMessage = new List<string>();
        public void AddErrorMessage( string message)
        {
            errorMessage.Add(message);
        }
        public void AddSuccessMessage(string massage)
        {
            successMessage.Add(massage);
        }
    }
}