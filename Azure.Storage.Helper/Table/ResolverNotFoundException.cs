using System;

namespace Euyuil.Azure.Storage.Helper.Table
{
    public class ResolverNotFoundException : Exception
    {
        public ResolverNotFoundException()
        {
        }

        public ResolverNotFoundException(string message) : base(message)
        {
        }

        public ResolverNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
