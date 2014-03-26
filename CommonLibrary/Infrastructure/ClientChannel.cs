using System;

using System.ServiceModel;

namespace Common_Library.Infrastructure
{
    public class ClientChannel<TService> : IDisposable
    {
      private readonly TService _proxy;

      public TService Proxy
      {
        get { return _proxy; }
      }

      public ClientChannel(TService proxy)
      {
        _proxy = proxy;
      }

      public void Dispose()
      {
        ((IClientChannel)_proxy).Close();
      }
    }
}
