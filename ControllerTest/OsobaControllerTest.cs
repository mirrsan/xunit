using Moq;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using ZadatakNeki.Controllers;
using ZadatakNeki.DTO;
using ZadatakNeki.Models;
using ZadatakNeki.Repositorys.IRepository;
using System;
using ZadatakNeki.Repository;

namespace ZadatakNeki.Test.ControllerTest
{
    public class OsobaControllerTest
    {
        public List<Osoba> ListaOsoba()

        {
            List<Osoba> lista = new List<Osoba>()
            {
                new Osoba()
                {
                    Ime = "fatma",
                    Prezime = "fatmovic",
                    Kancelarija = new Kancelarija() {Opis = "jes"}
                },
                new Osoba()
                {
                    Ime = "esma",
                    Prezime = "esmovic",
                    Kancelarija = new Kancelarija() {Opis = "biblioteka"}
                }
            };
            return lista;
        }

        public List<OsobaDTO> ListaOsobaDto()

        {
            List<OsobaDTO> lista = new List<OsobaDTO>()
            {
                new OsobaDTO()
                {
                    Ime = "esma",
                    Prezime = "esmovic",
                    Kancelarija = new KancelarijaDTO() {Opis = "biblioteka"}
                },
                new OsobaDTO()
                {
                    Ime = "esma",
                    Prezime = "esmovic",
                    Kancelarija = new KancelarijaDTO() {Opis = "biblioteka"}
                }
            };
            return lista;
        }

