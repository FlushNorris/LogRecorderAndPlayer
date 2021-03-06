﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer.Common
{
    public static class ResourceHelper
    {
        public static string GetResourceContent(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();            

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
            return null;
        }
    }
}
