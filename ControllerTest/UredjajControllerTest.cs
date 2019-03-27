using Autofac.Extras.Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using ZadatakNeki.Controllers;
using ZadatakNeki.DTO;
using ZadatakNeki.Models;
using ZadatakNeki.Repositorys.IRepository;

namespace ZadatakNeki.Test.ControllerTest
{
    public class UredjajControllerTest
    {
        public List<Uredjaj> ListaUredjaj()
        {
            List<Uredjaj> lista = new List<Uredjaj>()
            {
                new Uredjaj() {Naziv = "tv"},
                new Uredjaj() {Naziv = "laptop"},
                new Uredjaj() {Naziv = "punjac"},
                new Uredjaj() {Naziv = "telefon"}
            };
            return lista;
        }

        public List<UredjajDTO> ListaUredjajDto()
        {
            List<UredjajDTO> listaDto = new List<UredjajDTO>()
            {
                new UredjajDTO() {Naziv = "tv"},
                new UredjajDTO() {Naziv = "tv"},
                new UredjajDTO() {Naziv = "tv"},
                new UredjajDTO() {Naziv = "tv"}
            };
            return listaDto;
        }

        [Fact]
        public void Svi_KadRadi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(ListaUredjaj);
                mock.Mock<IMapper>().Setup(e => e.Map<UredjajDTO>(It.IsAny<Uredjaj>())).Returns(ListaUredjajDto()[0]);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Svi();
                var ocekujes = ListaUredjajDto();

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(2)]
        public void PoId_KadVracaUredjaj(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(ListaUredjaj()[0]);
                mock.Mock<IMapper>().Setup(e => e.Map<UredjajDTO>(It.IsAny<Uredjaj>())).Returns(ListaUredjajDto()[0]);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.PoId(id);
                var ocekujes = ListaUredjajDto()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(7)]
        public void PoId_KadUredjajNijePronadjen(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(() => null);

                var obj = mock.Create<UredjajController>();

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
                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Upisivanje(null);
                var ocekujes = "Upisi fino to.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void Upisivanje_KadSacuvaUredjaj()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IMapper>().Setup(e => e.Map<Uredjaj>(It.IsAny<UredjajDTO>()))
                    .Returns(ListaUredjaj()[0]);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Upisivanje(ListaUredjajDto()[0]);
                var ocekujes = "Nije uspelo, ahahah salim se sacuvao sam.";

                mock.Mock<IUredjajRepository>().Verify(e => e.DodajEntitet(It.IsAny<Uredjaj>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(1)]
        public void IzmenaPodataka_KadNemaEntitetaSaTimID(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(() => null);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.IzmenaPodataka(id, ListaUredjajDto()[0]);
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
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListaUredjaj()[0]);

                mock.Mock<IMapper>()
                    .Setup(e => e.Map(It.IsAny<UredjajDTO>(), It.IsAny<Uredjaj>()))
                    .Returns(ListaUredjaj()[0]);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.IzmenaPodataka(id, ListaUredjajDto()[0]);
                var ocekujes = "Vase izmene su sacuvane!";

                mock.Mock<IUredjajRepository>()
                    .Verify(e => e.Izmeni(It.IsAny<Uredjaj>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void Brisanje_KadNemaUredjajSaTimID(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(() => null);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Nema uredjaj sa datim ID.";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(2)]
        public void Brisanje_AkoJeUredjajKoriscen(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                Uredjaj uredjaj = new Uredjaj() {Id = 2, Naziv = "tv"};

                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(uredjaj);

                List<OsobaUredjaj> lista = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj() {Id = 14},
                    new OsobaUredjaj() {Id = 15, UredjajId = 2}
                };

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(lista);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Uredjaj nije izbrisan, zato sto se koristi ili je nekada vec bio koriscen.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(20012)]
        public void Brisanje_KadObriseUredjaj(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListaUredjaj()[0]);

                List<OsobaUredjaj> lista = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj() {Id = 14},
                    new OsobaUredjaj() {Id = 15, UredjajId = 2}
                };

                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(lista);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.Brisanje(id);
                var ocekujes = "Uredjaj je izbrisan iz baze podataka.";

                mock.Mock<IUredjajRepository>()
                    .Verify(e => e.ObrisiEntitet(It.IsAny<Uredjaj>()), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("tv")]
        public void PretragaPoImenu_KadVracaNotFound(string naziv)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.PretragaPoImenu(It.IsAny<string>()))
                    .Returns(() => null);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.PretragaPoImenu(naziv);
                var ocekujes = "Nista";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("tv")]
        public void PretragaPoImenu_KadVracaUredjaj(string naziv)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IUredjajRepository>().Setup(e => e.PretragaPoImenu(It.IsAny<string>()))
                    .Returns(ListaUredjaj()[0]);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.PretragaPoImenu(naziv);
                var ocekujes = ListaUredjaj()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Fact]
        public void PretragaUredjajiKojiSeKoriste_KadVracaUredjaje()
        {
            List<OsobaUredjaj> lista = new List<OsobaUredjaj>()
            {
                new OsobaUredjaj() {Id = 2, Uredjaj = new Uredjaj(){Naziv = "punjac"}}
            };

            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(lista);

                var obj = mock.Create<UredjajController>();

                var dobijas = obj.PretragaUredjajiKojiSeKoriste();

                List<UredjajDTO> ocekujes = new List<UredjajDTO>()
                {
                    new UredjajDTO() {Naziv = "punjac"}
                }; 

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Same(ocekujes.ToString(), result.Value.ToString());
            }
        }
    }
}
