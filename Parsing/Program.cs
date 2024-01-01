using System.Reflection;

namespace Parsing;

public class Queue
{
    public static void Main(string[] args)
    {
    }
    
    private readonly Queue<byte> messageQueue = new();

    public void AddToQueue(byte[] message)
    {
        foreach (byte b in message)
        {
            // Add the incoming message to the queue, one byte at a time
            messageQueue.Enqueue(b);
        }
    }
    
    public byte[] ReadHeader()
    {
        if (messageQueue.Count < 6) 
        {
            throw new InvalidOperationException("Insufficient data to read message. Total queue is shorter than expected message header length (6 bytes)");
        }
        
        // Create 'header' which is 6 bytes long
        byte[] header = new byte[6];
        
        // Read the oldest byte from messageQueue and add to 'header'. Then remove the byte from the queue. Repeat for 6 bytes.
        for (int i = 0; i < 6; i++)
        {
            header[i] = messageQueue.Dequeue();
        }
        
        return header;
    }

    public byte[] ReadDataPacket(int dataPacketLength)
    {
        if (messageQueue.Count < dataPacketLength) 
        {
            throw new InvalidOperationException($"Insufficient data to read message. Total queue is shorter than expected data packet length ({dataPacketLength} bytes)");
        }
        
        // Create 'dataPacket' which is 'dataPacketLength' bytes long
        byte[] dataPacket = new byte[dataPacketLength];
        
        // Read the oldest byte from messageQueue and add to 'dataPacket'. Then remove the byte from the queue. Repeat for 'dataPacketLength' number of bytes.
        for (int i = 0; i < dataPacketLength; i++)
        {
            dataPacket[i] = messageQueue.Dequeue();
        }
        
        return dataPacket;
    }
}

public class MessageUnpacker
{
    // Dictionary where keys are message identifiers and the corresponding values are Methods that process those messages
    private Dictionary<int, (Func<byte[], dynamic>, int expectedTotalMsgLength)> msgIdToFunc = new();
    
    // Constructor method automatically populates msgIdToFunc dictionary each time a new MessageUnpacker object is created
    public MessageUnpacker()
    {
        msgIdToFunc.Add(0x0212, (MOD_GET_CHANENABLESTATE, 6));
        msgIdToFunc.Add(0x0006, (HW_GET_INFO, 90));
        msgIdToFunc.Add(0x0066, (HUB_GET_BAYUSED, 6));
        msgIdToFunc.Add(0x0412, (MOT_GET_POSCOUNTER, 12));
        msgIdToFunc.Add(0x040B, (MOT_GET_ENCCOUNTER, 12));
        msgIdToFunc.Add(0x0415, (MOT_GET_VELPARAMS, 20));
        msgIdToFunc.Add(0x0418, (MOT_GET_JOGPARAMS, 28));
        msgIdToFunc.Add(0x043C, (MOT_GET_GENMOVEPARAMS, 12));
        msgIdToFunc.Add(0x0447, (MOT_GET_MOVERELPARAMS, 12));
        msgIdToFunc.Add(0x0452, (MOT_GET_MOVEABSPARAMS, 12));
        msgIdToFunc.Add(0x0442, (MOT_GET_HOMEPARAMS, 20));
        msgIdToFunc.Add(0x0425, (MOT_GET_LIMSWITCHPARAMS, 20));
        msgIdToFunc.Add(0x0444, (MOT_MOVE_HOMED, 6));
        msgIdToFunc.Add(0x0464, (MOT_MOVE_COMPLETED, 20));
        msgIdToFunc.Add(0x0466, (MOT_MOVE_STOPPED, 20));
        msgIdToFunc.Add(0x04A2, (MOT_GET_DCPIDPARAMS, 26));
        msgIdToFunc.Add(0x04B5, (MOT_GET_AVMODES, 10));
        msgIdToFunc.Add(0x04B2, (MOT_GET_POTPARAMS, 28));
        msgIdToFunc.Add(0x04B8, (MOT_GET_BUTTONPARAMS, 20));
        msgIdToFunc.Add(0x0491, (MOT_GET_USTATUSUPDATE, 20));
        msgIdToFunc.Add(0x042A, (MOT_GET_STATUSBITS, 12));
    }
    
