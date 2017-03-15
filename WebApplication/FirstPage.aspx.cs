using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebApplication
{
    public partial class FirstPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ViewState["Page_PreRender11:25:51:394"] = "Page_PreRender11:25:51:394";
            }

            if (true) //Page.IsPostBack)
            {
                var sb = new StringBuilder();
                foreach (string key in Session.Keys)
                {
                    if (true) //key.IndexOf("HttpModuleTest_") == 0)
                    {
                        sb.AppendLine(key + " = " + Session[key]);
                    }
                }
                var f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_SessionCheck.txt");
                f.Write(sb.ToString());
                f.Close();

                sb = new StringBuilder();
                foreach (string key in ViewState.Keys)
                {
                    if (true) //key.IndexOf("HttpModuleTest_") == 0)
                    {
                        sb.AppendLine(key + " = " + ViewState[key]);
                    }
                }
                f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_ViewStateCheck.txt");
                f.Write(sb.ToString());
                f.Close();
            }
            Session["HttpModuleTest"] = DateTime.Now.ToString("HH:mm:ss:fff");
            ViewState["HttpModuleTest"] = DateTime.Now.ToString("HH:mm:ss:fff");

            var pageType = this.GetType();
            //var dynMethod = this.GetType().GetMethod("SaveViewState", BindingFlags.NonPublic | BindingFlags.Instance);
            //var xxx = pageType.GetMethod("SaveViewState", BindingFlags.InvokeMethod | BindingFlags.NonPublic);
            //pageType.InvokeMember("SaveViewState", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, this, null);
            ////this.SaveViewState()   


            this.SaveStateComplete += FirstPage_SaveStateComplete;
            this.SaveStateComplete += FirstPage_SaveStateComplete1;
            var eventInfo = pageType.GetEvent("SaveStateComplete");
            var methodInfo = pageType.GetMethod("FirstPage_SaveStateCompleteX"); //, BindingFlags.Instance | BindingFlags.NonPublic);            
            //var obj = new FirstPage();
            Delegate d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            //eventInfo.AddEventHandler(this, methodInfo.CreateDelegate(eventInfo.EventHandlerType));

            //var test1 = GetSubscribedMethods(this, eventInfo).Any(x => x.Equals(methodInfo));
            eventInfo.RemoveEventHandler(this, d);
            eventInfo.AddEventHandler(this, d);
            eventInfo.RemoveEventHandler(this, d);
            eventInfo.AddEventHandler(this, d);
            eventInfo.RemoveEventHandler(this, d);
            eventInfo.AddEventHandler(this, d);

            //var eventInfo2 = pageType.GetEvent("SaveStateComplete");            
            //Delegate d2 = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            //var lst = new List<FieldInfo>();
            //foreach(EventInfo ei in pageType.GetEvents(AllBindings))
            //{
            //    Type dt = ei.DeclaringType;
            //    FieldInfo fi = dt.GetField(ei.Name, AllBindings);
            //    if (fi != null)
            //        lst.Add(fi);
            //}


            //var xxx = GetEventSubscribers(this, "SaveStateComplete");


            //var test2 = GetSubscribedMethods(this, eventInfo).Any(x => x.Equals(methodInfo));

            //public event EventHandler InitComplete;
            //public event EventHandler LoadComplete;
            //public event EventHandler PreInit;
            //public event EventHandler PreLoad;
            //public event EventHandler PreRenderComplete;
            //public event EventHandler SaveStateComplete;
            //public event EventHandler DataBinding;
            //public event EventHandler Disposed;
            //public event EventHandler Init;
            //public event EventHandler Load;
            //public event EventHandler PreRender;
            //public event EventHandler Unload;
            //public event EventHandler AbortTransaction;
            //public event EventHandler CommitTransaction;
            //public event EventHandler Error;



            //this.InitComplete += FirstPage_InitComplete;
            //this.InitComplete += FirstPage_InitComplete1;
            //FirstPage_SaveStateComplete1(null, null);
        }

        //static BindingFlags AllBindings
        //{
        //    get { return BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static; }
        //}

        //public static Delegate[] GetEventSubscribers(object target, string eventName)
        //{
        //    Type t = target.GetType();
        //    var eventInfo = t.GetEvent(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        //    do
        //    {
        //        FieldInfo[] fia = t.GetFields(
        //             BindingFlags.Static |
        //             BindingFlags.Instance |
        //             BindingFlags.NonPublic);

        //        foreach (FieldInfo fi in fia)
        //        {
        //            if (fi.Name == eventName)
        //            {
        //                Delegate d = fi.GetValue(target) as Delegate;
        //                if (d != null)
        //                    return d.GetInvocationList();
        //            }
        //            else if (fi.FieldType == typeof(EventHandlerList))
        //            {
        //                dynamic obj = fi.GetValue(target) as EventHandlerList;
        //                var eventHandlerFieldInfo = obj.GetType().GetField("head", BindingFlags.Instance | BindingFlags.NonPublic);
        //                do
        //                {
        //                    var listEntry = eventHandlerFieldInfo.GetValue(obj);
        //                    var handler = listEntry.GetType().GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
        //                    if (handler != null)
        //                    {
        //                        var subD = handler.GetValue(listEntry) as Delegate;
        //                        if (subD.GetType() != eventInfo.EventHandlerType)
        //                        {
        //                            eventHandlerFieldInfo = listEntry.GetType().GetField("next", BindingFlags.Instance | BindingFlags.NonPublic);
        //                            obj = listEntry;
        //                            continue;
        //                        }
        //                        if (subD != null)
        //                        {
        //                            return subD.GetInvocationList();
        //                        }
        //                    }
        //                }
        //                while (eventHandlerFieldInfo != null);
        //            }
        //        }

        //        t = t.BaseType;
        //    } while (t != null);

        //    return new Delegate[] { };
        //}

        //public IEnumerable<MethodInfo> GetSubscribedMethods(object obj, EventInfo eventInfo)
        //{
        //    Func<EventInfo, FieldInfo> ei2fi =
        //        ei => obj.GetType().Gete .GetField(ei.Name,
        //            BindingFlags.NonPublic |
        //            BindingFlags.Instance |
        //            BindingFlags.GetField);

        //    var eventFieldInfo = ei2fi(eventInfo);
        //    var eventFieldValue = (System.Delegate) eventFieldInfo?.GetValue(obj);

        //    return from subscribedDelegate in eventFieldValue?.GetInvocationList() ?? new Delegate[0]
        //           select subscribedDelegate.Method;
        //}

        public void FirstPage_SaveStateCompleteX(object sender, EventArgs e)
        {
            //this.ViewState
        }

        public void FirstPage_SaveStateComplete1(object sender, EventArgs e)
        {
        }

        private void FirstPage_SaveStateComplete(object sender, EventArgs e)
        {
        }

        private void FirstPage_InitComplete1(object sender, EventArgs e)
        {
        }

        private void FirstPage_InitComplete(object sender, EventArgs e)
        {
            
        }

        //protected override object SaveViewState()
        //{
        //    return base.SaveViewState();
        //}

        protected void btnButton_OnClick(object sender, EventArgs e)
        {
            var f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_btnButton_OnClick.txt");
            //f.Write(sb.ToString());
            f.Close();
            Session["SomeSessionKey"] = DateTime.Now.ToString(); //TODO LRAP-DateTime.Now should be used here...
        }
    }
}