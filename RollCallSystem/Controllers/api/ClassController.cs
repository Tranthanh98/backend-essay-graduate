using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class ClassController : BaseApiController
    {
        public ApiResult<List<RollCall>> RollCall(RollCallModel model)
        {
            return RCSService.RollCall(model);
        }
    }
}
