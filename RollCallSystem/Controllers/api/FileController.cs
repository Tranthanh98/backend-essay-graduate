using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;

namespace RollCallSystem.Controllers.api
{
    public class FileController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage GetFileDataById(int fileId)
        {
            return RCSService.GetFileDataById(fileId);
        }
        [HttpGet]
        public ApiResult<int> DeleteFile(int fileId)
        {
            return RCSService.DeleteFile(fileId);
        }
    }
}