    public dynamic UnpackMessage(Queue messageQueue)
    {
        // Read the 6 byte message header from the incoming message queue
        byte[] header = messageQueue.ReadHeader();
        
        // Extract the message ID from bytes 0 & 1 of the header
        int msgId = BitConverter.ToInt16(header, 0);
        if (!msgIdToFunc.ContainsKey(msgId))
        {
            throw new InvalidOperationException($"Unknown message ID: {msgId}");
        }
        
        // Extract the destination code from byte 4 of the header and check if a data packet is indicated
        byte dest = header[4];
        
        if ((dest & 0x80) != 0) // Message with data packet
        {
            // Lookup the message ID in the msgIdToFunc dictionary
            var (messageFunc, expectedLength) = msgIdToFunc[msgId];
            
            // Read the data packet from the incoming message queue
            byte[] dataPacket = messageQueue.ReadDataPacket(expectedLength - 6);
            
            // Pass the data packet to the corresponding parsing function and return the result
            return messageFunc.Invoke(dataPacket);
        }
        else // Message without data packet
        {
            // Lookup the message ID in the msgIdToFunc dictionary
            var (messageFunc, expectedLength) = msgIdToFunc[msgId];
            
            // Check 'header' total length matches the expected number of bytes for this function (always 6 bytes for header-only messages)
            if (header.Length != expectedLength)
            {
                throw new ArgumentException($"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected input array length ({header.Length} bytes) which does not match the expected length ({expectedLength} bytes).");
            }
            
            // Pass the data packet to the corresponding parsing function and return the result
            return messageFunc.Invoke(header);
        }
    }
    
