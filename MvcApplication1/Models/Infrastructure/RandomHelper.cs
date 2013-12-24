using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Infrastructure
{
    public static class RandomHelper
    {
        public static string GetRandomString()
        {
            return Guid.NewGuid().ToString().Substring(0, 4);
        }
    }
}