using MultiplayerProject.Source;

namespace MultiplayerProject.Source.Networking.Client
{
    /// <summary>
    /// Proxy for Client that checks for cheating before sending packets.
    /// </summary>
    public class ClientProxy
    {
        private readonly MultiplayerProject.Client _realClient;
        private readonly IPlayer _localPlayer;

        public ClientProxy(MultiplayerProject.Client realClient, IPlayer localPlayer)
        {
            _realClient = realClient;
            _localPlayer = localPlayer;
        }

        public void SendMessageToServer(BasePacket packet, MessageType type)
        {
            if (IsCheating(packet))
            {
                Logger.Instance?.Warning($"[Proxy] Cheating detected! Packet blocked: {type} ({packet.GetType().Name})");
                return;
            }

            _realClient.SendMessageToServer(packet, type);
        }

        private bool IsCheating(BasePacket packet)
        {
            
            // Example: Check for impossible speed or position in PlayerUpdatePacket
            if (packet is PlayerUpdatePacket p)
            {
                if (p.Speed > Application.PLAYER_MAX_SPEED * 1.5f || p.Speed < -Application.PLAYER_MAX_SPEED * 1.5f)
                    return true;
                if (p.XPosition < 0 || p.XPosition > Application.WINDOW_WIDTH ||
                    p.YPosition < 0 || p.YPosition > Application.WINDOW_HEIGHT)
                    return true;
            }

            // Example: Check for rapid fire cheating (fire rate too high)
            if (_localPlayer != null && _localPlayer.GetFireRateMultiplier() > 3.0f)
                return true;
                

            return false;
        }
    }
}