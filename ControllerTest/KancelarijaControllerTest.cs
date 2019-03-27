using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Autofac.Extras.Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ZadatakNeki.Controllers;
using ZadatakNeki.DTO;
using ZadatakNeki.Models;
using ZadatakNeki.Repositorys.IRepository;

namespace ZadatakNeki.Test.ControllerTest
{
    public class KancelarijaControllerTest
    {
        public List<Kancelarija> ListaKa()
        {
            List<Kancelarija> lista = new List<Kancelarija>()
                {
                    new Kancelarija() {Opis = "kuca"},
                    new Kancelarija() {Opis = "marketing"},
                    new Kancelarija() {Opis = "kantina"},
                    new Kancelarija() {Opis = "sala za sastanke"}
                };
            return lista;
        }

        public List<KancelarijaDTO> ListaKaDto()
        {
            List<KancelarijaDTO> dto = new List<KancelarijaDTO>()
            {
                new KancelarijaDTO() {Opis = "kuca"},
                new KancelarijaDTO() {Opis = "kuca"},
                new KancelarijaDTO() {Opis = "kuca"},
                new KancelarijaDTO() {Opis = "kuca"}
            };
            return dto;
        }

        [Fact]
        public void SveKancelarije_radi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.DajSveEntitete()).Returns(ListaKa);
                mock.Mock<IMapper>().Setup(e => e.Map<KancelarijaDTO>(It.IsAny<Kancelarija>())).Returns(ListaKaDto()[0]);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.SveKancelarije();
                var ocekujes = ListaKaDto();

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(10)]
        public void PoId_KadVracaKancelariju(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(ListaKa()[0]);
                mock.Mock<IMapper>().Setup(e => e.Map<KancelarijaDTO>(It.IsAny<Kancelarija>())).Returns(ListaKaDto()[0]);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.PoId(id);
                var ocekujes = ListaKaDto()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(7)]
        public void PoId_KadKancelarijaNijePronadjena(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(() => null);

                var obj = mock.Create<KancelarijaController>();

                var ocekujes = "Nema ti toga vamo.";
                var dobijas = obj.PoId(id);

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void Upisivanje_KadJeUlazniParametarNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.Upisivanje(null);
                var ocekujes = "Upisi fino to.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void Upisivanje_KadSacuvaKancelariju()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IMapper>().Setup(e => e.Map<Kancelarija>(It.IsAny<KancelarijaDTO>())).Returns(ListaKa()[0]);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.Upisivanje(ListaKaDto()[0]);
                var ocekujes = "Nije uspelo, ahahah salim se sacuvao sam.";

                mock.Mock<IKancelarijaRepository>().Verify(e => e.DodajEntitet(It.IsAny<Kancelarija>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(-1)]
        public void IzmenaPodataka_KadNemaEntitetaSaTimID(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.IzmenaPodataka(id, ListaKaDto()[0]);
                var ocekujes = "Ne mogu naci entitet koji zelis da menjas.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(1)]
        public void IzmenaPodataka_KadSuIzmeneSacuvane(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(ListaKa()[0]);

                mock.Mock<IMapper>()
                    .Setup(e => e.Map(It.IsAny<KancelarijaDTO>(), It.IsAny<Kancelarija>()))
                    .Returns(ListaKa()[0]);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.IzmenaPodataka(id, ListaKaDto()[0]);
                var ocekujes = "Vase izmene su sacuvane!";

                mock.Mock<IKancelarijaRepository>().Verify(e => e.Izmeni(It.IsAny<Kancelarija>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void Brisanje_KadNemaKancelarijeSaTimID(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(() => null);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Nema takve kancelarije.";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void Brisanje_KadSeBriseKancelarijaKantina(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var k = new Kancelarija() {Opis = "kantina"};

                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(k);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Necu obrisat kantinu.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void Brisanje_KadObriseKancelariju(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(ListaKa()[0]);

                List<Osoba> osobe = new List<Osoba>()
                {
                    new Osoba() {Id = 16, Kancelarija = ListaKa()[0]},
                    new Osoba() {Id = 13}
                };

                mock.Mock<IOsobaRepository>().Setup(e => e.DajSveEntitete()).Returns(osobe);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Kancelarija je izbrisana iz baze podataka.";

                mock.Mock<IKancelarijaRepository>()
                    .Verify(e => e.ObrisiEntitet(It.IsAny<Kancelarija>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("kuca")]
        public void PretragaPoNazivu_KadVracaNotFound(string opis)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.PretragaPoNazivu(It.IsAny<string>()))
                    .Returns(() => null);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.PretragaPoNazivu(opis);
                var ocekujes = "Nema";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("kuca")]
        public void PretragaPoNazivu_KadVracaKancelariju(string opis)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IKancelarijaRepository>().Setup(e => e.PretragaPoNazivu(It.IsAny<string>()))
                    .Returns(ListaKa()[0]);

                mock.Mock<IMapper>().Setup(e => e.Map<KancelarijaDTO>(It.IsAny<Kancelarija>()))
                    .Returns(ListaKaDto()[0]);

                var obj = mock.Create<KancelarijaController>();

                var dobijas = obj.PretragaPoNazivu(opis);
                var ocekujes = ListaKaDto()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }
    }
}
