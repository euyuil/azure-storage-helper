using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Euyuil.Azure.Storage.Helper.Tests
{
    public static class Expect
    {
        public static void Exception<T>(Action action) where T : Exception
        {
            try
            {
                action.Invoke();
                Assert.Fail($"Expected exception {typeof(T)} but no exception was caught.");
            }
            catch (T)
            {
            }
        }
    }
}
