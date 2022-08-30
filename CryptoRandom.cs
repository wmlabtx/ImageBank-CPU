using System;
using System.Security.Cryptography;

namespace ImageBank
{
    public sealed class CryptoRandom : IDisposable
    {
        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private byte[] _buffer;
        private int _bufferposition;
        private bool _isdisposed;
        private readonly object _this = new object();

        private void InitBuffer()
        {
            if (_buffer == null) { 
                _buffer = new byte[8000];
            }

            _rng.GetBytes(_buffer);
            _bufferposition = 0;

            _isdisposed = false;
        }

        private void EnsureRandomBuffer(int requiredbytes)
        {
            if (_buffer == null) {
                InitBuffer();
            }

            if (requiredbytes > _buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(requiredbytes), @"cannot be greater than random buffer");
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

        public int Next(int minvalue, int maxvalue)
        {
            if (minvalue > maxvalue) {
                throw new ArgumentOutOfRangeException(nameof(minvalue));
            }

            var diff = (ulong)(maxvalue - minvalue + 1);
            var random64 = GetRandom64();
            var remainder = (int)(random64 % diff);
            var randomnext = minvalue + remainder;
            return randomnext;
        }

        ~CryptoRandom()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isdisposed) {
                return;
            }

            if (disposing) {
                _rng?.Dispose();
            }

            _isdisposed = true;
        }
    }
}
