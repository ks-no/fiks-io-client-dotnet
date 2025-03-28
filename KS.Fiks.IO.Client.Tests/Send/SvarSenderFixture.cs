using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Client.Amqp;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Models;
using Moq;

namespace KS.Fiks.IO.Client.Tests.Send
{
    internal class SvarSenderFixture
    {
        private MottattMelding _mottattMelding;

        private Func<Task> _ack;
        private Func<Task> _nack;
        private Func<Task> _nackWithRequeue;

        public SvarSenderFixture()
        {
            SendHandlerMock = new Mock<ISendHandler>();
            _mottattMelding = null;
            _ack = Mock.Of<Func<Task>>();
            _nack = Mock.Of<Func<Task>>();
            _nackWithRequeue = Mock.Of<Func<Task>>();
        }

        public SvarSenderFixture WithMottattMelding(MottattMelding mottattMelding)
        {
            _mottattMelding = mottattMelding;
            return this;
        }

        public SvarSenderFixture WithAck(Func<Task> ack)
        {
            _ack = ack;
            return this;
        }

        public SvarSenderFixture WithNack(Func<Task> nack)
        {
            this._nack = nack;
            return this;
        }

        public SvarSenderFixture WithNackRequeue(Func<Task> nackRequeue)
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
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MeldingRequest>(), It.IsAny<IPayload[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendtMelding(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), "sendtMelding", Guid.NewGuid(), Guid.NewGuid(),
                    TimeSpan.Zero, null));
            SendHandlerMock.Setup(_ => _.Send(It.IsAny<MeldingRequest>(), It.IsAny<IList<IPayload>>(), It.Is<CancellationToken>(x => x.IsCancellationRequested)))
                .ThrowsAsync(new TaskCanceledException());
        }
    }
}