    private dynamic MOD_GET_CHANENABLESTATE(byte[] inputData)
    {
        // Extract parameters from the message header
        byte chanIdent = inputData[2];
        byte enableState = inputData[3];
        
        // Check parameters are in expected range
        if (enableState != 0x01 && enableState != 0x02)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'enable state'. Must be 0x01 (enabled) or 0x02 (disabled).");
        }
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, enableState);
    }
    
    // How does HW_RESPONSE work? The APT protocol says it transmits the fault code as a numerical value,
    // but the provided command structure (6 bytes) doesn't include any space for this numerical value.
    // HW_RESPONSE is also listed as a REQ, not a GET?
    // Similar issues for HW_RICHRESPONSE
    
    private dynamic HW_GET_INFO(byte[] inputData)
    {
        // Extract parameters and package into list
        int serialNumber = BitConverter.ToInt32(inputData, 6);
        long modelNumber = BitConverter.ToInt64(inputData, 10);
        short type = BitConverter.ToInt16(inputData, 18);
        int firmwareVersion = BitConverter.ToInt32(inputData, 20);
        short hardwareVersion = BitConverter.ToInt16(inputData, 84);
        short modState = BitConverter.ToInt16(inputData, 86);
        short numberOfChannels = BitConverter.ToInt16(inputData, 88);
        
        // Package parameters into a named tuple and return to calling method
        return (serialNumber, modelNumber, type, firmwareVersion, hardwareVersion, modState, numberOfChannels);
    }
    
    private dynamic HUB_GET_BAYUSED(byte[] inputData)
    {
        // Extract parameters and package into list
        sbyte bayIdent = (sbyte)inputData[2];
        if (bayIdent is < -0x01 or > 0x06)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered a 'bayIdent' out of the valid range (-0x01 to 0x06).");
        }
        return bayIdent;
    }
    
    private dynamic MOT_GET_POSCOUNTER(byte[] inputData)
    {
        // Extract parameters and package into list
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int position = BitConverter.ToInt32(inputData, 2);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, position);
    }
    
    private dynamic MOT_GET_ENCCOUNTER(byte[] inputData)
    {
        // Extract parameters and package into list
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int encoderCount = BitConverter.ToInt32(inputData, 2);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, encoderCount);
    }
    
    private dynamic MOT_GET_VELPARAMS(byte[] inputData)
    {
        // Extract parameters and package into list
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int minVelocity = BitConverter.ToInt32(inputData, 2);
        int acceleration = BitConverter.ToInt32(inputData, 6);
        int maxVelocity = BitConverter.ToInt32(inputData, 10);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, minVelocity, acceleration, maxVelocity);
    }

    private dynamic MOT_GET_JOGPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short jogMode = BitConverter.ToInt16(inputData, 2);
        int jogStepSize = BitConverter.ToInt32(inputData, 4);
        int jogMinVelocity = BitConverter.ToInt32(inputData, 8);
        int jogAcceleration = BitConverter.ToInt32(inputData, 12);
        int jogMaxVelocity = BitConverter.ToInt32(inputData, 16);
        short stopMode = BitConverter.ToInt16(inputData, 20);
        
        // Check parameters are in expected range
        if (jogMode is < 0x01 or > 0x02)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'jog mode'. Must be 0x01 (continuous) or 0x02 (single step)");
        }
        
        if (stopMode is < 0x01 or > 0x02)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'stop mode'. Must be 0x01 (abrupt) or 0x02 (profiled deceleration)");
        }
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, jogMode, jogStepSize, jogMinVelocity, jogAcceleration, jogMaxVelocity, stopMode);
    }

    private dynamic MOT_GET_GENMOVEPARAMS(byte[] inputData)
    {
        // Extract parameters
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int backlashDistance = BitConverter.ToInt32(inputData, 2);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, backlashDistance);
    }

    private dynamic MOT_GET_MOVERELPARAMS(byte[] inputData)
    {
        // Extract parameters
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int relativeDistance = BitConverter.ToInt32(inputData, 2);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, relativeDistance);
    }
    
    private dynamic MOT_GET_MOVEABSPARAMS(byte[] inputData)
    {
        // Extract parameters and package into list
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int absolutePosition = BitConverter.ToInt32(inputData, 2);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, absolutePosition);
    }

    private dynamic MOT_GET_HOMEPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short homingDirection = BitConverter.ToInt16(inputData, 2);
        short limitSwitch = BitConverter.ToInt16(inputData, 4);
        int homeVelocity = BitConverter.ToInt32(inputData, 6);
        int offsetDistance = BitConverter.ToInt32(inputData, 10);
        
        // Check parameters are in expected range
        if (homingDirection != 0x00 && homingDirection != 0x01 && homingDirection != 0x02)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'homing direction'. Must be 0x00 (not applicable) or 0x01 (forwards) or 0x02 (reverse)");
        }
        
        if (limitSwitch != 0x00 && limitSwitch != 0x01 && limitSwitch != 0x04)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'limit switch'. Must be 0x00 (not applicable) or 0x01 (hardware reverse) or 0x04 (hardware forward)");
        }
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, homingDirection, limitSwitch, homeVelocity, offsetDistance);
    }
    
    private dynamic MOT_GET_LIMSWITCHPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short cwHardLimit = BitConverter.ToInt16(inputData, 2);
        short ccwHardLimit = BitConverter.ToInt16(inputData, 4);
        int cwSoftLimit = BitConverter.ToInt32(inputData, 6);
        int ccwSoftLimit = BitConverter.ToInt32(inputData, 10);
        short limitMode = BitConverter.ToInt16(inputData, 14);
        
        // Extract 'limit mode' (number 1-6) and 'limit swap' (bool) from 'cwHardLimit'
        short cwHardLimitState = (short)(cwHardLimit & 0x07);
        bool cwHardLimitSwapped = (cwHardLimit & 0x80) != 0;
        
        // Extract 'limit mode' (number 1-6) and 'limit swap' (bool) from 'ccwHardLimit'
        short ccwHardLimitState = (short)(ccwHardLimit & 0x07);
        bool ccwHardLimitSwapped = (ccwHardLimit & 0x80) != 0;
        
        // Extract 'software limit mode' (number 1-3) and 'is rotation stage' (bool) from 'limitMode'
        short softLimitMode = (short)(limitMode & 0x07);
        bool isRotationStage = (limitMode & 0x80) != 0;
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, cwHardLimitState, cwHardLimitSwapped, ccwHardLimitState, ccwHardLimitSwapped, cwSoftLimit,
            ccwSoftLimit, softLimitMode, isRotationStage);
    }
    
    private dynamic MOT_MOVE_HOMED(byte[] inputData)
    {
        // Extract parameters and package into named tuple
        byte chanIdent = inputData[2];
        bool homeCompleted = true;
        return (chanIdent, homeCompleted);
    }
    
    // Protocol suggests that MOT_MOVE_COMPLETED is followed by a 14 byte long 'status update' message, but does not detail this.
    // The example MOTSTATUS does not appear later in the document. Is it correct to use the format outlined for 'MOT_GET_USTATUSUPDATE'?
    // Why does MOT_MOVE_COMPLETED not follow the same header structure as ALL the other messages which are followed by a data packet?!
    // This means I can't reliably use bytes 2 & 3 to indicate data packet length.
    // In 'MOT_GET_USTATUSUPDATE' velocity and motor current are described as 'words' (unsigned 16-bit integer) but the example has signs
    private dynamic MOT_MOVE_COMPLETED(byte[] inputData)
    {
        bool moveCompleted = true;
        
        // Send data packet to MOT_GET_USTATUSUPDATE to extract parameters
        var statusBools = MOT_GET_USTATUSUPDATE(inputData);
        
        // Package parameters into a named tuple and return to calling method
        return (moveCompleted, statusBools);
    }
    
    private dynamic MOT_MOVE_STOPPED(byte[] inputData)
    {
        bool stopCompleted = true;
        
        // Send data packet to MOT_GET_USTATUSUPDATE to extract parameters
        var statusBools = MOT_GET_USTATUSUPDATE(inputData);
        
        // Package parameters into a named tuple and return to calling method
        return (stopCompleted, statusBools);
    }
    
    private dynamic MOT_GET_DCPIDPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int proportional = BitConverter.ToInt32(inputData, 2);
        int integral = BitConverter.ToInt32(inputData, 6);
        int derivative = BitConverter.ToInt32(inputData, 10);
        int integralLimit = BitConverter.ToInt32(inputData, 14);
        short filterControl = BitConverter.ToInt16(inputData, 18);
        
        // Read 'filter control' to see which parameters are applied
        bool applyProportional = (filterControl & 0x01) != 0;
        bool applyIntegral = (filterControl & 0x02) != 0;
        bool applyDerivative = (filterControl & 0x04) != 0;
        bool applyIntegralLimit = (filterControl & 0x08) != 0;
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, applyProportional, proportional, applyIntegral, integral, applyDerivative, derivative,
            applyIntegralLimit, integralLimit);
    }

    private dynamic MOT_GET_AVMODES(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short modeBits = BitConverter.ToInt16(inputData, 2);
        
        // Read 'modeBits' to see which modes are applied
        bool LEDMODE_IDENT = (modeBits & 0x01) != 0;
        bool LEDMODE_LIMITSWITCH = (modeBits & 0x01) != 0;
        bool LEDMODE_MOVING = (modeBits & 0x01) != 0;
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, LEDMODE_IDENT, LEDMODE_LIMITSWITCH, LEDMODE_MOVING);
    }

    private dynamic MOT_GET_POTPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short zeroWnd = BitConverter.ToInt16(inputData, 2);
        int vel1 = BitConverter.ToInt32(inputData, 4);
        short wnd1 = BitConverter.ToInt16(inputData, 8);
        int vel2 = BitConverter.ToInt32(inputData, 10);
        short wnd2 = BitConverter.ToInt16(inputData, 14);
        int vel3 = BitConverter.ToInt32(inputData, 16);
        short wnd3 = BitConverter.ToInt16(inputData, 20);
        int vel4 = BitConverter.ToInt32(inputData, 22);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, zeroWnd, vel1, wnd1, vel2, wnd2, vel3, wnd3, vel4);
    }

    private dynamic MOT_GET_BUTTONPARAMS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        short mode = BitConverter.ToInt16(inputData, 2);
        int position1 = BitConverter.ToInt32(inputData, 4);
        int position2 = BitConverter.ToInt32(inputData, 8);
        short timeOut1 = BitConverter.ToInt16(inputData, 12);
        short timeOut2 = BitConverter.ToInt16(inputData, 14);
        
        // Check parameters are in expected range
        if (mode != 0x01 && mode != 0x02)
        {
            throw new ArgumentOutOfRangeException(nameof(inputData), $"{MethodBase.GetCurrentMethod()?.Name} encountered an unexpected 'mode'. Must be 0x01 (jog) or 0x02 (absolute position).");
        }
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, mode, position1, position2, timeOut1, timeOut2);
    }
    
    private dynamic MOT_GET_USTATUSUPDATE(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int position = BitConverter.ToInt32(inputData, 2);
        short velocity = BitConverter.ToInt16(inputData, 6);
        short motorCurrent = BitConverter.ToInt16(inputData, 8);
        int statusBits = BitConverter.ToInt32(inputData, 10);
        
        // Extract booleans from 'status bits'
        var statusBools = StatusBitsToBools(statusBits);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, position, velocity, motorCurrent, statusBools);
    }
    
    private dynamic MOT_GET_STATUSBITS(byte[] inputData)
    {
        // Extract parameters from the data packet
        short chanIdent = BitConverter.ToInt16(inputData, 0);
        int statusBits = BitConverter.ToInt32(inputData, 2);
        
        // Extract booleans from 'status bits'
        var statusBools = StatusBitsToBools(statusBits);
        
        // Package parameters into a named tuple and return to calling method
        return (chanIdent, statusBools);
    }
    
    private dynamic StatusBitsToBools(int statusBits)
    {
        bool P_MOT_SB_CWHARDLIMIT = (statusBits & 0x00000001) != 0;
        bool P_MOT_SB_CCWHARDLIMIT = (statusBits & 0x00000002) != 0;
        bool P_MOT_SB_CWSOFTLIMIT = (statusBits & 0x00000004) != 0;
        bool P_MOT_SB_CCWSOFTLIMIT = (statusBits & 0x00000008) != 0;
        bool P_MOT_SB_INMOTIONCW = (statusBits & 0x00000010) != 0;
        bool P_MOT_SB_INMOTIONCCW = (statusBits & 0x00000020) != 0;
        bool P_MOT_SB_JOGGINGCW = (statusBits & 0x00000040) != 0;
        bool P_MOT_SB_JOGGINGCCW = (statusBits & 0x00000080) != 0;
        bool P_MOT_SB_CONNECTED = (statusBits & 0x00000100) != 0;
        bool P_MOT_SB_HOMING = (statusBits & 0x00000200) != 0;
        bool P_MOT_SB_HOMED = (statusBits & 0x00000400) != 0;
        bool P_MOT_SB_INITILIZING = (statusBits & 0x00000800) != 0;
        bool P_MOT_SB_TRACKING = (statusBits & 0x00001000) != 0;
        bool P_MOT_SB_SETTLED = (statusBits & 0x00002000) != 0;
        bool P_MOT_SB_POSITIONERROR = (statusBits & 0x00004000) != 0;
        bool P_MOT_SB_INSTRERROR = (statusBits & 0x00008000) != 0;
        bool P_MOT_SB_INTERLOCK = (statusBits & 0x00010000) != 0;
        bool P_MOT_SB_OVERTEMP = (statusBits & 0x00020000) != 0;
        bool P_MOT_SB_BUSVOLTFAULT = (statusBits & 0x00040000) != 0;
        bool P_MOT_SB_COMMUTATIONERROR = (statusBits & 0x00080000) != 0;
        bool P_MOT_SB_DIGIP1 = (statusBits & 0x00100000) != 0;
        bool P_MOT_SB_DIGIP2 = (statusBits & 0x00200000) != 0;
        bool P_MOT_SB_DIGIP3 = (statusBits & 0x00400000) != 0;
        bool P_MOT_SB_DIGIP4 = (statusBits & 0x00800000) != 0;
        bool P_MOT_SB_OVERLOAD = (statusBits & 0x01000000) != 0;
        bool P_MOT_SB_ENCODERFAULT = (statusBits & 0x02000000) != 0;
        bool P_MOT_SB_OVERCURRENT = (statusBits & 0x04000000) != 0;
        bool P_MOT_SB_BUSCURRENTFAULT = (statusBits & 0x08000000) != 0;
        bool P_MOT_SB_POWEROK = (statusBits & 0x10000000) != 0;
        bool P_MOT_SB_ACTIVE = (statusBits & 0x20000000) != 0;
        bool P_MOT_SB_ERROR = (statusBits & 0x40000000) != 0;
        bool P_MOT_SB_ENABLED = (statusBits & 0x80000000) != 0;
        
        // Package parameters into a named tuple and return to calling method
        return (P_MOT_SB_CWHARDLIMIT, P_MOT_SB_CCWHARDLIMIT, P_MOT_SB_CWSOFTLIMIT, P_MOT_SB_CCWSOFTLIMIT,
            P_MOT_SB_INMOTIONCW, P_MOT_SB_INMOTIONCCW, P_MOT_SB_JOGGINGCW, P_MOT_SB_JOGGINGCCW, P_MOT_SB_CONNECTED,
            P_MOT_SB_HOMING, P_MOT_SB_HOMED, P_MOT_SB_INITILIZING, P_MOT_SB_TRACKING, P_MOT_SB_SETTLED,
            P_MOT_SB_POSITIONERROR, P_MOT_SB_INSTRERROR, P_MOT_SB_INTERLOCK, P_MOT_SB_OVERTEMP, P_MOT_SB_BUSVOLTFAULT,
            P_MOT_SB_COMMUTATIONERROR, P_MOT_SB_DIGIP1, P_MOT_SB_DIGIP2, P_MOT_SB_DIGIP3, P_MOT_SB_DIGIP4,
            P_MOT_SB_OVERLOAD, P_MOT_SB_ENCODERFAULT, P_MOT_SB_OVERCURRENT, P_MOT_SB_BUSCURRENTFAULT, P_MOT_SB_POWEROK,
            P_MOT_SB_ACTIVE, P_MOT_SB_ERROR, P_MOT_SB_ENABLED);
    }
}
