using System;
using System.IO;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using KS.Fiks.IO.Client.Amqp;
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
        private Action _nack;
        private Action _nackWithRequeue;

        public SvarSenderFixture()
        {
            SendHandlerMock = new Mock<ISendHandler>();
            _mottattMelding = null;
            _ack = Mock.Of<Action>();
            _nack = Mock.Of<Action>();
            _nackWithRequeue = Mock.Of<Action>();
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

        public SvarSenderFixture WithNack(Action nack)
        {
            this._nack = nack;
            return this;
        }

        public SvarSenderFixture WithNackRequeue(Action nackRequeue)
        {
            this._nackWithRequeue = nackRequeue;
            return this;
        }

        public Mock<ISendHandler> SendHandlerMock { get; }

        public Func<Task<Stream>> DefaultStreamProvider => Mock.Of<Func<Task<Stream>>>();

        public IAsicDecrypter DefaultDecrypter => Mock.Of<IAsicDecrypter>();

        public IFileWriter DefaultFileWriter => Mock.Of<IFileWriter>();

        internal SvarSender CreateSut()
        {
            SetupMocks();
            return new SvarSender(SendHandlerMock.Object, _mottattMelding, new AmqpAcknowledgeManager(_ack, _nack, _nackWithRequeue));
        }

        private void SetupMocks()
        {
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MeldingRequest>(), It.IsAny<IPayload[]>()))
                .ReturnsAsync(new SendtMelding(Guid.NewGuid(), Guid.NewGuid(), "sendtMelding", Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, null));
        }
    }
}