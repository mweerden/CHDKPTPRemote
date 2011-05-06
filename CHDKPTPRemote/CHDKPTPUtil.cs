// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using LibUsbDotNet;
using PTP;

namespace CHDKPTP
{
    public static class CHDKPTPUtil
    {
        public static int CHDK_VERSION_MAJOR = 2;
        public static int CHDK_VERSION_MINOR = 0;

        private static CHDKPTPDevice CheckSupported(PTPDevice ptpdev)
        {
            CHDKPTPDevice dev = ptpdev as CHDKPTPDevice;

            if (!dev.PTPSupported)
                return dev;

            try
            {
                if (!dev.Open())
                    return dev;

                CHDKPTPSession sess = new CHDKPTPSession(dev);
                sess.OpenSession();
                if (sess.CHDK_Version(out dev.CHDKVersionMajor, out dev.CHDKVersionMinor))
                {
                    if (dev.CHDKVersionMajor == CHDK_VERSION_MAJOR && dev.CHDKVersionMinor >= CHDK_VERSION_MINOR)
                    {
                        CHDK_ScriptSupport flags;
                        sess.CHDK_ScriptSupport(out flags);
                        if (flags.HasFlag(CHDK_ScriptSupport.PTP_CHDK_SCRIPT_SUPPORT_LUA))
                        {
                            dev.CHDKSupported = true;
                        }
                    }

                    sess.CloseSession();
                }
            }
            catch (PTPException)
            {
                // make sure CHDKSupported has not already been set
                dev.CHDKSupported = false;
            }
            finally
            {
                if (dev.IsOpen)
                  dev.Close();
            }

            return dev;
        }

        public static List<CHDKPTPDevice> FindDevices(bool only_supported = true, Func<UsbDevice,CHDKPTPDevice> constr = null)
        {
            if (constr == null)
                constr = x => new CHDKPTPDevice(x);

            List<CHDKPTPDevice> r = PTPUtil.FindDevices(only_supported, constr).ConvertAll<CHDKPTPDevice>(CheckSupported);

            if (only_supported)
                r = r.Where(x => x.CHDKSupported).ToList();

            return r;
        }
    }
}