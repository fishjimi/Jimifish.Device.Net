﻿using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceBase<T> : UWPDeviceBase, IDevice
    {
        #region Protected Properties
        protected T _ConnectedDevice;
        #endregion

        #region Protected Methods
        protected async Task<T> GetDevice(string id)
        {
            var hidDeviceOperation = FromIdAsync(id);
            var task = hidDeviceOperation.AsTask();
            var hidDevice = await task;
            return hidDevice;
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract IAsyncOperation<T> FromIdAsync(string id);
        #endregion

        #region Public Overrides
        public override async Task<bool> GetIsConnectedAsync()
        {
            return _ConnectedDevice != null;
        }

        public override async Task<byte[]> ReadAsync()
        {
            if (_IsReading)
            {
                throw new Exception("Reentry");
            }

            lock (_Chunks)
            {
                if (_Chunks.Count > 0)
                {
                    var retVal = _Chunks[0];
                    Tracer?.Trace(false, retVal);
                    _Chunks.RemoveAt(0);
                    return retVal;
                }
            }

            _IsReading = true;
            _TaskCompletionSource = new TaskCompletionSource<byte[]>();
            return await _TaskCompletionSource.Task;
        }
        #endregion

        #region Public Virtual Methods
        public virtual void Dispose()
        {
            if (_ConnectedDevice is IDisposable disposable) disposable?.Dispose();
            _TaskCompletionSource?.Task?.Dispose();
        }
        #endregion
    }
}
