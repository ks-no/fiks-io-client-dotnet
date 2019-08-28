using System;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Asic;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using Moq;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Client.Tests.Send
{
    internal class SvarSenderFixture
    {
        private MottattMelding _mottattMelding;

        private Action _ack;

        public SvarSenderFixture()
        {
            SendHandlerMock = new Mock<ISendHandler>();
            _mottattMelding = null;
            _ack = Mock.Of<Action>();
        }

        public SvarSenderFixture WithMottattMelding(MottattMelding mottattMelding)
        {
            _mottattMelding = mottattMelding;
            return this;
        }

        public SvarSenderFixture WithAck(Action ack)
        {
            _ack = ack;
            return this;
        }

        public Mock<ISendHandler> SendHandlerMock { get; }

        public Func<Task<Stream>> DefaultStreamProvider => Mock.Of<Func<Task<Stream>>>();

        public IAsicDecrypter DefaultDecrypter => Mock.Of<IAsicDecrypter>();

        public IFileWriter DefaultFileWriter => Mock.Of<IFileWriter>();

        internal SvarSender CreateSut()
        {
            SetupMocks();
            return new SvarSender(SendHandlerMock.Object, _mottattMelding, _ack);
        }

        private void SetupMocks()
        {
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MeldingRequest>(), It.IsAny<IPayload[]>()))
                           .ReturnsAsync(new SendtMelding(Guid.NewGuid(), "sendtMelding", Guid.NewGuid(), Guid.NewGuid(), TimeSpan.Zero));
        }
    }
}