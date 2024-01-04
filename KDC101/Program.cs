
public class KDC101
{
    private readonly object _locker = new();
    private readonly Queue<byte> _messageQueueObject = new();
    private readonly Dictionary<int, TaskCompletionSource<byte[]>> _pendingResponses = new();
    private readonly Incoming.MessageUnpacker _messageUnpackerObject = new();
    private readonly USB_Communication.UsbPeripheralChipFT232BM _usbChipObject = new();
    
    public KDC101() // Constructor method
    {
        Task.Run(ProcessMessages);
    }

    private async Task ProcessMessages()
    {
        while (true)
        {
            if (_messageQueueObject.Count > 0)
            {
                // Create a new instance of the 'MessageUnpacker' class. This causes the constructor method to be executed.
                Incoming.MessageUnpacker messageUnpackerObject = new();
                
                dynamic unpackedMessage = messageUnpackerObject.UnpackMessage(_messageQueueObject);
                
                // Extract parameters from the named tuple returned by 'UnpackMessage'
                int msgId = unpackedMessage.msgId;
                dynamic msgData = unpackedMessage.msgData;
                
                lock (_locker)
                {
                    if (_pendingResponses.TryGetValue(msgId, out TaskCompletionSource<byte[]>? tcs))
                    {
                        tcs.SetResult(msgData);
                        _pendingResponses.Remove(msgId);
                    }
                }
            }
            else
            {
                await Task.Delay(100); // or any other delay
            }
        }
    }
    
    public async Task MoveTo(int absolutePosition, int dest, int source, int chanIdent)
    {
        // TODO check motor is homed before initiating a move. Trigger homing method if not homed.
        
        var tcs = new TaskCompletionSource<byte[]>();
        lock (_locker)
        {
            _pendingResponses[0x0464] = tcs; // add to the dictionary
        }
        
        // Get packed command
        byte[] commandPacket = Outgoing.MessageBuilder.MOT_MOVE_ABSOLUTE(dest, source, chanIdent, absolutePosition);
        
        // Send packed command to FTDI driver. For example 'ftSend(commandPacket)';
        
        // Wait for the completion message
        var result = await tcs.Task;
        
        // Extract the response from the result
        var unpackedResult = _messageUnpackerObject.MOT_MOVE_COMPLETED(result);
        
        // Handle the unpacked result ..
    }
}