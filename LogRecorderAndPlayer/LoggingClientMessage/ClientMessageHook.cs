using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class ClientMessageLoggerAndPlayer : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new ClientMessageEndPointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ClientMessageEndPointBehavior);
            }
        }
    }


}
