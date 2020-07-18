using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RollCallSystem.Services
{
    public class RCSContext : EntitiesDB, IDisposable
    {
        public RCSContext():base()
        {
            Configuration.AutoDetectChangesEnabled = true;
            Configuration.EnsureTransactionsForFunctionsAndCommands = false;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;
#if DEBUG
            Database.Log = (s) => { Debug.WriteLine(s); };
#endif
        }
    }
}