// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.Descriptors;
using LibUsbDotNet.Info;

namespace PTP
{
    public static class PTPUtil
    {
        public static List<PTPDevice> FindDevices(bool only_supported = true, Func<UsbDevice,PTPDevice> constr = null)
        {
            List<PTPDevice> l = new List<PTPDevice>();

            if (constr == null)
                constr = x => new PTPDevice(x);

            foreach (UsbRegistry reg in UsbDevice.AllDevices)
            {
                UsbDevice dev;
                if (reg.Open(out dev))
                {
                    PTPDevice ptpdev = constr(dev);

                    for (int i = 0; i < dev.Configs.Count; i++)
                    {
                        UsbConfigInfo config_info = dev.Configs[i];

                        foreach (UsbInterfaceInfo interface_info in config_info.InterfaceInfoList)
                        {
                            if (interface_info.Descriptor.Class == ClassCodeType.Ptp)
                            {
                                bool rid_set = false;
                                bool wid_set = false;

                                foreach (UsbEndpointInfo endpoint_info in interface_info.EndpointInfoList)
                                {
                                    // BULK and assumed MaxPacketSize
                                    if ((endpoint_info.Descriptor.Attributes & 0x03) != 0x02 || endpoint_info.Descriptor.MaxPacketSize != 512)
                                        continue;

                                    if ((endpoint_info.Descriptor.EndpointID & 0x80) == 0)
                                    {
                                        ptpdev.WriterEndpointID = (WriteEndpointID)endpoint_info.Descriptor.EndpointID;
                                        wid_set = true;
                                    }
                                    else
                                    {
                                        ptpdev.ReaderEndpointID = (ReadEndpointID)endpoint_info.Descriptor.EndpointID;
                                        rid_set = true;
                                    }
                                }

                                if (rid_set && wid_set)
                                {
                                    ptpdev.ConfigurationID = config_info.Descriptor.ConfigID;
                                    ptpdev.InterfaceID = interface_info.Descriptor.InterfaceID;
                                    ptpdev.PTPSupported = true;
                                    break;
                                }
                            }
                            if (ptpdev.PTPSupported)
                                break;
                        }
                    }

                    if (!only_supported || ptpdev.PTPSupported)
                    {
                        l.Add(ptpdev);
                    }

                    dev.Close(); // always close so we don't have a list of open but unused devices
                }
            }

            return l;
        }
    }
}