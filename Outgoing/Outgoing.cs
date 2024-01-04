namespace Outgoing;

public abstract class MessageBuilder
{
    public static byte[] Pack(int msgid, int dest, int source, int param1 = 0, int param2 = 0, byte[]? dataPacket = null)
    {
        if (dataPacket != null)
        {
            if (param1 != 0 || param2 != 0)
                throw new ArgumentException("param1 and param2 must be zero when data is not null");

            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((short)msgid));
            buffer.AddRange(BitConverter.GetBytes((short)dataPacket.Length));
            buffer.Add((byte)(dest | 0x80));
            buffer.Add((byte)source);
            buffer.AddRange(dataPacket);
            return buffer.ToArray();
        }
        else
        {
            List<byte> buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes((short)msgid));
            buffer.Add((byte)param1);
            buffer.Add((byte)param2);
            buffer.Add((byte)dest);
            buffer.Add((byte)source);
            return buffer.ToArray();
        }
    }
    
    public byte[] MOD_IDENTIFY(int dest, int source, int chanIdent)
    {
        return Pack(0x0223, dest, source, chanIdent);
    }
    
    public byte[] MOD_SET_CHANENABLESTATE(int dest, int source, int chanIdent, int enableState)
    {
        return Pack(0x0210, dest, source, chanIdent, enableState);
    }
    
    public byte[] MOD_REQ_CHANENABLESTATE(int dest, int source, int chanIdent)
    {
        return Pack(0x0211, dest, source, chanIdent);
    }
    
    public byte[] HW_DISCONNECT(int dest, int source)
    {
        return Pack(0x0002, dest, source);
    }
    
    public byte[] HW_START_UPDATEMSGS(int dest, int source)
    {
        return Pack(0x0011, dest, source);
    }
    
    public byte[] HW_STOP_UPDATEMSGS(int dest, int source)
    {
        return Pack(0x0012, dest, source);
    }
    
    public byte[] HW_REQ_INFO(int dest, int source)
    {
        return Pack(0x0005, dest, source);
    }
    
    public byte[] HUB_REQ_BAYUSED(int dest, int source)
    {
        return Pack(0x0065, dest, source);
    }
    
    public byte[] MOT_SET_POSCOUNTER(int dest, int source, int chanIdent, int position)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(position));
        return Pack(0x0410, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_POSCOUNTER(int dest, int source, int chanIdent)
    {
        return Pack(0x0411, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_ENCCOUNTER(int dest, int source, int chanIdent, int encoderCount)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(encoderCount));
        return Pack(0x0409, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_ENCCOUNTER(int dest, int source, int chanIdent)
    {
        return Pack(0x040A, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_VELPARAMS(int dest, int source, int chanIdent, int minVelocity, int acceleration,
        int maxVelocity)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(minVelocity));
        data.AddRange(BitConverter.GetBytes(acceleration));
        data.AddRange(BitConverter.GetBytes(maxVelocity));
        return Pack(0x0413, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_VELPARAMS(int dest, int source)
    {
        return Pack(0x0414, dest, source);
    }

    public byte[] MOT_SET_JOGPARAMS(int dest, int source, int chanIdent, int jogMode, int stepSize, int minVelocity,
        int acceleration, int maxVelocity, int stopMode)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)jogMode));
        data.AddRange(BitConverter.GetBytes(stepSize));
        data.AddRange(BitConverter.GetBytes(minVelocity));
        data.AddRange(BitConverter.GetBytes(acceleration));
        data.AddRange(BitConverter.GetBytes(maxVelocity));
        data.AddRange(BitConverter.GetBytes((ushort)stopMode));
        return Pack(0x0416, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_JOGPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0417, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_GENMOVEPARAMS(int dest, int source, int chanIdent, int backlashDistance)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(backlashDistance));
        return Pack(0x043A, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_GENMOVEPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x043B, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_MOVERELPARAMS(int dest, int source, int chanIdent, int relativeDistance)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(relativeDistance));
        return Pack(0x0445, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_MOVERELPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0446, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_MOVEABSPARAMS(int dest, int source, int chanIdent, int absolutePosition)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(absolutePosition));
        return Pack(0x0450, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_MOVEABSPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0451, dest, source, chanIdent);
    }
    
    // The protocol example shows homeDirection and limitSwitch are not used. Should a 0 default value be applied?
    public byte[] MOT_SET_HOMEPARAMS(int dest, int source, int chanIdent, int homeVelocity, int offsetDistance = 0,
        int homeDirection = 0, int limitSwitch = 0)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)homeDirection));
        data.AddRange(BitConverter.GetBytes((ushort)limitSwitch));
        data.AddRange(BitConverter.GetBytes(homeVelocity));
        data.AddRange(BitConverter.GetBytes(offsetDistance));
        return Pack(0x0440, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_HOMEPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0441, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_LIMSWITCHPARAMS(int dest, int source, int chanIdent, int cwHardlimit, int ccwHardlimit,
        int cwSoftlimit, int ccwSoftlimit, int softLimitMode)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)cwHardlimit));
        data.AddRange(BitConverter.GetBytes((ushort)ccwHardlimit));
        data.AddRange(BitConverter.GetBytes(cwSoftlimit));
        data.AddRange(BitConverter.GetBytes(ccwSoftlimit));
        data.AddRange(BitConverter.GetBytes((ushort)softLimitMode));
        return Pack(0x0423, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_LIMSWITCHPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0424, dest, source, chanIdent);
    }
    
    public byte[] MOT_MOVE_HOME(int dest, int source, int chanIdent)
    {
        return Pack(0x0443, dest, source, chanIdent);
    }
    
    public byte[] MOT_MOVE_RELATIVE(int dest, int source, int chanIdent, int? relativeDistance = null)
    {
        if (relativeDistance.HasValue)
        {
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
            data.AddRange(
                BitConverter.GetBytes(relativeDistance
                    .Value)); // If we used 'relativeDistance' directly, even though it's known to not be null at this position in the code, it would still be treated as a nullable int, which is not the desired type for the BitConverter.GetBytes method call.
            return Pack(0x0448, dest, source, dataPacket: data.ToArray());
        }
        else
        {
            return Pack(0x0448, dest, source, chanIdent);
        }
    }
    
    public static byte[] MOT_MOVE_ABSOLUTE(int dest, int source, int chanIdent, int? absolutePosition = null)
    {
        if (absolutePosition.HasValue)
        {
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
            data.AddRange(BitConverter.GetBytes(absolutePosition.Value)); // If we used 'relativeDistance' directly, even though it's known to not be null at this position in the code, it would still be treated as a nullable int, which is not the desired type for the BitConverter.GetBytes method call.
            return Pack(0x0453, dest, source, dataPacket: data.ToArray());
        }
        else
        {
            return Pack(0x0453, dest, source, chanIdent);
        }
    }
    
    public byte[] MOT_MOVE_JOG(int dest, int source, int chanIdent, int direction)
    {
        if (direction != 0x01 && direction != 0x02)
        {
            throw new ArgumentException(
                "Invalid direction. MOT_MOVE_JOG 'direction' must be either 0x01 (forwards) or 0x02 (backwards). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        return Pack(0x046A, dest, source, chanIdent, direction);
    }
    
    public byte[] MOT_MOVE_VELOCITY(int dest, int source, int chanIdent, int direction)
    {
        if (direction != 0x01 && direction != 0x02)
        {
            throw new ArgumentException(
                "Invalid direction. MOT_MOVE_VELOCITY 'direction' must be either 0x01 (forwards) or 0x02 (backwards). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        return Pack(0x0457, dest, source, chanIdent, direction);
    }
    
    public byte[] MOT_MOVE_STOP(int dest, int source, int chanIdent, int stopMode)
    {
        if (stopMode != 0x01 && stopMode != 0x02)
        {
            throw new ArgumentException(
                "Invalid stop mode. MOT_MOVE_STOP 'stopMode' must be either 0x01 (abrupt) or 0x02 (profiled). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        return Pack(0x0465, dest, source, chanIdent, stopMode);
    }
    
    public byte[] MOT_SET_DCPIDPARAMS(int dest, int source, int chanIdent, int proportional = 0, int integral = 0,
        int differential = 0, int integralLimit = 0)
    {
        ushort filterControl = 0;

        if (proportional > 0)
        {
            filterControl |= 1;
        }

        if (integral > 0)
        {
            filterControl |= 2;
        }

        if (differential > 0)
        {
            filterControl |= 4;
        }

        if (integralLimit > 0)
        {
            filterControl |= 8;
        }

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(proportional));
        data.AddRange(BitConverter.GetBytes(integral));
        data.AddRange(BitConverter.GetBytes(differential));
        data.AddRange(BitConverter.GetBytes(integralLimit));
        data.AddRange(BitConverter.GetBytes(filterControl));

        return Pack(0x04A0, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_DCPIDPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x04A1, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_AVMODES(int dest, int source, int chanIdent, bool ledModeIdent = false,
        bool ledModeLimitSwitch = false, bool ledModeMoving = false)
    {
        ushort modeBits = 0;

        if (ledModeIdent)
        {
            modeBits += 1;
        }

        if (ledModeLimitSwitch)
        {
            modeBits += 2;
        }

        if (ledModeMoving)
        {
            modeBits += 8;
        }

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(modeBits));
        return Pack(0x04B3, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_AVMODES(int dest, int source, int chanIdent)
    {
        return Pack(0x04B4, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_POTPARAMS(int dest, int source, int chanIdent, int zeroWnd, int vel1, int wnd1, int vel2,
        int wnd2, int vel3, int wnd3, int vel4)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)zeroWnd));
        data.AddRange(BitConverter.GetBytes(vel1));
        data.AddRange(BitConverter.GetBytes((ushort)wnd1));
        data.AddRange(BitConverter.GetBytes(vel2));
        data.AddRange(BitConverter.GetBytes((ushort)wnd2));
        data.AddRange(BitConverter.GetBytes(vel3));
        data.AddRange(BitConverter.GetBytes((ushort)wnd3));
        data.AddRange(BitConverter.GetBytes(vel4));
        return Pack(0x04B0, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_POTPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x04B1, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_BUTTONPARAMS(int dest, int source, int chanIdent, int mode, int position1 = 0,
        int position2 = 0, int timeout1 = 2000, int timeout2 = 2000)
    {
        if (mode == 0x01)
        {
            if (position1 > 0 || position2 > 0)
            {
                throw new ArgumentException(
                    "Invalid position. MOT_SET_BUTTONPARAMS 'position1' and 'position2' must both be 0 if 'mode' is set to 0x01 (jog). See Thorlabs APT Communications Protocol (Issue 37) for more details");
            }
        }

        if (mode != 0x01 && mode != 0x02)
        {
            throw new ArgumentException(
                "Invalid mode. MOT_SET_BUTTONPARAMS 'mode' must be either 0x01 (jog) or 0x02 (go to position). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)mode));
        data.AddRange(BitConverter.GetBytes(position1));
        data.AddRange(BitConverter.GetBytes(position2));
        data.AddRange(BitConverter.GetBytes((ushort)timeout1));
        data.AddRange(BitConverter.GetBytes((ushort)timeout2));
        return Pack(0x04B6, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_BUTTONPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x04B7, dest, source, chanIdent);
    }
    
    public byte[] MOT_SET_EEPROMPARAMS(int dest, int source, int chanIdent, int msgidParam)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)msgidParam));
        return Pack(0x04B9, dest, source, dataPacket: data.ToArray());
    }
    
    // 'Messages Applicable to TDC001 and KDC101' lists MGMSG_MOT_REQ_DCSTATUSUPDATE (msgid = 0x0490) which should be found on page 130.
    // The actual function listed on page 130 is 'MGMSG_MOT_REQ_USTATUSUPDATE' (msgid = 0x0490)
    public byte[] MOT_REQ_USTATUSUPDATE(int dest, int source, int chanIdent)
    {
        return Pack(0x0490, dest, source, chanIdent);
    }
    
    // If using the USB port, this message called “server alive” must be sent by the server to the controller at least once a second
    public byte[] MOT_ACK_USTATUSUPDATE(int dest, int source)
    {
        return Pack(0x0492, dest, source);
    }
    
    public byte[] MOT_REQ_STATUSBITS(int dest, int source, int chanIdent)
    {
        return Pack(0x0429, dest, source, chanIdent);
    }
    
    public byte[] MOT_SUSPEND_ENDOFMOVEMSGS(int dest, int source)
    {
        return Pack(0x046B, dest, source);
    }
    
    public byte[] MOT_RESUME_ENDOFMOVEMSGS(int dest, int source)
    {
        return Pack(0x046C, dest, source);
    }
    
    // Should presetPos1 and presetPos2 be set to 0 when not in 'Go to Position' mode?
    // Should PresetPos3 and wJSSensitivity and wReserved be included?
    public byte[] MOT_SET_KCUBEMMIPARAMS(int dest, int source, int chanIdent, int jsMode, int jsMaxVel, int jsAccn,
        int dirSense, int presetPos1, int presetPos2, int dispBrightness, int dispTimeout, int dispDimLevel)
    {
        if (jsMode < 0x03)
        {
            if (presetPos1 > 0 || presetPos2 > 0)
            {
                throw new ArgumentException(
                    "Invalid position. MOT_SET_KCUBEMMIPARAMS 'presetPos1' and 'presetPos2' must both be 0 if 'jsMode' is set to 0x01 (velocity control) or 0x02 (jog). See Thorlabs APT Communications Protocol (Issue 37) for more details");
            }
        }

        if (jsMode != 0x01 && jsMode != 0x02)
        {
            throw new ArgumentException(
                "Invalid mode. MOT_SET_KCUBEMMIPARAMS 'jsMode' must be either 0x01 (velocity control) or 0x02 (jog) or 0x03 (go to position). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        if (dirSense < 0 || dirSense > 2)
        {
            throw new ArgumentException(
                "Invalid velocity wheel direction. MOT_SET_KCUBEMMIPARAMS 'dirSense' must be either 0x00 (wheel disabled) or 0x01 (positive) or 0x02 (negative). See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        if (dispBrightness < 0 || dispBrightness > 100)
        {
            throw new ArgumentException(
                "Invalid display brightness. MOT_SET_KCUBEMMIPARAMS 'dispBrightness' must be in the range 0 (off) to 100 (brightest) inclusive. See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        if (dispTimeout < 0 || dispTimeout > 480)
        {
            throw new ArgumentException(
                "Invalid display timeout. MOT_SET_KCUBEMMIPARAMS 'dispTimeout' must be in the range 0 (never) to 480 minutes (8 hours) inclusive. See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        if (dispDimLevel < 0 || dispDimLevel > 10)
        {
            throw new ArgumentException(
                "Invalid display dim level. MOT_SET_KCUBEMMIPARAMS 'dispDimLevel' must be in the range 0 (Off) to 10 (brightest) inclusive. See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)jsMode));
        data.AddRange(BitConverter.GetBytes(jsMaxVel));
        data.AddRange(BitConverter.GetBytes(jsAccn));
        data.AddRange(BitConverter.GetBytes((ushort)dirSense));
        data.AddRange(BitConverter.GetBytes(presetPos1));
        data.AddRange(BitConverter.GetBytes(presetPos2));
        data.AddRange(BitConverter.GetBytes((ushort)dispBrightness));
        data.AddRange(BitConverter.GetBytes((ushort)dispTimeout));
        data.AddRange(BitConverter.GetBytes((ushort)dispDimLevel));
        return Pack(0x0520, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_KCUBEMMIPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0521, dest, source, chanIdent);
    }
    
    // Should the 'Reserved' section described in the protocol be a ulong of zeros?
    // What are the accepted values for TrigPolarity?
    public byte[] MOT_SET_KCUBETRIGIOCONFIG(int dest, int source, int chanIdent, int trig1Mode, int trig1Polarity,
        int trig2Mode, int trig2Polarity)
    {
        // Valid values for trig1Mode and trig2Mode
        int[] validTrigModeValues = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

        if (!validTrigModeValues.Contains(trig1Mode) || !validTrigModeValues.Contains(trig2Mode))
        {
            throw new ArgumentException(
                "Invalid trigger mode. MOT_SET_KCUBETRIGIOCONFIG 'trig1Mode' and 'trig2Mode' must be one of the following: 0x00, 0x01, 0x02, 0x03, 0x04, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F. See Thorlabs APT Communications Protocol (Issue 37) for more details");
        }

        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes((ushort)trig1Mode));
        data.AddRange(BitConverter.GetBytes((ushort)trig1Polarity));
        data.AddRange(BitConverter.GetBytes((ushort)trig2Mode));
        data.AddRange(BitConverter.GetBytes((ushort)trig2Polarity));
        data.AddRange(BitConverter.GetBytes((ulong)0));
        return Pack(0x0523, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_KCUBETRIGCONFIG(int dest, int source, int chanIdent)
    {
        return Pack(0x0524, dest, source, param1: chanIdent);
    }
    
    public byte[] MOT_SET_KCUBEPOSTRIGPARAMS(int dest, int source, int chanIdent, int startPosFwd, int intervalFwd,
        int numPulsesFwd, int startPosRev, int intervalRev, int numPulsesRev, int pulseWidth, int numCycles)
    {
        var data = new List<byte>();
        data.AddRange(BitConverter.GetBytes((ushort)chanIdent));
        data.AddRange(BitConverter.GetBytes(startPosFwd));
        data.AddRange(BitConverter.GetBytes(intervalFwd));
        data.AddRange(BitConverter.GetBytes(numPulsesFwd));
        data.AddRange(BitConverter.GetBytes(startPosRev));
        data.AddRange(BitConverter.GetBytes(intervalRev));
        data.AddRange(BitConverter.GetBytes(numPulsesRev));
        data.AddRange(BitConverter.GetBytes(pulseWidth));
        data.AddRange(BitConverter.GetBytes(numCycles));
        return Pack(0x0526, dest, source, dataPacket: data.ToArray());
    }
    
    public byte[] MOT_REQ_KCUBEPOSTRIGPARAMS(int dest, int source, int chanIdent)
    {
        return Pack(0x0527, dest, source, param1: chanIdent);
    }
}
