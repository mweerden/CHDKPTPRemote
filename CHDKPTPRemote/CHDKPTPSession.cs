// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.Text;
using PTP;

namespace CHDKPTP
{
    public class CHDKPTPSession : PTPSession
    {
        public CHDKPTPSession(CHDKPTPDevice dev) : base(dev)
        {
        }

        public void SendCHDKCommand(CHDK_PTP_Command c, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
        {
            SendCommand(PTP_Operation.PTP_OC_CHDK, num_params + 1, (int)c, param1, param2, param3, param4);
        }

        public void SendCHDKCommand(CHDK_PTP_Command c, byte[] data, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
        {
            SendCommand(PTP_Operation.PTP_OC_CHDK, data, num_params + 1, (int)c, param1, param2, param3, param4);
        }

        public void SendCHDKCommand(CHDK_PTP_Command c, out byte[] data, int num_params, int param1 = 0, int param2 = 0, int param3 = 0, int param4 = 0)
        {
            SendCommand(PTP_Operation.PTP_OC_CHDK, out data, num_params + 1, (int)c, param1, param2, param3, param4);
        }

        public bool CHDK_Version(out int major, out int minor)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_Version, 0);

            if (ptp.Code == (ushort)PTP_Response.PTP_RC_OperationNotSupported)
            {
                major = -1;
                minor = -1;
                return false;
            }
            else
            {
                Ensure_PTP_RC_OK();
            }

            major = ptp.Param1;
            minor = ptp.Param2;

            return true;
        }

        public void CHDK_GetMemory(uint addr, int size, out byte[] data)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_GetMemory, out data, 2, (int)addr, size);
            Ensure_PTP_RC_OK();
        }

        public void CHDK_CallFunction(uint func, int param1, int param2, out byte[] data)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_CallFunction, out data, 4, 0x1, (int)func, param1, param2);
            Ensure_PTP_RC_OK();
        }

        public void CHDK_DownloadFile(string filename, out byte[] data)
        {
            if (filename.Substring(0,2) != "A/")
            {
                throw new PTPException("cannot download file: invalid path (should start with \"A/\")");
            }

            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_TempData, (new ASCIIEncoding()).GetBytes(filename), 1, 0);
            Ensure_PTP_RC_OK();

            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_DownloadFile, out data, 0);
            Ensure_PTP_RC_OK();
        }

        public void CHDK_ExecuteScript(string script, CHDK_ScriptLanguage language, out int script_id, out CHDK_ScriptErrorType status)
        {
            SendCHDKCommand(
                CHDK_PTP_Command.PTP_CHDK_ExecuteScript,
                (new System.Text.ASCIIEncoding()).GetBytes(script + "\x00"),
                1,
                (int)language
                );
            Ensure_PTP_RC_OK();

            script_id = ptp.Param1;
            status = (CHDK_ScriptErrorType)ptp.Param2;
        }

        public void CHDK_ScriptStatus(out CHDK_ScriptStatus flags)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_ScriptStatus, 0);
            Ensure_PTP_RC_OK();

            flags = (CHDK_ScriptStatus)ptp.Param1;
        }

        public void CHDK_ScriptSupport(out CHDK_ScriptSupport flags)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_ScriptSupport, 0);
            Ensure_PTP_RC_OK();

            flags = (CHDK_ScriptSupport)ptp.Param1;
        }

        public void CHDK_ReadScriptMsg(out CHDK_ScriptMsgType type, out int subtype, out int script_id, out byte[] data)
        {
            SendCHDKCommand(CHDK_PTP_Command.PTP_CHDK_ReadScriptMsg, out data, 0);
            Ensure_PTP_RC_OK();

            type = (CHDK_ScriptMsgType) ptp.Param1;
            subtype = ptp.Param2;
            script_id = ptp.Param3;
        }
    }
}