        [Fact]
        public void SveOsobe_radi()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>().Setup(e => e.DajSveEntitete()).Returns(ListaOsoba);
                mock.Mock<IMapper>().Setup(e => e.Map<OsobaDTO>(It.IsAny<Osoba>())).Returns(ListaOsobaDto()[0]);

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.SveOsobe();
                var ocekujes = ListaOsobaDto();

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData(10)]
        public void OsobaPoId_KadVracaOsobu(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var osoba = new Osoba()
                {
                    Ime = "w",
                    Prezime = "w",
                    Kancelarija = new Kancelarija() {Opis = "w"}

                };
                var osobaDto = new OsobaDTO()
                {
                    Ime = "w",
                    Prezime = "w",
                    Kancelarija = new KancelarijaDTO() {Opis = "w"}
                };
                mock.Mock<IOsobaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(osoba);
                mock.Mock<IMapper>().Setup(e => e.Map<OsobaDTO>(It.IsAny<Osoba>())).Returns(osobaDto);

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.OsobaPoId(id);

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(osobaDto, result.Value);
            }
        }

        [Theory]
        [InlineData(7)]
        public void OsobaPoId_KadOsobaNijePronadjena(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(() => null);

                var obj = mock.Create<OsobaController>();

                var ocekujes = "Nema ti toga vamo.";
                var dobijas = obj.OsobaPoId(id);

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void UpisivanjeOsobe_KadJeUlaziParametarNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<OsobaController>();

                var dobijas = obj.UpisivanjeOsobe(null);
                var ocekujes = "Niste upisali podatke da valja!";

                Assert.True(dobijas != null);
                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Fact]
        public void UpisivanjeOsobe_KadMetodSacuvaOsobu()
        {
            using (var mock = AutoMock.GetLoose())
            {
                OsobaDTO osoba = new OsobaDTO()
                {
                    Ime = "mirsan",
                    Prezime = "mirsan",
                    Kancelarija = new KancelarijaDTO() {Opis = "mirsan"}
                };

                mock.Mock<IMapper>().Setup(e => e.Map<Osoba>(It.IsAny<OsobaDTO>())).Returns(ListaOsoba()[0]);

                var obj = mock.Create<OsobaController>();
                var upisivanje = obj.UpisivanjeOsobe(osoba);

                mock.Mock<IOsobaRepository>().Verify(e => e.DodajEntitet(It.IsAny<Osoba>()), Times.Exactly(1));

                var ocekujes = "Osoba je sacuvana u bazi.";

                var result = Assert.IsType<OkObjectResult>(upisivanje);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(-4)]
        public void IzmenaOsobe_KadOsobaNijePronadjena(long id)
        {
            OsobaDTO osobaDto = new OsobaDTO()
            {
                Ime = "mirsan",
                Prezime = "mirsan",
                Kancelarija = new KancelarijaDTO() {Opis = "mirsan"}
            };

            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<OsobaController>();

                var ocekujes = "Nije pronadjena osoba sa datim ID.";
                var dobijas = obj.IzmenaOsobe(id, osobaDto);

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(8)]
        public void IzmenaOsobe_KadSuSacuvaneInfo(long id)
        {
            OsobaDTO osobaDto = new OsobaDTO()
            {
                Ime = "mirsan",
                Prezime = "mirsan",
                Kancelarija = new KancelarijaDTO() {Opis = "mirsan"}
            };
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>()
                    .Setup(e => e.EntitetPoId(It.IsAny<long>()))
                    .Returns(ListaOsoba()[0]);

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.IzmenaOsobe(id, osobaDto);
                var ocekujes = "Sacuvane su izmene.";

                mock.Mock<IUnitOfWork>().Verify(e => e.Sacuvaj(), Times.Exactly(1));

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(-1)]
        public void BrisanjeOsobe_KadOsobaNijePronadjena(long id)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<OsobaController>();

                var dobijas = obj.BrisanjeOsobe(id);
                var ocekujes = "Nema osobe koju hoces brisat.";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData(4)]
        public void BrisanjeOsobe_KadOsobaKoristiUredjaj(long id)
        {
            Osoba osoba = new Osoba()
            {
                Ime = "mirsan",
                Prezime = "kajovic",
                Kancelarija = new Kancelarija() {Opis = "kuca"}
            };
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>().Setup(e => e.EntitetPoId(It.IsAny<long>())).Returns(osoba);

                List<OsobaUredjaj> osobaUredjaj = new List<OsobaUredjaj>()
                {
                    new OsobaUredjaj(){Id = 16, Osoba = osoba },
                    new OsobaUredjaj() {Id = 4}
                };
                mock.Mock<IOsobaUredjajRepository>().Setup(e => e.DajSveEntitete()).Returns(osobaUredjaj);

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.BrisanjeOsobe(id);
                var ocekujes = "Osoba nije izbrisana iz baze zato sto je vec koristila/koristi neki od uredjaja.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("Ramiz")]
        public void PretragaPoImenuOsobe_KadNemaOsobeSaDadimImenom(string ime)
        {
            using (var mock = AutoMock.GetLoose())
            {
                var obj = mock.Create<OsobaController>();

                var dobijas = obj.PretragaPoImenuOsobe(ime);
                var ocekujes = "Nema te osobe";

                var result = Assert.IsType<NotFoundObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("ramiz")]
        public void PretragaPoImenuOsobe_KadVracaOsobu(string ime)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>()
                    .Setup(e => e.PretragaPoImenu(It.IsAny<string>()))
                    .Returns(ListaOsoba()[0]);

                mock.Mock<IMapper>().Setup(e => e.Map<OsobaDTO>(It.IsAny<Osoba>())).Returns(ListaOsobaDto()[0]);

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.PretragaPoImenuOsobe(ime);
                var ocekujes = ListaOsobaDto()[0];

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Equal(ocekujes.ToString(), result.Value.ToString());
            }
        }

        [Theory]
        [InlineData("marketing")]
        public void PretragaPoOpisuKancelarije_KadNemaKancelarije(string kancelarija)
        {
            using (var mock = AutoMock.GetLoose())
            {

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.PretragaPoOpisuKancelarije(kancelarija);
                var ocekujes = "Ne postoji ta kancelarija.";

                var result = Assert.IsType<BadRequestObjectResult>(dobijas);
                Assert.Equal(ocekujes, result.Value);
            }
        }

        [Theory]
        [InlineData("biblioteka")]
        public void PretragaPoOpisuKancelarije_KadVracaOsobe(string kancelarija)
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOsobaRepository>().Setup(e => e.DajSveEntitete()).Returns(ListaOsoba);

                mock.Mock<IMapper>().Setup(e => e.Map<OsobaDTO>(It.IsAny<Osoba>())).Returns(ListaOsobaDto()[0]);

                mock.Mock<IKancelarijaRepository>()
                    .Setup(e => e.PretragaPoNazivu(It.IsAny<string>()))
                    .Returns(new Kancelarija() {Opis = "ima"});

                var obj = mock.Create<OsobaController>();

                var dobijas = obj.PretragaPoOpisuKancelarije(kancelarija);

                List<OsobaDTO> ocekujes = new List<OsobaDTO>();
                ocekujes.Add(ListaOsobaDto()[0]);

                var result = Assert.IsType<OkObjectResult>(dobijas);
                Assert.Same(ocekujes.ToString(), result.Value.ToString());
            }
        }
    }
}
