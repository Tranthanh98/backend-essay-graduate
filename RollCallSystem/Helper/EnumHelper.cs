using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace RollCallSystem.Helper
{
    public static class EnumHelper
    {
        private static DisplayAttribute GetDisplayAttribute(Enum value)
        {
            return GetAttribute<DisplayAttribute>(value);
        }
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var attribute = value.GetType()
                .GetRuntimeField(value.ToString())
                .GetCustomAttributes(typeof(T), false)
                .SingleOrDefault() as T;
            return attribute;
        }
    }
}