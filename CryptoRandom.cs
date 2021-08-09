using System;
using System.Security.Cryptography;

namespace ImageBank
{
    public class CryptoRandom : IDisposable
    {
        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private byte[] _buffer;
        private int _bufferposition;
        private bool disposedvalue;
        private readonly object _this = new object();

        private void InitBuffer()
        {
            if (_buffer == null) { 
                _buffer = new byte[8000];
            }

            _rng.GetBytes(_buffer);
            _bufferposition = 0;
        }

        private void EnsureRandomBuffer(int requiredbytes)
        {
            if (_buffer == null) {
                InitBuffer();
            }

            if (requiredbytes > _buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(requiredbytes), "cannot be greater than random buffer");
            }

            if ((_buffer.Length - _bufferposition) < requiredbytes) {
                InitBuffer();
            }
        }

        public ulong GetRandom64()
        {
            lock (_this) {
                EnsureRandomBuffer(sizeof(ulong));
                var rand = BitConverter.ToUInt64(_buffer, _bufferposition);
                _bufferposition += sizeof(ulong);
                return rand;
            }
        }

        public short NextShort(short minvalue, short maxvalue)
        {
            if (minvalue > maxvalue) {
                throw new ArgumentOutOfRangeException(nameof(minvalue));
            }

            ulong diff = (ulong)(maxvalue - minvalue + 1);
            var random64 = GetRandom64();
            var remainder = (short)(random64 % diff);
            var randomnext = (short)(minvalue + remainder);
            return randomnext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedvalue) {
                if (disposing) {
                    _rng.Dispose();
                }

                _buffer = null;
                disposedvalue = true;
            }
        }

        ~CryptoRandom()
        {
             Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
