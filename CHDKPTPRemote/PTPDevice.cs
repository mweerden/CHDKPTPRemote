// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace PTP
{
    public class PTPDevice
    {
        private UsbDevice _Device;
        public UsbDevice Device { get { return _Device; } }
        public bool IsOpen { get { return _Device.IsOpen; } }
        public UsbEndpointReader Reader;
        public UsbEndpointWriter Writer;
        public byte ConfigurationID;
        public int InterfaceID;
        public ReadEndpointID ReaderEndpointID;
        public WriteEndpointID WriterEndpointID;
        public bool PTPSupported;
        protected string _Name;
        public string Name { get { return _Name; } }

        public PTPDevice(UsbDevice dev)
        {
            _Device = dev;
            PTPSupported = false;
            _Name = dev.Info.ProductString; // TODO: try get better name
            Reader = null;
            Writer = null;
            ConfigurationID = 1;
            InterfaceID = 0;
            ReaderEndpointID = ReadEndpointID.Ep01;
            WriterEndpointID = WriteEndpointID.Ep02;
        }

        ~PTPDevice()
        {
            if (IsOpen)
                Close();
        }

        public bool Open()
        {
            if (IsOpen)
                return false;

            if (!_Device.Open())
                return false;

            IUsbDevice whole = _Device as IUsbDevice;
            if (!ReferenceEquals(whole, null))
            {
                if (!whole.SetConfiguration(ConfigurationID) || !whole.ClaimInterface(InterfaceID))
                {
                    _Device.Close();
                    throw new PTPException("could not set USB device configuration and interface to " + ConfigurationID + " and " + InterfaceID + ", respectively");
                }
            }

            Writer = _Device.OpenEndpointWriter(WriterEndpointID);
            Reader = _Device.OpenEndpointReader(ReaderEndpointID);

            return true;
        }

        public bool Close()
        {
            if (!IsOpen)
                return false;

            IUsbDevice whole = _Device as IUsbDevice;
            if (!ReferenceEquals(whole, null))
            {
                whole.ReleaseInterface(InterfaceID);
            }

            return _Device.Close();
        }

        public override string ToString()
        {
            return _Device.Info.ProductString;
        }
    }
}