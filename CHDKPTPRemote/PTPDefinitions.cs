// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;

namespace PTP
{
    public enum PTP_Operation
    {
        PTP_OC_OpenSession = 0x1002,
        PTP_OC_CloseSession = 0x1003,
        PTP_OC_CHDK = 0x9999,
    }

    public enum PTP_Response
    {
        PTP_RC_OK = 0x2001,
        PTP_RC_GeneralError = 0x2002,
        PTP_RC_OperationNotSupported = 0x2005,
        PTP_RC_ParameterNotSupported = 0x2006,
        PTP_RC_InvalidParameter = 0x201D,
    }

    public class PTPException : Exception
    {
        public PTPException(string message) : base(message)
        {
        }
    }
}
