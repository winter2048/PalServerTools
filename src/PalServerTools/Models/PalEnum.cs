namespace PalServerTools.Models
{
    public class PalEnum
    {
        public enum PalServerState
        {
            Running,
            Stopped
        }

        public enum PalServerUpdateState
        {
            None,
            Updating,
            Success,
            Failed
        }
    }
}
