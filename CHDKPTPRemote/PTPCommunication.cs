// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Runtime.InteropServices;
using LibUsbDotNet.Main;

namespace PTP
{
    // TODO: try to ensure connection is not unnecessarily messed up after error
    public class PTPCommunication
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PTPReqRes
        {
            public int Length;
            public ushort Type;
            public ushort Code;
            public uint TransactionId;
            public int Param1;
            public int Param2;
            public int Param3;
            public int Param4;
            public int Param5;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 484)]
            public byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PTPData
        {
            public int Length;
            public ushort Type;
            public ushort Code;
            public uint TransactionId;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 500)]
            public byte[] data;
        }

        // PTP Request/Response contents
        // XXX might need to do little endian to/from big endian conversions!
        public ushort Code { get { return reqres.Code; } set { reqres.Code = value; } }
        //public int SessionId; // never really used!
        private uint TransactionId { get { return reqres.TransactionId; } set { reqres.TransactionId = value; } } // not really useful as public
        public int Param1 { get { return reqres.Param1; } set { reqres.Param1 = value; } }
        public int Param2 { get { return reqres.Param2; } set { reqres.Param2 = value; } }
        public int Param3 { get { return reqres.Param3; } set { reqres.Param3 = value; } }
        public int Param4 { get { return reqres.Param4; } set { reqres.Param4 = value; } }
        public int Param5 { get { return reqres.Param5; } set { reqres.Param5 = value; } }
        public int NParams;

        private PTPDevice _device;
        public PTPDevice device { get { return _device; } }

        // where we store the actual data to be sent/received
        private PTPReqRes reqres;
        private PTPData ptpdata;
        private IntPtr p_reqres;
        private IntPtr p_ptpdata;

        
        public PTPCommunication(PTPDevice dev)
        {
            _device = dev;

            reqres = new PTPReqRes();
            reqres.data = new byte[484];
            ptpdata = new PTPData();
            ptpdata.data = new byte[500];
            p_reqres = Marshal.AllocHGlobal(Marshal.SizeOf(reqres));
            p_ptpdata = Marshal.AllocHGlobal(Marshal.SizeOf(ptpdata));

            ResetAll();
        }

        ~PTPCommunication()
        {
            Marshal.FreeHGlobal(p_ptpdata);
            Marshal.FreeHGlobal(p_reqres);
        }

        public void ResetAll()
        {
            Code = 0;
            //SessionId = 1; // not used
            TransactionId = 0;

            ResetParams();
        }

        public void ResetParams()
        {
            Param1 = 0;
            Param2 = 0;
            Param3 = 0;
            Param4 = 0;
            Param5 = 0;
            NParams = 0;
        }

        // TODO: add handler for USB errors to use them in the exceptions below

        private void CheckError(ErrorCode err)
        {
            if (err != ErrorCode.None)
            {
                throw new PTPException("could not read/write data: usb error = " + err.ToString());
            }
        }

        private void CheckErrorAndLength(ErrorCode err, int len, int target_len)
        {
            if (err != ErrorCode.None)
            {
                throw new PTPException("could not read/write data: usb error = " + err.ToString());
            }
            else if (len != target_len)
            {
                throw new PTPException("could not read/write all data (" + len.ToString() + " bytes instead of " + target_len.ToString());
            }
        }

        private void SendRequest()
        {
            reqres.Length = 12 + 4 * NParams; // don't send unused parameters or data
            reqres.Type = 1; // PTP_USB_CONTAINER_COMMAND

            TransactionId += 1;

            Marshal.StructureToPtr(reqres, p_reqres, true);

            int len;
            ErrorCode err;

            err = _device.Writer.Write(p_reqres, 0, reqres.Length, 5000, out len);

            CheckErrorAndLength(err, len, reqres.Length);
        }

        private void ReceiveResponse()
        {
            int len;
            ErrorCode err;

            ResetParams();

            err = _device.Reader.Read(p_reqres, 0, Marshal.SizeOf(reqres), 5000, out len);

            CheckError(err);

            reqres = (PTPReqRes) Marshal.PtrToStructure(p_reqres, typeof(PTPReqRes));
        }

        private void SendData(byte[] data)
        {
            int count = (data.Length < 500) ? data.Length : 500;

            ptpdata.Length = 12 + data.Length;
            ptpdata.Type = 2; // PTP_USB_CONTAINER_DATA
            ptpdata.Code = Code;
            ptpdata.TransactionId = TransactionId;
            Array.Copy(data, ptpdata.data, count);

            Marshal.StructureToPtr(ptpdata, p_ptpdata, true);

            int len;
            ErrorCode err;

            err = _device.Writer.Write(p_ptpdata, 0, 12+count, 5000, out len);

            CheckErrorAndLength(err, len, 12 + count);

            // small amount of data -> we're done
            // (< instead of <= because of final if)
            if ( count < 500 )
            {
                return;
            }

            // more data to be sent
            if (count > 500)
            {
                err = _device.Writer.Write(data, count, data.Length-500, 5000, out len);

                CheckErrorAndLength(err, len, data.Length - 500);
            }

            // must send empty packet to signal end on multiples of 512
            // (doesn't seem to happen in libptp?)
            if ((data.Length + 12) % 512 == 0)
            {
                err = _device.Writer.Write(null, 0, 0, 5000, out len);

                CheckError(err);
            }
        }

        private void ReceiveData(out byte[] data)
        {
            int len;
            ErrorCode err;

            data = null;

            err = _device.Reader.Read(p_ptpdata, 0, 512, 5000, out len);

            CheckError(err);

            ptpdata = (PTPData)Marshal.PtrToStructure(p_ptpdata, typeof(PTPData));

            if (ptpdata.Length <= 512)
            {
                // must read empty end package if length is multiple of 512
                if (ptpdata.Length == 512)
                {
                    // if 0 length is used Read does nothing, so use 512 and supply p_ptpdata in case something is received
                    err = _device.Reader.Read(p_ptpdata, 0, 512, 5000, out len);
                    CheckErrorAndLength(err, len, 0);
                }

                data = new byte[ptpdata.Length - 12];
                Array.Copy(ptpdata.data, data, data.Length);

                return;
            }

            // N.B.: USBEndPointReader expects multiple of MaxPacketSize but does returns actually length
            int padded_remaining_length = (ptpdata.Length-1) & ~0x1ff; // ((ptpdata.Length-512)+511) % 512
            IntPtr p = Marshal.AllocHGlobal(padded_remaining_length);

            try
            {
                err = _device.Reader.Read(p, 0, padded_remaining_length, 5000, out len);
                CheckErrorAndLength(err, len, ptpdata.Length-512);

                // must read empty end package if length is multiple of 512
                if ((len & 0x1ff) == 0)
                {
                    // if 0 length is used Read does nothing, so use 512 and supply p_ptpdata in case something is received
                    err = _device.Reader.Read(p_ptpdata, 0, 512, 5000, out len);
                    CheckErrorAndLength(err, len, 0);
                }

                data = new byte[ptpdata.Length - 12];
                Array.Copy(ptpdata.data, data, 500);
                Marshal.Copy(p, data, 500, ptpdata.Length-512);
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
        }

        public void Send()
        {
            try
            {
                SendRequest();
                ReceiveResponse();
            }
            finally
            {
                TransactionId += 1;
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                SendRequest();
                SendData(data);
                ReceiveResponse();
            }
            finally
            {
                TransactionId += 1;
            }
        }
        
        public void Send(out byte[] data)
        {
            try
            {
                SendRequest();
                ReceiveData(out data);
                ReceiveResponse();
            }
            finally
            {
                TransactionId += 1;
            }
        }
    }
}
