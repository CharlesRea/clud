using System;

namespace Clud.Api.Infrastructure.DataAccess
{
    public class NavigationPropertyNotLoadedException : Exception
    {
        public NavigationPropertyNotLoadedException(string propertyName) :
            base($"Navigation property {propertyName} has not been loaded")
        { }
    }
}