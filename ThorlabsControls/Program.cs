using System.IO.Ports;

namespace ThorlabsControls;

public abstract class ThorlabsControls
{
    public static void Main(string[] args)
    {
    }

    public static SerialPort? GetDevicePort(string targetSerialNumber)
    {
        // Get a list of all available serial ports on the PC
        string[] allPortNames = SerialPort.GetPortNames();

        // Print each port name individually
        foreach (string portName in allPortNames)
        {
            Console.WriteLine(portName);
        }

        // Identify which ports are connected to Thorlabs devices
        foreach (string portName in allPortNames)
        {
            SerialPort? port = new SerialPort(portName, 115200);
            try
            {
                port.Open();

                // Are any additional initialisation steps required here?

                // Send HW_REQ_INFO command to the device to query its serial number

                // Read the response (format described by HW_GET_INFO in the Thorlabs APT protocol document)
                byte[] response = new byte[90];
                port.Read(response, 0, response.Length); // Read the whole response (90 bytes long)

                // Extract the serial number from the response
                byte[] serialNumberBytes = new byte[4];
                Array.Copy(response, 6, serialNumberBytes, 0, serialNumberBytes.Length);
                int serialNumber = BitConverter.ToInt32(serialNumberBytes, 0);

                // Check if serial number of connected device matches target serial number
                if (serialNumber.ToString() == targetSerialNumber)
                {
                    // Return this port to the calling method
                    return port;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                // Close the serial port
                if (port.IsOpen)
                {
                    port.Close();
                }
            }
        }

        return null;
    }
}