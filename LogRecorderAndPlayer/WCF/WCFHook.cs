using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class WCFLoggerAndPlayer : BehaviorExtensionElement
    {
        protected override object CreateBehavior()
        {
            return new WCFEndPointBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(WCFEndPointBehavior);
            }
        }
    }


}
