﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Cosmos.Debug.Common
{
    public static class NameValueCollectionHelper
    {
      /// <summary>Build a string from collection content. Each collection item is used to produce
      /// a 'key=value' string. Pairs are separated by a semi colon.</summary>
        public static string DumpToString(NameValueCollection value)
        {
            var xSB = new StringBuilder();
            foreach (string xKey in value.Keys)
            {
                xSB.AppendFormat("{0}={1};", xKey, (string)value[xKey]);
            }
            return xSB.ToString();
        }

        public static void LoadFromString(NameValueCollection target, string value)
        {
            if (target.Count > 0)
            {
                throw new Exception("Target is not empty!");
            }
            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            string[] xPairs = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var xPair in xPairs)
            {
                string[] xParts = xPair.Split('=');
                if (xParts.Length > 1)
                {
                    target.Add(xParts[0], xParts[1]);
                }
                else
                {
                    target.Add(xParts[0], "");
                }
            }
        }
    }
}