namespace TomLonghurst.AsyncRedisClient.Constants;

public class LastActionConstants
{
    internal static readonly string Connecting = "Connecting";
    internal static readonly string Reconnecting = "Reconnecting";
    internal static readonly string Authorizing = "Authorizing";
    internal static readonly string SelectingDatabase = "Selecting Database";
    internal static readonly string SettingClientName = "Setting Client Name";
        
    internal static readonly string WaitingForConnectingLock = "Waiting for Connecting lock to be free";
    internal static readonly string AuthenticatingSSLStreamAsClient = "Authenticating SSL Stream as Client";
    internal static readonly string CreatingSSLStreamPipe = "Creating SSL Stream Pipe";
    internal static readonly string CreatingSocketPipe = "Creating Socket Pipe";
        
    internal static readonly string WritingBytes = "Writing Bytes";
        
    internal static readonly string ThrowingCancelledException = "Throwing Cancelled Exception due to Cancelled Token";
    internal static readonly string ReadingDataInReadData = "Reading Data in ReadData";
    internal static readonly string ReadingDataInReadDataLoop = "Reading Data in ReadData Loop";
    internal static readonly string AdvancingBufferInReadDataLoop = "Advancing Buffer in ReadData Loop";
    internal static readonly string FindingEndOfLinePosition = "Finding End of Line Position";
    internal static readonly string ReadingUntilEndOfLinePositionFound = "Reading until End of Line found";
    internal static readonly string StartingResultProcessor = "Starting ResultProcessor.Processor";
        
    internal static readonly string DisposingClient = "Disposing Client";
    internal static readonly string DisposingNetwork = "Disposing Network";
}