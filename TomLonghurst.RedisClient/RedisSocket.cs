using System.Net.Sockets;

namespace TomLonghurst.RedisClient
{
    internal class RedisSocket : Socket
    {
        internal bool IsDisposed { get; private set; }
        
        internal RedisSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }

        internal RedisSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }

        internal RedisSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }
            
            base.Dispose(disposing);
        }
    }
}