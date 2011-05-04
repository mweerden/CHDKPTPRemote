// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace PTP
{
    public class PTPSession
    {
        private PTPDevice _device;
        public PTPDevice device { get { return _device; } }
        private bool _IsOpen;
        public bool IsOpen { get { return _IsOpen; } }
        protected PTPCommunication ptp;

        public PTPSession(PTPDevice dev)
        {
            _device = dev;
            ptp = new PTPCommunication(_device);
            _IsOpen = false;
        }

        public void SendCommand(PTP_Operation op, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0, int param5 = 0)
        {
            //ptp.ResetParams(); //not needed as all params are set anyway
            ptp.Code = (ushort)op;
            ptp.NParams = num_params; // perhaps check for value value
            ptp.Param1 = param1;
            ptp.Param2 = param2;
            ptp.Param3 = param3;
            ptp.Param4 = param4;
            ptp.Param5 = param5;

            ptp.Send();
        }

        public void SendCommand(PTP_Operation op, byte[] data, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0, int param5 = 0)
        {
            //ptp.ResetParams(); //not needed as all params are set anyway
            ptp.Code = (ushort)op;
            ptp.NParams = num_params; // perhaps check for value value
            ptp.Param1 = param1;
            ptp.Param2 = param2;
            ptp.Param3 = param3;
            ptp.Param4 = param4;
            ptp.Param5 = param5;

            ptp.Send(data);
        }

        public void SendCommand(PTP_Operation op, out byte[] data, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0, int param5 = 0)
        {
            //ptp.ResetParams(); //not needed as all params are set anyway
            ptp.Code = (ushort)op;
            ptp.NParams = num_params; // perhaps check for value value
            ptp.Param1 = param1;
            ptp.Param2 = param2;
            ptp.Param3 = param3;
            ptp.Param4 = param4;
            ptp.Param5 = param5;

            ptp.Send(out data);
        }

        public void Ensure_PTP_RC_OK()
        {
            if (ptp.Code != (ushort)PTP_Response.PTP_RC_OK)
            {
                throw new PTPException("could not get perform PTP operation (unexpected return code 0x" + ptp.Code.ToString("X4") + ")");
            }
        }

        public void OpenSession()
        {
            SendCommand(PTP_Operation.PTP_OC_OpenSession, 1, 1);
            Ensure_PTP_RC_OK();

            _IsOpen = true;
        }

        public void CloseSession()
        {
            SendCommand(PTP_Operation.PTP_OC_CloseSession, 0);
            Ensure_PTP_RC_OK();

            _IsOpen = false;
        }
    }
}
