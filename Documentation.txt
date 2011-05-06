Documentation for CHDKPTPRemote
April 29, 2011



CHDKPTPRemote is a .NET 4.0 library written in C# for working with cameras via
CHDK PTP[1,2]. It uses the LibUsbDotNet[3] library for USB communication.

Currently the library is build up as follows.

  LibUsbDotNet -- PTP -- CHDKPTP -- CHDKPTPRemote

PTP uses LibUsbDotNet to provide a PTP interface for communicating with
devices. CHDKPTP extends PTP for support of CHDK PTP specific functionality.
Finally, CHDKPTPRemote provides a simple interface with the most basic
essential operations such as script execution.

Except for LibUsbDotNet, the libraries use Exceptions when unexpected behaviour
is encountered. It is best to enclose all calls in try blocks and handle these
exceptions. Note that exceptions due to low-level problems (e.g. crashed
devices or conflicting implementations) might result in "broken" sessions.



Functions of CHDKPTPRemote.Session class

static List<CHDKPTPDevice> ListDevices(bool only_supported = true)
  Returns a list of available devices. If only_supported is true, the list is
  filtered such that only those devices remain that have a compatible CHDK PTP
  implementation.

Session(CHDKPTPDevice dev)
  Constructor for CHDKPTPRemote sessions. Does not actually initiate sessions.

void Connect()
  Makes connection to camera. Must be successfully called before calling any
  of the functions below.

void Disconnect()
  Terminates connection to camera. After this call this function and the
  following functions should no longer be called. Calling Connect again should
  be fine.

object ExecuteScript(string script, bool return_string_as_byte_array = false)
  Executes Lua script on camera. The functions waits until the script is done
  and, if available, returns its return value. Currently supported return types
  are booleans, integers and strings. String return values are interpreted as
  ASCII; if not desired - because the string contains binary data, for example
  - setting the return_string_as_byte_array parameter to true results in the
  string being returned as is as a byte[] object.

byte[] DownloadFile(string filename)
  Retrieves filename from the camera. Note that filename should contain a valid
  path (i.e. one starting with "A/").



Other useful members

In determining which device to connect to one might want to use the following.
 
In CHDKPTP.CHDKPTPDevice:
  int CHDKVersionMajor
    Major version of the CHDK PTP of the camera.
  int CHDKVersionMinor
    Minor version of the CHDK PTP of the camera.
  bool CHDKSupported
    Indicates whether or the library supports this camera. N.B.: This is
	determined in the call to ListDevices() that returned the device.

In CHDKPTP.CHDKPTPUtil:
  static int CHDK_VERSION_MAJOR
    Major version that this library supports. Can be changed in case one knows
	a different version works as well.
  static int CHDK_VERSION_MINOR
    Minimal minor version that this library supports. Can be changed in case
	one knows a different version works as well.

In PTP.PTPDevice:
  bool PTPSupported
    Indicates whether or not this device supports PTP.
  string Name
    Returns the name of the device (currently this is just the product name).
  string ToString()
    Returns a string identifying the device. This is just the Name plus, if
	appropriate, the CHDK PTP version.



Examples

Basic usage:

  using CHDKPTP
  using CHDKPTPRemote

  // we assume there is at least one connected and supported device
  var session = new Session(Session.ListDevices()[0]);
  session.Connect();

  // do stuff

  session.Disconnect();


Taking a picture:

  session.ExecuteScript("switch_mode_usb(1)"); // switch to record mode
  session.ExecuteScript("shoot()"); // take picture


Downloading image:

  File.WriteAllBytes("IMG_0001.JPG",session.DownloadFile("A/DCIM/100CANON/IMG_0001.JPG"));



References
1. Canon Hacker Development Kit (CHDK), http://chdk.wikia.com/
2. CHDK PTP forum thread, http://chdk.setepontos.com/index.php/topic,4338.0.html
3. LibUsbDotNet, http://libusbdotnet.sourceforge.net/



Copyright Muck van Weerdenburg 2011.
Distributed under the Boost Software License, Version 1.0.
(See accompanying file LICENSE_1_0.txt or copy at
http://www.boost.org/LICENSE_1_0.txt)