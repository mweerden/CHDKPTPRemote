// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

namespace CHDKPTP
{
    public enum CHDK_PTP_Command
    {
        PTP_CHDK_Version,
        PTP_CHDK_GetMemory,
        PTP_CHDK_SetMemory,
        PTP_CHDK_CallFunction,
        PTP_CHDK_TempData,
        PTP_CHDK_UploadFile,
        PTP_CHDK_DownloadFile,
        PTP_CHDK_ExecuteScript,
        PTP_CHDK_ScriptStatus,
        PTP_CHDK_ScriptSupport,
        PTP_CHDK_ReadScriptMsg,
        PTP_CHDK_WriteScriptMsg
    }

    public enum CHDK_ScriptLanguage
    {
        PTP_CHDK_SL_LUA,
        PTP_CHDK_SL_UBASIC
    }

    public enum CHDK_ScriptDataType
    {
        PTP_CHDK_TYPE_UNSUPPORTED,
        PTP_CHDK_TYPE_NIL,
        PTP_CHDK_TYPE_BOOLEAN,
        PTP_CHDK_TYPE_INTEGER,
        PTP_CHDK_TYPE_STRING,
        PTP_CHDK_TYPE_TABLE
    }

    public enum CHDK_ScriptMsgType
    {
        PTP_CHDK_S_MSGTYPE_NONE,
        PTP_CHDK_S_MSGTYPE_ERR,
        PTP_CHDK_S_MSGTYPE_RET,
        PTP_CHDK_S_MSGTYPE_USER
    }

    public enum CHDK_ScriptErrorType
    {
        PTP_CHDK_S_ERRTYPE_NONE,
        PTP_CHDK_S_ERRTYPE_COMPILE,
        PTP_CHDK_S_ERRTYPE_RUN
    }

    public enum CHDK_ScriptStatus
    {
        PTP_CHDK_SCRIPT_STATUS_RUN = 0x1,
        PTP_CHDK_SCRIPT_STATUS_MSG = 0x2
    }

    public enum CHDK_ScriptSupport
    {
        PTP_CHDK_SCRIPT_SUPPORT_LUA = 0x1
    }
}
