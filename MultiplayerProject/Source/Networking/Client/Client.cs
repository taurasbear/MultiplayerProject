using MultiplayerProject.Source;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using ProtoBuf;
using MultiplayerProject.Source.Networking.Client.Pipeline;

namespace MultiplayerProject
{
    class NotConnectedException : Exception
    {
        public NotConnectedException() : base("TcpClient not connected.")
        { }

        public NotConnectedException(string message) : base(message)
        { }
    }

    public class Client
    {
        public static event EmptyDelegate OnServerForcedDisconnect;
        public static event BasePacketDelegate OnLoadNewGame;
        public static event BasePacketDelegate OnGameOver;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private Thread _thread;

        private IScene _currentScene;

        public Client()
        {
            _tcpClient = new TcpClient();
        }

        public bool Connect(string hostname, int port)
        {
            try
            {
                _tcpClient.Connect(hostname, port);
                _stream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_stream, Encoding.UTF8);
                _reader = new BinaryReader(_stream, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        public void Run()
        {
            if (!_tcpClient.Connected)
                throw new NotConnectedException();

            try
            {
                // Start listening for messages from the server
                _thread = new Thread(new ThreadStart(ProcessServerResponse));
                _thread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Error: " + e.Message);
            }
        }

        public void Stop()
        {
            if (_thread.IsAlive)
                _thread.Abort();
        }

        public void SetCurrentScene(IScene scene)
        {
            _currentScene = scene;
        }

        private void CleanUp()
        {
            _tcpClient.Close();
        }

        public void SendMessageToServer(BasePacket packet, MessageType type)
        {
            // Check if the client is still connected before sending
            if (!_tcpClient.Connected || _stream == null || !_stream.CanWrite)
            {
                Console.WriteLine("Cannot send message: Connection is closed");
                return;
            }

            try
            {
                packet.SendDate = DateTime.UtcNow;
                packet.MessageType = (int)type;

                Serializer.SerializeWithLengthPrefix(_writer.BaseStream, packet, PrefixStyle.Base128);
                _writer.Flush();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failed to send message to server: " + ex.Message);
                // Connection was lost, cleanup will be handled by ProcessServerResponse
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error sending message: " + ex.Message);
            }
        }

        private void ProcessServerResponse()
        {
            var finalHandler = new FinalHandler();

            var loggingHandler = new LoggingHandler();
            loggingHandler.SetNext(finalHandler);

            var validationHandler = new ValidationHandler();
            validationHandler.SetNext(loggingHandler);

            var deserializeHandler = new DeserializeHandler();
            deserializeHandler.SetNext(validationHandler);

            var exceptionHandler = new ExceptionHandler();
            exceptionHandler.SetNext(deserializeHandler);

            var requestContext = new RequestContext 
            {
                Client = this
            };

            try
            {
                using (_stream)
                {
                    while (true)
                    {
                        requestContext.RawStream = _stream;
                        exceptionHandler.Handle(requestContext);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured: " + e.Message);
            }
            finally
            {
                CleanUp();
            }
        }

        public void ProcessServerPacket(BasePacket packet)
        {
            switch ((MessageType)packet.MessageType)
            {
                case MessageType.Server_Disconnect:
                    OnServerForcedDisconnect();
                    break;

                case MessageType.GI_ServerSend_LoadNewGame:
                    {
                        OnLoadNewGame(packet);
                        break;
                    }

                case MessageType.GI_ServerSend_GameOver:
                    {
                        OnGameOver(packet);
                        break;
                    }

                default:
                    {
                        // Let the current scene handle the message
                        if (_currentScene != null)
                            _currentScene.RecieveServerResponse(packet);
                        break;

                    }
            }
        }

        public static void RaiseServerForcedDisconnect()
        {
            OnServerForcedDisconnect?.Invoke();
        }

        public static void RaiseLoadNewGame(BasePacket packet)
        {
            OnLoadNewGame?.Invoke(packet);
        }

        public static void RaiseGameOver(BasePacket packet)
        {
            OnGameOver?.Invoke(packet);
        }
    }
}
