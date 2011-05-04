// Copyright Muck van Weerdenburg 2011.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using LibUsbDotNet; // XXX needed for call to CHDKPTPUtil.FindDevices
using CHDKPTP;

namespace CHDKPTPRemote
{
    public class Session
    {
        public static List<CHDKPTPDevice> ListDevices(bool only_supported = true)
        {
            return CHDKPTPUtil.FindDevices(only_supported);
        }

        private CHDKPTPSession _session;
        private bool have_video_details;
        private uint image_tranfer_function;
        private int width;
        private int height;
        private int vb_max_width;
        private int vb_max_height;
        private int vb_buffer_width;

        public Session(CHDKPTPDevice dev)
        {
            _session = new CHDKPTPSession(dev);
            have_video_details = false;
        }

        public void Connect()
        {
            _session.device.Open();
            try
            {
                _session.OpenSession();
            }
            catch (Exception e)
            {
                _session.device.Close();
                throw e;
            }
        }

        public void Disconnect()
        {
            have_video_details = false;
            try
            {
                _session.CloseSession();
            }
            catch (Exception)
            {
            }
            finally
            {
                _session.device.Close();
            }
        }

        // returns return data (if any) unless get_error is true
        private object GetScriptMessage(int script_id, bool return_string_as_byte_array, bool get_error = false)
        {
            CHDK_ScriptMsgType type;
            int subtype, script_id2;
            byte[] data;
            while (true)
            {
                _session.CHDK_ReadScriptMsg(out type, out subtype, out script_id2, out data);

                if (type == CHDK_ScriptMsgType.PTP_CHDK_S_MSGTYPE_NONE) // no more messages; no return value
                    return null;

                if (script_id2 != script_id) // ignore message from other scripts
                    continue;

                if (!get_error && type == CHDK_ScriptMsgType.PTP_CHDK_S_MSGTYPE_RET) // return info!
                {
                    switch ((CHDK_ScriptDataType)subtype)
                    {
                        case CHDK_ScriptDataType.PTP_CHDK_TYPE_BOOLEAN:
                            return (data[0] | data[1] | data[2] | data[3]) != 0;
                        case CHDK_ScriptDataType.PTP_CHDK_TYPE_INTEGER:
                            return data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
                        case CHDK_ScriptDataType.PTP_CHDK_TYPE_STRING:
                            if (return_string_as_byte_array)
                                return data;
                            else
                                return (new ASCIIEncoding()).GetString(data);
                        default:
                            throw new Exception("script returned unsupported data type: " + type.ToString());
                    }
                }

                if (type == CHDK_ScriptMsgType.PTP_CHDK_S_MSGTYPE_ERR) // hmm.. error
                {
                    if (get_error)
                    {
                        return (new ASCIIEncoding()).GetString(data);
                    }
                    else
                    {
                        throw new Exception("error running script: " + (new ASCIIEncoding()).GetString(data));
                    }
                }

                // ignore other (user) messages
            }
        }

        // TODO: should be able to distinguish "real" exceptions and script errors
        public object ExecuteScript(string script, bool return_string_as_byte_array = false)
        {
            int script_id;
            CHDK_ScriptErrorType status;
            _session.CHDK_ExecuteScript(script, CHDK_ScriptLanguage.PTP_CHDK_SL_LUA, out script_id, out status);

            if (status == CHDK_ScriptErrorType.PTP_CHDK_S_ERRTYPE_COMPILE)
            {
                object msg = GetScriptMessage(script_id, false, true);
                if (msg.GetType() == typeof(string))
                {
                    throw new Exception("script compilation error: " + (string)msg);
                }
                else
                {
                    throw new Exception("script compilation error (unknown reason)");
                }
            }

            // wait for end
            while (true)
            {
                CHDK_ScriptStatus flags;
                _session.CHDK_ScriptStatus(out flags);
                if (!flags.HasFlag(CHDK_ScriptStatus.PTP_CHDK_SCRIPT_STATUS_RUN))
                {
                    break;
                }

                System.Threading.Thread.Sleep(100);
            }

            // get result
            return GetScriptMessage(script_id, return_string_as_byte_array);
        }

        private int int_from_bytes(byte[] buf, int offset)
        {
            return buf[offset] | (buf[offset+1] << 8) | (buf[offset+2] << 16) | (buf[offset+3] << 24);
        }

