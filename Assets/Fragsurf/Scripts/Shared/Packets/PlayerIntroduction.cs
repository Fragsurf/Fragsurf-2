using Lidgren.Network;

namespace Fragsurf.Shared.Packets
{
    public class PlayerIntroduction : IBasePacket
    {
        public enum JoinStep
        {
            None,
            PendingFileSync,
            LoadingIntoGame,
            GameLoaded,
            Introduce
        }

        public JoinStep Step;

        public SendCategory Sc => SendCategory.UI_Important;
		public bool DisableAutoPool => false;
        public int ByteSize => 1;

        public PlayerIntroduction() { }
        public PlayerIntroduction(JoinStep step)
        {
            Step = step;
        }

        public void Reset()
        {
            Step = JoinStep.None;
        }

        public void Read(NetBuffer buffer)
        {
            Step = (JoinStep)buffer.ReadByte();
        }

        public void Write(NetBuffer buffer)
        {
            buffer.Write((byte)Step);
        }

    }
}
