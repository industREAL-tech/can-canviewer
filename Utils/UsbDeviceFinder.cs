using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Management;


namespace industREAL.CAN.CanViewer.Utils
{
    public class UsbDeviceFinder
    {
        public static int GetComPortByPid(string pid)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("USB discovery is only supported on Windows yet.");
                return -1;
            }
            try
            {
                // Lekérdezzük a Plug and Play eszközöket, amik soros portként funkcionálnak
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%)'"))
                {
                    foreach (var device in searcher.Get())
                    {
                        string pnpId = device["PNPDeviceID"]?.ToString() ?? "";
                        string caption = device["Caption"]?.ToString() ?? "";

                        // Ellenőrizzük, hogy a PNP ID tartalmazza-e a PID-et (pl. PID_0272)
                        if (pnpId.Contains($"PID_{pid.ToUpper()}"))
                        {
                            // Kinyerjük a COM port számát a név végéről, pl: "USB Serial Port (COM3)" -> 3
                            var match = Regex.Match(caption, @"\((COM(?<port>\d+))\)");
                            if (match.Success)
                            {
                                return Convert.ToInt32(match.Groups["port"].Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hiba a WMI lekérdezés során: " + ex.Message);
            }

            return 0;
        }
    }
}
