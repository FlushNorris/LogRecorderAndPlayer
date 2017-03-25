﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NamedPipesAssembly
{
    // Create a contract that can be used as a callback
    public interface IMyCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void NotifyClient();
    }

    // Define your service contract and specify the callback contract
    [ServiceContract(CallbackContract = typeof(IMyCallbackService))]
    public interface ISimpleService
    {
        [OperationContract(IsOneWay = false)]
        string ProcessData();
    }
}
