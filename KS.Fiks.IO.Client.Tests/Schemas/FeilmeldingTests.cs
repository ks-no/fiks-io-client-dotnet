using System;
using System.Collections.Generic;
using System.IO;
using KS.Fiks.IO.Client.Models.Feilmelding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Schemas
{
    public class FeilmeldingTests
    {
        [Fact]
        public void TestServerFeilModelValidateWithSchema_Ok()
        {
            var serverFeil = new ServerFeil()
            {
                CorrelationId = "testCorrelationId",
                ErrorId = "testErrorId",
                Feilmelding = "testFeilmelding"
            };
            Assert.True(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Serverfeil}.schema.json"));
        }

        [Fact]
        public void TestServerFeilModelValidateWithSchema_Wrong_Datatype_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                errorId = -2,
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Serverfeil}.schema.json"));
        }

        [Fact]
        public void TestServerFeilModelValidateWithSchema_Missing_Property_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Serverfeil}.schema.json"));
        }

        [Fact]
        public void TestUgyldigforespørselModelValidateWithSchema_Ok()
        {
            var serverFeil = new Ugyldigforespoersel()
            {
                CorrelationId = "testCorrelationId",
                ErrorId = "testErrorId",
                Feilmelding = "testFeilmelding"
            };
            Assert.True(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ugyldigforespørsel}.schema.json"));
        }

        [Fact]
        public void TestUgyldigforespørselModelValidateWithSchema_Wrong_Datatype_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                errorId = -2,
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ugyldigforespørsel}.schema.json"));
        }

        [Fact]
        public void TestUgyldigforespørselModelValidateWithSchema_Missing_Property_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ugyldigforespørsel}.schema.json"));
        }

        [Fact]
        public void TestIkkefunnetlModelValidateWithSchema_Ok()
        {
            var serverFeil = new Ikkefunnet()
            {
                CorrelationId = "testCorrelationId",
                ErrorId = "testErrorId",
                Feilmelding = "testFeilmelding"
            };
            Assert.True(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ikkefunnet}.schema.json"));
        }

        [Fact]
        public void TestIkkefunnetModelValidateWithSchema_Wrong_Datatype_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                errorId = -2,
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ikkefunnet}.schema.json"));
        }

        [Fact]
        public void TestIkkefunnetModelValidateWithSchema_Missing_Property_Fail()
        {
            var serverFeil = new
            {
                correlationId = "testCorrelationId",
                feilmelding = "testFeilmelding"
            };
            Assert.False(JsonValidates(JsonConvert.SerializeObject(serverFeil), $@"./Schema/{FeilmeldingMeldingTypeV1.Ikkefunnet}.schema.json"));
        }

        private static bool JsonValidates(string jsonString, string pathToSchema)
        {
            var isOk = true;
            using TextReader file = File.OpenText(pathToSchema);
            var validator = JObject.Parse(jsonString);
            var schema = JSchema.Parse(file.ReadToEnd());
            validator.Validate(schema, (o, a) =>
            {
                isOk = false;
                Console.Out.WriteLine(a.Message);
            });
            return isOk;
        }
    }
}