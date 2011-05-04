// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using LibUsbDotNet;
using PTP;

namespace CHDKPTP
{
    public class CHDKPTPDevice : PTPDevice
    {
        public int CHDKVersionMajor;
        public int CHDKVersionMinor;
        public bool CHDKSupported;

        public CHDKPTPDevice(UsbDevice dev)
            : base(dev)
        {
            CHDKVersionMajor = -1;
            CHDKVersionMinor = -1;
            CHDKSupported = false;
        }

        public override string ToString()
        {
            if (CHDKVersionMajor != -1 && CHDKVersionMinor != -1)
            {
                return base.ToString() + " (CHDK PTP v" + CHDKVersionMajor + "." + CHDKVersionMinor + ")";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}