        private void GetVideoDetails()
        {
            if (have_video_details)
                return;

            object result = ExecuteScript("return get_video_details();", true);
            if (result != null && result.GetType() == typeof(byte[]))
            {
                byte[] buf = (byte[])result;
                image_tranfer_function = (uint)int_from_bytes(buf, 0);
                width = int_from_bytes(buf, 4);
                height = int_from_bytes(buf, 8);
                vb_max_width = int_from_bytes(buf, 12);
                vb_max_height = int_from_bytes(buf, 16);
                vb_buffer_width = int_from_bytes(buf, 20);
            }
            else
            {
                throw new Exception("could not get video details; result = " + result.ToString());
            }
        }

        public void GetDisplayDimensions(out int width, out int height)
        {
            GetVideoDetails();

            width = this.width;
            height = this.height;
        }

        private byte clip(int val)
        {
            if (val > 255)
                return 255;
            else if (val < 0)
                return 0;
            else
                return (byte) val;
        }
        private void add_pixel(byte[] buf, int pos, byte y, sbyte u, sbyte v)
        {
            buf[pos]   = clip(((y << 12) + u * 7258            + 2048) >> 12); // b
            buf[pos+1] = clip(((y << 12) - u * 1411 - v * 2925 + 2048) >> 12); // g
            buf[pos+2] = clip(((y << 12)            + v * 5743 + 2048) >> 12); // r
        }
        private Bitmap GetLiveImage(bool set_active_area, ref Rectangle active_area, Bitmap old_img)
        {
            if (old_img != null)
            {
                if (old_img.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb)
                {
                    throw new Exception("cannot reuse Bitmap: PixelFormat is not Format24bppRgb");
                }
                if (old_img.Width != vb_max_width || old_img.Height != vb_max_height)
                {
                    throw new Exception("cannot reuse Bitmap; dimensions are incorrect");
                }
            }

            GetVideoDetails();

            byte[] img;
            _session.CHDK_CallFunction(image_tranfer_function, set_active_area?0x3:0x1, 0,out img);

            int image_start = 1;
            int image_end = img.Length;
            if (set_active_area)
            {
                if (img[0] == 0) // image comes first
                {
                    image_end -= 1 + 4 * 4;
                    active_area = new Rectangle(
                        int_from_bytes(img, image_end + 1),
                        int_from_bytes(img, image_end + 5),
                        int_from_bytes(img, image_end + 9),
                        int_from_bytes(img, image_end + 13)
                        );
                }
                else // image comes second
                {
                    image_start += 1 + 4 * 4;
                    active_area = new Rectangle(
                        int_from_bytes(img, 1),
                        int_from_bytes(img, 5),
                        int_from_bytes(img, 9),
                        int_from_bytes(img, 13)
                        );
                }
            }

            // convert uyvyyy to rgbrgbrgbrgb
            byte[] pixels = new byte[vb_max_width * vb_max_height * 3];
            for (int img_idx = image_start, pxls_idx = 0; img_idx < image_end; img_idx += ((vb_buffer_width - vb_max_width) * 6) / 4)
            {
                for (int x = 0; x < vb_max_width; x++, img_idx += 6, pxls_idx += 12)
                {
                    sbyte u = (sbyte)img[img_idx];
                    sbyte v = (sbyte)img[img_idx + 2];
                    add_pixel(pixels, pxls_idx, img[img_idx + 1], u, v);
                    add_pixel(pixels, pxls_idx + 3, img[img_idx + 3], u, v);
                    add_pixel(pixels, pxls_idx + 6, img[img_idx + 4], u, v);
                    add_pixel(pixels, pxls_idx + 9, img[img_idx + 5], u, v);
                }
            }

            // copy pixels to bitmap
            if (old_img == null)
                old_img = new Bitmap(vb_max_width, vb_max_height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData bmpd = old_img.LockBits(new Rectangle(0, 0, vb_max_width, vb_max_height), System.Drawing.Imaging.ImageLockMode.WriteOnly, old_img.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bmpd.Scan0, pixels.Length);
            old_img.UnlockBits(bmpd);

            return old_img;
        }

        public Bitmap GetLiveImage(Bitmap old_img = null)
        {
            Rectangle tmp = new Rectangle();
            return GetLiveImage(false, ref tmp, old_img);
        }

        public Bitmap GetLiveImage(out Rectangle active_area, Bitmap old_img = null)
        {
            active_area = new Rectangle();
            return GetLiveImage(true, ref active_area, old_img);
        }

        public byte[] DownloadFile(string filename)
        {
            byte[] buf;

            _session.CHDK_DownloadFile(filename, out buf);

            return buf;
        }
    }
}
