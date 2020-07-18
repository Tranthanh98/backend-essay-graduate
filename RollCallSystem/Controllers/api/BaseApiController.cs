using RollCallSystem.Models;
using RollCallSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RollCallSystem.Controllers.api
{
    public class BaseApiController : ApiController
    {
        protected RCSService RCSService;
        public BaseApiController()
        {
            RCSService = new RCSService();
        }
    }
}
